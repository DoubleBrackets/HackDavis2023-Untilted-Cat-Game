using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUIManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private CanvasGroup inputFieldGroup;
    [SerializeField] private Button submitButton;
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private TMP_Text placeholderInputText;

    private GPTConversationManager conversationManager;

    public event Action<string> OnInputFieldSubmit;

    public async UniTask Initialize(GPTConversationManager conversationManager)
    {
        this.conversationManager = conversationManager;
    }

    private void OnEnable()
    {
        chatInputField.onSubmit.AddListener(OnSubmit);
        submitButton.onClick.AddListener(OnSubmitButton);
    }
    
    private void OnDisable()
    {
        chatInputField.onSubmit.RemoveListener(OnSubmit);
        submitButton.onClick.RemoveListener(OnSubmitButton);
    }

    public async UniTask DisplayResponseText(string name,string text)
    {
        text = $"{name}:" + text;
        displayText.text = String.Empty;
        for (int i = 0; i < text.Length; i++)
        {
            displayText.text += text[i];
            var delay = text[i].GetTypeWriterDelay();
            await UniTask.Delay(delay);
        }
    }

    public void ClearResponseText()
    {
        displayText.text = String.Empty;
    }
    
    public async UniTask HideSendText()
    {
        string text = chatInputField.text;
        string filler = "";
        for (int i = 0; i < text.Length; i++)
        {
            chatInputField.text = text.Substring(0, text.Length - i);
            filler += " ";
            await UniTask.Delay(text[text.Length - i - 1].GetTypeWriterDelay());
        }

        chatInputField.text = String.Empty;
    }

    public async void SetInputBoxInteractable(bool show)
    {
        inputFieldGroup.interactable = (show);
        placeholderInputText.gameObject.SetActive(show);
    }
    
    public async void SetInputBoxVisible(bool show)
    {
        inputFieldGroup.SetVisible(show);
    }


    private void OnSubmitButton()
    {
        OnSubmit(chatInputField.text);
    }

    private void OnSubmit(string input)
    {
        input = input.Trim(' ');
        if (input != string.Empty)
        {
            OnInputFieldSubmit?.Invoke(input);
        }
            
    }
}
