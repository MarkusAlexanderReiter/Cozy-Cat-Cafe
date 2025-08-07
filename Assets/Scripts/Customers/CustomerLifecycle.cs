using System.Collections;
using UnityEngine.AI;
using UnityEngine;

/// <summary>
/// Basic lifecycle logic for a customer:
/// • On spawn, attempts to claim a free seat via <see cref="SeatManager"/>.
/// • If no seat is available, waits a random 10‒15 s and despawns unless a seat becomes free meanwhile.
/// • When destroyed, frees its seat again.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody2D))]

public class NavMeshDebug : MonoBehaviour
{
    void Start()
    {
        var agent = GetComponent<NavMeshAgent>();
        Debug.Log($"isOnNavMesh={agent.isOnNavMesh}");
        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas))
                Debug.Log("Nearest polygon at " + hit.position);
            else
                Debug.Log("No NavMesh within 2 m of spawn.");
        }
    }
}



public class CustomerLifecycle : MonoBehaviour
{
    #region Fields
    private Transform _assignedSeat;
    private Coroutine _waitCoroutine;
    private Coroutine _seatRoutine;
    [Tooltip("Optional; if assigned, customer will return to this transform before despawning.")]
    [SerializeField] private GameObject spawnPointObject;
    private Transform SpawnPointTransform => spawnPointObject != null ? spawnPointObject.transform : null;
    private Vector3 _spawnPos;
    private NavMeshAgent _agent;
    #endregion

