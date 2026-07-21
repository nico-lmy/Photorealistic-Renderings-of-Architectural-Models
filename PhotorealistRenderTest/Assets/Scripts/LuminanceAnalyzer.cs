using UnityEngine;

public class LuminanceAnalyzer : MonoBehaviour
{
    public ComputeShader heatmapShader;
    public float exposureValue = 12f;
    public float maxLuminance = 5000f;
    public float minLuminance = 0f;

    public void GenerateHeatmap(RenderTexture source, ref RenderTexture destination)
    {
        if (destination == null || destination.width != source.width || destination.height != source.height)
        {
            if (destination != null) destination.Release();
            destination = new RenderTexture(source.width, source.height, 0, RenderTextureFormat.ARGB32);
            destination.enableRandomWrite = true;
            destination.Create();
        }

        // On utilise les noms EXACTS définis dans le Compute Shader
        heatmapShader.SetTexture(0, "InputTexture", source);
        heatmapShader.SetTexture(0, "OutputTexture", destination);
        heatmapShader.SetFloat("_EV", exposureValue);
        heatmapShader.SetFloat("_MaxLuminance", maxLuminance);
        heatmapShader.SetFloat("_MinLuminance", minLuminance);

        int threadGroupsX = Mathf.CeilToInt(source.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(source.height / 8.0f);
        heatmapShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
    }
}