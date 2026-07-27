using UnityEngine;

public class RuntimeUIController : MonoBehaviour
{
    public enum DisplayMode { NotChosen, Simple, Cave }

    [Header("References")]
    public SunController sunController;
    public LuminanceAnalyzer luminanceAnalyzer;
    public PlayerController playerController;

    [Header("Freeze state source")]
    public SimpleController simpleController;
    public StereoController stereoController;

    [Header("Root GameObjects")]
    public GameObject stereolabRoot;
    public GameObject simpleCamRoot;

    [Header("UI Settings")]
    public KeyCode toggleKey = KeyCode.Tab;
    public float burgerSize = 60f;
    public float panelWidth = 450f;
    public float panelHeight = 700f;

    private bool panelOpen = false;
    private int activePanelCamIndex = -1;
    private DisplayMode mode = DisplayMode.NotChosen;
    private GUIStyle burgerStyle;
    private GUIStyle panelStyle;
    private GUIStyle labelStyle;
    private GUIStyle titleStyle;
    private GUIStyle bigButtonStyle;

    private System.Collections.Generic.Dictionary<string, string> textFieldBuffers = new System.Collections.Generic.Dictionary<string, string>();

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerController != null) playerController.enabled = false;
        if (simpleController != null) simpleController.enabled = false;
        if (stereoController != null) stereoController.enabled = false;
    }

    void Update()
    {
        if (mode == DisplayMode.NotChosen) return;

        bool isFrozen = mode == DisplayMode.Simple
            ? (simpleController != null && simpleController.isFrozen)
            : (stereoController != null && stereoController.isFrozen);

        if (!isFrozen && Input.GetKeyDown(toggleKey))
        {
            panelOpen = !panelOpen;
            if (!panelOpen) activePanelCamIndex = -1;
            else if (activePanelCamIndex == -1) activePanelCamIndex = 0;
            ApplyCursorState();
        }

        if (isFrozen && panelOpen)
        {
            panelOpen = false;
            activePanelCamIndex = -1;
            ApplyCursorState();
        }
    }

    void ApplyCursorState()
    {
        if (panelOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (playerController != null) playerController.enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (playerController != null) playerController.enabled = true;
        }
    }

    void EnsureStyles()
    {
        if (burgerStyle != null) return;

        burgerStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 28,
            alignment = TextAnchor.MiddleCenter
        };

        panelStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(15, 15, 15, 15) };

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            normal = { textColor = Color.white }
        };

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white },
            alignment = TextAnchor.MiddleCenter
        };

        bigButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 22,
            fixedHeight = 60
        };
    }

    void OnGUI()
    {
        EnsureStyles();

        if (mode == DisplayMode.NotChosen)
        {
            DrawModeSelection();
            return;
        }

        if (mode == DisplayMode.Cave)
        {
            Camera[] cams = Camera.allCameras;
            for (int i = 0; i < cams.Length; i++) { DrawUIForCamera(cams[i].pixelRect, i); }
        }
        else DrawUIForCamera(new Rect(0, 0, Screen.width, Screen.height), 0);
    }

    void DrawUIForCamera(Rect camRect, int camIndex)
    {
        bool isFrozen = mode == DisplayMode.Simple
            ? (simpleController != null && simpleController.isFrozen)
            : (stereoController != null && stereoController.isFrozen);

        if (isFrozen) return;

        float burgerX = camRect.x + 20f;
        float burgerY = camRect.y + camRect.height - burgerSize - 20f;

        Rect burgerRect = new Rect(burgerX, burgerY, burgerSize, burgerSize);

        if (GUI.Button(burgerRect, "☰", burgerStyle))
        {
            if (panelOpen && activePanelCamIndex == camIndex)
            {
                panelOpen = false;
                activePanelCamIndex = -1;
            }
            else
            {
                panelOpen = true;
                activePanelCamIndex = camIndex;
            }
            ApplyCursorState();
        }

        if (panelOpen && activePanelCamIndex == camIndex)
        {
            float estimatedHeight = 500f;

            float panelY = burgerY - estimatedHeight - 10f;
            panelY = Mathf.Max(panelY, camRect.y + 10f);

            Rect panelRect = new Rect(burgerX, panelY, panelWidth, estimatedHeight);
            GUILayout.BeginArea(panelRect, panelStyle);
            DrawPanelContent();
            GUILayout.EndArea();
        }
    }

    void DrawModeSelection()
    {
        float boxW = 300f, boxH = 220f;
        Rect rect = new Rect((Screen.width - boxW) / 2f, (Screen.height - boxH) / 2f, boxW, boxH);

        GUI.Box(rect, "");
        GUILayout.BeginArea(rect);
        GUILayout.Space(10);
        GUILayout.Label("Choose a display mode", titleStyle);
        GUILayout.Space(15);

        if (GUILayout.Button("Simple Camera", bigButtonStyle)) ChooseMode(DisplayMode.Simple);
        GUILayout.Space(10);
        if (GUILayout.Button("CAVE Cameras", bigButtonStyle)) ChooseMode(DisplayMode.Cave);
        GUILayout.EndArea();
    }

    void ChooseMode(DisplayMode chosen)
    {
        mode = chosen;

        bool isSimple = chosen == DisplayMode.Simple;

        if (simpleCamRoot != null) simpleCamRoot.SetActive(isSimple);
        if (stereolabRoot != null) stereolabRoot.SetActive(!isSimple);

        if (simpleController != null) simpleController.enabled = isSimple;
        if (stereoController != null) stereoController.enabled = !isSimple;

        if (playerController != null) playerController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void DrawPanelContent()
    {
        GUILayout.Label("Settings", titleStyle);
        GUILayout.Space(10);

        if (sunController != null && luminanceAnalyzer != null)
        {
            sunController.latitude = FloatFieldWithSlider("Latitude :", sunController.latitude, -90f, 90f, "lat");
            sunController.longitude = FloatFieldWithSlider("Longitude :", sunController.longitude, -180f, 180f, "lon");
            if (GUILayout.Button("Load New Position")) sunController.LoadNewPosition();
            GUILayout.Space(10);

            DrawHourField();
            GUILayout.Label($"Month : {sunController.month}", labelStyle);
            sunController.month = Mathf.RoundToInt(GUILayout.HorizontalSlider(sunController.month, 1, 12));
            GUILayout.Label($"Day : {sunController.dayOfMonth}", labelStyle);
            sunController.dayOfMonth = Mathf.RoundToInt(GUILayout.HorizontalSlider(sunController.dayOfMonth, 1, 31));
            GUILayout.Label($"Time zone : {sunController.timeZone:0}", labelStyle);
            sunController.timeZone = GUILayout.HorizontalSlider(sunController.timeZone, -12f, 12f);
            GUILayout.Space(10);

            GUILayout.Label($"Max Luminance : {luminanceAnalyzer.maxLuminance:0} cd/m²", labelStyle);
            luminanceAnalyzer.maxLuminance = GUILayout.HorizontalSlider(luminanceAnalyzer.maxLuminance, 100f, 20000f);
            GUILayout.Label($"Min Luminance : {luminanceAnalyzer.minLuminance:0} cd/m²", labelStyle);
            luminanceAnalyzer.minLuminance = GUILayout.HorizontalSlider(luminanceAnalyzer.minLuminance, 0f, 5000f);
        }
    }

    void DrawHourField()
    {
        int totalMinutes = Mathf.RoundToInt(sunController.hour * 60f);
        int h = totalMinutes / 60;
        int m = totalMinutes % 60;

        GUILayout.Label("Hour :", labelStyle);
        GUILayout.BeginHorizontal();

        string keyH = "hourH", keyM = "hourM";
        if (!textFieldBuffers.ContainsKey(keyH)) textFieldBuffers[keyH] = h.ToString("00");
        if (!textFieldBuffers.ContainsKey(keyM)) textFieldBuffers[keyM] = m.ToString("00");

        string hStr = GUILayout.TextField(textFieldBuffers[keyH], GUILayout.Width(40));
        GUILayout.Label(":", labelStyle, GUILayout.Width(10));
        string mStr = GUILayout.TextField(textFieldBuffers[keyM], GUILayout.Width(40));

        textFieldBuffers[keyH] = hStr;
        textFieldBuffers[keyM] = mStr;

        if (int.TryParse(hStr, out int parsedH) && int.TryParse(mStr, out int parsedM))
        {
            parsedH = Mathf.Clamp(parsedH, 0, 23);
            parsedM = Mathf.Clamp(parsedM, 0, 59);
            sunController.hour = parsedH + parsedM / 60f;
        }

        GUILayout.EndHorizontal();

        float newHour = GUILayout.HorizontalSlider(sunController.hour, 0f, 24f);
        if (!Mathf.Approximately(newHour, sunController.hour))
        {
            sunController.hour = newHour;
            int newH = Mathf.FloorToInt(newHour);
            int newM = Mathf.RoundToInt((newHour - newH) * 60f);
            textFieldBuffers[keyH] = newH.ToString("00");
            textFieldBuffers[keyM] = newM.ToString("00");
        }
    }

    float FloatFieldWithSlider(string label, float value, float min, float max, string key)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, labelStyle, GUILayout.Width(140));

        if (!textFieldBuffers.ContainsKey(key)) textFieldBuffers[key] = value.ToString("0.00");

        string newText = GUILayout.TextField(textFieldBuffers[key], GUILayout.Width(70));
        textFieldBuffers[key] = newText;

        if (float.TryParse(newText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float parsed))
            value = Mathf.Clamp(parsed, min, max);
        GUILayout.EndHorizontal();

        float newValue = GUILayout.HorizontalSlider(value, min, max);
        if (!Mathf.Approximately(newValue, value))
        {
            value = newValue;
            textFieldBuffers[key] = value.ToString("0.00");
        }

        return value;
    }
}