using UnityEngine;

public class SimpleLuminanceProbe : MonoBehaviour
{
    public SimpleController simpleController;
    
    [Tooltip("Multiplier for converting Unity's raw value to actual cd/m²")]
    public float calibrationFactor = 1000f; 

    private bool hasClicked = false;
    private Vector2 clickPositionGUI = Vector2.zero;

    private float totalLuminance = 0f;
    private float naturalLuminance = 0f;
    private float artificialLuminance = 0f;
    private float ratio = 0f;

    void Update()
    {
        if (simpleController == null || simpleController.mainCamera == null) return;

        if (simpleController.mainCamera.enabled)
        {
            hasClicked = false;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (simpleController.frozenTexture == null || simpleController.frozenTextureNat == null) return;

            Vector2 mousePos = Input.mousePosition;
            Vector2 mouseGUI = new Vector2(mousePos.x, Screen.height - mousePos.y);

            float u = mousePos.x / Screen.width;
            float v = mousePos.y / Screen.height;

            Color colorTotal = simpleController.frozenTexture.GetPixelBilinear(u, v);
            Color colorNat = simpleController.frozenTextureNat.GetPixelBilinear(u, v);

            float rawTotal = (0.2126f * colorTotal.r + 0.7152f * colorTotal.g + 0.0722f * colorTotal.b);
            float rawNat = (0.2126f * colorNat.r + 0.7152f * colorNat.g + 0.0722f * colorNat.b);

            totalLuminance = rawTotal * calibrationFactor;
            naturalLuminance = rawNat * calibrationFactor;
            artificialLuminance = Mathf.Max(0, totalLuminance - naturalLuminance);

            if (totalLuminance > 0.1f) ratio = (naturalLuminance / totalLuminance) * 100f;
            else ratio = 0f;

            hasClicked = true;
            clickPositionGUI = mouseGUI; 
        }
    }

    void OnGUI()
    {
        if (hasClicked && simpleController != null && !simpleController.mainCamera.enabled) 
        {
            GUI.color = Color.yellow;
            GUI.skin.label.fontSize = 24;
            string info = $"Total Luminance : {totalLuminance:0.0} cd/m²\n" +
                          $"Natural Luminance : {naturalLuminance:0.0} cd/m²\n" +
                          $"Artificial Luminance : {artificialLuminance:0.0} cd/m²\n" +
                          $"Natural-to-Total Ratio : {ratio:0.0} %";

            GUI.Label(new Rect(50, 50, 600, 300), info);

            GUI.color = Color.red;
            float size = 20f;
            GUI.DrawTexture(new Rect(clickPositionGUI.x - (size/2f), clickPositionGUI.y - 2f, size, 4f), Texture2D.whiteTexture); 
            GUI.DrawTexture(new Rect(clickPositionGUI.x - 2f, clickPositionGUI.y - (size/2f), 4f, size), Texture2D.whiteTexture); 
        }
    }
}