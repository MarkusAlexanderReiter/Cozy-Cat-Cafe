/* -----------------------------------------------------------
 * SpawnManager
 *  Spawns customers at a point when the "Spawn" input action
 *  is performed. Emits an event-channel notification so
 *  other systems (animations, audio, UI...) react.
 *
 
 * ----------------------------------------------------------*/
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Game.Pooling;

namespace Game.Spawning
{
    /// <summary>
    /// Manages customer spawning: queues requests, throttles input,
    /// rents pooled instances, and raises an event for listeners.
    /// </summary>
    [AddComponentMenu("Game/Spawning/Spawn Manager")]
public sealed class SpawnManager : MonoBehaviour
{
    #region Fields
    [Header("Dependencies")]
    [Tooltip("Event channel asset that broadcasts spawn events")]
    [SerializeField] private CustomerSpawnedChannelSO spawnedChannel;
    
    
    private PlayerControls inputActions;

    [Header("Spawn Settings")]
    [Tooltip("Point in world space where customers appear")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Default request if none queued")]
    [SerializeField] private SpawnRequestSO defaultRequest;

    [Header("Throttling")]
    [SerializeField, Min(0.05f)]
    private float spawnCooldown = 0.10f;

    [Header("Pooling")]
    [SerializeField] private PoolRegistry poolRegistry;

    // ──────────────────────────────── STATE ─────────────────
    private readonly Queue<SpawnRequestSO> queue = new();
    private float   lastSpawnTime = -Mathf.Infinity;
    private InputAction spawnAction;

    #endregion
    #region Unity
    /// <summary>
    /// Unity callback invoked when the component is constructed.
    /// Performs sanity checks on serialized fields and configures the input action.
    /// </summary>
    private void Awake()
    {
        Debug.Assert(spawnPoint     != null, $"{name} has no spawnPoint.",     this);
        Debug.Assert(spawnedChannel != null, $"{name} has no event channel.",  this);
        Debug.Assert(defaultRequest != null, $"{name} has no default request.",this);

        // Set up input
        inputActions ??= new PlayerControls();
        spawnAction = inputActions.Gameplay.Spawn;
    }

    // TODO(ticket-123): Replace temporary test input key with final spawn triggers (e.g., timers, game events).
    private void OnEnable()  => spawnAction.performed += OnSpawnPerformed;
    private void OnDisable() => spawnAction.performed -= OnSpawnPerformed;

    private void Start()     => spawnAction.Enable();
    private void OnDestroy() => spawnAction.Disable();

    #endregion
    #region Public
        /// <summary>
    /// Enqueues a spawn request; processed when the Spawn action is performed.
    /// </summary>
    public void QueueSpawn(SpawnRequestSO request) => queue.Enqueue(request);

    #endregion
    #region Private
    private void OnSpawnPerformed(InputAction.CallbackContext _) => TrySpawn();

    // ───────────────────────────── CORE ─────────────────────
    /// <summary>
    /// Throttles spawn input, dequeues the next request (or falls back to the default),
    /// validates it, and kicks off <see cref="SpawnRoutine"/>.
    /// </summary>
    // NOTE: called by input callback — keep it lean; everything heavy happens in the coroutine.
    private void TrySpawn()
    {
        if (Time.time - lastSpawnTime < spawnCooldown) return;

        SpawnRequestSO request = queue.Count > 0 ? queue.Dequeue() : defaultRequest;
        if (!Validate(request)) return;

        StartCoroutine(SpawnRoutine(request));
        lastSpawnTime = Time.time;
    }

    /// <summary>
    /// Coroutine that actually performs the spawn: rents an object from the pool,
    /// positions and configures it, then notifies listeners via the event channel.
    /// </summary>
    /// <remarks>
    /// Pool pre-warm guideline: During level load, call
    /// <c>pool.PreWarm(n);</c> or simply rent and immediately disable
    /// a few instances to avoid first-use hitch.
    /// </remarks>
    private IEnumerator SpawnRoutine(SpawnRequestSO request)
    {
        // 1. Get pool
        SimplePool pool = poolRegistry.GetOrCreatePool(request.prefab, 10);

        // 2. Rent object
        GameObject go = pool.Rent();
        if (go == null) yield break; // pool exhausted & not expandable

        // 3. Position / reset
        Transform t = go.transform;
        t.position = spawnPoint.position;
        t.rotation = Quaternion.identity;

        ConfigureCustomer(go, request);
        spawnedChannel.Raise(go);
        yield return null;
    }

    // Add quest/VIP components etc.
    /// <summary>
    /// Adds quest/VIP components to the spawned customer based on the request.
    /// </summary>
    private static void ConfigureCustomer(GameObject go, SpawnRequestSO req)
    {
        if (req.customerType == CustomerType.Quest)
        {
            var qt = go.GetComponent<QuestTrigger>() ?? go.AddComponent<QuestTrigger>();
            qt.Initialize(req.questData);
        }
        else if (req.customerType == CustomerType.Vip)
        {
            if (go.GetComponent<VipCustomer>() == null)
                go.AddComponent<VipCustomer>();
        }
    }

    // Runtime safety net
    /// <summary>
    /// Ensures the request and its prefab are non-null before proceeding.
    /// </summary>
    private static bool Validate(SpawnRequestSO req)
    {
        if (req == null || req.prefab == null)
        {
            Debug.LogError("SpawnRequest invalid – missing prefab.");
            return false;
        }
        return true;
    }

    #endregion
}
}

