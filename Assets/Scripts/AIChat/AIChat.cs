using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AIChat : MonoBehaviour
{
    public TMP_InputField m_inputField;
    public Button m_btnSend;
    public GameObject m_myWordsPreset;
    public GameObject m_aiWordsPreset;
    public ScrollRect scrollRect;

    private string APIKey = "sk-cc2c12e1a4ee40bca314f1b7a78f32e3";
    private const string APIUrl = "https://api.deepseek.com/v1/chat/completions";
    private const string ModelName = "deepseek-chat";

    [Serializable]
    public class ChatMessage
    {
        public string role;
        public string content;
    }

    [Serializable]
    private class ChatRequest
    {
        public string model;
        public ChatMessage[] messages;
        public float temperature = 0.7f;
    }

    [Serializable]
    private class ChatResponseChoice
    {
        public ChatMessage message;
    }

    [Serializable]
    private class ChatResponse
    {
        public ChatResponseChoice[] choices;
    }

    private readonly List<ChatMessage> history = new List<ChatMessage>();

    void Start()
    {
        m_btnSend.onClick.AddListener(OnSendMessage);
        m_myWordsPreset.SetActive(false);
        m_aiWordsPreset.SetActive(false);

        // 山海经系统
        history.Add(new ChatMessage
        {
            role = "system",
            content = "你是一位专业的山海经知识讲解员，用现代、通俗、易懂的语言回答。帮我介绍山海经中的异兽。回答简洁自然，不要古风，不要文言文，控制在100字左右。"
        });
    }

    private void OnSendMessage()
    {
        string input = m_inputField.text;
        if (string.IsNullOrWhiteSpace(input)) return;

        NewMyWords(input);
        m_inputField.text = "";

        StartCoroutine(SendRequest(input));
    }

    private void NewMyWords(string text)
    {
        GameObject obj = Instantiate(m_myWordsPreset, m_myWordsPreset.transform.parent);
        obj.SetActive(true);
        obj.GetComponentInChildren<TMP_Text>().text = text;
    }

    private TMP_Text NewAIWords()
    {
        GameObject obj = Instantiate(m_aiWordsPreset, m_aiWordsPreset.transform.parent);
        obj.SetActive(true);
        return obj.GetComponentInChildren<TMP_Text>();
    }

    IEnumerator SendRequest(string userMessage)
    {
        history.Add(new ChatMessage { role = "user", content = userMessage });

        ChatRequest req = new ChatRequest
        {
            model = ModelName,
            messages = history.ToArray()
        };

        string json = JsonUtility.ToJson(req);
        byte[] body = Encoding.UTF8.GetBytes(json);

        TMP_Text textCtrl = NewAIWords();
        textCtrl.text = "AI思考中...";

        using (UnityWebRequest request = new UnityWebRequest(APIUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = 30;
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + APIKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    ChatResponse resp = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
                    if (resp?.choices != null && resp.choices.Length > 0 && resp.choices[0].message != null)
                    {
                        string aiText = resp.choices[0].message.content;
                        textCtrl.text = aiText;
                        history.Add(new ChatMessage { role = "assistant", content = aiText });
                    }
                    else
                    {
                        textCtrl.text = "返回为空";
                    }
                }
                catch (Exception ex)
                {
                    textCtrl.text = "解析失败: " + ex.Message + "\n原始: " + request.downloadHandler.text;
                }
            }
            else
            {
                textCtrl.text = "请求失败: " + request.error + "\n" + request.downloadHandler.text;
            }
        }

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}