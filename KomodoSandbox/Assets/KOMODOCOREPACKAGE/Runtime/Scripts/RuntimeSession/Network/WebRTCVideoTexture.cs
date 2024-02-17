using System.Runtime.InteropServices;
using System;
using UnityEngine;
using UnityEngine.UI;
using NUnit.Framework;
using System.Collections.Generic;

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
    private static extern void SetupWebRTCTexture(int id, string name );

    //void Start()
    //{
    //    ProvideWebRTCTexture();
    //}

    Dictionary<int, RawImage> idToRawImage= new Dictionary<int, RawImage>();
    public int videoIndex;
    public void ProvideWebRTCTexture(RawImage image, string name)
    {
        rawImageToAssign= image;

        var texture = new Texture2D(256, 256);//(256, 256);
        int textureId = (int)texture.GetNativeTexturePtr();
       
    //    string name =  "remoteVideo_" + videoIndex;

        //provide the name for the html element that is being created in javascript side
        SetupWebRTCTexture(textureId, name);

       // videoIndex++;

        // GetComponent<Renderer>().material.SetTexture("_MainTex", texture);
        image.texture= texture;
    }
 
    public void ReceiveDimensions(string dimensions)
    {
        string[] dims = dimensions.Split(',');
        int width = int.Parse(dims[0]) ;
        int height = int.Parse(dims[1]);

        //transform.localScale = new Vector3(-width * 0.0001f,1, height * 0.0001f);

        rawImageToAssign.GetComponent<RectTransform>().sizeDelta = new Vector2(width * 1.5f, height *1.5f);

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