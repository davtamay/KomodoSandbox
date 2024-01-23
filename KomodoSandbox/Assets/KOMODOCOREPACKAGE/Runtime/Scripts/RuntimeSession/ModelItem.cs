using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Komodo.Utilities;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.Collections;

public class ModelItem : MonoBehaviour
{
    public int entityButtonIndex;
    public VisibilityToggle visibilityToggle;
    public LockToggle lockToggle;
    public TMP_InputField inputURL;
    public Button downloadButton;
    public Toggle isWholeObject;
    public ModelNameDisplay nameDisplay;
    public string inputURLText;
    public GameObject newInstance;

    public ModelData modelData;

    public int guid;
    public bool isNet_Call; //is coming from others send call

    public void SignalModelLibraryLoading()
    {
        downloadButton.transform.parent.parent.gameObject.SetActive(false);
     
    }
    public void SignalModelLibraryLoadingEnd()
    {
        downloadButton.transform.parent.parent.gameObject.SetActive(true);
    }

    //public void Initialize(string url, int butttonIndex, bool isDownloadPlaceHolder = false, Action<string, int> onPlaceHolderUsed = null, UnityAction onAssetClicked = null, UnityAction onAssetLoaded = null, bool isFromModelLibrary = false)
    //{
    //    InstantiateAssetCards.Instance.Initialize(this,url, butttonIndex, isDownloadPlaceHolder, onPlaceHolderUsed, onAssetClicked, onAssetLoaded);
    //}
   

    public bool IsNotEmpty(string url)
    {
        return !string.IsNullOrEmpty(url) && !string.IsNullOrWhiteSpace(url);
    }

    public bool IsValidGLTFExtension(string url)
    {
        string extension = Path.GetExtension(url);
        return extension != null && (extension.ToLower() == ".glb" || extension.ToLower() == ".gltf");
    }

