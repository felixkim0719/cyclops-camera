namespace OpenAI
{
    public static class Api
    {
        // GPT API ��������Ʈ URL
        public const string GPTUrl = "https://api.openai.com/v1/chat/completions";

        // Whisper API ��������Ʈ URL
        public const string WhisperUrl = "https://api.openai.com/v1/audio/transcriptions";
    }

    [System.Serializable]
    // GPT ���� �޽��� ����ü
    public struct ResponseMessage
    {
        // �޽����� ���� (��: "system", "user", "assistant")
        public string role;

        // �޽����� ���� (�ؽ�Ʈ)
        public string content;
    }

    [System.Serializable]
    // GPT ���信�� ������ �ϳ��� ��Ÿ���� ����ü
    public struct ResponseChoice
    {
        // �������� �ε���
        public int index;

        // �������� �޽��� (ResponseMessage)
        public ResponseMessage message;
    }

    [System.Serializable]
    // GPT�� ��ü ���� ����ü
    public struct Response
    {
        // ������ ���� ID
        public string id;

        // ������ ������ �迭 (ResponseChoice �迭)
        public ResponseChoice[] choices;
    }

    [System.Serializable]
    // GPT ��û���� �޽��� �ϳ��� ��Ÿ���� ����ü
    public struct RequestMessage
    {
        // �޽����� ���� (��: "system", "user", "assistant")
        public string role;

        // �޽����� ���� (�ؽ�Ʈ)
        public string content;
    }

    [System.Serializable]
    // GPT�� ���� ��ü ��û ����ü
    public struct Request
    {
        // ����� �� (��: "gpt-3.5-turbo")
        public string model;

        // ��û�� ���Ե� �޽��� �迭 (RequestMessage �迭)
        public RequestMessage[] messages;
    }

    [System.Serializable]
    public struct Request1
    {
        public string model;
        public RequestMessage[] messages;
        public int max_tokens;  // max_tokens �ʵ带 �߰�
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
 
