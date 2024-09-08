namespace OpenAI
{
    public static class Api
    {
        // GPT API 엔드포인트 URL
        public const string GPTUrl = "https://api.openai.com/v1/chat/completions";

        // Whisper API 엔드포인트 URL
        public const string WhisperUrl = "https://api.openai.com/v1/audio/transcriptions";
    }

    [System.Serializable]
    // GPT 응답 메시지 구조체
    public struct ResponseMessage
    {
        // 메시지의 역할 (예: "system", "user", "assistant")
        public string role;

        // 메시지의 내용 (텍스트)
        public string content;
    }

    [System.Serializable]
    // GPT 응답에서 선택지 하나를 나타내는 구조체
    public struct ResponseChoice
    {
        // 선택지의 인덱스
        public int index;

        // 선택지의 메시지 (ResponseMessage)
        public ResponseMessage message;
    }

    [System.Serializable]
    // GPT의 전체 응답 구조체
    public struct Response
    {
        // 응답의 고유 ID
        public string id;

        // 응답의 선택지 배열 (ResponseChoice 배열)
        public ResponseChoice[] choices;
    }

    [System.Serializable]
    // GPT 요청에서 메시지 하나를 나타내는 구조체
    public struct RequestMessage
    {
        // 메시지의 역할 (예: "system", "user", "assistant")
        public string role;

        // 메시지의 내용 (텍스트)
        public string content;
    }

    [System.Serializable]
    // GPT에 보낼 전체 요청 구조체
    public struct Request
    {
        // 사용할 모델 (예: "gpt-3.5-turbo")
        public string model;

        // 요청에 포함될 메시지 배열 (RequestMessage 배열)
        public RequestMessage[] messages;
    }

    [System.Serializable]
    public struct Request1
    {
        public string model;
        public RequestMessage[] messages;
        public int max_tokens;  // max_tokens 필드를 추가
    }

    [System.Serializable]
    public class OpenAIResponse
    {
        public Choice[] choices;
    }

    [System.Serializable]
    public class Choice
    {
        public Message message;
    }

    [System.Serializable]
    public class Message
    {
        public string content;
    }

}
 
