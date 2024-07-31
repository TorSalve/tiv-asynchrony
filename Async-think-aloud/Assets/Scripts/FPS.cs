using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FPS : MonoBehaviour
{
    TMP_Text fpsText;
    public int refreshRate = 10;
    int frameCounter;
    float totalTime;

    void Start()
    {
        fpsText = GetComponent<TMP_Text>();
        frameCounter = 0;
        totalTime = 0;
        
    }

    void Update()
    {
        if (frameCounter == refreshRate)
        {
            float averageFps = (1.0f / (totalTime / refreshRate));
            fpsText.text = averageFps.ToString("F1");
            frameCounter = 0;
            totalTime = 0;
        }
        else
        {
            totalTime += Time.deltaTime;
            frameCounter++;
        }
        
    }
}
