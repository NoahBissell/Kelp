using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class WaterHeightBakeTest : MonoBehaviour
{
    public ComputeShader compute;
    public RenderTexture target;

    public int resolution = 256;
    public float worldSize = 100f;
    public Vector2 worldCenter;
    public WaterSurface waterSurface;
    void Start()
    {
        
        waterSurface.SetGlobalTextures();
        target = new RenderTexture(resolution, resolution, 0,
            RenderTextureFormat.ARGBFloat);

        target.enableRandomWrite = true;
        target.Create();
    }

    void Update()
    {
        if (!waterSurface.SetGlobalTextures())
        {
            // print("Bruh");
            return;
        }

        // print("Not bruh");
        int kernel = compute.FindKernel("CSMain");

        compute.SetTexture(kernel, "Result", target);

        compute.SetInt("_Resolution", resolution);
        compute.SetFloat("_WorldSize", worldSize);
        compute.SetVector("_WorldCenterXZ", worldCenter);

        // IMPORTANT:
        // must match the water surface transform Y
        compute.SetFloat("_BaseWaterY", 0.0f);
        WaterSurface surface;
        

        compute.EnableKeyword("WATER_DECAL_COMPLETE");
        
        // Pick the one matching your Water Surface simulation.
        // Try ONE_BAND first just to see if it runs.
        compute.EnableKeyword("WATER_TWO_BANDS");
        // compute.EnableKeyword("WATER_THREE_BANDS");
        // compute.EnableKeyword("WATER_THREE_BANDS");
        
        compute.DisableKeyword("WATER_LOCAL_CURRENT");

        compute.Dispatch(kernel,
            Mathf.CeilToInt(resolution / 8.0f),
            Mathf.CeilToInt(resolution / 8.0f),
            1);

        compute.Dispatch(kernel,
            Mathf.CeilToInt(resolution / 8.0f),
            Mathf.CeilToInt(resolution / 8.0f),
            1);

        Shader.SetGlobalTexture("_WaterHeightTex", target);
    }
}