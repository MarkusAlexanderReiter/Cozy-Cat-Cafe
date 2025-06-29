/*─────────────────────────────────────────────────────────────
 * PoolRegistry
 *  Keeps a mapping prefab → SimplePool instance.
 *  SpawnManager asks the registry for a pool each time.
 *────────────────────────────────────────────────────────────*/
using System.Collections.Generic;
using UnityEngine;

namespace Game.Pooling
{

    /// <summary>
    /// Central registry that lazily creates and caches one <see cref="SimplePool"/>
    /// per prefab. Spawn systems query it instead of scattering pool components
    /// throughout the scene.
    /// </summary>
    public sealed class PoolRegistry : MonoBehaviour
{
    [SerializeField] private Transform poolsParent; // optional

    private readonly Dictionary<GameObject, SimplePool> dict = new();

        /// <summary>
        /// Returns an existing pool for <paramref name="prefab"/> or creates a new one.
        /// Newly created pools are warmed with <paramref name="warmSize"/> inactive objects.
        /// </summary>
        public SimplePool GetOrCreatePool(GameObject prefab, int warmSize = 10)
    {
        if (dict.TryGetValue(prefab, out var pool)) return pool;

        // Create a new pool GameObject
        GameObject go = new(prefab.name + "_Pool");
        if (poolsParent != null) go.transform.SetParent(poolsParent);
        pool = go.AddComponent<SimplePool>();
        pool.hideFlags = HideFlags.DontSave;
        pool.Init(prefab, warmSize);

        dict.Add(prefab, pool);
        return pool;
    }
}
}
