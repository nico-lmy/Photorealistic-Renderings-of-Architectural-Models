using UnityEngine;
using UnityEngine.InputSystem;

public class LuxMeter : MonoBehaviour
{
    public Camera m_Camera; 

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
                    Texture2D lightmapTex = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapColor;
                    colorTex = lightmapTex.GetPixelBilinear(hit.lightmapCoord.x, hit.lightmapCoord.y);
                }
                Debug.Log("Objet touché : " + hit.collider.gameObject.name + " | Lightmap coord. : " + hit.lightmapCoord + " | Couleur : " + colorTex);
            }
        }
    }
}
