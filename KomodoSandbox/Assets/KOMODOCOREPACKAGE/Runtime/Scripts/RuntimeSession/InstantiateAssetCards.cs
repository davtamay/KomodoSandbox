using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Runtime;
using UnityEngine.Events;
using System;
using UnityEditor;
using Komodo.Utilities;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;

[System.Serializable]
public struct ModelData
{
    public int modelType;

    public int guid;
    public string modelName;
    public string modelURL;
    public bool isWholeObject;
    public string imagePath;
    public float scale;

  //  public int guid;

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



    private void Awake()
    {
        modelButtonList.onPlaceholderModelItemChanged += (mi) => currentPlaceHolder = mi;
    }

    
    public ModelAssetCard MakeCard(ModelData modelData, bool createThumbnail = true)
    {
          GameObject cardObject = Instantiate(cardPrefab, contentParent);
       
        
        cardObject.transform.SetAsFirstSibling();



            ModelAssetCard card = cardObject.GetComponent<ModelAssetCard>();

        //card.modelData.modelName = modelData.modelName;
        //card.modelData.modelURL = modelData.modelURL;
        //card.modelData.scale = modelData.scale;//1;

        //card.modelData.isWholeObject = modelData.isWholeObject;
       




        //card.mbl = mbl;
         //   card.name = modelData.modelName;
          //  card.url = modelData.modelURL;
            card.onAssetCardClicked = onAssetCardClicked;
            card.onAssetLoaded = onAssetLoaded;
          //  card.scale = modelData.scale;

        if (createThumbnail)
        {
            Texture2D texture = Resources.Load<Texture2D>("thumbnails/" + modelData.modelName);

            if(texture)
            card.image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        //Button cardButton = cardObject.GetComponentInChildren<Button>();
        //if (cardButton != null)
        //{
        //    cardButton.onClick.AddListener(() => card.CardIsClicked(true));
        //}

     //   card.Instantiate();
        card.Instantiate(modelData);

          return card;


    }
    public Dictionary<string, ModelAssetCard> urlToModelAssetCardDictionary = new Dictionary<string, ModelAssetCard>();
    ModelLibrary modelLibrary;
    void Start()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("modelLibrary");
        string jsonText = jsonFile.text;
        modelLibrary = JsonUtility.FromJson<ModelLibrary>(jsonText);


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
            
            InstantiateAssetFromData(s);

        });
    }
    public void InstantiateAssetFromData(string s)
    {
        StartCoroutine(InstantiateAssetFromDataCoroutine(s));
    }


    public void InstantiateNewAssetToList(ModelData modelData, UnityAction onAssetClicked, UnityAction onAssetLoaded, bool isFromModelLibrary, bool net_call)
    {
        //ONLY CLICKING CARD FUNCTION
         StartCoroutine(InstantiateNewAssetToListCoroutine(modelData, onAssetCardClicked, onAssetLoaded, isFromModelLibrary, net_call));
    }
    private IEnumerator InstantiateNewAssetToListCoroutine(ModelData modelData, UnityAction onAssetClicked, UnityAction onAssetLoaded, bool isFromModelLibrary, bool net_call)
    {
       
        // Assuming mbl.InstantiateNewAssetToList is now a coroutine
        yield return StartCoroutine(mbl.InstantiateNewAssetToList(modelData, onAssetClicked, onAssetLoaded, isFromModelLibrary, net_call));
    }


    public IEnumerator InstantiateAssetFromDataCoroutine(string s)
    {
        ModelData data = JsonUtility.FromJson<ModelData>(s);

      //  bool doWeHaveItInOurLibrary = false;

        ModelAssetCard modelAssetCard = null;


        if (urlToModelAssetCardDictionary.ContainsKey(data.modelURL))
        {
            Debug.Log("FOUND ASSET CARD");

            modelAssetCard = urlToModelAssetCardDictionary[data.modelURL];

        }
        else
        {
            Debug.Log("NEW CARD");
            modelAssetCard = MakeCard(data, false);
            urlToModelAssetCardDictionary.Add(data.modelURL, modelAssetCard);



        }


        //modelAssetCard.modelData.pos = data.pos;
        //modelAssetCard.modelData.rot = data.rot;

        //modelAssetCard.modelData.guid = data.guid;
        Debug.Log("Receiver: " + data.guid);

        Debug.Log("Receive Pos" + modelAssetCard.modelData.pos);
        //  modelAssetCard.SetPosRot(modelData.pos, modelData.rot);
        // Debug.Log("Receiving");
        //await  modelAssetCard.ClickCardFromSender();
        modelAssetCard.ClickCardFromSender(data);

        yield return null;
        // return Task.CompletedTask;
    }


    public void InitializeNewInstanceFromCard(ModelData md, ModelItem modelItem, string url, int butttonIndex, bool isDownloadPlaceHolder = false, Action<string, int> onPlaceHolderUsed = null, UnityAction onAssetClicked = null, UnityAction onAssetLoaded = null, bool isFromModelLibrary = false)
    {

        StartCoroutine(InitializeNewInstanceFromCardCoroutine(md, modelItem, url, butttonIndex, isDownloadPlaceHolder, onPlaceHolderUsed, onAssetCardClicked, onAssetLoaded, isFromModelLibrary));
    }
    public IEnumerator InitializeNewInstanceFromCardCoroutine(ModelData md, ModelItem modelItem, string url, int butttonIndex, bool isDownloadPlaceHolder = false, Action<string, int> onPlaceHolderUsed = null, UnityAction onAssetClicked = null, UnityAction onAssetLoaded = null, bool isFromModelLibrary = false)
    {
        modelItem.nameDisplay.Initialize("");
        modelItem.visibilityToggle.Initialize(butttonIndex);
        modelItem.lockToggle.Initialize(butttonIndex);
      


        if (butttonIndex != -1)
            modelItem.entityButtonIndex = butttonIndex;

        if (isDownloadPlaceHolder)
        {
            modelItem.downloadButton.onClick.AddListener(() => StartCoroutine(HandleDownloadNewInstanceFromAssetCardCoroutine(md,modelItem,url, butttonIndex, onPlaceHolderUsed, onAssetClicked, onAssetLoaded, isFromModelLibrary)));
            modelItem.transform.SetAsFirstSibling();

        }
        else
        {
            Debug.Log("INSTANCE NO PLACE HOLDER");
            modelItem.nameDisplay.Set(name);
            UIManager.Instance.modelVisibilityToggleList.Add(butttonIndex, modelItem.visibilityToggle);
            UIManager.Instance.modelLockToggleList.Add(butttonIndex, modelItem.lockToggle);

            modelItem.transform.SetSiblingIndex(0);
        }
        yield return null;
    }
    public IEnumerator InitializeCheckForNewAssetCard(ModelItem modelItem, string url, int butttonIndex, bool isDownloadPlaceHolder = false, Action<string, int> onPlaceHolderUsed = null, UnityAction onAssetClicked = null, UnityAction onAssetLoaded = null, bool isFromModelLibrary = false)
    {
        modelItem.nameDisplay.Initialize("");
        modelItem.visibilityToggle.Initialize(butttonIndex);
        modelItem.lockToggle.Initialize(butttonIndex);



        if (butttonIndex != -1)
            modelItem.entityButtonIndex = butttonIndex;

        if (isDownloadPlaceHolder)
        {
            modelItem.downloadButton.onClick.AddListener(() => StartCoroutine(HandleDownloadClickForNewAssetCardCoroutine(modelItem, url, butttonIndex, onPlaceHolderUsed, onAssetClicked, onAssetLoaded, isFromModelLibrary)));
            modelItem.transform.SetAsFirstSibling();

        }
        else
        {
            Debug.Log("INSTANCE NO PLACE HOLDER");
            modelItem.nameDisplay.Set(name);
            UIManager.Instance.modelVisibilityToggleList.Add(butttonIndex, modelItem.visibilityToggle);
            UIManager.Instance.modelLockToggleList.Add(butttonIndex, modelItem.lockToggle);

            modelItem.transform.SetSiblingIndex(0);
        }
        yield return null;
    }

    private IEnumerator HandleDownloadNewInstanceFromAssetCardCoroutine(ModelData md, ModelItem modelItem, string url, int butttonIndex, Action<string, int> onPlaceHolderUsed, UnityAction onAssetClicked, UnityAction onAssetLoaded, bool isFromModelLibrary)
    {
        onAssetClicked?.Invoke();
       
        if (butttonIndex == -1)
            modelItem.entityButtonIndex = ModelImportInitializer.Instance.GetRoot().transform.childCount;

        modelItem.visibilityToggle.Initialize(modelItem.entityButtonIndex);
        modelItem.downloadButton.transform.parent.parent.gameObject.SetActive(false);

        if (modelItem.newInstance == null)
            modelItem.newInstance = new GameObject("placeholder");

        AddToModelList model = modelItem.GetComponent<AddToModelList>();
        if (model == null)
        {
            model = modelItem.newInstance.AddComponent<AddToModelList>();
            model.modelItemReference = modelItem;
            model.SetIndex(butttonIndex);
        }

        KomodoGLTFAssetV5 importInstance = model.GetComponent<KomodoGLTFAssetV5>();
       
        if (importInstance == null)
        {
            importInstance = modelItem.newInstance.AddComponent<KomodoGLTFAssetV5>();
        }

        //importInstance.Url = md.modelURL;
        importInstance.isNetCall = modelItem.isNet_Call;//net_call;
        //importInstance.guid = md.guid;

      //  importInstance.p

        

        model.onImportAttempted += (Message) =>
        {
            StartCoroutine(modelItem.ReturnToFunctionalInputUseAfterSeconds(2, Message, onAssetLoaded));
        };

        UnityWebRequest unityWebRequest = null;
        if (!string.IsNullOrEmpty(modelItem.inputURL.text) && modelItem.IsValidGLTFExtension(modelItem.inputURL.text) && modelItem.IsValidAbsoluteUri(modelItem.inputURL.text))
        {

            try
            {
                unityWebRequest = UnityWebRequest.Get(modelItem.inputURL.text);
            }
            catch
            {
                model.onImportAttempted("Error importing file check url");
                yield break;
            }

        //    yield return unityWebRequest.SendWebRequest();






            importInstance.TryImport(md, modelItem.inputURL.text);
        }
        else
        {
            model.onImportAttempted("Invalid input");
            yield break;
        }

        model.onFinishLoading += (importSuccess, indexInList) =>
        {
            if (importSuccess == "")
            {
                onAssetLoaded?.Invoke();

                if (!isFromModelLibrary)
                    modelItem.downloadButton.transform.parent.parent.gameObject.SetActive(false);


                if (UIManager.Instance.modelVisibilityToggleList.ContainsKey(indexInList))
                    UIManager.Instance.modelVisibilityToggleList[indexInList] = modelItem.visibilityToggle;
                else
                    UIManager.Instance.modelVisibilityToggleList.Add(indexInList, modelItem.visibilityToggle);

                if (UIManager.Instance.modelLockToggleList.ContainsKey(indexInList))
                    UIManager.Instance.modelLockToggleList[indexInList] = modelItem.lockToggle;
                else
                    UIManager.Instance.modelLockToggleList.Add(indexInList, modelItem.lockToggle);


                //  UIManager.Instance.modelLockToggleList.Add(indexInList, lockToggle);
                modelItem.visibilityToggle.Toggle(true, true);

                modelItem.lockToggle.transform.parent.gameObject.SetActive(true);

                modelItem.nameDisplay.Set(Path.GetFileName(modelItem.inputURL.text.TrimEnd('\\')));

                onPlaceHolderUsed?.Invoke(modelItem.nameDisplay.GetName(), ModelImportInitializer.Instance.GetRoot().transform.childCount);

                modelItem.transform.parent.GetChild(0).gameObject.SetActive(true);

                modelItem.transform.SetSiblingIndex(1);

                //  transform.SetAsLastSibling();


            }

        };

        modelItem.nameDisplay.Set("LOADING...");

        //  modelItem.transform.SetAsFirstSibling();

        //modelItem.transform.SetSiblingIndex(0);

        modelItem.transform.parent.GetChild(0).gameObject.SetActive(false);


        modelItem.downloadButton.gameObject.SetActive(false);
        modelItem.isWholeObject.gameObject.SetActive(false);
        modelItem.inputURL.gameObject.SetActive(false);
        modelItem.nameDisplay.gameObject.SetActive(true);


    //    modelItem.transform.SetAsLastSibling();

        if (!isFromModelLibrary)
        {
            model.thisUrl = modelItem.inputURL.text;
        }
        yield return null;
    }


    private IEnumerator HandleDownloadClickForNewAssetCardCoroutine(ModelItem modelItem, string url, int butttonIndex, Action<string, int> onPlaceHolderUsed, UnityAction onAssetClicked, UnityAction onAssetLoaded, bool isFromModelLibrary)
    {
        

        string Message = default;
        UnityWebRequest unityWebRequest= null;
        if (!string.IsNullOrEmpty(modelItem.inputURL.text) && modelItem.IsValidGLTFExtension(modelItem.inputURL.text) && modelItem.IsValidAbsoluteUri(modelItem.inputURL.text))
        {

            using (unityWebRequest = UnityWebRequest.Head(url))
            {
                // Send the request and wait for the desired response.
                yield return unityWebRequest.SendWebRequest();

                switch (unityWebRequest.result)
                {
                    case UnityWebRequest.Result.Success:
                      //  Message = "";//"Connection successful. Status Code: " + unityWebRequest.responseCode;
                        break;
                    case UnityWebRequest.Result.ConnectionError:
                      //  Message = "Error: " + unityWebRequest.error;
                        break;
                    case UnityWebRequest.Result.DataProcessingError:
                        Message = "Error: " + unityWebRequest.error;
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Message = "HTTP Error: " + unityWebRequest.error;
                        break;
                    default:
                        Message ="Unknown Error";
                        break;
                }
            }



        }
        else
        {
            Message = "Invalid input";
            yield break;
        }

        //model.onImportAttempted += (Message) =>
        //{
        StartCoroutine(modelItem.ReturnToFunctionalInputUseAfterSeconds(2, Message, () => {



            //loadScreen.SetActive(false);
            //

            if (string.IsNullOrEmpty(Message))// && unityWebRequest.result != UnityWebRequest.Result.DataProcessingError && unityWebRequest.result != UnityWebRequest.Result.ProtocolError && unityWebRequest.result != UnityWebRequest.Result.ConnectionError && unityWebRequest.result == UnityWebRequest.Result.Success)
            {

                ModelData modelData = new ModelData();
                modelData.modelURL = modelItem.inputURL.text;


                if (Mathf.Approximately(0, modelData.scale))
                        modelData.scale = 1;

                    //   modelData.scale = 1;
                    //  modelData.guid = Guid.NewGuid().GetHashCode();

                    if (!urlToModelAssetCardDictionary.ContainsKey(modelData.modelURL))
                urlToModelAssetCardDictionary.Add(modelData.modelURL, MakeCard(modelData, false));

            }
            else
            {
                Debug.LogError(Message);
            }

        }));
        //};

        //if(string.IsNullOrEmpty(Message))
        //{




        modelItem.nameDisplay.Set("LOADING...");

        //  modelItem.transform.SetAsFirstSibling();

        modelItem.transform.SetSiblingIndex(0);



        modelItem.downloadButton.gameObject.SetActive(false);
        modelItem.isWholeObject.gameObject.SetActive(false);
        modelItem.inputURL.gameObject.SetActive(false);
        modelItem.nameDisplay.gameObject.SetActive(true);



        //    modelItem.transform.SetAsLastSibling();

        //if (!isFromModelLibrary)
        //{
        //    model.thisUrl = modelItem.inputURL.text;
        //}
        yield return null;
    }


   


}
