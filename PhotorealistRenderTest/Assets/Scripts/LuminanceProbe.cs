using UnityEngine;

public class LuminanceProbe : MonoBehaviour
{
    public StereoController stereoController;
    
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
        if (stereoController == null || stereoController.cameras.Length == 0) return;
        if (stereoController.cameras[0].enabled)
        {
            hasClicked = false;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (stereoController.frozenTextures == null || stereoController.frozenTextures.Length == 0 || stereoController.frozenTextures[0] == null) return;

            Vector2 mousePos = Input.mousePosition;
            Vector2 mouseGUI = new Vector2(mousePos.x, Screen.height - mousePos.y);

            for (int i = 0; i < stereoController.cameras.Length; i++)
            {
                Camera cam = stereoController.cameras[i];
                Rect r = cam.rect;
                
                Rect guiRect = new Rect(
                    r.x * Screen.width,
                    (1f - r.y - r.height) * Screen.height,
                    r.width * Screen.width,
                    r.height * Screen.height
                );

                if (guiRect.Contains(mouseGUI))
                {
                    Texture2D texTotal = stereoController.frozenTextures[i];
                    Texture2D texNat = stereoController.frozenTexturesNat[i];
                    if (texTotal != null && texNat != null)
                    {
                        hasClicked = true;
                        clickPositionGUI = mouseGUI; 

                        float u = (mouseGUI.x - guiRect.x) / guiRect.width;
                        float v = 1f - ((mouseGUI.y - guiRect.y) / guiRect.height);

                        Color colorTotal = texTotal.GetPixelBilinear(u, v);
                        Color colorNat = texNat.GetPixelBilinear(u, v);

                        float rawTotal = (0.2126f * colorTotal.r + 0.7152f * colorTotal.g + 0.0722f * colorTotal.b);
                        float rawNat = (0.2126f * colorNat.r + 0.7152f * colorNat.g + 0.0722f * colorNat.b);

                        totalLuminance = rawTotal * calibrationFactor;
                        naturalLuminance = rawNat * calibrationFactor;
                        artificialLuminance = Mathf.Max(0, totalLuminance - naturalLuminance);

                        if (totalLuminance > 0.1f) ratio = (naturalLuminance / totalLuminance) * 100f;
                        else ratio = 0f;
                    }
                    break;
                }
            }
        }
    }

    void OnGUI()
    {
        if (hasClicked && stereoController != null && !stereoController.cameras[0].enabled) 
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