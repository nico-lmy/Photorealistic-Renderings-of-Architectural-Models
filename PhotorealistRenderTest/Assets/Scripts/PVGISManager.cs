using UnityEngine;
using System.Collections;
using UnityEngine.Networking; 

public class PVGISManager : MonoBehaviour
{
    public PVGISResponse pvgisData;

    public void FetchData(float lat, float lon)
    {
        Debug.Log("Downloading PVGIS Data for Lat: " + lat + " Lon: " + lon);
        StartCoroutine(GetPVGISData(lat, lon));         
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
                    pvgisData = JsonUtility.FromJson<PVGISResponse>(cleanJson);
                    Debug.Log(pages[page] + ":\nReceived all the data from TMY JSON table !\n");
                    break;
            }
        }
    }

    public TMYData GetDataForTime(string targetTime)
    {
        if (pvgisData == null || pvgisData.outputs == null) return null;
        foreach (TMYData data in pvgisData.outputs.tmy_hourly)
            if (data.time.EndsWith(targetTime)) return data;
        return null;
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
