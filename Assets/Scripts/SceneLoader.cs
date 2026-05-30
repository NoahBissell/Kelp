using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;
    public string currentScene;
    public string startScene;
    public float fadeDuration;

    public RawImage transitionImage;
    
    private void Awake()
    {
        instance = this;
        LoadScene(startScene);
    }

    private Texture2D GetCameraScreenshot()
    {
        Camera cam = Camera.main;
        int w = cam.pixelWidth;
        int h = cam.pixelHeight;
        
        RenderTexture rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
        rt.antiAliasing = 2;
        
        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;
        Texture2D output = new Texture2D(w, h, TextureFormat.ARGB32, false);
        output.ReadPixels(new Rect(0, 0, w, h), 0, 0, false);
        output.Apply();
        
        RenderTexture.active = null;
        cam.targetTexture = null;
        rt.DiscardContents();
        rt.Release();
        
        return output;
    }

    public void LoadScene(string sceneName)
    {
        if (sceneName.Equals(currentScene))
        {
            return;
        }
        
        string from = currentScene;
        string to = sceneName;

        bool useFade = false;
        if (Camera.main != null)
        {
            transitionImage.texture = GetCameraScreenshot();
            useFade = true;
        }
        
        Scene s = SceneManager.GetSceneByName(from);
        AsyncOperation op = SceneManager.LoadSceneAsync(to, LoadSceneMode.Additive);
        op.completed += (_) =>
        {
            if (s.IsValid())
                SceneManager.UnloadSceneAsync(s);
            
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(to));

            if(useFade)
                StartCoroutine(FadingOut());
        };
        
        currentScene = sceneName;

    }

    private IEnumerator FadingOut()
    {
        Color from = Color.white;
        Color to = new Color(1, 1, 1, 0);
        float t = 0f;

        while (t < fadeDuration)
        {
            transitionImage.color = Color.Lerp(from, to, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }

        transitionImage.color = to;
    }
}
