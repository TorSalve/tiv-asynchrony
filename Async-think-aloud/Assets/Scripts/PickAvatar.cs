using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Import the SceneManagement namespace

public class PickAvatar : MonoBehaviour
{
    public bool isStartFlagOn;
    public string sceneToLoad;

    private void Start()
    {
    }

    private void Update()
    {
        if (isStartFlagOn)
        {
            isStartFlagOn = false;
            StartBoxPressed();
        }
    }

    private void StartBoxPressed()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
