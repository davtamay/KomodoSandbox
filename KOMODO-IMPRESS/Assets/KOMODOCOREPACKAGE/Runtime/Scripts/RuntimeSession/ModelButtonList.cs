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

namespace Komodo.Runtime
{
    public class ModelButtonList : ButtonList
    {
        public ModelDataTemplate modelData;
        public GameObject placeHolderInputTemplate;

        private EntityManager entityManager;

        public override IEnumerator Start()
        {
            //entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            //check if we should set up models
            if (!ModelImportInitializer.IsAlive)
            {
                gameObject.SetActive(false);

                yield break;
            }
            else
            {
                gameObject.SetActive(true);
            }


            InstantiatePlaceHolderButton("", ModelImportInitializer.Instance.modelData.models.Count, true, 1.0f, null);

            yield return new WaitUntil(() => GameStateManager.Instance.isAssetImportFinished);

            InitializeButtons();

            NotifyIsReady();



        }

        public void InstantiateNewAssetToList(string url, string name, float scale, UnityAction onAssetLoadedCallback)
        {
          ModelItem modelItem =   InstantiatePlaceHolderButton("", ModelImportInitializer.Instance.GetRoot().transform.childCount, false, scale, onAssetLoadedCallback);


            modelItem.inputURL.text = url;

            modelItem.nameDisplay.Set(name);

            modelItem.downloadButton.onClick.Invoke();
    }

        protected override void InitializeButtons()
        {
            Debug.Log("initializing buttons");
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

                        modelItem.Initialize(i, modelData.models[i].name);
                    }
                    else
                    {
                        throw new MissingComponentException("modelItem on GameObject (from ModelButtonTemplate)");
                    }
                }
            }
        }


        public ModelItem InstantiatePlaceHolderButton(string name, int buttonIndex, bool addNewPlaceHolderAfterClick, float scale, UnityAction onAssetLoadedCallback)
        {
            //  yield return new WaitUntil(() => GameStateManager.Instance.isAssetImportFinished );
            // yield return null;
            // if (UIManager.IsAlive)
            // {
            GameObject item = Instantiate(placeHolderInputTemplate, transformToPlaceButtonUnder);

            if (item.TryGetComponent(out ModelItem modelItem))
            {

                //   Debug.Log("initializing buttons :" + modelItem);
                // if (name == null)
                // {
                //     Debug.LogError($"modelData.models[{buttonIndex}].name was null. Proceeding anyways.");

                //     name = "null";
                // }
                System.Action<string, int> addPlaceHolderAfterUsed = (s, i) => { if (addNewPlaceHolderAfterClick)

                        InstantiatePlaceHolderButton("", ModelImportInitializer.Instance.GetRoot().transform.childCount, true, scale, null
                            );
                        
                        };


                modelItem.Initialize(buttonIndex, modelItem.inputURL.text, scale, true, addPlaceHolderAfterUsed, onAssetLoadedCallback);

                return modelItem;
            }
            else
            {
                throw new MissingComponentException("modelItem on GameObject (from ModelButtonTemplate)");
            }

            //}
        }
        public void InstantiateButton(string name, int buttonIndex)
        {


            if (UIManager.IsAlive)
            {
                GameObject item = Instantiate(buttonTemplate, transformToPlaceButtonUnder);

                if (item.TryGetComponent(out ModelItem modelItem))
                {

                    //   Debug.Log("initializing buttons :" + modelItem);
                    // if (name == null)
                    // {
                    //     Debug.LogError($"modelData.models[{buttonIndex}].name was null. Proceeding anyways.");

                    //     name = "null";
                    // }

                    modelItem.Initialize(buttonIndex, name);
                }
                else
                {
                    throw new MissingComponentException("modelItem on GameObject (from ModelButtonTemplate)");
                }

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
}