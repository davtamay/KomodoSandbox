using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.AssetImport;
using Komodo.Runtime;
using Komodo.Utilities;
using System.Threading.Tasks;
using System;
using GLTFast.Logging;

public class AddToModelList : MonoBehaviour, ICodeLogger
{
    GameObject root;

    //[System.Serializable]
    private ModelImportData mID;


    public string name;
    //  public int id;
    public string thisUrl;

    public float scale = 1;
    public Vector3 position;
    public Vector3 euler_rotation;

    public bool isWholeObject = true;

    [ShowOnly]
    public int indexInList;


    public Action<string, int> onFinishLoading;
    public Action<string> onImportAttempted;



    public static int inputButtonOffset;

    //public void Awake()
    //{

    //    index = ModelImportInitializer.Instance.modelData.models.Count + 1;//inputButtonOffset;
    //   // inputButtonOffset += 1;
    //    // yield return null;

    //}
    public void SetIndex(int index)
    {
        this.indexInList = index;

    }


    public void SetFailed(string message)
    {
        Debug.Log(message);

      //  onFinishLoading?.Invoke(message);
    }
    //public void Setup(bool wasSuccesful)
    //{


    //    SetupAfterList(wasSuccesful);
    //}

    public static class TaskUtils
    {


        public static async Task WaitUntil(Func<bool> predicate, int sleep = 50)
        {
            while (!predicate())
            {


                await Task.Delay(sleep);
            }

        }
    }
    public  void Setup(bool wasSuccesful, string url, bool isNetCall = true)
    {
        thisUrl = url;
       // if(InstantiateAssetCards.Instance.urlToModelAssetCardDictionary.ContainsKey(url))
        var modelData = InstantiateAssetCards.Instance.urlToModelAssetCardDictionary[url].modelData;

        // yield return new WaitUntil(() => GameStateManager.Instance.isAssetImportFinished );

        if (!wasSuccesful)
        {
            onImportAttempted?.Invoke("Import Failed");
        //    onFinishLoading?.Invoke("Import Failed");
            return;
        }

            //take care of the initual input box for import -- we have to set the index later incase other assets are setup through model library
        if (indexInList == -1)
        {
            indexInList = ModelImportInitializer.Instance.GetRoot().transform.childCount;
        }

        // await TaskUtils.WaitUntil(() => GameStateManager.Instance.isAssetImportFinished);

        mID = new ModelImportData();
        mID.name = modelData.modelName;//name;
       // modelData.guid = indexInList;
        mID.buttonID = indexInList;
        mID.url = modelData.modelURL;//url;


        mID.guid = modelData.guid;

        //thisUrl = url;
        mID.scale = modelData.scale;
        mID.position = modelData.pos;
        mID.euler_rotation = euler_rotation;
        mID.isWholeObject = modelData.isWholeObject;

        Debug.Log("URL : " + modelData.modelURL);
        Debug.Log("GUID : " + modelData.guid);

        root = ModelImportInitializer.Instance.GetRoot();



        //make a button id association
        if (UIManager.Instance.entityIDtoButtonIDDictionary.ContainsKey(indexInList))
        {
            Debug.Log(modelData.guid + "--- this entitiy is already attached to button: " + indexInList);

        }
        else
        {
            UIManager.Instance.entityIDtoButtonIDDictionary.Add(modelData.guid, indexInList);

            if (!UIManager.Instance.buttonIDtoEntityIDDictionary.ContainsKey(indexInList))
                UIManager.Instance.buttonIDtoEntityIDDictionary.Add(indexInList, modelData.guid);

        }






        //we initiate this in the post process of our gltfimport;
        Debug.Log("SETUP GO, " + indexInList  + "   " + "with whole object" + mID.isWholeObject);
        GameObject komodoImportedModel = ModelImportPostProcessor.Instance.SetUpGameObject(indexInList, mID, gameObject);
        //ModelImportInitializer.Instance.modelButtonList.InstantiateButton(mID.name, index);  

        komodoImportedModel.transform.SetParent(root.transform, false);

        if (isNetCall)
        {
            //find the enabled camera
            Camera[] allCameras = ClientSpawnManager.Instance.mainPlayer_AvatarEntityGroup.transform.GetChild(0).GetComponentsInChildren<Camera>(true);
          //  Debug.Log("TOTAL CAMERAS " + allCameras.Length);
            foreach (var item in allCameras)
            {
                if (item.enabled && item.gameObject.activeInHierarchy)
                {
                 //   Debug.Log("CAMERA ON " + item.gameObject.name);
                    komodoImportedModel.transform.position = item.transform.TransformPoint(Vector3.forward * 1.2f);
                    Vector3 direction = item.transform.position - komodoImportedModel.transform.position;
                    direction.y = 0; // set the y-component of the direction vector to 0
                    Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
                    komodoImportedModel.transform.rotation = rotation;


                    // komodoImportedModel.transform.rotation = Quaternion.LookRotation(komodoImportedModel.transform.position - item.transform.position, Vector3.up);
                    break;
                }
            }

            NotifyOthersOfClick(komodoImportedModel.transform.position, komodoImportedModel.transform.rotation);

        }
        else
        {
            komodoImportedModel.transform.position = modelData.pos;
            komodoImportedModel.transform.rotation = modelData.rot;

        }

        GameStateManager.Instance.isAssetImportFinished = true;
        onFinishLoading?.Invoke("", indexInList);
       
    }

    public void NotifyOthersOfClick(Vector3 pos, Quaternion rot)
    {
       
        var data = InstantiateAssetCards.Instance.urlToModelAssetCardDictionary[thisUrl].modelData;

        Debug.Log("SEND URL: " + data.modelURL);
        data.pos= pos;
        data.rot= rot;

        Debug.Log("Sending: " + data.guid);
        //    ModelData data = new ModelData { modelName = name, modelURL = thisUrl, scale = scale, pos =pos, rot = rot};

        KomodoMessage ms = new KomodoMessage("asset", JsonUtility.ToJson(data));
        ms.Send();

       // Debug.Log("sending");
        


    }

    public void Error(LogCode code, params string[] messages)
    {
        SetFailed(messages[0]);
    }

    public void Warning(LogCode code, params string[] messages)
    {
       // SetFailed(messages[0]);
    }

    public void Info(LogCode code, params string[] messages)
    {
        //SetFailed(messages[0]);
    }

    public void Error(string message)
    {
        SetFailed(message);
    }

    public void Warning(string message)
    {
        SetFailed(message);
    }

    public void Info(string message)
    {
        SetFailed(message);
        //throw new NotImplementedException();
    }
}
