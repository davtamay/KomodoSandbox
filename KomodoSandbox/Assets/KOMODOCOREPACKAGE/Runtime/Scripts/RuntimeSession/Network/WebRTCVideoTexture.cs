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


    [DllImport("__Internal")]
    private static extern void RemoveWebRTCTexture(int id, string name);

    
    public int videoIndex;

    Dictionary<string, (int,RawImage)> nameToRawImage = new Dictionary<string, (int,RawImage)>();

  //  public Dictionary<int, RawImage> inactiveTextures = new Dictionary<int, RawImage>();
  public List<int> textureImages= new List<int>();

    public void ProvideWebRTCTexture(RawImage rawImage, string name)
    {
      //  rawImageToAssign= rawImage;

        //var texture = new Texture2D(256, 256);//(256, 256);
        //int textureId = (int)texture.GetNativeTexturePtr();


        Texture texture = default;
        int textureId = 0;

     


        if (nameToRawImage.ContainsKey(name))
        {
            texture = nameToRawImage[name].Item2.mainTexture;
            textureId = nameToRawImage[name].Item1;
        }
        else
        {
            texture = new Texture2D(256, 256);
            textureId = (int)texture.GetNativeTexturePtr();

            nameToRawImage.Add(name, (textureId, rawImage));
        }

        rawImage.texture = texture;

        //provide the name for the html element that is being created in javascript side
        SetupWebRTCTexture(textureId, name);

        
    }

    public void RemoveTexture(int id, string name)
    {
        //if(inactiveTextures.Count> 0) 
        textureImages.Add(id);


    }
 
    public void ReceiveDimensions(string dimensions)
    {
        string[] dims = dimensions.Split(',');

        string name = dims[0];
        int width = int.Parse(dims[1]) ;
        int height = int.Parse(dims[2]);

        Debug.Log("NAME OF CLIENT: "+ name);




        //transform.localScale = new Vector3(-width * 0.0001f,1, height * 0.0001f);

        nameToRawImage[name].Item2.GetComponent<RectTransform>().sizeDelta = new Vector2(width * 1.5f, height *1.5f);

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