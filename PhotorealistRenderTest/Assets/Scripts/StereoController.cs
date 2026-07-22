using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Stereolab.StereoProjection;

public class StereoController : MonoBehaviour
{
    [Header("References")]
    public Camera[] cameras = new Camera[3];
    public Volume volume;

    [Header("Path tracing")]
    public int maxSamples = 64;
    public int safetyMargin = 8;

    [Header("Detection")]
    public Transform trackedTransform;
    public float movementThreshold = 0.02f;
    public float rotationThreshold = 0.05f;

    [Header("Manual trigger")]
    public KeyCode captureKey = KeyCode.Space;
    public KeyCode forceUnfreezeKey = KeyCode.Backspace;
    public string captureButton = "";

    [Header("Light analysis")]
    public LuminanceAnalyzer luminanceAnalyzer;

    private PathTracing pathTracing;
    private GlobalIllumination globalIllumination;
    private RenderTexture[] leftRT = new RenderTexture[3];
    private RenderTexture[] rightRT = new RenderTexture[3];
    private RenderTexture[] leftRTGI = new RenderTexture[3];
    private RenderTexture[] rightRTGI = new RenderTexture[3];
    private RenderTexture[] heatmapLeftRTs = new RenderTexture[3];
    private RenderTexture[] heatmapRightRTs = new RenderTexture[3];
    private bool frozen = false;
    private bool capturing = false;
    private bool useFinalPT = false;
    private bool showHeatmap = false;
    private Vector3 lastPos;
    private Quaternion lastRot;
    private Texture2D legendTexture;
    private GUIStyle legendStyle;

    void Start()
    {
        if (volume != null && volume.profile.TryGet<PathTracing>(out var pt))
            pathTracing = pt;
        if (volume != null && volume.profile.TryGet<GlobalIllumination>(out var gi))
            globalIllumination = gi;

        if (trackedTransform != null)
        {
            lastPos = trackedTransform.position;
            lastRot = trackedTransform.rotation;
        }
        if (pathTracing != null) pathTracing.enable.value = false;
        if (globalIllumination != null) globalIllumination.active = true;

        for (int i = 0; i < cameras.Length; i++)
        {
            int w = Screen.width / 2;
            int h = Screen.height / 2;
            leftRT[i]  = new RenderTexture(w, h, 24, RenderTextureFormat.DefaultHDR);
            rightRT[i] = new RenderTexture(w, h, 24, RenderTextureFormat.DefaultHDR);
            leftRTGI[i]  = new RenderTexture(w, h, 24, RenderTextureFormat.DefaultHDR);
            rightRTGI[i] = new RenderTexture(w, h, 24, RenderTextureFormat.DefaultHDR);
            leftRT[i].Create();
            rightRT[i].Create();
            leftRTGI[i].Create();
            rightRTGI[i].Create();
        }
        CreateLegendTexture();

    }

