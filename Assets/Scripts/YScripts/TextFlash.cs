using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextFlash : MonoBehaviour
{
    public float t = 0;

    public int sign = 1;

    public int duration = 255;

    public Material textMat;

    private float textAlbedo;
    private Color textColor;
    
    // Start is called before the first frame update
    void Start()
    {
        textColor = gameObject.GetComponent<TextMeshProUGUI>().color;
    }

    // Update is called once per frame
    void Update()
    {

        t += sign*Time.deltaTime;

        if (t >= duration)
        {
            sign = -1;
        }
        else if (t <= 0)
        {
            sign = 1;
        }

        textAlbedo = t;
        textColor = new Color(textColor.r, textColor.g, textColor.b, textAlbedo);

        gameObject.GetComponent<TextMeshProUGUI>().faceColor = textColor;
    }
}
