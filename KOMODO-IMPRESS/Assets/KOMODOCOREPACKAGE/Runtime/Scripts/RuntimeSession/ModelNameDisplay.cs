using System;
using UnityEngine;
using UnityEngine.UI;

namespace Komodo.Runtime
{
    [RequireComponent(typeof(Text))]
    public class ModelNameDisplay : MonoBehaviour
    {
        private Text display;

        public string GetName()=>display.text;

        public void Initialize (String name)
        {
            display = GetComponent<Text>();

            this.display.text = "...";

            this.display.text = name;
        }

        public void Set(String text){

            display.text = text;
        }
    }
}