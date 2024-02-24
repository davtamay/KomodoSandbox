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

    [DllImport("__Internal")]
    private static extern void RequestVideoReadyCheck(string name, int id);


    public int videoIndex;

    Dictionary<string, (int,RawImage)> nameToRawImage = new Dictionary<string, (int,RawImage)>();

    Stack<string> inactivateClientNames = new Stack<string>();

   
  // public Dictionary<int, RawImage> inactiveTextures = new Dictionary<int, RawImage>();
  public Stack<int> inactiveTextureImages= new Stack<int>();

    //public Dictionary<int, Texture> inactiveTextureImageMap = new Dictionary<int, Texture>();

    public void ProvideWebRTCTexture(RawImage rawImage, string name)
    {
      //  rawImageToAssign= rawImage;

        //var texture = new Texture2D(256, 256);//(256, 256);
        //int textureId = (int)texture.GetNativeTexturePtr();


        Texture texture = default;
        int textureId = 0;

     
        if(inactivateClientNames.Count > 0)
        {
           var iname = inactivateClientNames.Pop();

            textureId = nameToRawImage[iname].Item1;
            
            texture = nameToRawImage[iname].Item2.mainTexture;


            nameToRawImage.Remove(iname);


            if (nameToRawImage.ContainsKey(name))
                nameToRawImage[name] = (textureId, rawImage);
            else
               nameToRawImage.Add(name, (textureId, rawImage));


         //   nameToRawImage[iname].Item2 = (textureId, rawImage);

           // nameToRawImage[]

        }
        else
        {
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

        }





        rawImage.texture = texture;

        //provide the name for the html element that is being created in javascript side
        
        // SetupWebRTCTexture(textureId, name);

        RequestVideoReadyCheck(name, textureId);



    }
    public void OnVideoReady(string name)
    {

        if (nameToRawImage.ContainsKey(name))
        {
            var textureId = nameToRawImage[name].Item1;
            SetupWebRTCTexture(textureId, name);
        }

    }

    //public void OnVideoReady(string name)
    //{
    //    RawImage rawImage = null;
    //    Texture texture = default;
    //    int textureId = 0;


    //    if (nameToRawImage.ContainsKey(name))
    //    {
    //        rawImage = nameToRawImage[name].Item2;
    //    }


    //    if (rawImage == null)
    //    {
    //        Debug.LogError("NO RAWIMAGE FOUND WHEN RECEIVING CLIENT NAME ");
    //        return;

    //    }


    //    //  if(nameToRawImage[name].Item1 == -1)
    //    texture = new Texture2D(256, 256);


    //    // if (inactiveTextureImages.Count == 0)
    //    textureId = (int)texture.GetNativeTexturePtr();
    //    //else
    //    //    textureId = inactiveTextureImages.Pop();



    //    nameToRawImage[name] = (textureId, rawImage);



    //    rawImage.texture = texture;


    //    SetupWebRTCTexture(textureId, name);






    //    //if (nameToRawImage.ContainsKey(name))
    //    //{
    //    //    texture = nameToRawImage[name].Item2.mainTexture;
    //    //    textureId = nameToRawImage[name].Item1;
    //    //}
    //    //else
    //    //{
    //    //    texture = new Texture2D(256, 256);

    //    //    if (inactiveTextureImages.Count == 0)
    //    //    {

    //    //        textureId = (int)texture.GetNativeTexturePtr();

    //    //    }
    //    //    else
    //    //    {
    //    //        textureId = inactiveTextureImages.Pop();
    //    //    }

    //    //    nameToRawImage.Add(name, (textureId, rawImage));
    //    //}

    //    //rawImage.texture = texture;

    //    //provide the name for the html element that is being created in javascript side
    //    //SetupWebRTCTexture(textureId, name);




    //}


    public void RemoveTexture(string name)
    {
        if (nameToRawImage.ContainsKey(name)) {

            RemoveWebRTCTexture(nameToRawImage[name].Item1, name);

            inactivateClientNames.Push(name);
            //inactiveTextureImages.Push(nameToRawImage[name].Item1);


            //nameToRawImage.Remove(name);

        }

        //if(inactiveTextures.Count> 0) 
        //textureImages.Add(id);


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