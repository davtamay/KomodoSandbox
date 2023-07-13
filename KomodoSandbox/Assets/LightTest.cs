using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightTest : MonoBehaviour
{
    public Light light;
    public void LightSelection(int lightSetting)
    {
        switch (lightSetting)
        {
            case 0:
                light.gameObject.SetActive(false);
                light.shadows = LightShadows.None;
                break;
            case 1:
                light.gameObject.SetActive(true);
                light.shadows = LightShadows.None;
                break;

            case 2:
                light.gameObject.SetActive(true);
                light.shadows = LightShadows.Soft;
                break;


        }

    }
}
