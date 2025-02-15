using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;

public class STT : MonoBehaviour
{
    [SerializeField] private Button recordButton;
    [SerializeField] private Image progressBar;
    [SerializeField] private Text message;

    private readonly string fileName = "output.wav";
    private readonly int duration = 5;
    private AudioClip clip;
    private bool isRecording;
    private float time;

    private string apiUrl = "http://localhost:5000/transcribe"; // Replace with actual API URL
    private string selectedMic = null;

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
        for (int i = 0; i < devices.Length; i++)
        {
            Debug.Log($"[{i}] {devices[i]}");
        }

        // Select the first microphone
        selectedMic = devices[0];

        recordButton.onClick.AddListener(StartRecording);
#endif
    }

    private void StartRecording()
    {
        if (selectedMic == null) return;

        isRecording = true;
        recordButton.interactable = false;
        time = 0f;

#if !UNITY_WEBGL
        clip = Microphone.Start(selectedMic, false, duration, 44100);
#endif
    }

    private void EndRecording()
    {
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
            }
            else
            {
                message.text = "Error: " + www.error;
            }
        }
        recordButton.interactable = true;
    }

    private void Update()
    {
        if (isRecording)
        {
            time += Time.deltaTime;
            progressBar.fillAmount = time / duration;

            if (time >= duration)
            {
                time = 0;
                isRecording = false;
                EndRecording();
            }
        }
        else
        {
            progressBar.fillAmount = 0f; // Reset the progress bar when not recording
        }
    }

    [System.Serializable]
    private class TranscriptionResponse
    {
        public string transcript;
    }
}
