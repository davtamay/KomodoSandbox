using Komodo.AssetImport;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ModelButtonList : ButtonList
{
    public ModelDataTemplate modelData;
    public GameObject placeHolderInputTemplate;
    public UnityAction<ModelItem> onPlaceholderModelItemChanged;

    public void Instantiate(UnityAction onModelClick, UnityAction onModelLoaded)
    {
        StartCoroutine(InstantiateList(onModelClick, onModelLoaded));
    }

    //ONLY DOWNLOAD BUTTON DOES THIS
    private IEnumerator InstantiateList(UnityAction onModelClick, UnityAction onModelLoaded)
    {
      
        if (!ModelImportInitializer.IsAlive)
        {
            gameObject.SetActive(false);
            yield break;
        }
        else
        {
            gameObject.SetActive(true);
        }

        Debug.Log("DDSDSDS");

        yield return StartCoroutine(InstantiatePlaceHolderForAssetCard(new ModelData(), -1, true, onModelClick, onModelLoaded, false, true, (m) => { onPlaceholderModelItemChanged?.Invoke(m); }));



        //  yield return StartCoroutine(InstantiatePlaceHolderButton(new ModelData(), -1, true, onModelClick, onModelLoaded));
    }

    public IEnumerator InstantiateNewAssetToList(ModelData modelData, UnityAction onAssetClicked, UnityAction onAssetLoadedCallback, bool isFromModelLibrary, bool net_call = true)
    {
        //if (net_call)
        //{
            var buttonIndex = ModelImportInitializer.Instance.GetRoot().transform.childCount;
            yield return StartCoroutine(InstantiatePlaceHolderButtonForNewInstance(modelData, buttonIndex, false, onAssetClicked, onAssetLoadedCallback, isFromModelLibrary, net_call, (m) =>
            {

                m.modelData = modelData;
                m.inputURL.text = modelData.modelURL;
                m.nameDisplay.Set(name);
                m.isNet_Call = net_call;
                m.guid = modelData.guid;

                if (Mathf.Approximately(0, modelData.scale))
                    modelData.scale = 1;

                Debug.Log("scale within anonimous: " + modelData.scale);

                m.downloadButton.onClick.Invoke();

                //   m.
            }));
        //}
        //else
        //{
        //    //receiving call from others to instantiate
        //    InstantiateAssetCards.Instance.InitializeNewInstanceFromCard(modelData, mo, modelData.modelURL, butttonIndex, onPlaceHolderUsed, onAssetClicked, onAssetLoaded, isFromModelLibrary)
        //}
    }

    private IEnumerator InstantiatePlaceHolderButtonForNewInstance(ModelData modelData, int buttonIndex, bool addNewPlaceHolderAfterClick, UnityAction onAssetClicked, UnityAction onAssetLoadedCallback, bool isFromModelLibrary, bool netCall, Action<ModelItem> callback)
    {
        GameObject item = Instantiate(placeHolderInputTemplate, transformToPlaceButtonUnder);
       

        if (item.TryGetComponent(out ModelItem modelItem))
        {
            Action<string, int> addPlaceHolderAfterUsed = (s, i) =>
            {
                if (addNewPlaceHolderAfterClick)
                    StartCoroutine(InstantiatePlaceHolderButtonForNewInstance(modelData, -1, true, onAssetClicked, onAssetLoadedCallback, false, netCall, (m) => {

                        onPlaceholderModelItemChanged?.Invoke(m);


                    }));
            };

          StartCoroutine(  InstantiateAssetCards.Instance.InitializeNewInstanceFromCardCoroutine(modelData,  modelItem, modelData.modelURL, buttonIndex, true, addPlaceHolderAfterUsed, onAssetClicked: onAssetClicked, onAssetLoaded: onAssetLoadedCallback));
            // modelItem.Initialize( modelData.modelURL, buttonIndex, true, addPlaceHolderAfterUsed, onAssetClicked, onAssetLoadedCallback, isFromModelLibrary);
            callback.Invoke(modelItem);

            yield return null;
            // Additional logic after initialization, if needed.
        }
        else
        {
            throw new MissingComponentException("modelItem on GameObject (from ModelButtonTemplate)");
        }
    }

    private IEnumerator InstantiatePlaceHolderForAssetCard(ModelData modelData, int buttonIndex, bool addNewPlaceHolderAfterClick, UnityAction onAssetClicked, UnityAction onAssetLoadedCallback, bool isFromModelLibrary, bool netCall, Action<ModelItem> callback)
    {
        GameObject item = Instantiate(placeHolderInputTemplate, transformToPlaceButtonUnder);


        if (item.TryGetComponent(out ModelItem modelItem))
        {
            Action<string, int> addPlaceHolderAfterUsed = (s, i) =>
            {
                if (addNewPlaceHolderAfterClick)
                    StartCoroutine(InstantiatePlaceHolderForAssetCard(modelData, -1, true, onAssetClicked, onAssetLoadedCallback, false, netCall, (m) => {

                        onPlaceholderModelItemChanged?.Invoke(m);


                    }));
            };


       StartCoroutine(     InstantiateAssetCards.Instance.InitializeCheckForNewAssetCard(modelItem, modelData.modelURL, buttonIndex, true, addPlaceHolderAfterUsed,onAssetClicked: onAssetClicked,onAssetLoaded: onAssetLoadedCallback));//(modelItem, modelData.modelURL, butttonIndex, isDownloadPlaceHolder, onPlaceHolderUsed, onAssetClicked, onAssetLoaded);

          //  modelItem.Initialize(modelData.modelURL, buttonIndex, true, addPlaceHolderAfterUsed, onAssetClicked, onAssetLoadedCallback, isFromModelLibrary);
            callback.Invoke(modelItem);

            yield return null;
            // Additional logic after initialization, if needed.
        }
        else
        {
            throw new MissingComponentException("modelItem on GameObject (from ModelButtonTemplate)");
        }
    }


    protected override void NotifyIsReady()
    {
        base.NotifyIsReady();

        if (UIManager.IsAlive)
        {
            UIManager.Instance.isModelButtonListReady = true;
        }
    }
}
