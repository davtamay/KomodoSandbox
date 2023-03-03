using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Komodo.Utilities;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using GLTFast.Logging;
using UnityEngine.Networking;
using UnityEngine.Events;

namespace Komodo.Runtime
{
    public class ModelItem : MonoBehaviour
    {
        [ShowOnly]
        public int index;

        public VisibilityToggle visibilityToggle;

        public LockToggle lockToggle;

        public TMP_InputField inputURL;
        public Button downloadButton;
        public Toggle isWholeObject;

        public ModelNameDisplay nameDisplay;

        public string inputURLText;

        GameObject newInstance;



        public async void Initialize(int index, String name, float scale = 1, bool isDownloadPlaceHolder = false, Action<string, int> onPlaceHolderUsed = null, UnityAction onAssetLoaded = null)
        {
            nameDisplay.Initialize("");


            visibilityToggle.Initialize(index);
            lockToggle.Initialize(index);
            this.index = index;

            if (isDownloadPlaceHolder)
            {

                downloadButton.onClick.AddListener(delegate ()
                {
                downloadButton.transform.parent.parent.gameObject.SetActive(false);

                AddToModelList model;
                KomodoGLTFAssetV5 importInstance;

                if (newInstance == null)
                {
                    newInstance = new GameObject("placeholder");

                }


                if (newInstance.TryGetComponent(out AddToModelList currentModel))
                {
                    model = currentModel;
                    importInstance = currentModel.GetComponent<KomodoGLTFAssetV5>();


                }
                else
                {

                    model = newInstance.AddComponent<AddToModelList>();
                    model.SetIndex(index);
                        model.scale = scale;

                    importInstance = newInstance.AddComponent<KomodoGLTFAssetV5>();

                    model.onImportAttempted += (Message) =>
                    {
                      StartCoroutine( ReturnToFunctionalInputUseAfterSeconds(2, Message));


                    };


                }




                    if (!string.IsNullOrEmpty(inputURL.text))
                    {
                        if (!IsValidGLTFExtension(inputURL.text))
                        {
                             model.onImportAttempted("No .glb/.gltf extension provided");
                            return;

                        }

                        try
                        {
                            UnityWebRequest.Get(inputURL.text);

                        }
                        catch
                        {
                            model.onImportAttempted("Error importing file check url");
                            return;

                        }
                    }
                    else
                    {
                        model?.onImportAttempted("No Input Provided");
                        return;
                    }


                    importInstance.TryImport(inputURL.text);



                    model.onFinishLoading += (importSuccess) =>
                    {
                        if (importSuccess == "")
                        {
                            onAssetLoaded?.Invoke();

                            UIManager.Instance.modelVisibilityToggleList.Add(visibilityToggle);
                            UIManager.Instance.modelLockToggleList.Add(lockToggle);
                            visibilityToggle.Toggle(true);

                            lockToggle.transform.parent.gameObject.SetActive(true);

                            nameDisplay.Set(Path.GetFileName(model.url.TrimEnd('\\')));
                            //  Path.GetFileName( model.url.TrimEnd('\\'))

                            onPlaceHolderUsed?.Invoke(nameDisplay.GetName(), index);



                        }

                    };

                    model.isWholeObject = !isWholeObject.isOn;
                    model.url = inputURL.text;
                    downloadButton.gameObject.SetActive(false);
                    isWholeObject.gameObject.SetActive(false);
                    inputURL.gameObject.SetActive(false);

                    nameDisplay.Initialize("LOADING...");
                    nameDisplay.gameObject.SetActive(true);
                    // lockToggle.transform.gameObject.SetActive(true);
                    //model.Setup();

                    //  index = model.index;




                });

                return;
            }
            else
            {
                nameDisplay.Set(name);

                visibilityToggle.Initialize(this.index);

                UIManager.Instance.modelVisibilityToggleList.Add(visibilityToggle);

                //lockToggle.Initialize(this.index);

                UIManager.Instance.modelLockToggleList.Add(lockToggle);

                //nameDisplay.Initialize(name);


            }




        }

        public IEnumerator ReturnToFunctionalInputUseAfterSeconds(int seconds, string message)
        {



            inputURL.gameObject.SetActive(false);
            downloadButton.transform.parent.gameObject.SetActive(false);
            nameDisplay.gameObject.SetActive(true);
            nameDisplay.Set(message);
            // isWholeObject.gameObject.SetActive(false);

            yield return new WaitForSeconds(seconds);
           // await System.Threading.Tasks.Task.Delay(seconds * 1000);


            inputURL.gameObject.SetActive(true);
            nameDisplay.gameObject.SetActive(false);


            downloadButton.transform.parent.parent.gameObject.SetActive(true);
            downloadButton.transform.parent.gameObject.SetActive(true);


            lockToggle.transform.parent.gameObject.SetActive(false);
            isWholeObject.gameObject.SetActive(true);
            downloadButton.gameObject.SetActive(true);

            if (newInstance)
                Destroy(newInstance);

        }


        public bool IsNotEmpty(string url)
        {


            if (string.IsNullOrEmpty(url) || string.IsNullOrWhiteSpace(url))
            {
                return false;
                // throw new System.Exception("model name cannot be empty.");
            }
            else
                return true;

        }

        public bool IsValidGLTFExtension(string url)
        {

            string extension = null;

            //to check for illigal characters
            try
            {
                extension = Path.GetExtension(url);
            }
            catch
            {
                return false;
            }

            //to check if there is no extension given
            if (extension == null)
                return false;

            if (extension.ToLower() == ".glb" || extension.ToLower() == ".gltf")
            {
                return true;
                // throw new System.Exception("model name cannot be empty.");
            }
            else
                return false;
        }

    }
}