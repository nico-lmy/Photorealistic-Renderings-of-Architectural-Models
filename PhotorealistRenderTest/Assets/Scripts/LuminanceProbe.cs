using UnityEngine;

public class LuminanceProbe : MonoBehaviour
{
    public StereoController stereoController;
    
    [Tooltip("Multiplier for converting Unity's raw value to actual cd/m²")]
    public float calibrationFactor = 1000f; 

    private float currentLuminance = 0f;
    private bool hasClicked = false;
    private Vector2 clickPositionGUI = Vector2.zero;

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
                    Texture2D tex = stereoController.frozenTextures[i];
                    if (tex != null)
                    {
                        hasClicked = true;
                        clickPositionGUI = mouseGUI; 

                        float u = (mouseGUI.x - guiRect.x) / guiRect.width;
                        float v = 1f - ((mouseGUI.y - guiRect.y) / guiRect.height);

                        Color hdrColor = tex.GetPixelBilinear(u, v);

                        float rawLuminance = (0.2126f * hdrColor.r + 0.7152f * hdrColor.g + 0.0722f * hdrColor.b);
                        currentLuminance = rawLuminance * calibrationFactor;
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
            GUI.skin.label.fontSize = 30;
            GUI.Label(new Rect(50, 50, 400, 50), "Luminance : " + currentLuminance.ToString("0.0") + " cd/m²");

            GUI.color = Color.red;
            float size = 20f;
            GUI.DrawTexture(new Rect(clickPositionGUI.x - (size/2f), clickPositionGUI.y - 2f, size, 4f), Texture2D.whiteTexture); 
            GUI.DrawTexture(new Rect(clickPositionGUI.x - 2f, clickPositionGUI.y - (size/2f), 4f, size), Texture2D.whiteTexture); 
        }
    }
}