    void Update()
    {
        if (trackedTransform == null || capturing) return;

        lastPos = trackedTransform.position;
        lastRot = trackedTransform.rotation;

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
                for (int i = 0; i < cameras.Length; i++)
                {
                    // Génération pour l'oeil gauche ET l'oeil droit
                    luminanceAnalyzer.GenerateHeatmap(leftRT[i], ref heatmapLeftRTs[i]);
                    luminanceAnalyzer.GenerateHeatmap(rightRT[i], ref heatmapRightRTs[i]);
                }
            }
        }
        
        bool pressed = Input.GetKeyDown(captureKey); 
        if (!string.IsNullOrEmpty(captureButton)) pressed = pressed || Input.GetButtonDown(captureButton);
        if (pressed && !frozen) StartCoroutine(CaptureRoutine());
    }

    IEnumerator CaptureRoutine()
    {
        capturing = true;
        useFinalPT = false; 
        frozen = true;

        StereolabInstance.autoFlip = false;

        StereolabInstance.ForceEye(true);
        yield return CaptureRTGIQuick(leftRTGI);
        StereolabInstance.ForceEye(false);
        yield return CaptureRTGIQuick(rightRTGI);

        if (pathTracing != null) pathTracing.enable.value = true;

        // oeil gauche
        StereolabInstance.ForceEye(true);
        yield return AccumulateInto(leftRT);

        // oeil droit
        StereolabInstance.ForceEye(false);
        yield return AccumulateInto(rightRT);

        useFinalPT = true;
        if (pathTracing != null) pathTracing.enable.value = false; 
        if (globalIllumination != null) globalIllumination.active = false;
        foreach (var cam in cameras) cam.enabled = false;
        StereolabInstance.autoFlip = true;
        capturing = false;
    }

    IEnumerator AccumulateInto(RenderTexture[] dst)
    {
        var prev = new RenderTexture[cameras.Length];
        var prevRect = new Rect[cameras.Length];
        for (int i = 0; i < cameras.Length; i++)
        {
            prev[i] = cameras[i].targetTexture;
            prevRect[i] = cameras[i].rect;
            cameras[i].rect = new Rect(0, 0, 1, 1);  
            cameras[i].targetTexture = dst[i];
        }

        int frames = maxSamples + safetyMargin;
        for (int f = 0; f < frames; f++)
            yield return new WaitForEndOfFrame();

        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].targetTexture = prev[i];
            cameras[i].rect = prevRect[i];
        }
    }

    IEnumerator CaptureRTGIQuick(RenderTexture[] dst)
    {
        var prev = new RenderTexture[cameras.Length];
        var prevRect = new Rect[cameras.Length];
        for (int i = 0; i < cameras.Length; i++)
        {
            prev[i] = cameras[i].targetTexture;
            prevRect[i] = cameras[i].rect;
            cameras[i].rect = new Rect(0, 0, 1, 1);
            cameras[i].targetTexture = dst[i];
        }

        for (int f = 0; f < 2; f++)
            yield return new WaitForEndOfFrame();

        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].targetTexture = prev[i];
            cameras[i].rect = prevRect[i];
        }
    }

    void Unfreeze()
    {
        frozen = false;
        showHeatmap = false;
        foreach (var cam in cameras) cam.enabled = true;
        StereolabInstance.autoFlip = true;
    }

    void OnGUI()
    {
        if (frozen && Event.current.type == EventType.Repaint)
            for (int i = 0; i < cameras.Length; i++)
            {
                RenderTexture src = null;
                if (showHeatmap && heatmapLeftRTs[i] != null && heatmapRightRTs[i] != null) 
                    src = StereolabInstance.renderLeftEye ? heatmapLeftRTs[i] : heatmapRightRTs[i];
                else src = useFinalPT ?
                    (StereolabInstance.renderLeftEye ? leftRT[i] : rightRT[i])
                    : (StereolabInstance.renderLeftEye ? leftRTGI[i] : rightRTGI[i]);
                if (src == null) continue;
                
                Rect r = cameras[i].rect;
                GUI.DrawTexture(new Rect(r.x * Screen.width,
                                (1f - r.y - r.height) * Screen.height,
                                r.width * Screen.width,
                                r.height * Screen.height), src, ScaleMode.StretchToFill, false);
            }

        if (capturing) 
        {
            string msg = "Rendering...";
            int fontSize = 40;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = fontSize;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;

            float boxW = 300, boxH = 80;
            Rect rect = new Rect((Screen.width - boxW) / 2f,
                                (Screen.height - boxH) / 2f,
                                boxW, boxH);

            // fond semi-transparent
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
            legendStyle = new GUIStyle(GUI.skin.label);
            legendStyle.fontSize = 20;
            legendStyle.normal.textColor = Color.white;
            legendStyle.alignment = TextAnchor.MiddleLeft;
            legendStyle.wordWrap = false;
        }

        float width = 40f;
        float height = 400f;
        float bgWidth = 160f;
        float startX = Screen.width - bgWidth - width - 20f;
        float startY = (Screen.height - height) / 2f;

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
        for (int y = 0; y < 100; y++)
        {
            float t = y / 99f;
            legendTexture.SetPixel(0, y, GetHeatmapColor(t));
        }
        legendTexture.Apply();
    }

    Color GetHeatmapColor(float t)
    {
        t = Mathf.Clamp01(t);
        Color c0 = Color.blue;
        Color c1 = Color.cyan;
        Color c2 = Color.green;
        Color c3 = Color.yellow;
        Color c4 = Color.red;

        if (t < 0.25f) return Color.Lerp(c0, c1, t / 0.25f);
        if (t < 0.50f) return Color.Lerp(c1, c2, (t - 0.25f) / 0.25f);
        if (t < 0.75f) return Color.Lerp(c2, c3, (t - 0.50f) / 0.25f);
        return Color.Lerp(c3, c4, (t - 0.75f) / 0.25f);
    }

    void OnDestroy()
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            if (leftRT[i]) leftRT[i].Release();
            if (rightRT[i]) rightRT[i].Release();
            if (leftRTGI[i]) leftRTGI[i].Release();
            if (rightRTGI[i]) rightRTGI[i].Release();
            if (heatmapLeftRTs[i]) heatmapLeftRTs[i].Release();
            if (heatmapRightRTs[i]) heatmapRightRTs[i].Release();
        }
    }
}