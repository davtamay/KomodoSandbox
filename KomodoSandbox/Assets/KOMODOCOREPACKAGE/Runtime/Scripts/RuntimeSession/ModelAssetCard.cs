using Komodo.Runtime;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using Komodo.AssetImport;

public class ModelAssetCard : MonoBehaviour
{
    public Sprite sprite;
    public Image image;
    public TextMeshProUGUI nameTextComponent;
    public TextMeshProUGUI urlTextComponent;
    //public ModelButtonList mbl;
    public Material material;
    public UnityAction onAssetCardClicked;
    public UnityAction onAssetLoaded;
    public Toggle wholeObjectToggle;
    public ModelData modelData;



    public void Instantiate(ModelData modelData)
    {
        this.modelData = modelData;

        nameTextComponent.text = modelData.modelName;
        image = GetComponentInChildren<Image>(true);
        image.sprite = sprite;
        nameTextComponent.text = modelData.modelName;
        urlTextComponent.text = modelData.modelURL;
    }

    public void ClickCardFromSender(ModelData md)
    {
      InstantiateAssetCards.Instance.InstantiateNewAssetToList(md, onAssetCardClicked, onAssetLoaded, true, false);
    }

    public void CardIsClicked(bool net_call = true)
    {
        if (net_call)
        {
            Guid myGUID = Guid.NewGuid();
            modelData.guid = myGUID.GetHashCode();
            Debug.Log(modelData.guid);
        }
        modelData.isWholeObject = !wholeObjectToggle.isOn;
        InstantiateAssetCards.Instance.InstantiateNewAssetToList(modelData, onAssetCardClicked, onAssetLoaded, true, net_call);
    }

    //private IEnumerator InstantiateNewAssetToListCoroutine(ModelData modelData, UnityAction onAssetClicked, UnityAction onAssetLoaded, bool isFromModelLibrary, bool net_call)
    //{
    //    // Assuming mbl.InstantiateNewAssetToList is now a coroutine
    //    yield return StartCoroutine(mbl.InstantiateNewAssetToList(modelData, onAssetClicked, onAssetLoaded, isFromModelLibrary, net_call));
    //}
}


//public class ModelAssetCard : MonoBehaviour
//{

//    //public string name;
//    //public string url;
//    //public float scale;
//    public Sprite sprite;
//    public Image image;
//    public TextMeshProUGUI nameTextComponent;
//    public TextMeshProUGUI urlTextComponent;
//    public ModelButtonList mbl;
//    public Material material;
//    public UnityAction onAssetCardClicked;
//    public UnityAction onAssetLoaded;
//    public Toggle wholeObjectToggle;

//    //public Vector3 pos;
//    //public Quaternion rot;

//    public ModelData modelData;


//    //public void SetPosRot(Vector3 pos, Quaternion rot)
//    //{
//    //    this.modelData.pos = pos;
//    //    this.modelData.rot = rot;
//    //}

//    // public List<ModelData> modelLibrary;
//    public void Instantiate()
//    {
//        nameTextComponent.text = modelData.modelName;
//        image = GetComponentInChildren<Image>(true);
//        image.sprite = sprite;

//        nameTextComponent.text = modelData.modelName;
//        urlTextComponent.text = modelData.modelURL;



//    }

//    public async Task ClickCardFromSender()
//    {
//        await mbl.InstantiateNewAssetToList(modelData, onAssetCardClicked, onAssetLoaded, true, false);
//    }

//    public async void CardIsClicked(bool net_call = true)
//    {
//        if (net_call)
//        {
//            Guid myGUID = Guid.NewGuid();
//            modelData.guid = myGUID.GetHashCode();
//            Debug.Log(modelData.guid);

//        }
//        modelData.isWholeObject = !wholeObjectToggle.isOn;

//        await mbl.InstantiateNewAssetToList(modelData, onAssetCardClicked, onAssetLoaded, true, net_call);

//        //  return Task.CompletedTask;

//    }


//}