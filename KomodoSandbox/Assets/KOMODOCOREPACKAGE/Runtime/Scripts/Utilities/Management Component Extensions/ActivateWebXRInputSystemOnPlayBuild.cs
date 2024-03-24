using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebXR.InputSystem;

public class ActivateWebXRInputSystemOnPlayBuild : MonoBehaviour
{
   public void Start()
    {
       #if !UNITY_EDITOR
        var webxrInputSystem = new GameObject("WebXRInputSystem");
        webxrInputSystem.AddComponent<WebXRInputSystem>();
        DontDestroyOnLoad(webxrInputSystem);
      #endif
    }
}
