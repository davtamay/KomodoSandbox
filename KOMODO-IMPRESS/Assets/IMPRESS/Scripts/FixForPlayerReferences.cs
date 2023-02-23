using UnityEngine;
using Komodo.Runtime;

namespace Komodo.IMPRESS
{
    public class FixForPlayerReferences : MonoBehaviour
    {
        public void Awake ()
        {
            ControllersManager.Initialize();
        }

        public void OnDestroy ()
        {
            ControllersManager.Deinitialize();
        }
    }
}