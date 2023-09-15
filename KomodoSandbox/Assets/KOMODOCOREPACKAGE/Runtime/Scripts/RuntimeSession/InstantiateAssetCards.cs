using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Runtime;
using UnityEngine.Events;
using System;
using UnityEditor;
using Komodo.Utilities;

[System.Serializable]
public struct ModelData
{
    public int id;
    public string modelName;
    public string modelURL;
    public bool isWholeObject;
    public string imagePath;
    public float scale;

    public Vector3 pos;
    public Quaternion rot;
}
[System.Serializable]
public class ModelLibrary
{
    public List<ModelData> modelLibrary;
}
public class InstantiateAssetCards : SingletonComponent<InstantiateAssetCards>
{
    public static InstantiateAssetCards Instance
    {
        get { return ((InstantiateAssetCards)_Instance); }
        set { _Instance = value; }
    }


    public ModelButtonList mbl;
    public GameObject cardPrefab;
    public GameObject loadScreen;

    public ModelItem currentPlaceHolder;
    public ModelButtonList modelButtonList;

   public UnityAction onAssetCardClicked;
    public UnityAction onAssetLoaded;

    public Transform contentParent;


    //public UnityAction onNewPlaceHolderProvided;


    private void Awake()
    {
        modelButtonList.onPlaceholderModelItemChanged += (mi) => currentPlaceHolder = mi;
    }

    
    public ModelAssetCard MakeCard(ModelData modelData, bool createThumbnail = true)
    {
          GameObject cardObject = Instantiate(cardPrefab, contentParent);
            ModelAssetCard card = cardObject.GetComponent<ModelAssetCard>();

        card.modelData.modelName = modelData.modelName;
        card.modelData.modelURL = modelData.modelURL;
        card.modelData.scale = 1;

        card.modelData.isWholeObject = modelData.isWholeObject;

       




        card.mbl = mbl;
         //   card.name = modelData.modelName;
          //  card.url = modelData.modelURL;
            card.onAssetCardClicked = onAssetCardClicked;
            card.onAssetLoaded = onAssetLoaded;
          //  card.scale = modelData.scale;

        if (createThumbnail)
        {
            Texture2D texture = Resources.Load<Texture2D>("thumbnails/" + modelData.modelName);
            card.image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

            card.Instantiate();

          return card;


    }
    public Dictionary<string, ModelAssetCard> urlToModelAssetCardDictionary = new Dictionary<string, ModelAssetCard>();
    
    void Start()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("modelLibrary");
        string jsonText = jsonFile.text;
        ModelLibrary modelLibrary = JsonUtility.FromJson<ModelLibrary>(jsonText);


        //we have to capture the placeholder and model library 
        onAssetCardClicked = () => {

            currentPlaceHolder.SignalModelLibraryLoading();
            loadScreen.gameObject.SetActive(true);
        
        };

        onAssetLoaded = () => {

            currentPlaceHolder.SignalModelLibraryLoadingEnd();
            loadScreen.gameObject.SetActive(false); 
        };

        modelButtonList.Instantiate(onAssetCardClicked, onAssetLoaded);

        foreach (ModelData modelData in modelLibrary.modelLibrary)
        {
           var modelAssetCard =  MakeCard(modelData);

            if (!urlToModelAssetCardDictionary.ContainsKey(modelData.modelURL))
                urlToModelAssetCardDictionary.Add(modelData.modelURL, modelAssetCard);

        }



        GlobalMessageManager.Instance.Subscribe("asset", (s) => {

            ModelData data = JsonUtility.FromJson<ModelData>(s);

            bool doWeHaveItInOurLibrary = false;

            ModelAssetCard modelAssetCard= null;

            foreach (var item in modelLibrary.modelLibrary)
            {
                Debug.Log("ITEM : " + item.modelURL + "---- ITEMReceive : " + data.modelURL);

                if (string.Equals(item.modelURL, data.modelURL, StringComparison.CurrentCultureIgnoreCase))
                {
                    Debug.Log("LOOKING IN EQUAL NAMES");
                    if (urlToModelAssetCardDictionary.ContainsKey(item.modelURL))
                    {
                        modelAssetCard = urlToModelAssetCardDictionary[item.modelURL];
                        Debug.Log("FOUND ASSET CARD");
                    }

                    doWeHaveItInOurLibrary = true;
                    break;
                }


            }
                if (!doWeHaveItInOurLibrary)
                {
                  Debug.Log("NEW CARD");
                  modelAssetCard = MakeCard(data, false);
                }

            modelAssetCard.modelData.pos = data.pos;
            modelAssetCard.modelData.rot = data.rot;

          //  modelAssetCard.SetPosRot(modelData.pos, modelData.rot);
            Debug.Log("Receiving");
            modelAssetCard.CardIsClicked(false);




        });
    }

  
}
