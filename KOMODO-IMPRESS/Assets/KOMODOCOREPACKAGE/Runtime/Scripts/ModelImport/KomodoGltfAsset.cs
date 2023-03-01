// Copyright 2020 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System.IO;
using System.Collections;
using UnityEngine;
using Komodo.AssetImport;
using GLTFast.Logging;
using GLTFast.Materials;
using GLTFast.Loading;
using System.Threading.Tasks;

namespace GLTFast
{
    public class KomodoGLTFAsset : GltfAssetBase
    {
        [Tooltip("file location to load the glTF from.")]
        public string location;

        private System.Action<GameObject> callback;

        private Loading.IDownloadProvider downloadProvider;

            GltfImport GLTFastInstance;

             /// <summary>
        /// Latest scene's instance.
        /// </summary>
        public GameObjectSceneInstance SceneInstance { get; protected set; }

// [SerializeField]
//         InstantiationSettings instantiationSettings;
         //private GLTFastInstance gLTFastInstance;

      protected override IInstantiator GetDefaultInstantiator(ICodeLogger logger)
        {
            return new GameObjectInstantiator(Importer, transform, logger);//, instantiationSettings);
        }
        // public void Load(string location, System.Action<GameObject> callback) {
        //     this.location = location;
        //     this.callback = callback;

        //     downloadProvider = new Loading.KomodoDownloadProvider();
        //     //WebGLMemoryStats.LogMoreStats("KomodoGLTFAsset.Load base.Load BEFORE");
        //     base.Load(location, downloadProvider);
        //     //WebGLMemoryStats.LogMoreStats("KomodoGLTFAsset.Load base.Load AFTER");
        // }
        
        /// <summary>
        /// Scene to load (-1 loads glTFs default scene)
        /// </summary>
        protected int SceneId => sceneId;
         [SerializeField]
        [Tooltip("Override scene to load (-1 loads glTFs default scene)")]
        int sceneId = -1;
        

        public async void LoadAsset(string url, System.Action<GameObject> callback){

            currentCallback = callback;

downloadProvider = new Loading.KomodoDownloadProvider();
            await Load(url, downloadProvider);
        }

        System.Action<GameObject> currentCallback;
 public override async Task<bool> Load(
            string gltfUrl, 
            IDownloadProvider downloadProvider = null,
            IDeferAgent deferAgent = null,
            IMaterialGenerator materialGenerator = null,
            ICodeLogger logger = null
            )
        {
            logger = logger ?? new ConsoleLogger();
            var success = await base.Load(gltfUrl, downloadProvider, deferAgent, materialGenerator, logger);
            if (success)
            {
                if (deferAgent != null) await deferAgent.BreakPoint();
                // Auto-Instantiate
                if (sceneId >= 0)
                {
                 //   success = await InstantiateScene(sceneId, logger);
                }
                else
                {
                    success = await Instantiate(logger);
                }
            }
            return success;
        }

        protected override void PostInstantiation(IInstantiator instantiator, bool success) {
            if (!success) {
                Debug.LogError("Error loading GLTF with GLTFast.", gameObject);
            }
            //currentCallback(gameObject.transform.GetChild(0).GetChild(0).gameObject);
            Debug.Log("komoddo",gameObject );



base.PostInstantiation(instantiator, success);
          //  StartCoroutine(Instantiate(gameObject));

            //base.OnLoadComplete(success);
        }

        public void Awake(){

            GLTFastInstance = new GltfImport();
        }

      public override void ClearScenes()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            SceneInstance = null;
        }

        /**
        * Ask GLTFast to instantiate a GameObject, wait for it to finish, 
        * and then call our model setup callback when it is finished.
        */
        public IEnumerator Instantiate (GameObject result) {

            yield return new WaitUntil ( () => {
                bool success = GLTFastInstance.InstantiateScene(result.transform);

                //Debug.Log($"Instantiate {gameObject.name}: {success}");

                return success;
            });

            if (callback == null) {
                Debug.LogWarning("No post-processing will be done on the imported model.");

                yield break;
            }

            callback(result);
        }
    }
}
