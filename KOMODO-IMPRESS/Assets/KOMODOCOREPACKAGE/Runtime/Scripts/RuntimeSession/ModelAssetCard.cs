using Komodo.Runtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;

public class ModelAssetCard : MonoBehaviour
{
    
    public string name;
    public string url;
    public float scale;
    public Sprite sprite;
    public Image image;
    public TextMeshProUGUI nameTextComponent;
    public TextMeshProUGUI urlTextComponent;
    public ModelButtonList mbl;
    public Material material;
    public UnityAction onAssetCardClicked;
    public UnityAction onAssetLoaded;
    public Toggle wholeObjectToggle;


    // public List<ModelData> modelLibrary;
    public void Instantiate()
    {
        nameTextComponent.text = name;
        image = GetComponentInChildren<Image>(true);
        image.sprite = sprite;

        nameTextComponent.text = name;
        urlTextComponent.text = url;

       
    }
    public void CardIsClicked()
    {
     //   onAssetCardClicked.Invoke();


      

        mbl.InstantiateNewAssetToList(url, name, scale, !wholeObjectToggle.isOn, onAssetCardClicked, onAssetLoaded, true);

    }

}
