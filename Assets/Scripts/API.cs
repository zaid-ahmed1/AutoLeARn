using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class API : MonoBehaviour
{
    private const string API_URL = "http://localhost:5000/api";
    public APIResponse response;
    public static CarInfo carInfo;
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
        QueryAgent(carInfo, response.image_file_name);
    }

    public void ConvertLangToStruct(string text, string type)
    {
        Debug.Log("Sending request to /lang_to_struct");
        StartCoroutine(SendPostRequest("/lang_to_struct", new TextStructData { text = text, type = type }));
    }

    public void QueryAgent(CarInfo carInfo, string imagePath)
    {
        Debug.Log("Sending request to /agent");
        StartCoroutine(SendPostRequest("/agent", new AgentData { car_info = carInfo, image_file_name = imagePath }));
    }

    private IEnumerator SendGetRequest(string endpoint)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(API_URL + endpoint))
        {
            yield return request.SendWebRequest();
            HandleResponse(request, endpoint);
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
            HandleResponse(request, endpoint);
        }
    }

    private void HandleResponse(UnityWebRequest request, string endpoint)
    {
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Success Response from {endpoint}: " + request.downloadHandler.text);

            // Parse the JSON response into the APIResponse class
            response = JsonUtility.FromJson<APIResponse>(request.downloadHandler.text);

            if (response.success)
            {
                // Handle success cases based on the endpoint
                switch (endpoint)
                {
                    case "/window_screenshot":
                        if (!string.IsNullOrEmpty(response.image_file_name))
                        {
                            // Save the image name to a variable
                            string savedImageName = response.image_file_name;
                            Debug.Log("Saved Image Name: " + savedImageName);

                            // You can now use the savedImageName variable as needed
                        }
                        break;

                    case "/agent":
                        if (!string.IsNullOrEmpty(response.step_breakdown))
                        {
                            Debug.Log("Step Breakdown: " + response.step_breakdown);
                            Debug.Log("Original Text: " + response.original_text);
                        }
                        break;

                    default:
                        Debug.Log("Response received for endpoint: " + endpoint);
                        break;
                }
            }
            else
            {
                Debug.LogError($"API Error: {response.error}");
            }
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
    public CarInfo car_info;
    public string image_file_name;
}

[System.Serializable]
public class CarInfo
{
    public string make;
    public string model;
    public int year;
    public string issue_with_car;
}


[System.Serializable]
public class APIResponse
{
    public bool success;
    public string message;
    public string image_file_name; // Store the image name here
    public string step_breakdown; // For /agent endpoint
    public string original_text; // For /agent endpoint
    public string error; // Store error messages if any
}