    public bool IsValidAbsoluteUri(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    public IEnumerator ReturnToFunctionalInputUseAfterSeconds(int seconds, string message, UnityAction onAssetLoadingFinished)
    {
        inputURL.gameObject.SetActive(false);
        downloadButton.transform.parent.gameObject.SetActive(false);
        nameDisplay.gameObject.SetActive(true);
        nameDisplay.Set(message);

        yield return new WaitForSeconds(seconds);

        inputURL.gameObject.SetActive(true);
        nameDisplay.gameObject.SetActive(false);
        lockToggle.transform.parent.gameObject.SetActive(false);
        isWholeObject.gameObject.SetActive(true);
        downloadButton.gameObject.SetActive(true);

        downloadButton.transform.parent.parent.gameObject.SetActive(true);
        downloadButton.transform.parent.gameObject.SetActive(true);

        onAssetLoadingFinished?.Invoke();
    }
}


//using System;
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using Komodo.Utilities;
//using System.IO;
//using System.Collections;
//using System.Threading.Tasks;
//using GLTFast.Logging;
//using UnityEngine.Networking;
//using UnityEngine.Events;

////namespace Komodo.Runtime
////{
//public class ModelItem : MonoBehaviour
//{
//    [ShowOnly]
//    public int entityButtonIndex;

//    public VisibilityToggle visibilityToggle;

//    public LockToggle lockToggle;

//    public TMP_InputField inputURL;
//    public Button downloadButton;
//    public Toggle isWholeObject;

//    public ModelNameDisplay nameDisplay;

//    public string inputURLText;

//    GameObject newInstance;



//    public async Task Initialize(string url, int butttonIndex, bool isDownloadPlaceHolder = false, Action<string, int> onPlaceHolderUsed = null, UnityAction onAssetClicked = null, UnityAction onAssetLoaded = null, bool isFromModelLibrary = false, bool net_call = true)
//    {

//        nameDisplay.Initialize("");


//        visibilityToggle.Initialize(butttonIndex);
//        lockToggle.Initialize(butttonIndex);

//        if (butttonIndex != -1)
//            this.entityButtonIndex = butttonIndex;


//        if (isDownloadPlaceHolder)
//        {

//            downloadButton.onClick.AddListener(delegate ()
//            {
//                onAssetClicked?.Invoke();


//                if (butttonIndex == -1)
//                    this.entityButtonIndex = ModelImportInitializer.Instance.GetRoot().transform.childCount;

//                visibilityToggle.Initialize(this.entityButtonIndex);

//                downloadButton.transform.parent.parent.gameObject.SetActive(false);

//                AddToModelList model;
//                KomodoGLTFAssetV5 importInstance;

//                if (newInstance == null)
//                {
//                    newInstance = new GameObject("placeholder");
//                }


//                if (newInstance.TryGetComponent(out AddToModelList currentModel))
//                {
//                    model = currentModel;
//                    //model.isWholeObject = isWhole;
//                    importInstance = currentModel.GetComponent<KomodoGLTFAssetV5>();

//                    Debug.Log("debg: " + currentModel.thisUrl);

//                    importInstance.Url = url;

//                    importInstance.isNetCall = net_call;


//                }
//                else
//                {

//                    model = newInstance.AddComponent<AddToModelList>();
//                    model.modelItemReference = this;

//                    //  model.isWholeObject = isWhole;
//                    model.SetIndex(butttonIndex);

//                    importInstance = newInstance.AddComponent<KomodoGLTFAssetV5>();

//                    importInstance.Url = url;

//                    importInstance.isNetCall = net_call;



//                    model.onImportAttempted += async (Message) =>
//                    {

//                        await ReturnToFunctionalInputUseAfterSeconds(2, Message, onAssetLoaded);

//                    };


//                }




//                if (!string.IsNullOrEmpty(inputURL.text))
//                {
//                    if (!IsValidGLTFExtension(inputURL.text))
//                    {
//                        model.onImportAttempted("No .glb/.gltf extension provided");
//                        return;



//                    }

//                    if (!IsValidAbsoluteUri(inputURL.text))
//                    {
//                        model.onImportAttempted("Path is not absolute");
//                        return;


//                    }

//                    try
//                    {
//                        UnityWebRequest.Get(inputURL.text);

//                    }
//                    catch
//                    {
//                        model.onImportAttempted("Error importing file check url");
//                        return;

//                    }
//                }
//                else
//                {
//                    model?.onImportAttempted("No Input Provided");
//                    return;
//                }


//                importInstance.TryImport(inputURL.text);



//                model.onFinishLoading += (importSuccess, indexInList) =>
//                {
//                    if (importSuccess == "")
//                    {
//                        onAssetLoaded?.Invoke();

//                        if (!isFromModelLibrary)
//                            downloadButton.transform.parent.parent.gameObject.SetActive(false);


//                        if (UIManager.Instance.modelVisibilityToggleList.ContainsKey(indexInList))
//                            UIManager.Instance.modelVisibilityToggleList[indexInList] = visibilityToggle;
//                        else
//                            UIManager.Instance.modelVisibilityToggleList.Add(indexInList, visibilityToggle);

//                        if (UIManager.Instance.modelLockToggleList.ContainsKey(indexInList))
//                            UIManager.Instance.modelLockToggleList[indexInList] = lockToggle;
//                        else
//                            UIManager.Instance.modelLockToggleList.Add(indexInList, lockToggle);


//                        //  UIManager.Instance.modelLockToggleList.Add(indexInList, lockToggle);
//                        visibilityToggle.Toggle(true, true);

//                        lockToggle.transform.parent.gameObject.SetActive(true);

//                        nameDisplay.Set(Path.GetFileName(inputURL.text.TrimEnd('\\')));

//                        onPlaceHolderUsed?.Invoke(nameDisplay.GetName(), ModelImportInitializer.Instance.GetRoot().transform.childCount);


//                        transform.SetAsLastSibling();
//                    }

//                };

//                //have to check if this is coming from input box or is selected from modellibrary
//                nameDisplay.Set("LOADING...");
//                downloadButton.gameObject.SetActive(false);
//                isWholeObject.gameObject.SetActive(false);
//                inputURL.gameObject.SetActive(false);

//                nameDisplay.gameObject.SetActive(true);

//                if (!isFromModelLibrary)
//                {
//                    //  model.isWholeObject = isWhole;//!isWholeObject.isOn;
//                    model.thisUrl = inputURL.text;

//                }



//            });

//            return;
//        }
//        else
//        {
//            nameDisplay.Set(name);

//            //visibilityToggle.Initialize(this.index);

//            UIManager.Instance.modelVisibilityToggleList.Add(butttonIndex, visibilityToggle);

//            //lockToggle.Initialize(this.index);

//            UIManager.Instance.modelLockToggleList.Add(butttonIndex, lockToggle);

//            //nameDisplay.Initialize(name);


//        }

//        //  return Task.CompletedTask;


//    }

//    public void SignalModelLibraryLoading()
//    {
//        downloadButton.transform.parent.parent.gameObject.SetActive(false);
//        //      nameDisplay.gameObject.SetActive(true);
//        //  nameDisplay.Set
//        //  StartCoroutine(ReturnToFunctionalInputUseAfterSeconds(2, "Loading..."));
//    }
//    public void SignalModelLibraryLoadingEnd()
//    {
//        downloadButton.transform.parent.parent.gameObject.SetActive(true);
//    }

//    public async Task ReturnToFunctionalInputUseAfterSeconds(int seconds, string message, UnityAction onAssetLoadingFinished)
//    {
//        //if (onAssetLoadingFinished == null)
//        //{
//        //    await Task.Delay(seconds * 1000);
//        //    return;

//        //}




//        inputURL.gameObject.SetActive(false);
//        downloadButton.transform.parent.gameObject.SetActive(false);
//        nameDisplay.gameObject.SetActive(true);
//        nameDisplay.Set(message);
//        // isWholeObject.gameObject.SetActive(false);

//        await Task.Delay(seconds * 1000);
//        //  yield return new WaitForSeconds(seconds);
//        // await System.Threading.Tasks.Task.Delay(seconds * 1000);


//        inputURL.gameObject.SetActive(true);
//        nameDisplay.gameObject.SetActive(false);

//        lockToggle.transform.parent.gameObject.SetActive(false);
//        isWholeObject.gameObject.SetActive(true);
//        downloadButton.gameObject.SetActive(true);





//            downloadButton.transform.parent.parent.gameObject.SetActive(true);
//        downloadButton.transform.parent.gameObject.SetActive(true);




//        onAssetLoadingFinished?.Invoke();

//        if(onAssetLoadingFinished == null)
//          InstantiateAssetCards.Instance.loadScreen.gameObject.SetActive(false);
//        //if (newInstance)
//        //    Destroy(newInstance);
//        // SignalModelLibraryLoadingEnd();
//        // downloadButton.transform.parent.parent.gameObject.SetActive(false);

//    }


//    public bool IsNotEmpty(string url)
//    {


//        if (string.IsNullOrEmpty(url) || string.IsNullOrWhiteSpace(url))
//        {
//            return false;
//            // throw new System.Exception("model name cannot be empty.");
//        }
//        else
//            return true;

//    }

//    public bool IsValidGLTFExtension(string url)
//    {

//        string extension = null;

//        //to check for illigal characters
//        try
//        {
//            extension = Path.GetExtension(url);
//        }
//        catch
//        {
//            return false;
//        }

//        //to check if there is no extension given
//        if (extension == null)
//            return false;

//        if (extension.ToLower() == ".glb" || extension.ToLower() == ".gltf")
//        {
//            return true;
//            // throw new System.Exception("model name cannot be empty.");
//        }
//        else
//            return false;
//    }



//    public static bool IsValidAbsoluteUri(string url)
//    {
//        return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
//               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
//    }





//}
////}