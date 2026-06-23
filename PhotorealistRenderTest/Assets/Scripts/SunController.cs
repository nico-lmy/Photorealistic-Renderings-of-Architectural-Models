using UnityEngine;

public class SunController : MonoBehaviour
{
    public PVGISManager pvgisManager;
    
    [Header("Geographic coordinates")]
    [Range(-90f, 90f)]
    public float latitude; 
    [Range(-180f, 180f)]
    public float longitude;

    [Tooltip("Time zone relative to UTC (e.g., 1 for France during winter)")]
    [Range(-12f, 12f)]
    public float timeZone = 1f; 

    [Header("Time settings")]
    [Tooltip("Day of the year from 1 to 365")]
    [Range(1f, 365f)]
    public float day; 
    [Tooltip("Standard time (the time of the watch)")]
    [Range(0f, 24f)]
    public float hour; 

    void Start()
    {
        if (pvgisManager != null) pvgisManager.FetchData(latitude, longitude);
    }

    // Update is called once per frame
        void Update()
    {

        // on corrige le temps

        // Equation du Temps (EoT, en minutes) --> corrige les variations dues à l'orbite en ellipse de la Terre
        float B = (360f * (day - 81f)) / 365f * Mathf.Deg2Rad;
        float EoT = 9.87f * Mathf.Sin(2f * B) - 7.53f * Mathf.Cos(B) - 1.5f * Mathf.Sin(B);

        // Méridien Standard Local (LSTM) --> longitude de référence du fuseau horaire
        float LSTM = 15f * timeZone; 

        // correction temporelle totale (en minutes), prend en compte la longitude réelle et l'EoT
        float timeCorrection = 4f * (longitude - LSTM) + EoT;

        // heure réelle par rapport à la position du soleil
        float localSolarTime = hour + (timeCorrection / 60f);

        // inclinaison de la Terre par rapport aux rayons du soleil selon la saison
        float declension = -23.45f * Mathf.Cos((360f/365f) * (day + 10) * Mathf.Deg2Rad);
        // angle horaire = de combien de degrés le soleil s'est déplacé depuis le midi solaire
        float angle = 15f * (localSolarTime - 12f);

        // élévation du soleil par rapport à l'horizon
        float sinAltitude = Mathf.Sin(latitude * Mathf.Deg2Rad) * Mathf.Sin(declension * Mathf.Deg2Rad) 
                          + Mathf.Cos(latitude * Mathf.Deg2Rad) * Mathf.Cos(declension * Mathf.Deg2Rad) * Mathf.Cos(angle * Mathf.Deg2Rad);
        sinAltitude = Mathf.Clamp(sinAltitude, -1f, 1f);    // les valeurs doivent rester entre -1 et 1 pour l'arc sinus
        float sunAltitude = Mathf.Asin(sinAltitude) * Mathf.Rad2Deg; 

        // azimut = position du soleil sur la boussole : nord, sud, est, ouest
        float cosAzimut = (Mathf.Sin(declension * Mathf.Deg2Rad) - Mathf.Sin(latitude * Mathf.Deg2Rad) * Mathf.Sin(sunAltitude * Mathf.Deg2Rad)) 
                        / (Mathf.Cos(latitude * Mathf.Deg2Rad) * Mathf.Cos(sunAltitude * Mathf.Deg2Rad));
        cosAzimut = Mathf.Clamp(cosAzimut, -1f, 1f); 
        float azimut = Mathf.Acos(cosAzimut) * Mathf.Rad2Deg;
        
        if (localSolarTime > 12) azimut = 360f - azimut;        // Si on a passé le midi solaire, on inverse l'azimut pour le passage à l'ouest

        // on rotate la directional light (soleil) avec l'altitude et la position sur la boussole qu'on a calculé
        transform.rotation = Quaternion.Euler(sunAltitude, azimut, 0);

        // On formate la date au format PVGIS 
        System.DateTime date = new System.DateTime(2026, 1, 1).AddDays(day - 1).AddHours(hour);
        string targetTime = date.ToString("MMdd:HH00");
        if (pvgisManager != null)
        {
            TMYData currentSolarData = pvgisManager.GetDataForTime(targetTime);
            if (currentSolarData != null)
                Debug.Log("Pour le " + targetTime + " -> Rayonnement direct : " + currentSolarData.Gbn + " W/m2");
        }

    }

}
