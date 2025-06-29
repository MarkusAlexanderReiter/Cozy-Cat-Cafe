/*─────────────────────────────────────────────────────────────
 * SimplePool
 *  A lightweight generic object pool for MonoBehaviours.
 *  - Pre-warms N instances on Awake.
 *  - Optional expansion when empty.
 *  - Zero allocations after warm-up.
 *  - Public Size / ActiveCount for debugging.
 *────────────────────────────────────────────────────────────*/
using System.Collections.Generic;
using UnityEngine;

namespace Game.Pooling
{
    /// <summary>
    /// Generic object pool that reuses <see cref="GameObject"/> instances to
    /// eliminate expensive <c>Instantiate</c>/<c>Destroy</c> calls and GC churn.
    /// </summary>
    public sealed class SimplePool : MonoBehaviour
    {
        // ────── Inspector ──────
        [Header("Pool Settings")]
        [SerializeField] private GameObject prefab;
        [SerializeField, Min(1)] private int initialSize = 10;
        [SerializeField] private bool expandIfFull = true;

        // ────── Runtime ──────
        private readonly Stack<GameObject> stack = new();
        public int Size         => stack.Count + activeCount;
        public int ActiveCount  => activeCount;
        private int activeCount;
        private bool initialized;

        // ────── Lifecycle ─────
        private void Awake()
        {
            // When set up via Inspector, warm immediately. Otherwise, wait for Init().
            if (prefab != null)
            {
                Warm(initialSize);
                initialized = true;
            }
        }

        private void Warm(int count)
        {
            for (int i = 0; i < count; i++)
                stack.Push(CreateInstance());
        }

        private GameObject CreateInstance()
        {
            GameObject go = Instantiate(prefab, transform);
            go.SetActive(false);
            go.AddComponent<PoolReturnHook>().Init(this);
            return go;
        }

        // ────── API ──────
        /// <summary>
        /// Initializes a pool created at runtime (e.g. by <see cref="PoolRegistry"/>).
        /// Must be called exactly once after <c>AddComponent</c>.
        /// </summary>
        public void Init(GameObject prefab, int warmSize)
        {
            if (initialized) return;

            this.prefab = prefab;
            initialSize = Mathf.Max(1, warmSize);
            Warm(initialSize);
            initialized = true;
        }
        /// <summary>
        /// Retrieves an inactive object from the pool, expanding if necessary/allowed.
        /// The returned object is activated before being handed to the caller.
        /// </summary>
        public GameObject Rent()
        {
            if (stack.Count == 0)
            {
                if (!expandIfFull)
                {
                    Debug.LogWarning($"{name}: pool exhausted.");
                    return null;
                }
                Warm(1);
            }

            GameObject go = stack.Pop();
            go.SetActive(true);
            activeCount++;
            return go;
        }

        /// <summary>
        /// Returns an object to the pool (called automatically by <see cref="PoolReturnHook"/>).
        /// The object is deactivated and pushed back onto the internal stack.
        /// </summary>
        internal void Return(GameObject go)
        {
            go.SetActive(false);
            stack.Push(go);
            activeCount--;
        }

        /// <summary>
        /// Pre-warms additional instances at runtime (e.g. during a loading screen)
        /// to avoid the first-time instantiation hitch. It simply instantiates
        /// <paramref name="count"/> extra objects and keeps them inactive in the pool.
        /// </summary>
        public void PreWarm(int count)
        {
            if (count <= 0) return;
            Warm(count);
        }
    }

    /*---------------------------------------------------------*/
    /* Helper component—attached automatically by the pool.
     * When the object is disabled *for any reason* (e.g.,
     * animation event, outside code), it returns itself.    */
    /*---------------------------------------------------------*/
    internal sealed class PoolReturnHook : MonoBehaviour
    {
        private SimplePool owner;
        internal void Init(SimplePool pool) => owner = pool;

        private void OnDisable()
        {
            if (owner != null && gameObject.scene.isLoaded)
                owner.Return(gameObject);
        }
    }
}
