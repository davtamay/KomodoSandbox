using System.Collections.Generic;
using UnityEngine;
using Komodo.Utilities;

public class ImpressTabManager : MonoBehaviour
{
    public List<Alternate_Button_Function> buttonList = new List<Alternate_Button_Function>();

    public void Start()
    {
        foreach (var item in buttonList)
        {
            item.onFirstClick.AddListener(() => SetButtonAndUncheckOthers(item)); 
        }
           
    }
    public void SetButtonAndUncheckOthers(Alternate_Button_Function selectedButton)
    {
        foreach (var item in buttonList)
        {
            if (item.GetInstanceID() != selectedButton.GetInstanceID())
            {
                item.onSecondClick.Invoke();
                item.isFirstClick = false;

            }
        }

    }
}
