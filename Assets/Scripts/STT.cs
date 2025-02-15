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
    [SerializeField] private Dropdown dropdown;

    private readonly string fileName = "output.wav";
    private readonly int duration = 5;
    private AudioClip clip;
    private bool isRecording;
    private float time;

    private string apiUrl = "http://localhost:5000/transcribe"; // Replace with actual API URL

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        dropdown.options.Add(new Dropdown.OptionData("Microphone not supported on WebGL"));
#else
        foreach (var device in Microphone.devices)
        {
            dropdown.options.Add(new Dropdown.OptionData(device));
        }

        recordButton.onClick.AddListener(StartRecording);
        dropdown.onValueChanged.AddListener(ChangeMicrophone);

        var index = PlayerPrefs.GetInt("user-mic-device-index");
        dropdown.SetValueWithoutNotify(index);
#endif
    }

    private void ChangeMicrophone(int index)
    {
        PlayerPrefs.SetInt("user-mic-device-index", index);
    }

    private void StartRecording()
    {
        isRecording = true;
        recordButton.enabled = false;

        var index = PlayerPrefs.GetInt("user-mic-device-index");

#if !UNITY_WEBGL
        clip = Microphone.Start(dropdown.options[index].text, false, duration, 44100);
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
        Microphone.End(null);
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

            if (time >= duration)
            {
                time = 0;
                isRecording = false;
                EndRecording();
            }
        }
    }

    [System.Serializable]
    private class TranscriptionResponse
    {
        public string transcript;
    }
}
