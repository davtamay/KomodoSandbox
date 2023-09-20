using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using Komodo.Utilities;
using Komodo.Runtime;
using System.Collections;

//namespace Komodo.IMPRESS
//{
    public class TriggerCreatePrimitive : MonoBehaviour
    {
        public void OnEnable()
        {
            if(CreatePrimitiveManager.Instance)
            //only create when our cursor is Off
            if (UIManager.IsAlive && UIManager.Instance.GetCursorActiveState()) {
                return;
            }

            if (!CreatePrimitiveManager.Instance.GetPrimitiveUpdateStatus())
                return;

            CreatePrimitiveManager.Instance.CreatePrimitive();
        //    StartCoroutine(CreatePrimitiveAfterCheckingState());

           
        }

        //public IEnumerator CreatePrimitiveAfterCheckingState()
        //{
        //    yield return null;
         
        //    if (!CreatePrimitiveManager.Instance.GetPrimitiveUpdateStatus())
        //        yield break;

        //    CreatePrimitiveManager.Instance.CreatePrimitive();
        //}
    }
//}
