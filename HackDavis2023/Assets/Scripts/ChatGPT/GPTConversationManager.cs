using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class GPTConversationManager : MonoBehaviour
{
    private string apiKey;

    private ConversationInstance currentInstance;

    private ChatConfigSO config;

    // Events
    public event Action<string> OnNewMessage;

    public async UniTask<bool> AttemptToInitializeWithKey(ChatConfigSO initConfig, string initApiKey, CancellationToken ct)
    {
        currentInstance = new()
        {
            model = initConfig.Model,
            temperature = initConfig.Temperature
        };

        foreach (var bannedWord in initConfig.BannedTokens)
        {
            currentInstance.logit_bias.TryAdd(bannedWord, -100);
        }
        
        currentInstance.SetMessage(ConversationInstance.Role.User, "Say Hello.");
        
        string body = currentInstance.ToRequestBody();
        
        var request = await SendGPTPostRequest(body, initApiKey, ct);

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error sending APIKey request: " + request.error);
            Debug.Log(request.downloadHandler.text);
            return false;
        }
        else
        {
            Debug.Log("APIKey Response: " + request.downloadHandler.text);
            var response = JsonConvert.DeserializeObject<Response>(request.downloadHandler.text);
            var message = response.choices[0].message;

            this.config = initConfig;
            this.apiKey = initApiKey;
            
            return true;
        }
    }
    
    public async UniTask<string> SendChatRequest(ConversationInstance.Role role,string promptText, CancellationToken ct)
    {
        currentInstance.SetMessage(role, promptText);

        string jsonString = currentInstance.ToRequestBody();

        var request = await SendGPTPostRequest(jsonString, apiKey, ct);

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error sending request: " + request.error);
            Debug.Log(request.downloadHandler.text);
            return request.error;
        }
        else
        {
            Debug.Log("Response: " + request.downloadHandler.text);
            var response = JsonConvert.DeserializeObject<Response>(request.downloadHandler.text);
            var message = response.choices[0].message;

            OnNewMessage?.Invoke(promptText);
            OnNewMessage?.Invoke(message.content);
            
            return message.content;
        }
    }

    private async UniTask<UnityWebRequest> SendGPTPostRequest(string bodyJSONString, string requestAPIKey, CancellationToken ct)
    {
        UnityWebRequest request = UnityWebRequest.Post(
            "https://api.openai.com/v1/chat/completions", 
            new WWWForm());
        
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + requestAPIKey);

        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(bodyJSONString);
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);

        try
        {
            await request.SendWebRequest().WithCancellation(ct);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return request;
    }
}

public class Response
{
    public struct Choice
    {
        public int index;
        public string finish_reason;
        public ConversationInstance.ChatMessage message;
    }

    public List<Choice> choices;
}

public class ConversationInstance
{
    public enum Role
    {
        User,
        Assistant,
        System
    }
    
    public string model;
    public ChatMessage[] messages = new ChatMessage[1];
    public float temperature;

    public Dictionary<int, float> logit_bias = new();

    public void SetMessage(Role role, string messageContent)
    {
        messages[0] = (new ChatMessage(role, messageContent));
    }
    
    public string ToRequestBody()
    {
        string result = JsonConvert.SerializeObject(this);
        Debug.Log(result);
        return result;
    }
    
    public struct ChatMessage
    {
        public string role;
        public string content;

        public ChatMessage(Role messageRole, string content)
        {
            switch (messageRole)
            {
                case Role.Assistant:
                    role = "assistant";
                    break;
                case Role.User:
                    role = "user";
                    break;
                case Role.System:
                    role = "system";
                    break;
                default:
                    role = "";
                    break;
            }

            this.content = content;
        }
    }
}
