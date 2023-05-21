using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ChatConfig", fileName = "ChatConfigSO")]
public class ChatConfigSO : ScriptableObject
{
    [field: SerializeField] public string CatName { get; set; }
    [field: SerializeField] public string Model { get; set; }
    [field: SerializeField] public float Temperature { get; set; }
    [field: SerializeField] public List<int> BannedTokens { get; set; }
    [field: SerializeField, TextArea(minLines:2, maxLines:100)] public string InitialTextBox { get; set; }
    [field: SerializeField, TextArea(minLines:2, maxLines:100)] public string InitialPrompt { get; set; }
    [field: SerializeField, TextArea(minLines:2, maxLines:100)] public List<string> InstructionSet { get; set; }
}
