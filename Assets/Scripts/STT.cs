using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using TMPro;

public class STT : MonoBehaviour
{
    [SerializeField] private Toggle recordToggle;
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI message;

    private readonly string fileName = "output.wav";
    private readonly int duration = 5;
    private AudioClip clip;
    private bool isRecording;
    private float time;

    private string apiUrl = "http://localhost:5000/transcribe"; // Replace with actual API URL
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
            recordToggle.interactable = false;
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

        // Add listener for the toggle
        recordToggle.onValueChanged.AddListener(ToggleRecording);
#endif
    }

    private void ToggleRecording(bool isOn)
    {
        if (isOn)
        {
            message.text = "Listening...";
            recordingCoroutine = StartCoroutine(AutoRecordLoop());
        }
        else
        {
            if (recordingCoroutine != null)
            {
                StopCoroutine(recordingCoroutine);
                recordingCoroutine = null;
            }
            isRecording = false;
            progressBar.fillAmount = 0f;
            message.text = "Stopped.";
        }
    }

    private IEnumerator AutoRecordLoop()
    {
        while (recordToggle.isOn)
        {
            StartRecording();
            yield return new WaitForSeconds(duration);
            EndRecording();
            yield return new WaitForSeconds(1f); // Small delay before restarting
        }
    }

    private void StartRecording()
    {
        if (selectedMic == null) return;

        isRecording = true;
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
    }

    private void Update()
    {
        if (isRecording)
        {
            time += Time.deltaTime;
            progressBar.fillAmount = time / duration;
        }
        else
        {
            progressBar.fillAmount = 0f; // Reset when not recording
        }
    }

    [System.Serializable]
    private class TranscriptionResponse
    {
        public string transcript;
    }
}
