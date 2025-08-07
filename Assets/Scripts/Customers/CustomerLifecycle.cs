using System.Collections;
using System; // Added for time tracking and enums
using UnityEngine.AI;
using UnityEngine;
using TMPro; // For request UI label

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



public enum CustomerRequestType
{
    // TODO: Expand with actual menu items
    Drink,
    Dish,
    Item
}

public partial class CustomerLifecycle : MonoBehaviour
{
    #region Fields
    private Transform _assignedSeat;
    // ----- New Customer Goal System -----
    private CustomerRequestType _currentRequest;
    private float _spawnTime; // Time when customer becomes active
    private float _lastWaitDuration; // Time spent waiting for a seat
    private bool _isSatisfied;
    // Visual feedback (emote bubble). Assign a prefab in the inspector.
    [Tooltip("Prefab for happy/angry emote bubble.")]
    [SerializeField] private GameObject emoteBubblePrefab;
    // ----- End New System -----
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
        // Ensure label starts hidden when the customer is (re)enabled
        SetRequestLabelVisible(false);
        _currentRequest = CustomerRequestType.Drink; // TODO: Randomize or assign based on level design
        // Show the request label now that we have a request
        SetRequestLabelVisible(true);
        _spawnTime = Time.time;
        _lastWaitDuration = 0f;
        _isSatisfied = false;
        _seatRoutine = null;
        _waitCoroutine = null;
        // Reset agent for new run
        _agent.enabled = true;
        _agent.isStopped = true;
        EnsureOnNavMesh(transform.position);
        Debug.Log($"_requestLabel is {(_requestLabel ? "assigned" : "NULL")}", this);

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
            // Record how long the customer waited for a seat
            _lastWaitDuration = Time.time - _spawnTime;
            // TODO: Use _lastWaitDuration to influence satisfaction later
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
        float waitTime = UnityEngine.Random.Range(10f, 15f);
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

        // Customer has reached the seat – show the request label
        SetRequestLabelVisible(true);

        // Sit duration (randomised)
        float seatedTime = UnityEngine.Random.Range(3f, 5f);
        // TODO: Here you could trigger a request fulfillment check (e.g., player delivered correct item)
        // For now we simulate a simple satisfaction evaluation based on wait time.
        EvaluateSatisfaction();
        yield return new WaitForSeconds(seatedTime);

        // Leave seat
        if (_assignedSeat != null)
        {
            SeatManager.Instance.FreeSeat(_assignedSeat);
            _assignedSeat = null;
            // Hide request label now that the customer is leaving
            SetRequestLabelVisible(false);
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
        // Show visual feedback based on satisfaction
        ShowEmote(_isSatisfied);
        // Reset emote after a short display time
        if (emoteBubbleInstance != null)
        {
            Destroy(emoteBubbleInstance, 1.5f);
        }

        // Despawn / return to pool
        _agent.enabled = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Warps the NavMeshAgent onto the NavMesh near a given position if it's currently off-mesh.
    /// Safe-guards pooled objects whose spawn point may be slightly outside the baked area.
    /// </summary>
    private GameObject emoteBubbleInstance;

    private void EvaluateSatisfaction()
    {
        // Simple metric: if wait time < 5 seconds, consider satisfied.
        // TODO: Incorporate correct item delivery check.
        _isSatisfied = _lastWaitDuration <= 5f;
        // Use _currentRequest to avoid unused field warning (placeholder logic)
        var request = _currentRequest; // currently unused, will affect future logic
    }
    

    private void ShowEmote(bool happy)
    {
        if (emoteBubblePrefab == null) return;
        // Instantiate bubble above the customer
        emoteBubbleInstance = Instantiate(emoteBubblePrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity, transform);
        // TODO: Set bubble sprite or animation based on 'happy' flag.
        // Example placeholder: change color
        var renderer = emoteBubbleInstance.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = happy ? Color.green : Color.red;
        }
    }

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