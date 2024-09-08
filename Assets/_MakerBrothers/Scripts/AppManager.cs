using System.Collections;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Android;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;
using OpenAI;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Audio;




public class AppManager : MonoBehaviour
{
    [SerializeField]
    GameObject loadingScreen;
    WebCamTexture camTexture;
    public RawImage cameraViewImage; // ī�޶� ������ ȭ��
    public InputField UserInputField;
    private const string OpenAIApiUrl = "https://api.openai.com/v1/chat/completions";
    private const string ApiKey = "";
    public AudioSource aud;
    public AudioClip audioClip;
    public GameObject popupPanel; // �˾�â Panel�� ������ ����
    string base64Image_old = null;
    string transcription ="";

    private const int sampleRate = 44100;
    private const float maxSilenceDuration = 2.5f; // max duration of silence in seconds before stopping recording
    private const float silenceThreshold = 0.05f; // silence threshold (lower means more sensitive)
    private void Start()
    {
        aud = GetComponent<AudioSource>();

        // ����ũ ���� ��û
        RequestMicrophonePermission();
        //Debug.Log("start");
        // ī�޶� �ʱ�ȭ
        // ī�޶� ���� ��û �� �ʱ�ȭ
        RequestCameraPermission();
        // "HasSeenPopup" Ű�� ����� ���� �ִ��� Ȯ��
        if (PlayerPrefs.GetInt("HasSeenPopup", 0) == 0)
        {
            // ���� 0�̸� (ó�� �����̸�) �˾�â�� ���̰� �ϰ�, ���� 1�� ����
            popupPanel.SetActive(true);
            PlayerPrefs.SetInt("HasSeenPopup", 1);
            PlayerPrefs.Save(); // ����
        }
        else
        {
            // �̹� �˾��� �ôٸ� �˾�â�� ������ �ʰ� ��
            popupPanel.SetActive(false);
        }
    }

    private void RequestCameraPermission()
    {
        if (Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            // ī�޶� ������ �̹� �ο��� ���
            cameraOn();
        }
        else
        {
            // ���� ��û
            Permission.RequestUserPermission(Permission.Camera);
        }
    }
    public void ClosePopup()
    {
        popupPanel.SetActive(false);
    }

