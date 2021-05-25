using Komodo.Runtime;
using UnityEngine;

namespace Komodo.IMPRESS
{
    public class TriggerUnlink : MonoBehaviour
    {
        private LinkedGroup currentLinkedGroup;
        public KomodoControllerInteraction LcontrollerInteraction;
        public KomodoControllerInteraction RcontrollerInteraction;

        public void OnTriggerEnter(Collider collider)
        {
            GroupManager.Instance.RemoveFromLinkedGroup(collider);
            //if (collider.CompareTag("Interactable"))
            //{
                //if (collider.TryGetComponent(out LinkedGroup lg))
                //{
                //    currentLinkedGroup = lg;

                //    //disable main collider and enable the child elements to remove from main
                //    lg.transform.GetComponent<BoxCollider>().enabled = false;

                //    foreach (Transform colItems in lg.transform.GetChild(0))
                //        colItems.GetComponent<Collider>().enabled = true;
                //}

                ////handle the items that we touch
                //if (currentLinkedGroup)
                //{
                //    if (collider.transform.IsChildOf(currentLinkedGroup.transform.GetChild(0)))
                //    {
                //        collider.transform.parent = null;
                //        currentLinkedGroup.uniqueIdToParentofLinks.Remove(collider);

                //        currentLinkedGroup.RefreshLinkedGroup();

                //        if (currentLinkedGroup.transform.GetChild(0).childCount == 0)
                //        {
                //            ////RE-enable our grabing funcionality by droping destroyed object
                //            if (LcontrollerInteraction.currentTransform != null)
                //                if (LcontrollerInteraction.currentTransform.GetInstanceID() == currentLinkedGroup.transform.GetInstanceID())
                //                {
                //                    LcontrollerInteraction.Drop();
                //                    //   LcontrollerInteraction.PickUp(collider.transform);
                //                    //   collider.transform.SetParent(linkCollectionParent, true);
                //                }
                //            if (RcontrollerInteraction.currentTransform != null)
                //                if (RcontrollerInteraction.currentTransform.GetInstanceID() == currentLinkedGroup.transform.GetInstanceID())
                //                {
                //                    RcontrollerInteraction.Drop();
                //                    //     RcontrollerInteraction.PickUp(collider.transform);
                //                    //   collider.transform.SetParent(linkCollectionParent, true);
                //                }


                //            Destroy(currentLinkedGroup.transform.GetChild(0).gameObject);
                //            Destroy(currentLinkedGroup.gameObject);

                //        //    currentLinkedGroup.uniqueIdToParentofLinks.Remove(0);

                //        }


                //    }
                //}
         //   }
        }

        //call this from button to enable boundingbox rendering when it is turned on
        public void SetEnable()
        {
            if (currentLinkedGroup)
                currentLinkedGroup.GetComponent<MeshRenderer>().enabled = true;
        }
        public void SetDisable()
        {
            if (currentLinkedGroup)
            {

                currentLinkedGroup.transform.GetComponent<BoxCollider>().enabled = true;

                foreach (Transform colItems in currentLinkedGroup.transform.GetChild(0))
                    colItems.GetComponent<BoxCollider>().enabled = false;

                currentLinkedGroup.GetComponent<MeshRenderer>().enabled = false;

                currentLinkedGroup = null;
            }
        }

        //public void OnTriggerExit(Collider collider)
        //{
        //    if (collider.CompareTag("Interactable"))
        //    {
        //        if (collider.TryGetComponent(out LinkedGroup lg))
        //        {
        //            //disable main collider and enable the child elements to remove from main
        //            lg.transform.GetComponent<BoxCollider>().enabled = true;

        //            foreach (Transform colItems in lg.transform.GetChild(0))
        //                colItems.GetComponent<BoxCollider>().enabled = false;
        //        }


        //    }
        //}
    }
}