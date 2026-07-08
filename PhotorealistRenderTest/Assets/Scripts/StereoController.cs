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
    public float timeBeforeCapture = 0.5f;

    private PathTracing pathTracing;
    private RenderTexture[] leftRT = new RenderTexture[3];
    private RenderTexture[] rightRT = new RenderTexture[3];
    private RenderTexture[] leftRTGI = new RenderTexture[3];
    private RenderTexture[] rightRTGI = new RenderTexture[3];
    private bool frozen = false;
    private bool capturing = false;
    private bool useFinalPT = false;
    private Vector3 lastPos;
    private Quaternion lastRot;
    private float stopTimer = 0f;

    void Start()
    {
        if (volume != null && volume.profile.TryGet<PathTracing>(out var pt))
            pathTracing = pt;

        if (trackedTransform != null)
        {
            lastPos = trackedTransform.position;
            lastRot = trackedTransform.rotation;
        }

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

    }

    void Update()
    {
        if (trackedTransform == null || capturing) return;

        float dist  = Vector3.Distance(lastPos, trackedTransform.position);
        float angle = Quaternion.Angle(lastRot, trackedTransform.rotation);
        bool moving = dist > movementThreshold || angle > rotationThreshold;

        lastPos = trackedTransform.position;
        lastRot = trackedTransform.rotation;

        if (moving)
        {
            stopTimer = 0f;
            if (frozen) Unfreeze();
            if (pathTracing != null && pathTracing.enable.value)
                pathTracing.enable.value = false;
        }
        else if (!frozen)
        {
            stopTimer += Time.deltaTime;
            if (stopTimer >= timeBeforeCapture)
                StartCoroutine(CaptureRoutine());
        }
    }

    IEnumerator CaptureRoutine()
    {
        capturing = true;
        useFinalPT = false; 

        StereolabInstance.autoFlip = false;

        StereolabInstance.ForceEye(true);
        yield return CaptureRTGIQuick(leftRTGI);
        StereolabInstance.ForceEye(false);
        yield return CaptureRTGIQuick(rightRTGI);

        EnableFrozenBlit();
        frozen = true;
        StereolabInstance.autoFlip = true; 

        if (pathTracing != null) pathTracing.enable.value = true;
        StereolabInstance.autoFlip = false; 

        // oeil gauche
        StereolabInstance.ForceEye(true);
        yield return AccumulateInto(leftRT);

        // oeil droit
        StereolabInstance.ForceEye(false);
        yield return AccumulateInto(rightRT);

        if (pathTracing != null) pathTracing.enable.value = true;

        useFinalPT = true;
        pathTracing.enable.value = false; 
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

    void EnableFrozenBlit()
    {
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    void OnEndCameraRendering(ScriptableRenderContext ctx, Camera cam)
    {
        Debug.Log("OnEndCameraRendering");
        if (!frozen) return;
        int idx = System.Array.IndexOf(cameras, cam);
        if (idx < 0) return;

        RenderTexture src; 
        if (useFinalPT) src = StereolabInstance.renderLeftEye ? leftRT[idx] : rightRT[idx];
        else src = StereolabInstance.renderLeftEye ? leftRTGI[idx] : rightRTGI[idx];

        if (src == null) return;

        Rect r = cam.rect;

        RenderTexture.active = null;
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, Screen.width, Screen.height, 0);

        Graphics.DrawTexture(new Rect(r.x * Screen.width,
                                    r.y * Screen.height,
                                    r.width * Screen.width,
                                    r.height * Screen.height), src);

        GL.PopMatrix();
    }

    void Unfreeze()
    {
        frozen = false;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        StereolabInstance.autoFlip = true;
        stopTimer = 0f;
    }

    void OnDestroy()
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            if (leftRT[i]) leftRT[i].Release();
            if (rightRT[i]) rightRT[i].Release();
            if (leftRTGI[i]) leftRTGI[i].Release();
            if (rightRTGI[i]) rightRTGI[i].Release();
        }
    }
}
