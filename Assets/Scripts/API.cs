using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class API : MonoBehaviour
{
    private const string API_URL = "http://localhost:5000/api";

    public void AnalyzeScreen(string windowTitle)
    {
        Debug.Log("Sending request to /analyze_screen");
        StartCoroutine(SendPostRequest("/analyze_screen", new ScreenData { window_title = windowTitle }));
    }

    public void GetWindows()
    {
        Debug.Log("Sending request to /windows");
        StartCoroutine(SendGetRequest("/windows"));
    }

    public void TakeWindowScreenshot(string windowTitle)
    {
        Debug.Log("Sending request to /window_screenshot");
        StartCoroutine(SendPostRequest("/window_screenshot", new ScreenData { window_title = windowTitle }));
    }

    public void ConvertLangToStruct(string text, string type)
    {
        Debug.Log("Sending request to /lang_to_struct");
        StartCoroutine(SendPostRequest("/lang_to_struct", new TextStructData { text = text, type = type }));
    }

    public void QueryAgent(string previousText, string text, CarInfo carInfo, string imagePath = null)
    {
        Debug.Log("Sending request to /agent");
        StartCoroutine(SendPostRequest("/agent", new AgentData { previous_text = previousText, text = text, car_info = carInfo, image_path = imagePath }));
    }

    private IEnumerator SendGetRequest(string endpoint)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(API_URL + endpoint))
        {
            yield return request.SendWebRequest();
            HandleResponse(request);
        }
    }

    private IEnumerator SendPostRequest<T>(string endpoint, T data)
    {
        string jsonData = JsonUtility.ToJson(data);
        using (UnityWebRequest request = new UnityWebRequest(API_URL + endpoint, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            HandleResponse(request);
        }
    }

    private void HandleResponse(UnityWebRequest request)
    {
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Success Response: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Failed with error: " + request.error);
            Debug.LogError("Response Code: " + request.responseCode);
            Debug.LogError("Error Response: " + request.downloadHandler.text);
        }
    }
}

[System.Serializable]
public class ScreenData
{
    public string window_title;
}

[System.Serializable]
public class TextStructData
{
    public string text;
    public string type;
}

[System.Serializable]
public class AgentData
{
    public string previous_text;
    public string text;
    public CarInfo car_info;
    public string image_path;
}

[System.Serializable]
public class CarInfo
{
    public string make;
    public string model;
    public int year;
    public string issue_with_car;
}
