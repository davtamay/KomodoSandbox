// University of Illinois/NCSA
// Open Source License
// http://otm.illinois.edu/disclose-protect/illinois-open-source-license

// Copyright (c) 2020 Grainger Engineering Library Information Center.  All rights reserved.

// Developed by: IDEA Lab
//               Grainger Engineering Library Information Center - University of Illinois Urbana-Champaign
//               https://library.illinois.edu/enx

// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal with
// the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to
// do so, subject to the following conditions:
// * Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimers.
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimers in the documentation
//   and/or other materials provided with the distribution.
// * Neither the names of IDEA Lab, Grainger Engineering Library Information Center,
//   nor the names of its contributors may be used to endorse or promote products
//   derived from this Software without specific prior written permission.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
// CONTRIBUTORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS WITH THE
// SOFTWARE.

using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Komodo.AssetImport;
using System;
using UnityEngine.Events;
using System.Threading.Tasks;

//namespace Komodo.Runtime
//{
    public class ModelButtonList : ButtonList
    {
        public ModelDataTemplate modelData;
        public GameObject placeHolderInputTemplate;

        private EntityManager entityManager;

        public bool isInitiateOnStart = true;

        //private ModelItem currentPlaceholderModelItem;
        //public ModelItem GetCurrentPlaceholderModelItem () => currentPlaceholderModelItem;

        public UnityAction<ModelItem> onPlaceholderModelItemChanged;
        //public  void Awake()
        //{ 


        //}
        //public void Start()
        //{
        //    if (isInitiateOnStart)
        //        Instantiate(null,null);
        //}

        public void Instantiate(UnityAction onModelClick, UnityAction onModelLoaded)
        {

            InstantiateList(onModelClick, onModelLoaded);

        }
        public async void InstantiateList(UnityAction onModelClick, UnityAction onModelLoaded)
        {
            //entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            //check if we should set up models
            if (!ModelImportInitializer.IsAlive)
            {
                gameObject.SetActive(false);

                return;
            }
            else
            {
                gameObject.SetActive(true);
            }

            //create input box for importing custom imports

            //onPlaceholderModelItemChanged?.Invoke(await InstantiatePlaceHolderButton( new ModelData(), -1, true, true, onModelClick, onModelLoaded));
            ModelItem modelItem = await InstantiatePlaceHolderButton(new ModelData(), -1, true, true, onModelClick, onModelLoaded);
            onPlaceholderModelItemChanged?.Invoke(modelItem);

            //yield return new WaitUntil(() => GameStateManager.Instance.isAssetImportFinished);

            //InitializeButtons();

            //NotifyIsReady();



        }

        //public struct ModelDetails
        //{
        //   public string url;
        //   public string name;
        //    public float scale;
        //    public bool isWhole,


        //    public UnityAction onAssetClicked;
        //    public UnityAction onAssetLoadedCallback, 

        //    bool isFromModelLibrary, bool net_call = true, Vector3 pos = default, Quaternion rot = default

        //}
        public async Task<ModelItem> InstantiateNewAssetToList(ModelData modelData, bool isWhole, UnityAction onAssetClicked, UnityAction onAssetLoadedCallback, bool isFromModelLibrary, bool net_call = true)
        {
            //this means we cant depend on the button index since it is based on the user index --> need to only use guid
            var buttonIndex = ModelImportInitializer.Instance.GetRoot().transform.childCount;

            //   ModelItem modelItem = InstantiatePlaceHolderButton(url, ModelImportInitializer.Instance.GetRoot().transform.childCount, false, modelData.scale, isWhole, onAssetClicked, onAssetLoadedCallback, isFromModelLibrary, net_call, pos, rot);
            ModelItem modelItem = await InstantiatePlaceHolderButton(modelData, buttonIndex, false, isWhole, onAssetClicked, onAssetLoadedCallback, isFromModelLibrary, net_call);

            modelItem.inputURL.text = modelData.modelURL;

            modelItem.nameDisplay.Set(name);

            modelItem.downloadButton.onClick.Invoke();

            return modelItem;

        }





        protected override void InitializeButtons()
        {
            //   Debug.Log("initializing buttons");
            if (!transformToPlaceButtonUnder)
            {
                transformToPlaceButtonUnder = transform;
            }

            if (!modelData)
            {
                throw new UnassignedReferenceException("modelData on ModelButtonList");
            }

            if (modelData.models == null)
            {
                throw new System.Exception("expected modelData to have models, but it was null");
            }


            for (int i = 0; i < modelData.models.Count; i++)
            {

                if (UIManager.IsAlive)
                {
                    GameObject item = Instantiate(buttonTemplate, transformToPlaceButtonUnder);

                    if (item.TryGetComponent(out ModelItem modelItem))
                    {
                        string name = modelData.models[i].name;
                        Debug.Log("initializing buttons :" + modelItem);
                        if (name == null)
                        {
                            Debug.LogError($"modelData.models[{i}].name was null. Proceeding anyways.");

                            name = "null";
                        }

                        //    modelItem.Initialize(i, modelData.models[i].name);
                    }
                    else
                    {
                        throw new MissingComponentException("modelItem on GameObject (from ModelButtonTemplate)");
                    }
                }
            }
        }
        //  public void InstantiateNewAssetToList(ModelData modelData, bool isWhole, UnityAction onAssetClicked, UnityAction onAssetLoadedCallback, bool isFromModelLibrary, bool net_call = true)
        // public void InstantiateNewAssetToList(string url, string name, float scale, bool isWhole, UnityAction onAssetClicked, UnityAction onAssetLoadedCallback, bool isFromModelLibrary, bool net_call = true, Vector3 pos = default, Quaternion rot = default)


        public async Task<ModelItem> InstantiatePlaceHolderButton(ModelData modelData, int buttonIndex, bool addNewPlaceHolderAfterClick, bool isWhole, UnityAction onAssetClicked, UnityAction onAssetLoadedCallback, bool isFromModelLibrary = false, bool netCall = true)
        {
            var tcs = new TaskCompletionSource<ModelItem>();

            GameObject item = Instantiate(placeHolderInputTemplate, transformToPlaceButtonUnder);

            if (item.TryGetComponent(out ModelItem modelItem))
            {
                //create new custom import box after inputing and adding custom asset
                Action<string, int> addPlaceHolderAfterUsed = async (s, i) =>
                {
                    if (addNewPlaceHolderAfterClick)
                        onPlaceholderModelItemChanged?.Invoke(await InstantiatePlaceHolderButton(modelData, -1, true, true, onAssetClicked, onAssetLoadedCallback, false, netCall));
                };



               await modelItem.Initialize(modelData.modelURL, buttonIndex, isWhole, true, addPlaceHolderAfterUsed, onAssetClicked, onAssetLoadedCallback, isFromModelLibrary, netCall);

                // modelItem.Initialize(buttonIndex, modelItem.inputURL.text, scale, isWhole, true, addPlaceHolderAfterUsed, onAssetClicked, onAssetLoadedCallback, isFromModelLibrary, netCall, pos, rot);
                tcs.SetResult(modelItem);
               
                return tcs.Task.Result;
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
//}