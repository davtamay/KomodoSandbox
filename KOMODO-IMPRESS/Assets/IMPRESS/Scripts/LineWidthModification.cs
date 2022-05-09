using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineWidthModification : MonoBehaviour
{
    public LineRenderer LDraw;
    public LineRenderer RDraw;
    public Text displayLineWidth;

    // Start is called before the first frame update
    void Start()
    {
        displayLineWidth.text = LDraw.widthMultiplier.ToString("0.00") ;
    }

    public void ChangeLineWidth(float change)
    {
        var c = Mathf.Clamp(LDraw.widthMultiplier + change, 0.01f, 5f);

        LDraw.widthMultiplier = c;
        RDraw.widthMultiplier = c;
        displayLineWidth.text = c.ToString("0.00");


    }
}
