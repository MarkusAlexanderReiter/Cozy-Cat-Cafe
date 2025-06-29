/* -----------------------------------------------------------
 * SpawnRequestSO
 *  Designer-authored asset describing *what* to spawn.
 *  OnValidate() shouts in the Inspector if something’s wrong.
 * ----------------------------------------------------------*/
using UnityEngine;

public enum CustomerType { Regular, Vip, Quest }

[CreateAssetMenu(menuName = "Spawning/Spawn Request")]
public sealed class SpawnRequestSO : ScriptableObject
{
    [Header("Essential")]
    public GameObject prefab;
    public CustomerType customerType = CustomerType.Regular;

    [Header("Quest-only")]
    public QuestData questData;

    /* ---------- EDITOR-TIME GUARD RAILS ---------- */
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (prefab == null)
            Debug.LogError($"[{name}] has no prefab!", this);

        if (customerType == CustomerType.Quest && questData == null)
            Debug.LogError($"[{name}] Quest customer needs QuestData.", this);
    }
#endif
}
