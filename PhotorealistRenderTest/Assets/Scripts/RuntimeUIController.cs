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

    [Header("Luminaires")]
    public LuminairePlacementController placementController;
    public PlacedLuminaireRegistry placedLuminaireRegistry;

    [Header("Edit gizmo")]
    public float gizmoScale = 0.3f;
    private GameObject editMarkerInstance;

    [Header("UI Settings")]
    public KeyCode toggleKey = KeyCode.Tab;
    public float burgerSize = 60f;
    public float panelWidth = 450f;
    public float panelHeight = 700f;

    private enum PanelTab { Settings, Luminaires }
    private System.Collections.Generic.HashSet<string> expandedThumbnails = new System.Collections.Generic.HashSet<string>();
    private System.Collections.Generic.Dictionary<int, Vector3> eulerCache = new System.Collections.Generic.Dictionary<int, Vector3>();
    private System.Collections.Generic.Dictionary<string, float> sliderAnchors = new System.Collections.Generic.Dictionary<string, float>();
    private Vector2 luminairesScrollPos;
    private PanelTab currentTab = PanelTab.Settings;
    private bool panelOpen = false;
    public bool IsPanelOpen => panelOpen;
    private int activePanelCamIndex = -1;
    public DisplayMode mode = DisplayMode.NotChosen;
    private GUIStyle burgerStyle;
    private GUIStyle panelStyle;
    private GUIStyle labelStyle;
    private GUIStyle titleStyle;
    private GUIStyle bigButtonStyle;
    private GUIStyle tabButtonStyle;
    private GUIStyle tabButtonActiveStyle;
    private GameObject selectedLuminaire = null;
    private float editPosRange = 2f;

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
        UpdateEditMarker();
    }

    void UpdateEditMarker()
    {
        if (selectedLuminaire == null)
        {
            if (editMarkerInstance != null)
            {
                Destroy(editMarkerInstance);
                editMarkerInstance = null;
            }
            return;
        }

        if (editMarkerInstance != null)
        {
            editMarkerInstance.transform.position = selectedLuminaire.transform.position;
            editMarkerInstance.transform.rotation = selectedLuminaire.transform.rotation;
            float pulse = 0.75f + 0.25f * Mathf.Sin(Time.unscaledTime * 6f);
            editMarkerInstance.transform.localScale = Vector3.one * gizmoScale * pulse;
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

        tabButtonStyle = new GUIStyle(GUI.skin.button) { fontSize = 16, fixedHeight = 35 };
        tabButtonActiveStyle = new GUIStyle(tabButtonStyle);
        tabButtonActiveStyle.normal.textColor = Color.yellow;
        tabButtonActiveStyle.fontStyle = FontStyle.Bold;
    }

    void OnGUI()
    {
        EnsureStyles();
        if (mode != DisplayMode.NotChosen) DrawEditTarget();

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
            float panelY = burgerY - panelHeight - 10f;
            panelY = Mathf.Max(panelY, camRect.y + 10f);

            Rect panelRect = new Rect(burgerX, panelY, panelWidth, panelHeight);
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
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Settings", currentTab == PanelTab.Settings ? tabButtonActiveStyle : tabButtonStyle))
            currentTab = PanelTab.Settings;
        if (GUILayout.Button("Lights", currentTab == PanelTab.Luminaires ? tabButtonActiveStyle : tabButtonStyle))
            currentTab = PanelTab.Luminaires;
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        if (currentTab == PanelTab.Settings) DrawSettingsTab();
        else DrawLuminairesTab();
    }

    void DrawSettingsTab()
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

    void DrawLuminairesTab()
    {
        GUILayout.Label("Add a light", titleStyle);
        GUILayout.Space(10);
        float scrollHeight = panelHeight - 150f;
        luminairesScrollPos = GUILayout.BeginScrollView(luminairesScrollPos, GUILayout.Height(scrollHeight));


        if (placementController == null || placementController.catalog == null)
        {
            GUILayout.Label("No catalog assigned.", labelStyle);
            GUILayout.EndScrollView();
            return;
        }

        bool isPlacing = placementController.IsPlacing;

        foreach (var profile in placementController.catalog.luminaires)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(profile.luminaireName, labelStyle, GUILayout.Width(120));

            bool hasThumbnail = profile.thumbnail != null;
            GUI.enabled = hasThumbnail;
            if (GUILayout.Button("View", GUILayout.Width(45), GUILayout.Height(25)))
            {
                if (expandedThumbnails.Contains(profile.luminaireName))
                    expandedThumbnails.Remove(profile.luminaireName);
                else
                    expandedThumbnails.Add(profile.luminaireName);
            }
            GUI.enabled = true;

            GUILayout.FlexibleSpace();

            bool isCurrentSelection = isPlacing && placementController.CurrentProfile == profile;
            string btnLabel = isCurrentSelection ? "Placing..." : "Place";

            if (GUILayout.Button(btnLabel, GUILayout.Width(60)))
            {
                if (isCurrentSelection) placementController.CancelPlacementPublic();
                else placementController.StartPlacement(profile);
            }
            GUILayout.EndHorizontal();

            if (hasThumbnail && expandedThumbnails.Contains(profile.luminaireName))
            {
                GUILayout.Space(5);
                Rect thumbRect = GUILayoutUtility.GetRect(150, 150, GUILayout.ExpandWidth(false));
                GUI.DrawTexture(thumbRect, profile.thumbnail, ScaleMode.ScaleToFit);
            }

            GUILayout.Space(5);
        }

        GUILayout.Space(20);
        GUILayout.Label("Placed lights", titleStyle);
        GUILayout.Space(10);

        if (placedLuminaireRegistry == null)
        {
            GUILayout.Label("No registry assigned.", labelStyle);
            GUILayout.EndScrollView();
            return;
        }

        var placed = placedLuminaireRegistry.GetAll();
        if (placed.Count == 0) GUILayout.Label("None placed yet.", labelStyle);
        else
        {
            var snapshot = new System.Collections.Generic.List<GameObject>(placed);
            foreach (var go in snapshot)
            {
                if (go == null) continue;
                bool isSelected = (selectedLuminaire == go);
                GUILayout.BeginHorizontal();
                GUILayout.Label(go.name, isSelected ? tabButtonActiveStyle : labelStyle, GUILayout.Width(120));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(isSelected ? "Close" : "Edit", GUILayout.Width(55)))
                    selectedLuminaire = isSelected ? null : go;
                if (GUILayout.Button("Delete", GUILayout.Width(60))) 
                {
                    if (selectedLuminaire == go) selectedLuminaire = null;
                    ClearEditBuffers(go);
                    placedLuminaireRegistry.RemoveSpecific(go);
                }
                GUILayout.EndHorizontal();
                if (isSelected)
                {
                    GUILayout.Space(5);
                    DrawLuminaireEditor(go);
                    GUILayout.Space(10);
                }
            }
        }
        GUILayout.EndScrollView();
    }

    void DrawLuminaireEditor(GameObject go)
    {
        GUILayout.BeginVertical(GUI.skin.box);
        string id = go.GetInstanceID().ToString();

        GUILayout.Label("Transform", titleStyle);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Slider range (m) :", labelStyle, GUILayout.Width(150));
        if (!textFieldBuffers.ContainsKey("posRange"))
            textFieldBuffers["posRange"] = editPosRange.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        textFieldBuffers["posRange"] = GUILayout.TextField(textFieldBuffers["posRange"], GUILayout.Width(60));

        if (float.TryParse(textFieldBuffers["posRange"], System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float parsedRange))
            editPosRange = Mathf.Clamp(parsedRange, 0.05f, 100f);

        GUILayout.EndHorizontal();
        GUILayout.Space(4);
        go.transform.position    = Vector3FieldWithSliders("Position", go.transform.position, id + "_pos", editPosRange, false);

        var light = go.GetComponentInChildren<Light>();
        if (light == null)
        {
            GUILayout.Label("No light found.", labelStyle);
            GUILayout.EndVertical();
            return;
        }

        var hdLight = light.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>();

        GUILayout.Label("Light", titleStyle);

        float intensity = light.intensity;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Intensity :", labelStyle, GUILayout.Width(90));

        string keyI = id + "_int";
        if (!textFieldBuffers.ContainsKey(keyI))
            textFieldBuffers[keyI] = intensity.ToString("0.##");
        else
        {
            if (float.TryParse(textFieldBuffers[keyI], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float bufferedI))
            {
                if (Mathf.Abs(bufferedI - intensity) > 0.01f)
                    textFieldBuffers[keyI] = intensity.ToString("0.##");
            }
        }

        string sI = GUILayout.TextField(textFieldBuffers[keyI], GUILayout.Width(80));
        textFieldBuffers[keyI] = sI;

        if (float.TryParse(sI, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float parsedI))
            intensity = Mathf.Max(0f, parsedI);

        if (hdLight != null)
            GUILayout.Label(light.lightUnit.ToString(), labelStyle, GUILayout.Width(60));

        GUILayout.EndHorizontal();

        float newI = GUILayout.HorizontalSlider(intensity, 0f, 5000f);
        if (!Mathf.Approximately(newI, intensity))
        {
            intensity = newI;
            textFieldBuffers[keyI] = intensity.ToString("0.##");
        }

        light.intensity = intensity;
        GUILayout.BeginHorizontal();
        light.useColorTemperature = GUILayout.Toggle(light.useColorTemperature, "", GUILayout.Width(20));
        GUILayout.Label("Use color temperature", labelStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if (light.useColorTemperature)
        {
            GUILayout.Label($"Color temp : {light.colorTemperature:0} K", labelStyle);
            light.colorTemperature = GUILayout.HorizontalSlider(light.colorTemperature, 1500f, 10000f);
        }
        else
        {
            GUILayout.Label("Color (RGB)", labelStyle);
            Color c = light.color;
            GUILayout.BeginHorizontal();
            GUILayout.Label("R", labelStyle, GUILayout.Width(15));
            c.r = GUILayout.HorizontalSlider(c.r, 0f, 1f);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("G", labelStyle, GUILayout.Width(15));
            c.g = GUILayout.HorizontalSlider(c.g, 0f, 1f);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("B", labelStyle, GUILayout.Width(15));
            c.b = GUILayout.HorizontalSlider(c.b, 0f, 1f);
            GUILayout.EndHorizontal();
            light.color = c;
        }
        GUILayout.Label($"Range : {light.range:0.00} m", labelStyle);
        light.range = GUILayout.HorizontalSlider(light.range, 0.1f, 50f);
        if (light.type == LightType.Spot)
        {
            GUILayout.Label($"Spot angle : {light.spotAngle:0}°", labelStyle);
            light.spotAngle = GUILayout.HorizontalSlider(light.spotAngle, 1f, 179f);
        }
        bool en = light.enabled;
        Color prevBg = GUI.backgroundColor;
        GUI.backgroundColor = en ? new Color(0.3f, 0.7f, 0.3f) : new Color(0.7f, 0.3f, 0.3f);
        if (GUILayout.Button(en ? "ON  (click to disable)" : "OFF  (click to enable)", GUILayout.Height(28)))
            light.enabled = !en;
        GUI.backgroundColor = prevBg;
        GUILayout.EndVertical();
    }

    void DrawEditTarget()
    {
        if (selectedLuminaire == null) return;

        Camera cam = null;
        if (mode == DisplayMode.Cave && activePanelCamIndex >= 0 && stereoController != null
            && stereoController.cameras != null && activePanelCamIndex < stereoController.cameras.Length)
            cam = stereoController.cameras[activePanelCamIndex];

    if (cam == null && simpleCamRoot != null) cam = simpleCamRoot.GetComponentInChildren<Camera>();
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 wp = selectedLuminaire.transform.position;
        Vector3 sp = cam.WorldToScreenPoint(wp);

        bool behind = sp.z < 0f;
        float gx = sp.x;
        float gy = Screen.height - sp.y;

        Color prev = GUI.color;
        GUI.color = new Color(1f, 0.85f, 0.2f, 0.9f);

        if (!behind && gx > 0 && gx < Screen.width && gy > 0 && gy < Screen.height)
        {
            float s = 34f;
            GUI.DrawTexture(new Rect(gx - s * 0.5f, gy - 1f, s, 2f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(gx - 1f, gy - s * 0.5f, 2f, s), Texture2D.whiteTexture);

            GUIStyle st = new GUIStyle(GUI.skin.label);
            st.fontSize = 16;
            st.normal.textColor = new Color(1f, 0.85f, 0.2f);
            GUI.Label(new Rect(gx + 20f, gy - 10f, 220f, 24f), selectedLuminaire.name, st);
        }
        else
        {
            float cx = Mathf.Clamp(gx, 30f, Screen.width - 30f);
            float cy = Mathf.Clamp(gy, 30f, Screen.height - 30f);
            if (behind) { cx = Screen.width - cx; cy = Screen.height - cy; }
            GUI.DrawTexture(new Rect(cx - 10f, cy - 10f, 20f, 20f), Texture2D.whiteTexture);
            GUIStyle st = new GUIStyle(GUI.skin.label);
            st.fontSize = 14;
            st.normal.textColor = new Color(1f, 0.85f, 0.2f);
            GUI.Label(new Rect(cx + 14f, cy - 8f, 200f, 20f), "→ " + selectedLuminaire.name, st);
        }
        GUI.color = prev;
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

    Vector3 Vector3FieldWithSliders(string label, Vector3 v, string key, float sliderRange, bool isAngle)
    {
        GUILayout.Label(label, labelStyle);
        v.x = AxisFieldSlider("X", v.x, key + "_x", sliderRange, isAngle);
        v.y = AxisFieldSlider("Y", v.y, key + "_y", sliderRange, isAngle);
        v.z = AxisFieldSlider("Z", v.z, key + "_z", sliderRange, isAngle);
        return v;
    }

    float AxisFieldSlider(string axis, float value, string key, float sliderRange, bool isAngle)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(axis, labelStyle, GUILayout.Width(18));

        var ci = System.Globalization.CultureInfo.InvariantCulture;

        if (!textFieldBuffers.ContainsKey(key))
            textFieldBuffers[key] = value.ToString("0.###", ci);

        string before = textFieldBuffers[key];
        string after = GUILayout.TextField(before, GUILayout.Width(70));
        textFieldBuffers[key] = after;

        bool typed = (after != before);

        if (float.TryParse(after, System.Globalization.NumberStyles.Float, ci, out float parsed))
            value = parsed;

        if (!sliderAnchors.ContainsKey(key) || typed)
            sliderAnchors[key] = value;

        float anchor = sliderAnchors[key];

        float min, max;
        if (isAngle)
        {
            min = 0f;
            max = 360f;
        }
        else
        {
            float r = sliderRange > 0f ? sliderRange : 1f;
            min = anchor - r;
            max = anchor + r;
        }

        float shown = Mathf.Clamp(value, min, max);
        float newVal = GUILayout.HorizontalSlider(shown, min, max);

        if (!Mathf.Approximately(newVal, shown))
        {
            value = newVal;
            textFieldBuffers[key] = value.ToString("0.###", ci);
        }

        GUILayout.EndHorizontal();
        return value;
    }

    void ClearEditBuffers(GameObject go)
    {
        if (go == null) return;
        int goId = go.GetInstanceID();
        eulerCache.Remove(goId);

        string prefix = goId.ToString();
        var keys = new System.Collections.Generic.List<string>(textFieldBuffers.Keys);
        foreach (var k in keys)
            if (k.StartsWith(prefix)) textFieldBuffers.Remove(k);
            var anchorKeys = new System.Collections.Generic.List<string>(sliderAnchors.Keys);
        foreach (var k in anchorKeys)
            if (k.StartsWith(prefix)) sliderAnchors.Remove(k);
    }

    public void ClosePanel()
    {
        panelOpen = false;
        activePanelCamIndex = -1;
        ApplyCursorState();
    }

    void OnDestroy()
    {
        if (editMarkerInstance != null) Destroy(editMarkerInstance);
    }

}