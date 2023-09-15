using Komodo.Runtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;
using Komodo.AssetImport;

public class ModelAssetCard : MonoBehaviour
{
    
    //public string name;
    //public string url;
    //public float scale;
    public Sprite sprite;
    public Image image;
    public TextMeshProUGUI nameTextComponent;
    public TextMeshProUGUI urlTextComponent;
    public ModelButtonList mbl;
    public Material material;
    public UnityAction onAssetCardClicked;
    public UnityAction onAssetLoaded;
    public Toggle wholeObjectToggle;

    //public Vector3 pos;
    //public Quaternion rot;

    public ModelData modelData;


    //public void SetPosRot(Vector3 pos, Quaternion rot)
    //{
    //    this.modelData.pos = pos;
    //    this.modelData.rot = rot;
    //}

    // public List<ModelData> modelLibrary;
    public void Instantiate()
    {
        nameTextComponent.text = modelData.modelName;
        image = GetComponentInChildren<Image>(true);
        image.sprite = sprite;

        nameTextComponent.text = modelData.modelName;
        urlTextComponent.text = modelData.modelURL;



    }


    public void CardIsClicked(bool net_call = true)
    {
        
        mbl.InstantiateNewAssetToList(modelData, !wholeObjectToggle.isOn, onAssetCardClicked, onAssetLoaded, true, net_call);
    
    
    }

    
}
