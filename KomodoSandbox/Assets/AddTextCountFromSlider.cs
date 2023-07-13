using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class AddTextCountFromSlider : MonoBehaviour
{


    public TextMeshProUGUI textMeshProUGUI;
    public Slider thisSlider;
    public string text;

    public void Awake()
    {
        thisSlider = GetComponent<Slider>();
        thisSlider.onValueChanged.AddListener(

            (value) =>
            {
                textMeshProUGUI.text = text + " " + value;

            });
            
    }


}
