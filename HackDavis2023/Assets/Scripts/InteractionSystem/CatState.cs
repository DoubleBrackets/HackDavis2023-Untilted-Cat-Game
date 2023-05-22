using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class CatState
{
    private StringBuilder currentConversation = new();
    private string initialPrompt;

    private InteractionManager interactionManager;

    public CatState(ChatConfigSO config, InteractionManager interactionManager)
    {
        initialPrompt = config.InitialPrompt;
        this.interactionManager = interactionManager;
    }

    public void AddConversationMessage(ConversationInstance.Role role, string text)
    {
        text = Regex.Replace(text, @"\(.*\)", "");
        text = Regex.Replace(text, @"\s+", " ");
        
        string append = $"{Environment.NewLine}" +
                        $"{(role == ConversationInstance.Role.User ? "Friend:" : "You:")}" +
                        $"{text}";
        

        currentConversation.Append(append);
    }

    public async UniTask ProcessCatResponse(string response)
    {
        AddConversationMessage(ConversationInstance.Role.Assistant, response);
        var split = response.Split('(');
        if (split.Length == 2)
        {
            var split2 = split[1].Split(')');
            if (split2.Length > 1)
            {
                await interactionManager.PerformInteraction(split2[0]);
            }
        }
    }

    public string GeneratePrompt()
    {
        var currentLocation = interactionManager.CurrentLocation.interactionConfigSo;

        string locationPrompt = "";

        for (int i = 0; i < interactionManager.locations.Count; i++)
        {
            if (i == interactionManager.CurrLocIndex) 
                continue;
            var loc = interactionManager.locations[i].interactionConfigSo;
            locationPrompt += $"{Environment.NewLine}{loc.name}:{loc.Description}";
        }


        return $"{initialPrompt}{Environment.NewLine}" +
               //$"The timestamped conversation so far has been:{currentConversation.ToString()}" +
               $"The conversation so far has been:{currentConversation.ToString()} {Environment.NewLine}" +
               $"{Environment.NewLine}IMPORTANT: First give your verbal only response to the conversation, with no narration. Try to keep the conversation going" +
               $"You both are currently at {currentLocation.name}:{currentLocation.Description}." +
               $"You can move any of the following locations{Environment.NewLine}" +
               $"{locationPrompt}" + 
               $". IMPORTANT: If you want to move locations based on your verbal response to the conversation, write only the location name surrounded by parenthesis on a new line. You move at most once.";
    }
}
