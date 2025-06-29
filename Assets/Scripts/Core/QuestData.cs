/* -----------------------------------------------------------
 * QuestData
 *  A minimal ScriptableObject so SpawnManager compiles.
 *  Expand with rewards, icons, dialogue, etc. later.
 * ----------------------------------------------------------*/
using UnityEngine;

[CreateAssetMenu(fileName = "QuestData", menuName = "Game/Quests/Quest Data")]
public sealed class QuestData : ScriptableObject
{
    [Tooltip("Unique identifier, e.g. 'first_milestone'")]
    public string questId = "new_quest";

    [TextArea(2, 4)]
    public string description = "Describe the quest…";
}
