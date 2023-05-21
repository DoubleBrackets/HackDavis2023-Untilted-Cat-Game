using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ConvoDebugSO", fileName = "DebugSO")]
public class ConversationDebugSO : ScriptableObject
{
    [TextArea(1, 1000)] public string Dialogue;
}
