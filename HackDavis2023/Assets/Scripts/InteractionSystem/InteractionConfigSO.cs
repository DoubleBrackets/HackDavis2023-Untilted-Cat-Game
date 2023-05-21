using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "InteractionSO", fileName = "InteractionSO")]
public class InteractionConfigSO : ScriptableObject
{
    [TextArea] public string Name;
    [TextArea] public string Description;
    public List<InteractionConfigSO> AccessibleInteractions;

    public event Action OnInteractionTriggered;
}
