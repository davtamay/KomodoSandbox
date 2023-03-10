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
    public string url;

    public float scale = 1;
    public Vector3 position;
    public Vector3 euler_rotation;

    public bool isWholeObject = true;

    [ShowOnly]
    public int index;


    public Action<string> onFinishLoading;
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
        this.index = index;

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
    public async void Setup(bool wasSuccesful)
    {
   
        // yield return new WaitUntil(() => GameStateManager.Instance.isAssetImportFinished );

        if (!wasSuccesful)
        {
            onImportAttempted?.Invoke("Import Failed");
        //    onFinishLoading?.Invoke("Import Failed");
            return;
        }

            //take care of the initual input box for import -- we have to set the index later incase other assets are setup through model library
        if (index == -1)
        {
            index = ModelImportInitializer.Instance.GetRoot().transform.childCount;
        }

        await TaskUtils.WaitUntil(() => GameStateManager.Instance.isAssetImportFinished);

        mID = new ModelImportData();
        mID.name = name;
        mID.id = index;
        mID.url = url;
        mID.scale = scale;
        mID.position = position;
        mID.euler_rotation = euler_rotation;
        mID.isWholeObject = isWholeObject;


        root = ModelImportInitializer.Instance.GetRoot();


        //we initiate this in the post process of our gltfimport;
        Debug.Log("SETUP GO, " + index  + "   " + "with whole object" + mID.isWholeObject);
        GameObject komodoImportedModel = ModelImportPostProcessor.SetUpGameObject(index, mID, gameObject);
        //ModelImportInitializer.Instance.modelButtonList.InstantiateButton(mID.name, index);  

        komodoImportedModel.transform.SetParent(root.transform, false);

        //find the enabled camera
        Camera[] allCameras = ClientSpawnManager.Instance.mainPlayer_AvatarEntityGroup.transform.GetChild(0).GetComponentsInChildren<Camera>(true);
        Debug.Log("TOTAL CAMERAS " + allCameras.Length);
        foreach (var item in allCameras)
        {
            if (item.enabled && item.gameObject.activeInHierarchy)
            {
                Debug.Log("CAMERA ON " + item.gameObject.name);
                komodoImportedModel.transform.position = item.transform.TransformPoint(Vector3.forward * 1.2f);
                Vector3 direction = item.transform.position - komodoImportedModel.transform.position;
                direction.y = 0; // set the y-component of the direction vector to 0
                Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
                komodoImportedModel.transform.rotation = rotation;


                // komodoImportedModel.transform.rotation = Quaternion.LookRotation(komodoImportedModel.transform.position - item.transform.position, Vector3.up);
                break;
            }
        }
        //foreach (Transform item in ClientSpawnManager.Instance.mainPlayer_AvatarEntityGroup.transform.GetChild(0))
        //{
        //    if (item.gameObject.activeInHierarchy)
        //    {
        //        //   Debug.Log("item " + item.gameObject.name, item.gameObject);
        //        komodoImportedModel.transform.position = item.TransformPoint(Vector3.forward * 1.2f);//item.GetChild(0).position; 

        //        //  item.GetChild(0).TransformPoint(Vector3.forward * 0.2f);
        //        // komodoImportedModel.transform.LookAt(item.GetChild(0).position);
        //         komodoImportedModel.transform.rotation = Quaternion.LookRotation(komodoImportedModel.transform.position - item.position, Vector3.up);
        //        //  komodoImportedModel.transform.localRotation.SetLookRotation(item.GetChild(0).position); // = Quaternion.LookRotation(item.GetChild(0).position, Vector3.up);
        //        //komodoImportedModel.transform.rotation = item.GetChild(0).localEulerAngles.y; //Quaternion.Euler(item.GetChild(0).TransformDirection(Vector3.forward));
        //        break;
        //    }
        //}
        //komodoImportedModel.transform.position =  ClientSpawnManager.Instance.mainPlayer_AvatarEntityGroup.gameObject.transform.TransformPoint(Vector3.forward);

        // if(!wasSuccesful)
        //           onFinishLoading?.Invoke("Import Failed");
        // else
        onFinishLoading?.Invoke("");
        //    await System.Threading.Tasks.Task.Delay;
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
