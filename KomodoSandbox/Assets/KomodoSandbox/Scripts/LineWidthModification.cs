//using Komodo.IMPRESS;
using Komodo.IMPRESS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineWidthModification : MonoBehaviour
{
    public LineRenderer LSelect;
    public LineRenderer RSelect;
    public LineRenderer LDraw;
    public LineRenderer RDraw;
    public Text displayLineWidth;

    // Start is called before the first frame update
    void Awake()
    {
        displayLineWidth.text = LDraw.widthMultiplier.ToString("0.00") ;



      

        float initialScale = LDraw.widthMultiplier * 1.2f; //* 2f;

        //LSelect.widthMultiplier = initialScale ;
        //RSelect.widthMultiplier = initialScale ;
        LDraw.widthMultiplier = initialScale;
        RDraw.widthMultiplier = initialScale;


        float adjustedScale = initialScale * 5;
        //LSelect.widthMultiplier = adjustedScale;
        //RSelect.widthMultiplier = adjustedScale;
        LDraw.widthMultiplier = adjustedScale;
        RDraw.widthMultiplier = adjustedScale;

        displayLineWidth.text = adjustedScale.ToString("0.00");

        WorldPulling.Instance.onChangeScale += (newScale)=> 
        {
            //newScale *= 1.2f ;
            float adjustedScale = initialScale * newScale;
            //LSelect.widthMultiplier = adjustedScale;
            //RSelect.widthMultiplier = adjustedScale;
            LDraw.widthMultiplier = adjustedScale;
            RDraw.widthMultiplier = adjustedScale;

          //  Debug.Log("CUSTOM SCALLING LINE " + newScale);
            displayLineWidth.text = adjustedScale.ToString("0.00");

        };
    }

    public void ChangeLineWidth(float change)
    {
        var c = Mathf.Clamp(LDraw.widthMultiplier + change, 0.01f, 20f);

        LDraw.widthMultiplier = c;
        RDraw.widthMultiplier = c;
        displayLineWidth.text = c.ToString("0.00");


    }
}
