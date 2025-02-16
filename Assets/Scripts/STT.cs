using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using System.Text;
using TMPro;

public class STT : MonoBehaviour
{
    [SerializeField] private Button recordButton;
    [SerializeField] private TextMeshProUGUI message;

    private readonly string fileName = "output.wav";
    private readonly int duration = 5; // Fixed recording duration in seconds
    private AudioClip clip;
    private bool isRecording;
    private string apiUrl = "http://localhost:5000/api/transcribe"; // Replace with actual API URL
    private string selectedMic = null;
    private Coroutine recordingCoroutine;

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        message.text = "Microphone not supported on WebGL";
#else
        var devices = Microphone.devices;
        if (devices.Length == 0)
        {
            message.text = "No microphone detected!";
            recordButton.interactable = false;
            return;
        }

        // Print all available microphones
        Debug.Log("Available Microphones:");
        foreach (var device in devices)
        {
            Debug.Log(device);
        }

        // Select the first microphone
        selectedMic = devices[0];

        // Add listener for the button
        recordButton.onClick.AddListener(OnRecordButtonPressed);
#endif
    }

    public void OnRecordButtonPressed()
    {
        Debug.Log("Record button pressed");
        if (!isRecording)
        {
            message.text = "Listening...";
            recordingCoroutine = StartCoroutine(RecordForDuration());
        }
    }

    private IEnumerator RecordForDuration()
    {
        StartRecording();
        yield return new WaitForSeconds(duration); // Wait for the fixed duration
        EndRecording();
    }

    private void StartRecording()
    {
        if (selectedMic == null) return;

        isRecording = true;

#if !UNITY_WEBGL
        clip = Microphone.Start(selectedMic, false, duration, 44100);
#endif
    }

    private void EndRecording()
    {
        isRecording = false;
        message.text = "Transcribing...";
        StartCoroutine(ProcessRecording());
    }

    private IEnumerator ProcessRecording()
    {
#if !UNITY_WEBGL
        Microphone.End(selectedMic);
#endif

        byte[] data = SaveWav.Save(fileName, clip);
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(filePath, data);

        yield return StartCoroutine(UploadAudio(filePath));
    }

    private IEnumerator UploadAudio(string filePath)
    {
        byte[] audioData = File.ReadAllBytes(filePath);
        WWWForm form = new WWWForm();
        form.AddBinaryData("audio", audioData, "audio.wav", "audio/wav");

        using (UnityWebRequest www = UnityWebRequest.Post(apiUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var responseJson = JsonUtility.FromJson<TranscriptionResponse>(www.downloadHandler.text);
                message.text = responseJson.transcript;

                // Invoke ConvertLangToStruct if transcription is successful
                string textToConvert = responseJson.transcript;
                string structType = "CarInfo"; // Change this to the desired struct type
                StartCoroutine(ConvertLangToStruct(textToConvert, structType));
            }
            else
            {
                message.text = "Error: " + www.error;
            }
        }
    }

    private IEnumerator ConvertLangToStruct(string text, string structType)
    {
        TextStructData data = new TextStructData { text = text, type = structType };
        string jsonData = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest("http://localhost:5000/api/lang_to_struct", "POST"))
        {
            Debug.Log(text);
            Debug.Log(structType);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Converted struct: {request.downloadHandler.text}");

                // Parse the JSON response
                CarInfo parsedResponse = JsonUtility.FromJson<CarInfo>(request.downloadHandler.text);

                // Check if any field is missing or invalid
                if (parsedResponse.year == -1 || 
                    parsedResponse.issue_with_car == "Unknown" || 
                    parsedResponse.make == "Unknown" || 
                    parsedResponse.model == "Unknown")
                {
                    message.text = "Sorry, please ask again - we need the make, model, year, and service.";
                }
                else
                {
                    message.text = $"Issue: {parsedResponse.issue_with_car}\nMake: {parsedResponse.make}\nModel: {parsedResponse.model}\nYear: {parsedResponse.year}";
                    API.carInfo = parsedResponse;
                }
            }
            else
            {
                Debug.LogError($"Failed to convert text to struct: {request.error}");
                message.text = "Error processing request.";
            }
        }
    }

    [System.Serializable]
    private class TranscriptionResponse
    {
        public string transcript;
    }

    [System.Serializable]
    private class TextStructData
    {
        public string text;
        public string type;
    }
}
