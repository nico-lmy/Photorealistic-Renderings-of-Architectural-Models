using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class SimpleController : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public Volume volume;

    [Header("Path tracing")]
    public int maxSamples = 64;
    public int safetyMargin = 8;

    [Header("Detection")]
    public Transform trackedTransform;

    [Header("Manual trigger")]
    public KeyCode captureKey = KeyCode.Space;
    public KeyCode forceUnfreezeKey = KeyCode.Backspace;

    [Header("Light analysis")]
    public LuminanceAnalyzer luminanceAnalyzer;
    public GameObject artificialLightsParent;

    private PathTracing pathTracing;
    private GlobalIllumination globalIllumination;
    
    private RenderTexture mainRT;
    private RenderTexture mainRT_Nat;
    private RenderTexture mainRTGI;
    private RenderTexture heatmapRT;
    
    [HideInInspector] public Texture2D frozenTexture;
    [HideInInspector] public Texture2D frozenTextureNat;
    
    private bool frozen = false;
    private bool capturing = false;
    private bool useFinalPT = false;
    private bool showHeatmap = false;
    
    private Texture2D legendTexture;
    private GUIStyle legendStyle;

    void Start()
    {
        if (volume != null && volume.profile.TryGet<PathTracing>(out var pt))
            pathTracing = pt;
        if (volume != null && volume.profile.TryGet<GlobalIllumination>(out var gi))
            globalIllumination = gi;

        if (pathTracing != null) pathTracing.enable.value = false;
        if (globalIllumination != null) globalIllumination.active = true;

        int w = Screen.width;
        int h = Screen.height;
        
        mainRT = new RenderTexture(w, h, 24, RenderTextureFormat.DefaultHDR);
        mainRT_Nat = new RenderTexture(w, h, 24, RenderTextureFormat.DefaultHDR);
        mainRTGI = new RenderTexture(w, h, 24, RenderTextureFormat.DefaultHDR);
        
        mainRT.Create();
        mainRT_Nat.Create();
        mainRTGI.Create();
        
        frozenTexture = new Texture2D(w, h, TextureFormat.RGBAHalf, false);
        frozenTextureNat = new Texture2D(w, h, TextureFormat.RGBAHalf, false);

        CreateLegendTexture();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (capturing) return;

        if (frozen && Input.GetKeyDown(forceUnfreezeKey))
        {
            Unfreeze();
            if (globalIllumination != null) globalIllumination.active = true;
            if (pathTracing != null && pathTracing.enable.value)
                pathTracing.enable.value = false;
        }

        if (frozen && !capturing && Input.GetKeyDown(KeyCode.H))
        {
            showHeatmap = !showHeatmap; 
            if (showHeatmap && luminanceAnalyzer != null)
            {
                luminanceAnalyzer.GenerateHeatmap(mainRT, ref heatmapRT);
            }
        }
        
        if (Input.GetKeyDown(captureKey) && !frozen) 
        {
            StartCoroutine(CaptureRoutine());
        }
    }

    IEnumerator CaptureRoutine()
    {
        capturing = true;
        useFinalPT = false; 
        frozen = true;

        yield return CaptureRTGIQuick(mainRTGI);

        if (pathTracing != null) pathTracing.enable.value = true;

        if (artificialLightsParent != null) artificialLightsParent.SetActive(false);
        yield return AccumulateInto(mainRT_Nat);

        if (artificialLightsParent != null) artificialLightsParent.SetActive(true);
        yield return AccumulateInto(mainRT);

        useFinalPT = true;
        if (pathTracing != null) pathTracing.enable.value = false; 
        if (globalIllumination != null) globalIllumination.active = false;
        
        mainCamera.enabled = false;

        RenderTexture.active = mainRT;
        frozenTexture.ReadPixels(new Rect(0, 0, mainRT.width, mainRT.height), 0, 0);
        frozenTexture.Apply();

        RenderTexture.active = mainRT_Nat;
        frozenTextureNat.ReadPixels(new Rect(0, 0, mainRT_Nat.width, mainRT_Nat.height), 0, 0);
        frozenTextureNat.Apply();
        
        RenderTexture.active = null;
        
        capturing = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    IEnumerator AccumulateInto(RenderTexture dst)
    {
        var prev = mainCamera.targetTexture;
        mainCamera.targetTexture = dst;

        int frames = maxSamples + safetyMargin;
        for (int f = 0; f < frames; f++)
            yield return new WaitForEndOfFrame();

        mainCamera.targetTexture = prev;
    }

    IEnumerator CaptureRTGIQuick(RenderTexture dst)
    {
        var prev = mainCamera.targetTexture;
        mainCamera.targetTexture = dst;

        for (int f = 0; f < 2; f++)
            yield return new WaitForEndOfFrame();

        mainCamera.targetTexture = prev;
    }

    void Unfreeze()
    {
        frozen = false;
        showHeatmap = false;
        mainCamera.enabled = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OnGUI()
    {
        GUI.depth = 10;
        if (frozen && Event.current.type == EventType.Repaint)
        {
            RenderTexture src = null;
            if (showHeatmap && heatmapRT != null) src = heatmapRT;
            else src = useFinalPT ? mainRT : mainRTGI;
            
            if (src != null)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), src, ScaleMode.StretchToFill, false);
            }
        }

        if (capturing) 
        {
            string msg = "Rendering...";
            GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 40, alignment = TextAnchor.MiddleCenter };
            style.normal.textColor = Color.white;

            float boxW = 300, boxH = 80;
            Rect rect = new Rect((Screen.width - boxW) / 2f, (Screen.height - boxH) / 2f, boxW, boxH);

            GUI.color = new Color(0, 0, 0, 0.5f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(rect, msg, style);
        }
        
        if (frozen && showHeatmap && !capturing) DrawLegend();
    }

    void DrawLegend()
    {
        if (legendStyle == null)
        {
            legendStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleRight, wordWrap = false };
            legendStyle.normal.textColor = Color.white;
        }

        float width = 40f, height = 400f, bgWidth = 160f, startX = 30f, startY = (Screen.height - height) / 2f;

        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.DrawTexture(new Rect(startX - 15, startY - 30, width + bgWidth + 30, height + 60), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.DrawTexture(new Rect(startX, startY, width, height), legendTexture);

        if (luminanceAnalyzer != null)
        {
            float max = luminanceAnalyzer.maxLuminance;
            float min = luminanceAnalyzer.minLuminance;
            float mid = (max + min) / 2f;
            float labelWidth = 150f;

            GUI.Label(new Rect(startX + width + 15, startY - 15, labelWidth, 30), max.ToString("0") + " cd/m²", legendStyle);
            GUI.Label(new Rect(startX + width + 15, startY + (height / 2f) - 15, labelWidth, 30), mid.ToString("0") + " cd/m²", legendStyle);
            GUI.Label(new Rect(startX + width + 15, startY + height - 15, labelWidth, 30), min.ToString("0") + " cd/m²", legendStyle);
        }
    }

    void CreateLegendTexture()
    {
        legendTexture = new Texture2D(1, 100);
        for (int y = 0; y < 100; y++) legendTexture.SetPixel(0, y, GetHeatmapColor(y / 99f));
        legendTexture.Apply();
    }

    Color GetHeatmapColor(float t)
    {
        t = Mathf.Clamp01(t);
        Color c0 = new Color(0.0f, 0.0f, 0.0f);
        Color c1 = new Color(0.34f, 0.06f, 0.43f);
        Color c2 = new Color(0.87f, 0.32f, 0.23f);
        Color c3 = new Color(0.96f, 0.68f, 0.16f);
        Color c4 = new Color(0.99f, 0.99f, 0.75f);

        if (t < 0.25f) return Color.Lerp(c0, c1, t / 0.25f);
        if (t < 0.50f) return Color.Lerp(c1, c2, (t - 0.25f) / 0.25f);
        if (t < 0.75f) return Color.Lerp(c2, c3, (t - 0.50f) / 0.25f);
        return Color.Lerp(c3, c4, (t - 0.75f) / 0.25f);
    }

    void OnDestroy()
    {
        if (mainRT) mainRT.Release();
        if (mainRT_Nat) mainRT_Nat.Release();
        if (mainRTGI) mainRTGI.Release();
        if (heatmapRT) heatmapRT.Release();
    }
}