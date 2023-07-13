//create a monobehaviour that provides the framerate for webgl in screenspace also i wanted to be optimized
//so i made this script
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class FrameRate : MonoBehaviour
{
    public TextMeshProUGUI fpsText;
    public float deltaTime;
    public float updateRate = 4.0f;  // 4 updates per sec.
    public float frameRate;
    private void Start()
    {
        Application.targetFrameRate = 60;
        StartCoroutine(UpdateFrameRate());
    }
    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }
    IEnumerator UpdateFrameRate()
    {
        WaitForSeconds wait = new WaitForSeconds(1.0f / updateRate);
        while (true)
        {
            frameRate = 1.0f / deltaTime;
            fpsText.text = "FPS: " + frameRate.ToString("F2");
            yield return wait;
        }
    }
}