    private void Update()
    {
        // ī�޶� ������ ���Ǿ����� ��� Ȯ��
        if (Permission.HasUserAuthorizedPermission(Permission.Camera) && camTexture == null)
        {
            // ������ �ο��Ǿ��� ī�޶� �ʱ�ȭ���� �ʾ����� ī�޶� �ʱ�ȭ
            cameraOn();
        }
        // ����ũ ������ ���Ǿ����� ��� Ȯ��
        if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            // ������ �ο��Ǿ����� ���� ���� ��� ��� ����
            // ��: RecSnd() ȣ�� �Ǵ� �ٸ� ����� ��� ȣ��
        }
    }

    private void RequestMicrophonePermission()
    {
        if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            // ������ �̹� �ο��� ���
            Debug.Log("����ũ ������ �̹� �ο��Ǿ����ϴ�.");
        }
        else
        {
            // ���� ��û
            Permission.RequestUserPermission(Permission.Microphone);
        }
    }



    public void PlaySnd()
    {
        aud.Play();
    }
    private IEnumerator DisplayPictureAndRestartCamera()
    {
        // picture.jpg ���� ��� ����
        string filePath = Path.Combine(Application.persistentDataPath, "picture.jpg");

        if (File.Exists(filePath))
        {
            // ������ �����ϸ� �о Texture2D�� ��ȯ
            byte[] imageBytes = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageBytes);

            // Texture2D�� RawImage�� ǥ��
            cameraViewImage.texture = texture;
        }
        else
        {
            Debug.LogWarning("Saved picture not found at: " + filePath);
        }

        // ���� �ð� ���� ������ ���̵��� ��� ���
        yield return new WaitForSeconds(1f); // 2�� ���
       
    }
        public void RecSnd()
    {

        // picture.jpg ������ �о� RawImage�� ǥ���ϰ� ī�޶� �ٽ� ����
        StartCoroutine(DisplayPictureAndRestartCamera());
        if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Buttonsound();
            // �ڷ�ƾ ������ �����ϰų� ���� ������ ���ϴ�.
            Invoke("StartRecording", 0.5f); // 0.5�� �Ŀ� ����

          
        }
        else
        {
            Debug.LogWarning("����ũ ������ �ʿ��մϴ�.");
            RequestMicrophonePermission(); // ������ ������ �ٽ� ��û
        }
    }

    private void StartRecording()
    {
        StartCoroutine(RecordAndMonitorSilence());
    }
    public void Buttonsound()
    {
        // '1.wav' ������ Resources �������� �ҷ��ɴϴ�.
        audioClip = Resources.Load<AudioClip>("button");

        // AudioSource�� Ŭ���� �Ҵ��մϴ�.
        aud.clip = audioClip;

        // ������� ����մϴ�.
        aud.Play();
    }

    public void Ai()
    {
        // '1.wav' ������ Resources �������� �ҷ��ɴϴ�.
        audioClip = Resources.Load<AudioClip>("ai");

        // AudioSource�� Ŭ���� �Ҵ��մϴ�.
        aud.clip = audioClip;

        // ������� ����մϴ�.
        aud.Play();
    }
    public void Guide()
    {
        // '1.wav' ������ Resources �������� �ҷ��ɴϴ�.
        audioClip = Resources.Load<AudioClip>("guide");

        // AudioSource�� Ŭ���� �Ҵ��մϴ�.
        aud.clip = audioClip;

        // ������� ����մϴ�.
        aud.Play();
    }
    private IEnumerator RecordAndMonitorSilence()
    {
  
        aud.clip = Microphone.Start(null, false, 10, sampleRate);
        int lastSample = 0;
        float silenceTimer = 0f;

        while (Microphone.IsRecording(null))
        {
            int currentPosition = Microphone.GetPosition(null);
            int samplesAvailable = currentPosition - lastSample;
            if (samplesAvailable > 0)
            {
                float[] samples = new float[samplesAvailable];
                aud.clip.GetData(samples, lastSample);

                float averageVolume = 0f;
                foreach (var sample in samples)
                {
                    averageVolume += Mathf.Abs(sample);
                }
                averageVolume /= samplesAvailable;

                if (averageVolume < silenceThreshold)
                {
                    silenceTimer += Time.deltaTime;
                }
                else
                {
                    silenceTimer = 0f;
                }

                if (silenceTimer >= maxSilenceDuration)
                {
                    Microphone.End(null);
                    SaveClipAsWAV(aud.clip);
                    yield break;
                }

                lastSample = currentPosition;
            }

            yield return null;
        }

        Microphone.End(null);

        SaveClipAsWAV(aud.clip);
    }

    private async void SaveClipAsWAV(AudioClip clip)
    {
        // �ܺ� ������� Documents ������ ���� ���� ��� ����
        string filePath = Path.Combine(Application.persistentDataPath, "recordedAudio.wav");

        // Convert AudioClip to WAV data
        byte[] wavData = WavUtility.FromAudioClip(clip);

        // ���� ����
        File.WriteAllBytes(filePath, wavData);

        Debug.Log("Saved recorded audio as WAV: " + filePath);

        await UploadAudioForTranscription(filePath);
    }

    public async Task UploadAudioForTranscription(string filePath)
    {
        Ai();
        loadingScreen.SetActive(true);
        if (!File.Exists(filePath))
        {
            Debug.LogError("������ ã�� �� �����ϴ�: " + filePath);
            return;
        }

        string apiUrl = "https://api.openai.com/v1/audio/transcriptions";
        //string apiKey = "your_openai_api_key";

        // �� ������ ����
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
    {
        new MultipartFormFileSection("file", File.ReadAllBytes(filePath), Path.GetFileName(filePath), "audio/mpeg"), // mp3 ������ ���
        new MultipartFormDataSection("model", "whisper-1")
    };
        string extractedText = "";
        using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, formData))
        {
            // ��� ����
            request.SetRequestHeader("Authorization", $"Bearer {ApiKey}");

            // ��û ������
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // ���� �ؽ�Ʈ�� �ٷ� ���
                transcription = request.downloadHandler.text;
               

                // Search for the "text" key
                string searchKey = "\"text\":";
                int keyIndex = transcription.IndexOf(searchKey);

                if (keyIndex != -1)
                {
                    // Find the start of the actual text value after "text":
                    int startIndex = transcription.IndexOf("\"", keyIndex + searchKey.Length) + 1;
                    // Find the end of the text value
                    int endIndex = transcription.IndexOf("\"", startIndex);

                    // Extract the text between the quotation marks
                    extractedText = transcription.Substring(startIndex, endIndex - startIndex);

                    // Print or save the extracted text
                    //Console.WriteLine(extractedText);
                }
                else
                {
                    //Console.WriteLine("The key 'text' was not found.");
                }
            }
            CallOpenAIVision2(extractedText,base64Image_old);
        }
    }





    public void cameraOn()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }

        if (WebCamTexture.devices.Length == 0) // ī�޶� ������
        {
            Debug.Log("No Camera");
            return;
        }

        WebCamDevice[] devices = WebCamTexture.devices;
        int selectedCameraIndex = -1;

        // �ĸ� ī�޶� ã��
        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].isFrontFacing == false)
            {
                selectedCameraIndex = i;
                break;
            }
        }
        if (selectedCameraIndex >= 0)
        {
            camTexture = new WebCamTexture(devices[selectedCameraIndex].name);
            camTexture.requestedFPS = 30;
            cameraViewImage.texture = camTexture;
            camTexture.Play();
        }
    }

    private void Awake()
    {
        Application.targetFrameRate = 30;
       
    }

    private IEnumerator RestartCameraAfterProcessing()
    {
        yield return new WaitForSeconds(0.5f); // �ʿ信 ���� ����

        if (camTexture != null)
        {
            camTexture.Stop(); // ���� ī�޶� ���߱�
            camTexture = new WebCamTexture(camTexture.deviceName); // ���ο� WebCamTexture ����
            camTexture.requestedFPS = 30;
            cameraViewImage.texture = camTexture;
            camTexture.Play(); // ī�޶� �ٽ� ����
            Debug.Log("Camera restarted with new WebCamTexture.");
        }
    }
    // ���� �Կ� ��ư ������
    Texture2D photo;
    public void TakePicture()
    {
        // ����̽� ī�޶� ����
        //...
        Ai();

        // ī�޶� ���߱�
        camTexture.Pause(); // �Ǵ� camTexture.Stop();

        // ���� �Կ�
        photo = new Texture2D(camTexture.width, camTexture.height);
        photo.SetPixels(camTexture.GetPixels());
        photo.Apply();

        // ������ ī�޶� ȭ�鿡 ǥ��
        cameraViewImage.texture = photo;

        loadingScreen.SetActive(true);

        // �̹����� JPEG�� ��ȯ
        byte[] imageBytes = photo.EncodeToJPG();

        // �̹��� ���Ϸ� ����
        string filePath = Path.Combine(Application.persistentDataPath, "picture.jpg");
        File.WriteAllBytes(filePath, imageBytes);
        Debug.Log("Saved picture to: " + filePath);

        // �̹����� Base64�� ��ȯ
        string base64Image = Convert.ToBase64String(imageBytes);

        // �ʿ��� ��� �̹��� �޸� ����
        //Destroy(photo);

        // �Կ� ������ -> ���μ��� ����
        base64Image_old = base64Image;
        CallOpenAIVision(base64Image);


    }
    
    IEnumerator Process(string base64Image)
    {
        //��ũ�� ���
        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f); // 0.1�� ���

        // API ������Ʈ
        visionApi(base64Image);

        yield return new WaitForSeconds(0.1f); // 0.1�� ���

    
   

        // �ؼ� ���� -> ��ũ�� ��� ����
        loadingScreen.SetActive(false);
    }
    
    public void ImageAnalysisNotice(string dataStr)
    {
        // ���ټ� Ŭ������ ��ũ�� ����
        AssistiveSupport.notificationDispatcher.SendAnnouncement(dataStr);
    }


    public async void visionApi(string base64Image)
    {

        string userInput = "�� �̹����� ���� �ѱ� 170�ڷ� ������";
        await OpenAI.OpenAIUtil.InvokeChatWithImage(userInput, base64Image);
        string jsonResult = OpenAI.OpenAIUtil.GPTReceivedMessage;
        ImageAnalysisNotice(jsonResult);             
      
    }

    public async Task<string> InvokeChatWithImage(string promptText, string base64Image)
    {
        float startTime = Time.realtimeSinceStartup;

        string jsonRequestBody = $@"{{
        ""model"": ""gpt-4o-mini"",
        ""messages"": [
            {{
                ""role"": ""user"",
                ""content"": [
                    {{
                        ""type"": ""text"",
                        ""text"": ""{promptText}""
                    }},
                    {{
                        ""type"": ""image_url"",
                        ""image_url"": {{
                            ""url"": ""data:image/jpeg;base64,{base64Image}"",
                            ""detail"": ""high""
                        }}
                    }}
                ]
            }}
        ],
        ""max_tokens"": 300
    }}";

        using (UnityWebRequest request = new UnityWebRequest(OpenAIApiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = 30;

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {ApiKey}");

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseJson = request.downloadHandler.text;
                OpenAIResponse response = JsonUtility.FromJson<OpenAIResponse>(responseJson);
                string content = response.choices[0].message.content;
                return content;
            }
            else
            {
                Debug.LogError($"Error: {request.error}");
                return $"Error occurred during API call: {request.error}";
            }
        }

        float endTime = Time.realtimeSinceStartup;
        Debug.Log($"Response time: {endTime - startTime} seconds");
    }
    // �Լ��� ȣ���ϴ� ����
    public async void CallOpenAIVision(string base64Image)
    {

        string prompt = "�� �ð�������̾�,�� �̹����� ���� ���ڸ� ��� �����ϰ� ���ڰ� ������ ������ 200�ڷ� ������";
        // base64Image = "���⿡ base64�� ���ڵ��� �̹��� �����͸� ��������";

        string result = await InvokeChatWithImage(prompt, base64Image);
        UnityEngine.Debug.Log(result);
        string resultStr = result;
        ImageAnalysisNotice(resultStr);
        //StartCoroutine(Process(result));


        // �ؼ� ���� -> ��ũ�� ��� ����
        loadingScreen.SetActive(false);
        Destroy(photo);
        // ī�޶� �ٽ� ����
        StartCoroutine(RestartCameraAfterProcessing());
    }

    public async void CallOpenAIVision2(string extractedText,string base64Image)
    {
        if (string.IsNullOrEmpty(base64Image))
        {
            Texture2D photo = new Texture2D(camTexture.width, camTexture.height);
            photo.SetPixels(camTexture.GetPixels());
            photo.Apply();

            // �̹����� JPEG�� ��ȯ
            byte[] imageBytes = photo.EncodeToJPG();
            base64Image = Convert.ToBase64String(imageBytes);

            // �ʿ��� ��� �̹��� �޸� ����
            Destroy(photo);
            // �Կ� ������ -> ���μ��� ����
            base64Image_old = base64Image;
        }
        string prompt = extractedText;
        // base64Image = "���⿡ base64�� ���ڵ��� �̹��� �����͸� ��������";
        //UnityEngine.Debug.Log(base64Image);
        string result = await InvokeChatWithImage(prompt, base64Image);
        UnityEngine.Debug.Log(result);
        string resultStr = result;
        ImageAnalysisNotice(resultStr);
        //StartCoroutine(Process(result));


        // �ؼ� ���� -> ��ũ�� ��� ����
        loadingScreen.SetActive(false);
        Destroy(photo);
        // ī�޶� �ٽ� ����
        if (camTexture != null)
        {

            // ī�޶� �ٽ� ����
            StartCoroutine(RestartCameraAfterProcessing());
        }
        else
        {
            cameraOn(); // ���� camTexture�� null�̶�� ī�޶� �ٽ� �ʱ�ȭ
        }
    }
}
