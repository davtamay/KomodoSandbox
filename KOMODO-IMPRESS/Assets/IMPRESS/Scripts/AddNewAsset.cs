// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Komodo.Runtime;
// using Komodo.AssetImport;


// namespace Komodo.IMPRESS
// {
//     public class AddNewAsset : MonoBehaviour
//     {
//         public GameObject modelList;


//             // public string name;
//             // public int id;
//             // public string url;

//             // public float scale;
//             // public Vector3 position;
//             // public Vector3 euler_rotation;

//             // public bool isWholeObject;

//         //public List<ModelFile> localFiles;
//         //public ModelImportInitializer.Instance.MOD loader;
//         public void Awake(){


//         }
//         public IEnumerator AddModel(){

//           //  new ModelDataTemplate();

//             // modelList.transform.SetParent(transform, false);
//  //yield return ModelImportInitializer.Instance.loader.TryLoadLocalFile(localFiles[i].location, localFiles[i].name, localFiles[i].size, progressDisplay, gObject =>
//                 {
//                     //WebGLMemoryStats.LogMoreStats($"ModelImportPostProcessor.SetUpGameObject {model.name} BEFORE");

//                     GameObject komodoImportedModel = ModelImportPostProcessor.SetUpGameObject(menuIndex, model, gObject, settings ?? null);

//                     //WebGLMemoryStats.LogMoreStats($"ModelImportPostProcessor.SetUpGameObject {model.name} AFTER");

//                     komodoImportedModel.transform.SetParent(list.transform, false);

//                     modelsToInstantiate -= 1;
//                 });
        
            
//         }
//         public IEnumerator RetrieveModelFiles()
//         {
//             Text text = UIManager.Instance.initialLoadingCanvasProgressText;

//             for (int i = 0; i < modelData.models.Count; i += 1 )
//             {
//                 //Debug.Log($"retrieving model #{i}");

//                 int menuIndex = i;

//                 var model = modelData.models[i];
//                 VerifyModelData(model);

//                 progressDisplay.text = $"{model.name}: Retrieving";

//                 //download or load our model
//                 yield return loader.GetFileFromURL(model, progressDisplay, menuIndex, localFile => {
//                     localFiles.Add(localFile);
//                     modelsToRetrieve -= 1;
//                 });
//             }
//         }

//         public IEnumerator InstantiateModels()
//         {
//             for (int i = 0; i < localFiles.Count; i += 1 )
//             {
//                 int menuIndex = i;

//                 var model = modelData.models[i];
//                 VerifyModelData(model);

//                 yield return loader.TryLoadLocalFile(localFiles[i].location, localFiles[i].name, localFiles[i].size, progressDisplay, gObject =>
//                 {
//                     //WebGLMemoryStats.LogMoreStats($"ModelImportPostProcessor.SetUpGameObject {model.name} BEFORE");

//                     GameObject komodoImportedModel = ModelImportPostProcessor.SetUpGameObject(menuIndex, model, gObject, settings ?? null);

//                     //WebGLMemoryStats.LogMoreStats($"ModelImportPostProcessor.SetUpGameObject {model.name} AFTER");

//                     komodoImportedModel.transform.SetParent(list.transform, false);

//                     modelsToInstantiate -= 1;
//                 });
//             }
//         }
//     }
// }
