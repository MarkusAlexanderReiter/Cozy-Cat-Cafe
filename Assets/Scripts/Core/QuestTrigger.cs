/* -----------------------------------------------------------
 * QuestTrigger
 *  Attach to a customer to mark it as quest-related.
 * ----------------------------------------------------------*/
using UnityEngine;

public sealed class QuestTrigger : MonoBehaviour
{
    /// <summary>Data assigned by SpawnManager.</summary>
    public QuestData QuestData { get; private set; }

    /// <summary>SpawnManager injects the SO here.</summary>
    public void Initialize(QuestData data) => QuestData = data;

    // Add OnPlayerInteract() etc. later
}
