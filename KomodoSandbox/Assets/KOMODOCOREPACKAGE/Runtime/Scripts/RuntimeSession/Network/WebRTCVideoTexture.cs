using System.Runtime.InteropServices;
using System;
using UnityEngine;
using CSCore.XAudio2;
using UnityEngine.UI;

public class WebRTCVideoTexture : MonoBehaviour
{
    public Texture2D webGLTexture;
    private IntPtr texturePtr;

    public RawImage rawImageToAssign;

    //[DllImport("__Internal")]
    //private static extern void SetupWebRTCTexture();


    //[DllImport("__Internal")]
    //private static extern void GetVideoDimensions();


    [DllImport("__Internal")]
    private static extern void SetupWebRTCTexture(int id);

    void Start()
    {
        ProvideWebRTCTexture();
    }

    public void ProvideWebRTCTexture()
    {
        var texture = new Texture2D(256, 256);//(256, 256);
        int textureId = (int)texture.GetNativeTexturePtr();
        SetupWebRTCTexture(textureId);

        // GetComponent<Renderer>().material.SetTexture("_MainTex", texture);
        rawImageToAssign.GetComponent<RawImage>().texture= texture;
    }
 
    public void ReceiveDimensions(string dimensions)
    {
        string[] dims = dimensions.Split(',');
        int width = int.Parse(dims[0]);
        int height = int.Parse(dims[1]);

        //transform.localScale = new Vector3(-width * 0.0001f,1, height * 0.0001f);

        rawImageToAssign.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);

        Debug.Log($"Dimensions w: {-width} +  h: {height}");

    }
    

    //void Update()
    //{
    //    //if (webGLTexture != null)
    //    //{
    //    //  //  Debug.Log("Updating WebGL texture");
    //    //    webGLTexture.UpdateExternalTexture(texturePtr);
    //    //}
    //}
}