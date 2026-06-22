using UnityEngine;
using System.Collections;
using UnityEngine.Networking; 

public class PVGISManager : MonoBehaviour
{
    public float testLat = 48.85f; 
    public float testLon = 2.35f; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(GetPVGISData(testLat, testLon));         
    }

    IEnumerator GetPVGISData(float lat, float lon)
    {
        string url = "https://re.jrc.ec.europa.eu/api/v5_3/tmy?lat=" + lat + "&lon=" + lon + "&outputformat=json";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url.Replace(',', '.')))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = url.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    string rawJson = webRequest.downloadHandler.text;
                    string cleanJson = rawJson.Replace("time(UTC)", "time")
                                            .Replace("Gb(n)", "Gbn")
                                            .Replace("Gd(h)", "Gdh");
                    PVGISResponse data = JsonUtility.FromJson<PVGISResponse>(cleanJson);
                    Debug.Log(pages[page] + ":\nReceived:\n" 
                                            + "Time: " + data.outputs.tmy_hourly[0].time + "\n"
                                            + "Direct irradiance: " + data.outputs.tmy_hourly[0].Gbn + "\n"
                                            + "Diffuse irradiance: " + data.outputs.tmy_hourly[0].Gdh + "\n");
                    break;
            }
        }
    }
}

[System.Serializable]
public class PVGISResponse
{
    public PVGISOutputs outputs;
}

[System.Serializable]
public class PVGISOutputs
{
    public TMYData[] tmy_hourly; 
}

[System.Serializable]
public class TMYData
{
    public string time;
    public float Gbn;
    public float Gdh;
}
