using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameDirector : MonoBehaviour
{
    [SerializeField] private GPTConversationManager conversationManager;
    [SerializeField] private ChatUIManager chatUI;
    [SerializeField] private APIKeyEntryUI apiKeyHandler;
    [SerializeField] private ChatConfigSO chatConfig;
    [SerializeField] private ConversationDebugSO debugSO;
    [SerializeField] private InteractionManager interactionManager;

    [Header("Camera Controls")]
    [SerializeField] private GameObject InitialCamera;

    private string apiKey = String.Empty;

    [SerializeField] private CatState catState;
    
    private void Start()
    {
        #if UNITY_EDITOR
            apiKey = Key.apiKey;
#endif

        interactionManager.Initialize();
        catState = new CatState(chatConfig, interactionManager);
        
        Startup(this.GetCancellationTokenOnDestroy());
        chatUI.SetInputBoxVisible(false);
    }

    private async UniTaskVoid Startup(CancellationToken ct)
    {
        apiKeyHandler.SetLoadingVisualsActive(APIKeyEntryUI.LoadingState.Idle);
        
        // Wait for valid key
        bool keyValid = false;
        
#if UNITY_EDITOR
        if (apiKey != String.Empty)
        {
            apiKeyHandler.SetLoadingVisualsActive(APIKeyEntryUI.LoadingState.Loading);
            keyValid = await conversationManager.AttemptToInitializeWithKey(chatConfig, apiKey.Trim(' '), ct);
            apiKeyHandler.SetLoadingVisualsActive(APIKeyEntryUI.LoadingState.Error);
        }
#endif
        
        while (!keyValid)
        {
            string apiKey = await apiKeyHandler.WaitForAPIKeySubmit(ct);
            apiKeyHandler.SetLoadingVisualsActive(APIKeyEntryUI.LoadingState.Loading);
            keyValid = await conversationManager.AttemptToInitializeWithKey(chatConfig, apiKey.Trim(' '), ct);
            apiKeyHandler.SetLoadingVisualsActive(APIKeyEntryUI.LoadingState.Error);
        }
        
        apiKeyHandler.SetLoadingVisualsActive(APIKeyEntryUI.LoadingState.Idle);
        // Entry animation
        InitialCamera.SetActive(false);
        await apiKeyHandler.TransitionOut();
        await UniTask.Delay(TimeSpan.FromSeconds(4f));

        await chatUI.DisplayResponseText(chatConfig.CatName, chatConfig.InitialTextBox);
        chatUI.SetInputBoxVisible(true);
        
        // Setup main gameplay
        await chatUI.Initialize(conversationManager);

        chatUI.OnInputFieldSubmit += (x) => OnPromptSubmit(x);
    }

    private async UniTaskVoid OnPromptSubmit(string prompt)
    {
        Debug.Log($"Sent Chat Request {prompt}");
        
        catState.AddConversationMessage(ConversationInstance.Role.User, prompt);

        string promptString = catState.GeneratePrompt();
        
#if UNITY_EDITOR
        debugSO.Dialogue = promptString;
#endif
        chatUI.SetInputBoxInteractable(false);
        chatUI.ClearResponseText();

        var hideSendTextTask = chatUI.HideSendText();
        var output = await conversationManager.SendChatRequest(ConversationInstance.Role.User, promptString, destroyCancellationToken);

        await hideSendTextTask;

        output = output.Replace("\"", String.Empty);
        if (output.Contains(':'))
        {
            var splits = output.Split(':');
            output = splits[splits.Length - 1];
        }
        
        string displayOutput = Regex.Replace(output, @"\(.*\)", "");
        displayOutput = Regex.Replace(displayOutput, @"\s+", " ");

        var a = chatUI.DisplayResponseText(chatConfig.CatName,displayOutput);
        var b = catState.ProcessCatResponse(output);
        
        await UniTask.WhenAll(a, b);
        
        chatUI.SetInputBoxInteractable(true);
    }
}