    #region Unity
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        // NavMeshPlus is often added after the prefab already exists, so ensure the agent component
        // is present at runtime to avoid MissingComponentException.
        if (_agent == null)
        {
            _agent = gameObject.AddComponent<NavMeshAgent>();
            // Configure 2D-friendly defaults
            _agent.angularSpeed = 0f;
            _agent.height = 0f;
            _agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }
        // 2D settings
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
    }

    private void OnEnable()
    {
        // Reset state each time object is activated from pool
        _assignedSeat = null;
        _seatRoutine = null;
        _waitCoroutine = null;
        // Reset agent for new run
        _agent.enabled = true;
        _agent.isStopped = true;
        EnsureOnNavMesh(transform.position);

        // Determine spawn position: prefer explicit spawnPoint Transform if assigned
        _spawnPos = SpawnPointTransform != null ? SpawnPointTransform.position : transform.position;
        TryClaimSeat();
    }
    private void Start()
    {
        Debug.Assert(SeatManager.Instance != null, "SeatManager not in scene");
        // Nothing -- handled in OnEnable()
    }

    private void OnDisable()
    {
        // When object is pooled (disabled) release seat & stop waiting
        if (_waitCoroutine != null)
        {
            StopCoroutine(_waitCoroutine);
            _waitCoroutine = null;
        }
        if (_seatRoutine != null)
        {
            StopCoroutine(_seatRoutine);
            _seatRoutine = null;
        }

        if (_assignedSeat != null && SeatManager.Instance != null)
        {
            SeatManager.Instance.FreeSeat(_assignedSeat);
            _assignedSeat = null;
        }
    }

    private void OnDestroy()
    {
        // Safety net – in case object is truly destroyed, also free seat
        if (_assignedSeat != null && SeatManager.Instance != null)
        {
            SeatManager.Instance.FreeSeat(_assignedSeat);
        }
    }
    #endregion

    #region Public
    /// <summary>
    /// Message hook invoked by <see cref="SeatManager"/> when a seat becomes free for this customer.
    /// </summary>
    public void OnSeatAvailable()
    {
        if (_assignedSeat != null) return; // Already seated

        TryClaimSeat();
    }

    /// <summary>
    /// Assigns the spawn point transform for this customer. Used when the prefab is instantiated from a dynamic overlay scene.
    /// </summary>
    /// <param name="point">Transform that represents the spawn location.</param>
    public void SetSpawnPoint(Transform point)
    {
        // Store the reference so SpawnPointTransform works
        spawnPointObject = point != null ? point.gameObject : null;
        // Also cache the exact spawn position (including Y) for later use
        if (point != null)
        {
            _spawnPos = point.position;
        }
        else
        {
            // If no point is provided, fall back to current transform position
            _spawnPos = transform.position;
        }
    }
    #endregion

    #region Private
    private void TryClaimSeat()
    {
        _assignedSeat = SeatManager.Instance.RequestSeat(gameObject);

        if (_assignedSeat != null)
        {
            // Seat acquired – cancel any waiting and snap to seat (movement later)
            if (_waitCoroutine != null)
            {
                StopCoroutine(_waitCoroutine);
                _waitCoroutine = null;
            }

            // Delay 3 s before teleporting to seat then start seated lifecycle
            if (_seatRoutine != null)
                StopCoroutine(_seatRoutine);
            _seatRoutine = StartCoroutine(SeatLifecycleRoutine());
        }
        else if (_waitCoroutine == null) // Already waiting?
        {
            _waitCoroutine = StartCoroutine(WaitAndDespawnRoutine());
        }
    }

    private IEnumerator WaitAndDespawnRoutine()
    {
        // Randomised patience between 10–15 seconds
        float waitTime = Random.Range(10f, 15f);
        yield return new WaitForSeconds(waitTime);

        // Final attempt before giving up
        if (_assignedSeat == null)
        {
            _assignedSeat = SeatManager.Instance.RequestSeat(gameObject);
        }

        if (_assignedSeat == null)
        {
            // Remove from wait queue then return to pool (disable GameObject)
            SeatManager.Instance.RemoveFromQueue(gameObject);
            gameObject.SetActive(false);
        }
        else
        {
            if (_seatRoutine != null)
                StopCoroutine(_seatRoutine);
            _seatRoutine = StartCoroutine(SeatLifecycleRoutine());
        }
    }
    /// <summary>
    /// Sequence: wait 3 s, teleport to seat, stay 10 s, teleport back to spawn, wait 2 s, despawn.
    /// </summary>
    private IEnumerator SeatLifecycleRoutine()
    {
        // Wait 3 s before moving to seat
        yield return new WaitForSeconds(3f);
        if (_assignedSeat != null)
        {
            _agent.isStopped = false;
            _agent.SetDestination(_assignedSeat.position);
            // wait until agent reaches seat (only while on NavMesh)
            while (_agent.isOnNavMesh && (_agent.pathPending || _agent.remainingDistance > 0.05f))
                yield return null;
            _agent.isStopped = true;
        }

        // Sit duration (randomised)
        float seatedTime = Random.Range(3f, 5f);
        yield return new WaitForSeconds(seatedTime);

        // Leave seat
        if (_assignedSeat != null)
        {
            SeatManager.Instance.FreeSeat(_assignedSeat);
            _assignedSeat = null;
        }

        // Pathfind back to spawn position
        // Debug logging: record current position before returning to spawn
        Debug.Log($"[CustomerLifecycle] Returning to spawn. Current position: {transform.position}, SpawnPos: {(SpawnPointTransform != null ? SpawnPointTransform.position : _spawnPos)}");
        EnsureOnNavMesh(transform.position);
        if (_agent.enabled)
        {
            _agent.isStopped = false;
            _agent.SetDestination(SpawnPointTransform != null ? SpawnPointTransform.position : _spawnPos);
                Debug.Log($"[CustomerLifecycle] SetDestination called. Target: {(SpawnPointTransform != null ? SpawnPointTransform.position : _spawnPos)}");
            while (_agent.isOnNavMesh && (_agent.pathPending || _agent.remainingDistance > 0.05f))
                yield return null;
            // Snap exactly onto the spawn point. NavMeshAgent stops at remainingDistance, so warp to precise position.
                // Directly teleport to spawn (bypass NavMesh to keep exact Y)
                Vector3 targetPos = SpawnPointTransform != null ? SpawnPointTransform.position : _spawnPos;
                // Stop the agent before disabling it to avoid "Stop can only be called on an active agent" error
                _agent.isStopped = true;
                _agent.enabled = false;
                transform.position = targetPos;
                Debug.Log($"[CustomerLifecycle] Teleported to spawn. New position: {transform.position}");
        }
        else
        {
            // Fallback: just teleport if agent is disabled
                // Fallback teleport (use full spawn position)
                Vector3 fallbackPos = SpawnPointTransform != null ? SpawnPointTransform.position : _spawnPos;
                transform.position = fallbackPos;
                Debug.Log($"[CustomerLifecycle] Fallback teleport to spawn. New position: {transform.position}");
                Debug.Log($"[CustomerLifecycle] Fallback teleport to spawn. New position: {transform.position}");
        }

        yield return new WaitForSeconds(2f);

        // Despawn / return to pool
        _agent.enabled = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Warps the NavMeshAgent onto the NavMesh near a given position if it's currently off-mesh.
    /// Safe-guards pooled objects whose spawn point may be slightly outside the baked area.
    /// </summary>
    private void EnsureOnNavMesh(Vector3 nearPos)
    {
        if (_agent.isOnNavMesh) return;
        if (NavMesh.SamplePosition(nearPos, out var hit, 1f, NavMesh.AllAreas))
        {
            Debug.Log($"[CustomerLifecycle] SamplePosition hit at {hit.position} for nearPos {nearPos}");
            _agent.Warp(hit.position);
        }
        else
        {
            Debug.LogWarning($"[CustomerLifecycle] EnsureOnNavMesh failed: no NavMesh near {nearPos}");
            // As a fallback, disable pathfinding for this run
            _agent.enabled = false;
        }
    }

    #endregion
}
