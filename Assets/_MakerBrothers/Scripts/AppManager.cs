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
    public RawImage cameraViewImage; // 카메라가 보여질 화면
    public InputField UserInputField;
    private const string OpenAIApiUrl = "https://api.openai.com/v1/chat/completions";
    private const string ApiKey = "";
    public AudioSource aud;
    public AudioClip audioClip;
    public GameObject popupPanel; // 팝업창 Panel을 연결할 변수
    string base64Image_old = null;
    string transcription ="";

    private const int sampleRate = 44100;
    private const float maxSilenceDuration = 2.5f; // max duration of silence in seconds before stopping recording
    private const float silenceThreshold = 0.05f; // silence threshold (lower means more sensitive)
    private void Start()
    {
        aud = GetComponent<AudioSource>();

        // 마이크 권한 요청
        RequestMicrophonePermission();
        //Debug.Log("start");
        // 카메라 초기화
        // 카메라 권한 요청 및 초기화
        RequestCameraPermission();
        // "HasSeenPopup" 키로 저장된 값이 있는지 확인
        if (PlayerPrefs.GetInt("HasSeenPopup", 0) == 0)
        {
            // 값이 0이면 (처음 실행이면) 팝업창을 보이게 하고, 값을 1로 설정
            popupPanel.SetActive(true);
            PlayerPrefs.SetInt("HasSeenPopup", 1);
            PlayerPrefs.Save(); // 저장
        }
        else
        {
            // 이미 팝업을 봤다면 팝업창을 보이지 않게 함
            popupPanel.SetActive(false);
        }
    }

    private void RequestCameraPermission()
    {
        if (Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            // 카메라 권한이 이미 부여된 경우
            cameraOn();
        }
        else
        {
            // 권한 요청
            Permission.RequestUserPermission(Permission.Camera);
        }
    }
    public void ClosePopup()
    {
        popupPanel.SetActive(false);
    }

    private void Update()
    {
        // 카메라 권한이 허용되었는지 계속 확인
        if (Permission.HasUserAuthorizedPermission(Permission.Camera) && camTexture == null)
        {
            // 권한이 부여되었고 카메라가 초기화되지 않았으면 카메라 초기화
            cameraOn();
        }
        // 마이크 권한이 허용되었는지 계속 확인
        if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            // 권한이 부여되었으면 녹음 관련 기능 사용 가능
            // 예: RecSnd() 호출 또는 다른 오디오 기능 호출
        }
    }

    private void RequestMicrophonePermission()
    {
        if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            // 권한이 이미 부여된 경우
            Debug.Log("마이크 권한이 이미 부여되었습니다.");
        }
        else
        {
            // 권한 요청
            Permission.RequestUserPermission(Permission.Microphone);
        }
    }



    public void PlaySnd()
    {
        aud.Play();
    }
    private IEnumerator DisplayPictureAndRestartCamera()
    {
        // picture.jpg 파일 경로 설정
        string filePath = Path.Combine(Application.persistentDataPath, "picture.jpg");

        if (File.Exists(filePath))
        {
            // 파일이 존재하면 읽어서 Texture2D로 변환
            byte[] imageBytes = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageBytes);

            // Texture2D를 RawImage에 표시
            cameraViewImage.texture = texture;
        }
        else
        {
            Debug.LogWarning("Saved picture not found at: " + filePath);
        }

        // 일정 시간 동안 사진이 보이도록 잠시 대기
        yield return new WaitForSeconds(1f); // 2초 대기
       
    }
        public void RecSnd()
    {

        // picture.jpg 파일을 읽어 RawImage에 표시하고 카메라 다시 시작
        StartCoroutine(DisplayPictureAndRestartCamera());
        if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Buttonsound();
            // 코루틴 시작을 지연하거나 따로 실행해 봅니다.
            Invoke("StartRecording", 0.5f); // 0.5초 후에 시작

          
        }
        else
        {
            Debug.LogWarning("마이크 권한이 필요합니다.");
            RequestMicrophonePermission(); // 권한이 없으면 다시 요청
        }
    }

    private void StartRecording()
    {
        StartCoroutine(RecordAndMonitorSilence());
    }
    public void Buttonsound()
    {
        // '1.wav' 파일을 Resources 폴더에서 불러옵니다.
        audioClip = Resources.Load<AudioClip>("button");

        // AudioSource에 클립을 할당합니다.
        aud.clip = audioClip;

        // 오디오를 재생합니다.
        aud.Play();
    }

    public void Ai()
    {
        // '1.wav' 파일을 Resources 폴더에서 불러옵니다.
        audioClip = Resources.Load<AudioClip>("ai");

        // AudioSource에 클립을 할당합니다.
        aud.clip = audioClip;

        // 오디오를 재생합니다.
        aud.Play();
    }
    public void Guide()
    {
        // '1.wav' 파일을 Resources 폴더에서 불러옵니다.
        audioClip = Resources.Load<AudioClip>("guide");

        // AudioSource에 클립을 할당합니다.
        aud.clip = audioClip;

        // 오디오를 재생합니다.
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
        // 외부 저장소의 Documents 폴더에 파일 저장 경로 설정
        string filePath = Path.Combine(Application.persistentDataPath, "recordedAudio.wav");

        // Convert AudioClip to WAV data
        byte[] wavData = WavUtility.FromAudioClip(clip);

        // 파일 저장
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
            Debug.LogError("파일을 찾을 수 없습니다: " + filePath);
            return;
        }

        string apiUrl = "https://api.openai.com/v1/audio/transcriptions";
        //string apiKey = "your_openai_api_key";

        // 폼 데이터 설정
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
    {
        new MultipartFormFileSection("file", File.ReadAllBytes(filePath), Path.GetFileName(filePath), "audio/mpeg"), // mp3 파일의 경우
        new MultipartFormDataSection("model", "whisper-1")
    };
        string extractedText = "";
        using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, formData))
        {
            // 헤더 설정
            request.SetRequestHeader("Authorization", $"Bearer {ApiKey}");

            // 요청 보내기
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // 응답 텍스트를 바로 출력
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

        if (WebCamTexture.devices.Length == 0) // 카메라 없으면
        {
            Debug.Log("No Camera");
            return;
        }

        WebCamDevice[] devices = WebCamTexture.devices;
        int selectedCameraIndex = -1;

        // 후면 카메라 찾기
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
        yield return new WaitForSeconds(0.5f); // 필요에 따라 조정

        if (camTexture != null)
        {
            camTexture.Stop(); // 기존 카메라 멈추기
            camTexture = new WebCamTexture(camTexture.deviceName); // 새로운 WebCamTexture 생성
            camTexture.requestedFPS = 30;
            cameraViewImage.texture = camTexture;
            camTexture.Play(); // 카메라 다시 시작
            Debug.Log("Camera restarted with new WebCamTexture.");
        }
    }
    // 사진 촬영 버튼 누르기
    Texture2D photo;
    public void TakePicture()
    {
        // 디바이스 카메라 오픈
        //...
        Ai();

        // 카메라 멈추기
        camTexture.Pause(); // 또는 camTexture.Stop();

        // 사진 촬영
        photo = new Texture2D(camTexture.width, camTexture.height);
        photo.SetPixels(camTexture.GetPixels());
        photo.Apply();

        // 사진을 카메라 화면에 표시
        cameraViewImage.texture = photo;

        loadingScreen.SetActive(true);

        // 이미지를 JPEG로 변환
        byte[] imageBytes = photo.EncodeToJPG();

        // 이미지 파일로 저장
        string filePath = Path.Combine(Application.persistentDataPath, "picture.jpg");
        File.WriteAllBytes(filePath, imageBytes);
        Debug.Log("Saved picture to: " + filePath);

        // 이미지를 Base64로 변환
        string base64Image = Convert.ToBase64String(imageBytes);

        // 필요한 경우 이미지 메모리 해제
        //Destroy(photo);

        // 촬영 끝나면 -> 프로세스 시작
        base64Image_old = base64Image;
        CallOpenAIVision(base64Image);


    }
    
    IEnumerator Process(string base64Image)
    {
        //스크린 잠금
        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f); // 0.1초 대기

        // API 리퀘스트
        visionApi(base64Image);

        yield return new WaitForSeconds(0.1f); // 0.1초 대기

    
   

        // 해석 종료 -> 스크린 잠금 해제
        loadingScreen.SetActive(false);
    }
    
    public void ImageAnalysisNotice(string dataStr)
    {
        // 접근성 클래스로 토크백 전송
        AssistiveSupport.notificationDispatcher.SendAnnouncement(dataStr);
    }


    public async void visionApi(string base64Image)
    {

        string userInput = "이 이미지에 대해 한글 170자로 설명해";
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
    // 함수를 호출하는 예시
    public async void CallOpenAIVision(string base64Image)
    {

        string prompt = "난 시각장애인이야,이 이미지에 대해 글자를 모두 설명하고 글자가 없으면 사진을 200자로 묘사해";
        // base64Image = "여기에 base64로 인코딩된 이미지 데이터를 넣으세요";

        string result = await InvokeChatWithImage(prompt, base64Image);
        UnityEngine.Debug.Log(result);
        string resultStr = result;
        ImageAnalysisNotice(resultStr);
        //StartCoroutine(Process(result));


        // 해석 종료 -> 스크린 잠금 해제
        loadingScreen.SetActive(false);
        Destroy(photo);
        // 카메라 다시 시작
        StartCoroutine(RestartCameraAfterProcessing());
    }

    public async void CallOpenAIVision2(string extractedText,string base64Image)
    {
        if (string.IsNullOrEmpty(base64Image))
        {
            Texture2D photo = new Texture2D(camTexture.width, camTexture.height);
            photo.SetPixels(camTexture.GetPixels());
            photo.Apply();

            // 이미지를 JPEG로 변환
            byte[] imageBytes = photo.EncodeToJPG();
            base64Image = Convert.ToBase64String(imageBytes);

            // 필요한 경우 이미지 메모리 해제
            Destroy(photo);
            // 촬영 끝나면 -> 프로세스 시작
            base64Image_old = base64Image;
        }
        string prompt = extractedText;
        // base64Image = "여기에 base64로 인코딩된 이미지 데이터를 넣으세요";
        //UnityEngine.Debug.Log(base64Image);
        string result = await InvokeChatWithImage(prompt, base64Image);
        UnityEngine.Debug.Log(result);
        string resultStr = result;
        ImageAnalysisNotice(resultStr);
        //StartCoroutine(Process(result));


        // 해석 종료 -> 스크린 잠금 해제
        loadingScreen.SetActive(false);
        Destroy(photo);
        // 카메라 다시 시작
        if (camTexture != null)
        {

            // 카메라 다시 시작
            StartCoroutine(RestartCameraAfterProcessing());
        }
        else
        {
            cameraOn(); // 만약 camTexture가 null이라면 카메라를 다시 초기화
        }
    }
}
