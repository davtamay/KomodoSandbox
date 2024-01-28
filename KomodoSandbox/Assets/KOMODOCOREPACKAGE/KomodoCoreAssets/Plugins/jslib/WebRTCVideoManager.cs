using System.Runtime.InteropServices;
using System;
using UnityEngine;

public class WebRTCVideoTexture : MonoBehaviour
{
    public Texture2D webGLTexture;
    private IntPtr texturePtr;


    [DllImport("__Internal")]
    private static extern int[] GetVideoDimensions();


    [DllImport("__Internal")]
    private static extern void UpdateCanvasTexture(int id);

    void Start()
    {
        GetVideoDimensions();
    }
 
    public void ReceiveDimensions(string dimensions)
    {
        string[] dims = dimensions.Split(',');
        int width = int.Parse(dims[0]);
        int height = int.Parse(dims[1]);

        var texture = new Texture2D(256, 256);
        int textureId = (int)texture.GetNativeTexturePtr();
        UpdateCanvasTexture(textureId);

        GetComponent<Renderer>().material.SetTexture("_MainTex", texture);


    }
    

    void Update()
    {
        if (webGLTexture != null)
        {
          //  Debug.Log("Updating WebGL texture");
            webGLTexture.UpdateExternalTexture(texturePtr);
        }
    }
}