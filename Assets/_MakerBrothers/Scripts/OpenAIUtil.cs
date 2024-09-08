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
        // 요청의 타임아웃 시간 (0이면 무제한)
        private const int TimeOut = 0;

        // HTTP 요청을 처리할 UnityWebRequest 객체
        private static UnityWebRequest _postRequest;

        // OpenAI API 키 (비공식적 예시로 실제 사용 시 보안을 위해 환경 변수나 안전한 저장소에서 관리해야 함)
        private static string _apiKey = "";

        // GPT와 Whisper의 응답을 저장할 변수
        public static string GPTReceivedMessage = "";
        public static string WhisperReceivedMessage = "";

        // Whisper API의 응답 구조체
        public class TranscriptionResponse
        {
            // 음성 인식 결과 텍스트
            public string Text;
        }

        // GPT API 요청의 JSON 본문을 생성하는 메서드
        static string CreateChatRequestBody(string prompt)
        {
            // 요청 메시지 생성
            var msg = new OpenAI.RequestMessage
            {
                role = "user", // 메시지 역할 설정
                content = prompt // 사용자로부터 받은 프롬프트
            };

            // 전체 요청 객체 생성
            var req = new OpenAI.Request
            {
                model = "gpt-4o-mini", // 사용할 모델 설정
                messages = new[] { msg } // 메시지 배열에 요청 메시지 추가
            };

            // 요청 객체를 JSON으로 변환하여 반환
            return JsonUtility.ToJson(req);
        }

        // GPT API를 호출하는 비동기 메서드
        public static async Task InvokeChat(string prompt)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // POST 요청을 위한 UnityWebRequest 객체 생성
            _postRequest = UnityWebRequest.Put(OpenAI.Api.GPTUrl, CreateChatRequestBody(prompt));

            // 타임아웃 시간 설정
            _postRequest.timeout = TimeOut;

            // HTTP 요청 설정
            _postRequest.method = "POST"; // POST 메서드 사용
            _postRequest.SetRequestHeader("Content-Type", "application/json"); // 요청 헤더에 콘텐츠 타입 설정
            _postRequest.SetRequestHeader("Authorization", "Bearer " + _apiKey); // API 키를 이용한 인증 설정

            // 비동기적으로 웹 요청을 전송
            UnityWebRequestAsyncOperation operation = _postRequest.SendWebRequest();
            await operation;

            // 응답 초기화
            GPTReceivedMessage = "";

            if (operation.isDone)
            {
                // 응답 데이터 처리
                var json = _postRequest.downloadHandler.text;
                var data = JsonUtility.FromJson<OpenAI.Response>(json);
                GPTReceivedMessage = data.choices[0].message.content.Replace("\n\n", ""); // 응답 메시지에서 불필요한 줄 바꿈 제거
            }

            // 요청 객체 리소스 해제
            _postRequest.Dispose();

            stopwatch.Stop();
            Debug.Log(stopwatch.ElapsedMilliseconds / 1000f); // 응답 시간(초) 로그 출력
        }
        public static async Task InvokeChatWithImage(string promptText, string imageUrl)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // 이미지 URL을 텍스트로 포함하여 content를 생성
            //string combinedContent = promptText + "," + imageUrl;
            // base64 이미지 데이터를 포함하여 content를 생성

            string combinedContent = promptText + "," + "data:image/jpeg;base64," + imageUrl;
            var requestBody = new OpenAI.Request1
            {
                model = "gpt-4o",
                messages = new[]

                {
            new OpenAI.RequestMessage
            {
                role = "user",
                content = combinedContent // 텍스트와 URL을 결합하여 content에 전달
            }
        },
                max_tokens = 300
            };





            // JSON 형식으로 변환
            string jsonRequestBody = JsonUtility.ToJson(requestBody);

            // POST 요청을 위한 UnityWebRequest 객체 생성
            _postRequest = new UnityWebRequest(OpenAI.Api.GPTUrl, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonRequestBody);
            _postRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            _postRequest.downloadHandler = new DownloadHandlerBuffer();

            // 타임아웃 시간 설정
            _postRequest.timeout = TimeOut;

            // HTTP 요청 설정
            _postRequest.SetRequestHeader("Content-Type", "application/json");
            _postRequest.SetRequestHeader("Authorization", "Bearer " + _apiKey);

            // 비동기적으로 웹 요청을 전송
            UnityWebRequestAsyncOperation operation = _postRequest.SendWebRequest();
            await operation;

            // 응답 초기화
            GPTReceivedMessage = "";

            if (operation.isDone)
            {
                // 응답 데이터 처리
                var json = _postRequest.downloadHandler.text;
                var data = JsonUtility.FromJson<OpenAI.Response>(json);
                Debug.Log(data.choices[0].message.content);
                GPTReceivedMessage = data.choices[0].message.content.Replace("\n\n", "");
            }

            // 요청 객체 리소스 해제
            _postRequest.Dispose();

            stopwatch.Stop();
            Debug.Log(stopwatch.ElapsedMilliseconds / 1000f); // 응답 시간(초) 로그 출력
        }


        //public static async Task InvokeWhisper(AudioClip clip)
        //{
        //    // AudioClip을 WAV 포맷으로 변환
        //    byte[] audioData = WavUtility.FromAudioClip(clip);

        //    // POST 요청에 사용할 폼 데이터 생성
        //    WWWForm form = new WWWForm();
        //    form.AddBinaryData("file", audioData, "audio.wav"); // 오디오 파일 추가
        //    form.AddField("model", "whisper-1"); // 사용할 모델 설정

        //    // Whisper API에 대한 POST 요청 생성
        //    using (UnityWebRequest request = UnityWebRequest.Post(OpenAI.Api.WhisperUrl, form))
        //    {
        //        request.SetRequestHeader("Authorization", $"Bearer {_apiKey}"); // API 키를 이용한 인증 설정

        //        await request.SendWebRequest(); // 비동기적으로 웹 요청 전송

        //        if (request.result == UnityWebRequest.Result.Success)
        //        {
        //            // 응답 데이터 처리
        //            string response = request.downloadHandler.text;
        //            TranscriptionResponse transcriptionResponse = JsonUtility.FromJson<TranscriptionResponse>(response);
        //            WhisperReceivedMessage = transcriptionResponse.text; // 응답 텍스트 저장
        //        }
        //        else
        //        {
        //            Debug.Log(request.error); // 오류 로그 출력
        //        }
        //    }
        }
    }

