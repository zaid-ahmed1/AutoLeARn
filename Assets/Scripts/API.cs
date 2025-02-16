using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public static class UnityWebRequestExtensions
{
    public static bool IsSuccess(this UnityWebRequest request)
    {
        // Check for network errors
        if (request.isNetworkError || request.isHttpError)
        {
            return false;
        }

        // Check for successful response codes
        if (request.responseCode == 0 || request.responseCode == (long)System.Net.HttpStatusCode.OK)
        {
            return true;
        }

        return false;
    }
}

public class API : MonoBehaviour
{
    private const string API_URL = "http://localhost:5000/api";
    public APIResponse response;
    public static CarInfo carInfo;

    public void GetWindows()
    {
        Debug.Log("Sending request to /windows");
        StartCoroutine(SendGetRequest("/windows"));
    }

    public void TakeWindowScreenshot(string windowTitle)
    {
        Debug.Log("Sending request to /window_screenshot");
        StartCoroutine(TakeWindowScreenshotCoroutine(windowTitle));
    }

    private IEnumerator TakeWindowScreenshotCoroutine(string windowTitle)
    {
        yield return StartCoroutine(
            SendPostRequest("/window_screenshot", new ScreenData { window_title = windowTitle }));
        QueryAgent(carInfo, response.filename);
    }

    public void ConvertLangToStruct(string text, string type)
    {
        Debug.Log("Sending request to /lang_to_struct");
        StartCoroutine(SendPostRequest("/lang_to_struct", new TextStructData { text = text, type = type }));
    }

    public void QueryAgent(CarInfo carInfo, string imagePath)
    {
        Debug.Log("Sending request to /agent");
        StartCoroutine(SendPostRequest("/agent", new AgentData { car_info = carInfo, filename = imagePath }));
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
        if (request.IsSuccess())
        {
            Debug.Log($"Success Response from {endpoint}: " + request.downloadHandler.text);

            try
            {
                // Parse the JSON response into the APIResponse class
                response = JsonUtility.FromJson<APIResponse>(request.downloadHandler.text);
                Debug.Log("Response object:");
                Debug.Log(JsonUtility.ToJson(response, true)); // Pretty print the entire response object

                if (response != null)
                {
                    Debug.Log($"Success: {response.success}");
                    Debug.Log($"Message: {response.message}");
                    Debug.Log($"Filename: {response.filename}");
                    Debug.Log($"Original Text: {response.original_text}");
                    Debug.Log($"Error: {response.error}");

                    if (response.success)
                    {
                        // Handle success cases based on the endpoint
                        Debug.Log(endpoint);
                        switch (endpoint)
                        {
                            case "/window_screenshot":
                                if (!string.IsNullOrEmpty(response.filename))
                                {
                                    // Save the image name to a variable
                                    string savedImageName = response.filename;
                                    Debug.Log("Saved Image Name: " + savedImageName);

                                    // You can now use the savedImageName variable as needed
                                }
                                break;

                            case "/agent":
                                if (response.step_breakdown != null)
                                {
                                    Debug.Log("Step Breakdown: " + JsonUtility.ToJson(response.step_breakdown, true));
                                    Debug.Log("Original Text: " + response.original_text);
                                }
                                Debug.Log("VVV");
                                Debug.Log(JsonUtility.ToJson(response.step_breakdown, true));
                                break;

                            default:
                                Debug.Log("Response received for endpoint: " + endpoint);
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError($"API Error: {response.error}");
                        if (endpoint == "/agent")
                        {
                            Debug.Log("Can't call a function");
                            return;
                        }
                    }
                }
                else
                {
                    Debug.LogError("Response is null.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"JSON Parsing Error: {ex.Message}");
                Debug.LogError($"Raw Response: {request.downloadHandler.text}");
            }
        }
        else
        {
            Debug.LogError($"Failed with error: {request.error}");
            Debug.LogError($"Response Code: {request.responseCode}");
            Debug.LogError($"Error Response: {request.downloadHandler.text}");
            Debug.LogError($"Error occurred at endpoint: {endpoint}");
            if (endpoint == "/agent")
            {
                Debug.Log("Can't call a function");
            }
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
        public string filename;
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
        public string filename; // Store the image name here
        public StepsTutorial step_breakdown; // For /agent endpoint
        public string original_text; // For /agent endpoint
        public string error; // Store error messages if any
    }
    
    [System.Serializable]
    public class Step
    {
        public int step_number;
        public string step_description;
    }

    [System.Serializable]
    public class StepsTutorial
    {
        public string title;
        public string description;
        public string additional_context;
        public string[] sources;
        public Step[] steps;
    }