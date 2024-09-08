using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace OpenAI
{
    static class OpenAIUtil
    {
        // ��û�� Ÿ�Ӿƿ� �ð� (0�̸� ������)
        private const int TimeOut = 0;

        // HTTP ��û�� ó���� UnityWebRequest ��ü
        private static UnityWebRequest _postRequest;

        // OpenAI API Ű (������� ���÷� ���� ��� �� ������ ���� ȯ�� ������ ������ ����ҿ��� �����ؾ� ��)
        private static string _apiKey = "";

        // GPT�� Whisper�� ������ ������ ����
        public static string GPTReceivedMessage = "";
        public static string WhisperReceivedMessage = "";

        // Whisper API�� ���� ����ü
        public class TranscriptionResponse
        {
            // ���� �ν� ��� �ؽ�Ʈ
            public string Text;
        }

        // GPT API ��û�� JSON ������ �����ϴ� �޼���
        static string CreateChatRequestBody(string prompt)
        {
            // ��û �޽��� ����
            var msg = new OpenAI.RequestMessage
            {
                role = "user", // �޽��� ���� ����
                content = prompt // ����ڷκ��� ���� ������Ʈ
            };

            // ��ü ��û ��ü ����
            var req = new OpenAI.Request
            {
                model = "gpt-4o-mini", // ����� �� ����
                messages = new[] { msg } // �޽��� �迭�� ��û �޽��� �߰�
            };

            // ��û ��ü�� JSON���� ��ȯ�Ͽ� ��ȯ
            return JsonUtility.ToJson(req);
        }

        // GPT API�� ȣ���ϴ� �񵿱� �޼���
        public static async Task InvokeChat(string prompt)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // POST ��û�� ���� UnityWebRequest ��ü ����
            _postRequest = UnityWebRequest.Put(OpenAI.Api.GPTUrl, CreateChatRequestBody(prompt));

            // Ÿ�Ӿƿ� �ð� ����
            _postRequest.timeout = TimeOut;

            // HTTP ��û ����
            _postRequest.method = "POST"; // POST �޼��� ���
            _postRequest.SetRequestHeader("Content-Type", "application/json"); // ��û ����� ������ Ÿ�� ����
            _postRequest.SetRequestHeader("Authorization", "Bearer " + _apiKey); // API Ű�� �̿��� ���� ����

            // �񵿱������� �� ��û�� ����
            UnityWebRequestAsyncOperation operation = _postRequest.SendWebRequest();
            await operation;

            // ���� �ʱ�ȭ
            GPTReceivedMessage = "";

            if (operation.isDone)
            {
                // ���� ������ ó��
                var json = _postRequest.downloadHandler.text;
                var data = JsonUtility.FromJson<OpenAI.Response>(json);
                GPTReceivedMessage = data.choices[0].message.content.Replace("\n\n", ""); // ���� �޽������� ���ʿ��� �� �ٲ� ����
            }

            // ��û ��ü ���ҽ� ����
            _postRequest.Dispose();

            stopwatch.Stop();
            Debug.Log(stopwatch.ElapsedMilliseconds / 1000f); // ���� �ð�(��) �α� ���
        }
        public static async Task InvokeChatWithImage(string promptText, string imageUrl)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // �̹��� URL�� �ؽ�Ʈ�� �����Ͽ� content�� ����
            //string combinedContent = promptText + "," + imageUrl;
            // base64 �̹��� �����͸� �����Ͽ� content�� ����

            string combinedContent = promptText + "," + "data:image/jpeg;base64," + imageUrl;
            var requestBody = new OpenAI.Request1
            {
                model = "gpt-4o",
                messages = new[]

                {
            new OpenAI.RequestMessage
            {
                role = "user",
                content = combinedContent // �ؽ�Ʈ�� URL�� �����Ͽ� content�� ����
            }
        },
                max_tokens = 300
            };





            // JSON �������� ��ȯ
            string jsonRequestBody = JsonUtility.ToJson(requestBody);

            // POST ��û�� ���� UnityWebRequest ��ü ����
            _postRequest = new UnityWebRequest(OpenAI.Api.GPTUrl, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonRequestBody);
            _postRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            _postRequest.downloadHandler = new DownloadHandlerBuffer();

            // Ÿ�Ӿƿ� �ð� ����
            _postRequest.timeout = TimeOut;

            // HTTP ��û ����
            _postRequest.SetRequestHeader("Content-Type", "application/json");
            _postRequest.SetRequestHeader("Authorization", "Bearer " + _apiKey);

            // �񵿱������� �� ��û�� ����
            UnityWebRequestAsyncOperation operation = _postRequest.SendWebRequest();
            await operation;

            // ���� �ʱ�ȭ
            GPTReceivedMessage = "";

            if (operation.isDone)
            {
                // ���� ������ ó��
                var json = _postRequest.downloadHandler.text;
                var data = JsonUtility.FromJson<OpenAI.Response>(json);
                Debug.Log(data.choices[0].message.content);
                GPTReceivedMessage = data.choices[0].message.content.Replace("\n\n", "");
            }

            // ��û ��ü ���ҽ� ����
            _postRequest.Dispose();

            stopwatch.Stop();
            Debug.Log(stopwatch.ElapsedMilliseconds / 1000f); // ���� �ð�(��) �α� ���
        }


        //public static async Task InvokeWhisper(AudioClip clip)
        //{
        //    // AudioClip�� WAV �������� ��ȯ
        //    byte[] audioData = WavUtility.FromAudioClip(clip);

        //    // POST ��û�� ����� �� ������ ����
        //    WWWForm form = new WWWForm();
        //    form.AddBinaryData("file", audioData, "audio.wav"); // ����� ���� �߰�
        //    form.AddField("model", "whisper-1"); // ����� �� ����

        //    // Whisper API�� ���� POST ��û ����
        //    using (UnityWebRequest request = UnityWebRequest.Post(OpenAI.Api.WhisperUrl, form))
        //    {
        //        request.SetRequestHeader("Authorization", $"Bearer {_apiKey}"); // API Ű�� �̿��� ���� ����

        //        await request.SendWebRequest(); // �񵿱������� �� ��û ����

        //        if (request.result == UnityWebRequest.Result.Success)
        //        {
        //            // ���� ������ ó��
        //            string response = request.downloadHandler.text;
        //            TranscriptionResponse transcriptionResponse = JsonUtility.FromJson<TranscriptionResponse>(response);
        //            WhisperReceivedMessage = transcriptionResponse.text; // ���� �ؽ�Ʈ ����
        //        }
        //        else
        //        {
        //            Debug.Log(request.error); // ���� �α� ���
        //        }
        //    }
        }
    }

