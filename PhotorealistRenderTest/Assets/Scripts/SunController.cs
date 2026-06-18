using UnityEngine;

public class SunController : MonoBehaviour
{
    [Range(-90f, 90f)]
    public float latitude; 

    [Range(1f, 365f)]
    public float day; 

    [Range(0f, 24f)]
    public float hour; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
        void Update()
    {
        float declension = -23.45f * Mathf.Cos((360f/365f) * (day + 10) * Mathf.Deg2Rad);
        float angle = 15 * (hour - 12);

        float sinAltitude = Mathf.Sin(latitude * Mathf.Deg2Rad) * Mathf.Sin(declension * Mathf.Deg2Rad) 
                          + Mathf.Cos(latitude * Mathf.Deg2Rad) * Mathf.Cos(declension * Mathf.Deg2Rad) * Mathf.Cos(angle * Mathf.Deg2Rad);
        sinAltitude = Mathf.Clamp(sinAltitude, -1f, 1f); 
        float sunAltitude = Mathf.Asin(sinAltitude) * Mathf.Rad2Deg; 

        float cosAzimut = (Mathf.Sin(declension * Mathf.Deg2Rad) - Mathf.Sin(latitude * Mathf.Deg2Rad) * Mathf.Sin(sunAltitude * Mathf.Deg2Rad)) 
                        / (Mathf.Cos(latitude * Mathf.Deg2Rad) * Mathf.Cos(sunAltitude * Mathf.Deg2Rad));
        cosAzimut = Mathf.Clamp(cosAzimut, -1f, 1f); 
        float azimut = Mathf.Acos(cosAzimut) * Mathf.Rad2Deg;
        
        if (hour > 12) azimut = 360f - azimut;

        transform.rotation = Quaternion.Euler(sunAltitude, azimut, 0);
    }

}
