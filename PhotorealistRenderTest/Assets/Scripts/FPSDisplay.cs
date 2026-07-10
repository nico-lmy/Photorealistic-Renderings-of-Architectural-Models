using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    float deltaTime = 0f;

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;
        Debug.Log($"vSync={QualitySettings.vSyncCount}, target={Application.targetFrameRate}");
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        float fps = 1f / deltaTime;
        GUIStyle style = new GUIStyle();
        style.fontSize = 40;
        style.normal.textColor = Color.green;
        GUI.Label(new Rect(10, 10, 300, 50), string.Format("{0:0.} FPS", fps), style);
    }
}