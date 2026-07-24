using UnityEngine;
using System.Collections;
using UnityEngine.Networking; 
using System.Globalization;

public class PVGISManager : MonoBehaviour
{
    public PVGISResponse pvgisData;
    public event System.Action OnDataReceived;

    public void FetchData(float lat, float lon)
    {
        Debug.Log("Downloading PVGIS Data for Lat: " + lat + " Lon: " + lon);
        StartCoroutine(GetPVGISData(lat, lon));         
    }

    IEnumerator GetPVGISData(float lat, float lon)
    {
        string url = string.Format(CultureInfo.InvariantCulture, "https://re.jrc.ec.europa.eu/api/v5_3/tmy?lat={0}&lon={1}&outputformat=json", lat, lon);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string rawJson = webRequest.downloadHandler.text;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Network Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    PVGISError err = null;
                    try { err = JsonUtility.FromJson<PVGISError>(rawJson); }
                    catch { /* JSON invalide ou vide */ }
                    string reason = (err != null && !string.IsNullOrEmpty(err.message)) ? err.message : webRequest.error;
                    Debug.LogWarning("PVGIS rejected the position (" + lat + ", " + lon + ") : " + reason);
                    break;
                case UnityWebRequest.Result.Success:
                    string cleanJson = rawJson.Replace("time(UTC)", "time")
                                            .Replace("Gb(n)", "Gbn")
                                            .Replace("Gd(h)", "Gdh");
                    pvgisData = JsonUtility.FromJson<PVGISResponse>(cleanJson);
                    Debug.Log("Received all the data from TMY JSON table !\n");
                    OnDataReceived?.Invoke();
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
public class PVGISError
{
    public string message;
    public int status;
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
