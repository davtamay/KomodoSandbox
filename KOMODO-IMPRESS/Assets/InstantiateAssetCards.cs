using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Runtime;
using UnityEngine.Events;
using System;

[System.Serializable]
public struct ModelData
{
    public string modelName;
    public string modelURL;
    public string imagePath;
    public float scale;
}
[System.Serializable]
public class ModelLibrary
{
    public List<ModelData> modelLibrary;
}
public class InstantiateAssetCards : MonoBehaviour
{
    public ModelButtonList mbl;
    public GameObject cardPrefab;
    public GameObject loadScreen;

    public ModelItem currentPlaceHolder;
    public ModelButtonList modelButtonList;
    //public UnityAction onNewPlaceHolderProvided;


    private void Awake()
    {
        modelButtonList.onPlaceholderModelItemChanged += (mi) => currentPlaceHolder = mi;
    }

    void Start()
    {
       

        TextAsset jsonFile = Resources.Load<TextAsset>("modelLibrary");
        string jsonText = jsonFile.text;
        ModelLibrary modelLibrary = JsonUtility.FromJson<ModelLibrary>(jsonText);

      

        //we have to capture the placeholder and model library to avoid 
        UnityAction onAssetCardClicked = () => {
            currentPlaceHolder.SignalModelLibraryLoading();
            loadScreen.gameObject.SetActive(true); };
        UnityAction onAssetLoaded = () => {
            currentPlaceHolder.SignalModelLibraryLoadingEnd();

            loadScreen.gameObject.SetActive(false); };

        modelButtonList.Instantiate(onAssetCardClicked, onAssetLoaded);

        foreach (ModelData modelData in modelLibrary.modelLibrary)
        {
            GameObject cardObject = Instantiate(cardPrefab, transform);
            ModelAssetCard card = cardObject.GetComponent<ModelAssetCard>();
            card.mbl = mbl;
            card.name = modelData.modelName;
            card.url = modelData.modelURL;
            card.onAssetCardClicked = onAssetCardClicked;
            card.onAssetLoaded = onAssetLoaded;
            card.scale = modelData.scale;

            Texture2D texture = Resources.Load<Texture2D>("thumbnails/" + modelData.modelName);
            card.image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            card.Instantiate();
            // Load the sprite for the card's image and assign it to the card's 'sprite' field
            // ...
        }
    }

  
}
