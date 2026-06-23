using UnityEngine;

public class SunController : MonoBehaviour
{
    public PVGISManager pvgisManager;
    public Light directionalLight; 
    
    [Header("Geographic coordinates")]
    [Range(-90f, 90f)]
    public float latitude; 
    [Range(-180f, 180f)]
    public float longitude;

    [Tooltip("Time zone relative to UTC (e.g., 1 for France during winter)")]
    [Range(-12f, 12f)]
    public float timeZone = 1f; 

    [Header("Time settings")]
    [Tooltip("Month of the year (1-12)")]
    [Range(1, 12)]
    public int month = 3; 
    [Tooltip("Day of the month (1-31)")]
    [Range(1, 31)]
    public int dayOfMonth = 1; 
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
        // on sécurise le jour max pour un mois
        int maxDaysInMonth = System.DateTime.DaysInMonth(2026, month);
        dayOfMonth = Mathf.Clamp(dayOfMonth, 1, maxDaysInMonth);

        // on prend précisément le n-ième jour de l'année
        System.DateTime currentDate = new System.DateTime(2026, month, dayOfMonth);
        float day = currentDate.DayOfYear; 

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

        // On formate la date au format PVGIS et on fait une interpolation entre les heures (pour avoir une transition plus douce)
        int hour1 = Mathf.FloorToInt(hour); 
        float fraction = hour - hour1; 

        System.DateTime date1 = currentDate.AddHours(hour1);
        System.DateTime date2 = currentDate.AddHours(hour1 + 1);
        string targetTime1 = date1.ToString("MMdd:HH00");
        string targetTime2 = date2.ToString("MMdd:HH00");
        if (pvgisManager != null)
        {
            TMYData solarData1 = pvgisManager.GetDataForTime(targetTime1);
            TMYData solarData2 = pvgisManager.GetDataForTime(targetTime2);
            if (solarData1 != null && solarData2 != null) 
            {
                // on applique la radiance directe trouvée à l'intensité du soleil en interpolant pour les heures entre
                float interpolatedRadiation = Mathf.Lerp(solarData1.Gbn, solarData2.Gbn, fraction); 
                float luxIntensity = interpolatedRadiation* 120f; 
                if (directionalLight != null) directionalLight.intensity = luxIntensity;
                Debug.Log("Pour le " + targetTime1 + " -> Rayonnement direct : " + solarData1.Gbn + " W/m2 | Intensité : " + luxIntensity + " Lux");
            }
        }

    }

}
