using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class LightingManager : MonoBehaviour
{
    public Material defaultSkybox;
    public Color ambientColor = Color.gray;
    public float ambientIntensity = 1.0f;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyLightingSettings();
    }

    private void ApplyLightingSettings()
    {
        if (defaultSkybox != null)
        {
            RenderSettings.skybox = defaultSkybox;
        }
        
        RenderSettings.ambientMode = AmbientMode.Skybox;
        RenderSettings.ambientSkyColor = ambientColor;
        RenderSettings.ambientIntensity = ambientIntensity;
        DynamicGI.UpdateEnvironment();
    }
}
