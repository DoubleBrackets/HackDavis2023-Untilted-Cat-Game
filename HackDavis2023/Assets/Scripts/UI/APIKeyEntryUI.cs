using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class APIKeyEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField apiKeyInputField;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private CanvasGroup loadingGroup;
    [SerializeField] private CanvasGroup selfGroup;

    private void OnEnable()
    {
        selfGroup.SetVisible(true);
    }

    public async UniTask TransitionOut()
    {
        selfGroup.SetVisible(false);
    }
    
    public async UniTask<string> WaitForAPIKeySubmit(CancellationToken ct)
    {
        string response = String.Empty;

        void SetResponse(string val)
        {
            if (val.Trim(' ') == string.Empty) return;
            response = val;
        }

        apiKeyInputField.onSubmit.AddListener(SetResponse);

        await UniTask.WaitUntil(() => response != String.Empty);

        apiKeyInputField.onSubmit.RemoveListener(SetResponse);
        
        return response;
    }

    public enum LoadingState
    {
        Idle,
        Loading,
        Error
    }
    
    public void SetLoadingVisualsActive(LoadingState show)
    {
        switch (show)
        {
            case LoadingState.Error:
                loadingGroup.SetVisible(true);
                loadingText.text = "API Key Failed :(";
                break;
            case LoadingState.Idle:
                loadingGroup.SetVisible(false);
                break;
            case LoadingState.Loading:
                loadingGroup.SetVisible(true);
                loadingText.text = "Checking... :O";
                break;
        }
    }
}
