using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic; 

public class LuxMeter : MonoBehaviour
{
    public Camera m_Camera; 
    private Dictionary<int, Texture2D> readableLightmaps = new Dictionary<int, Texture2D>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (m_Camera == null) m_Camera = Camera.main; 
    }

    // Update is called once per frame
    void Update()
    {
        var mouse = Mouse.current; 
        if (mouse.leftButton.wasPressedThisFrame) 
        {
            Vector3 mousePosition = mouse.position.ReadValue();
            Ray ray = m_Camera.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Color colorTex = Color.black; 
                Renderer renderer = hit.collider.GetComponent<Renderer>();
                if (renderer.lightmapIndex >= 0 && renderer != null) 
                {
                    int lmIndex = renderer.lightmapIndex;
                    if (!readableLightmaps.ContainsKey(lmIndex)) 
                    {
                        Texture2D originalLightmap = LightmapSettings.lightmaps[lmIndex].lightmapColor;
                        readableLightmaps[lmIndex] = CreateReadableTexture(originalLightmap);
                    }
                    Texture2D readableTex = readableLightmaps[lmIndex];
                    colorTex = readableTex.GetPixelBilinear(hit.lightmapCoord.x, hit.lightmapCoord.y);
                }
                Debug.Log("Objet touché : " + hit.collider.gameObject.name + " | Lightmap coord. : " + hit.lightmapCoord + " | Couleur : " + colorTex);
            }
        }
    }

    // copie texture GPU -> CPU
    private Texture2D CreateReadableTexture(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Linear);
            
        Graphics.Blit(source, renderTex);
        
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        
        Texture2D readableTex = new Texture2D(source.width, source.height, TextureFormat.RGBAHalf, false, true);
        readableTex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableTex.Apply(); 
        
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        
        return readableTex;
    }

}
