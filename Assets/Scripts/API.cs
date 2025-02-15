using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class API : MonoBehaviour
{
    private const string API_URL = "http://localhost:5000";

    public void CalculateNumbers(float[] numbers)
    {
        StartCoroutine(SendCalculateRequest(numbers));
    }

    public void ProcessText(string text)
    {
        StartCoroutine(SendProcessTextRequest(text));
    }

    public void AnalyzeScreen(string windowTitle)
    {
        Debug.Log("Sending request to /analyze_screen");
        StartCoroutine(SendAnalyzeScreenRequest(windowTitle));
    }

    private IEnumerator SendCalculateRequest(float[] numbers)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        NumbersData data = new NumbersData { numbers = numbers };
        string jsonData = JsonUtility.ToJson(data);
        UnityEngine.Debug.Log("Sending JSON data to /calculate: " + jsonData);

        using (UnityWebRequest request = new UnityWebRequest(API_URL + "/calculate", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            stopwatch.Stop();
            UnityEngine.Debug.Log("Time taken for /calculate: " + stopwatch.ElapsedMilliseconds + " ms");

            if (request.result == UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.Log("Response: " + request.downloadHandler.text);
            }
            else
            {
                UnityEngine.Debug.LogError("Error: " + request.error);
                UnityEngine.Debug.LogError("Response: " + request.downloadHandler.text);
            }
        }
    }

    private IEnumerator SendProcessTextRequest(string text)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        TextData data = new TextData { text = text };
        string jsonData = JsonUtility.ToJson(data);
        UnityEngine.Debug.Log("Sending JSON data to /process_text: " + jsonData);

        using (UnityWebRequest request = new UnityWebRequest(API_URL + "/process_text", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            stopwatch.Stop();
            UnityEngine.Debug.Log("Time taken for /process_text: " + stopwatch.ElapsedMilliseconds + " ms");

            if (request.result == UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.Log("Response: " + request.downloadHandler.text);
            }
            else
            {
                UnityEngine.Debug.LogError("Error: " + request.error);
                UnityEngine.Debug.LogError("Response: " + request.downloadHandler.text);
            }
        }
    }

    private IEnumerator SendAnalyzeScreenRequest(string windowTitle)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        ScreenData data = new ScreenData { window_title = windowTitle };
        string jsonData = JsonUtility.ToJson(data);
        UnityEngine.Debug.Log($"Sending request to /analyze_screen");
        UnityEngine.Debug.Log($"Window Title: '{windowTitle}'");
        UnityEngine.Debug.Log($"JSON payload: {jsonData}");

        using (UnityWebRequest request = new UnityWebRequest(API_URL + "/analyze_screen", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            stopwatch.Stop();
            UnityEngine.Debug.Log($"Request completed in {stopwatch.ElapsedMilliseconds}ms");

            if (request.result == UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.Log($"Success Response: {request.downloadHandler.text}");
            }
            else
            {
                UnityEngine.Debug.LogError($"Failed with error: {request.error}");
                UnityEngine.Debug.LogError($"Response Code: {request.responseCode}");
                UnityEngine.Debug.LogError($"Error Response: {request.downloadHandler.text}");
            
                // Print headers for debugging
                var headers = request.GetResponseHeaders();
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        UnityEngine.Debug.Log($"Header - {header.Key}: {header.Value}");
                    }
                }
            }
        }
    }
}

// Wrapper class for the numbers array
[System.Serializable]
public class NumbersData
{
    public float[] numbers;
}

// Wrapper class for the text string
[System.Serializable]
public class TextData
{
    public string text;
}

// Wrapper class for the screen data
[System.Serializable]
public class ScreenData
{
    public string window_title;
}