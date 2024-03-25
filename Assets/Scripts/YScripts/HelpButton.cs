using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpButton : MonoBehaviour
{
    public GameObject backButtonText1;
    public GameObject resetButtonText2;
    public GameObject helpButtonText3;

    public GameObject helpCanvas;

    private bool textShown;
    
    // Start is called before the first frame update
    void Start()
    {
        backButtonText1.SetActive(false);
        resetButtonText2.SetActive(false);
        helpButtonText3.SetActive(false);
        
        helpCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HelpButtonPress()
    {
        if (textShown == false)
        {
            backButtonText1.SetActive(true);
            resetButtonText2.SetActive(true);
            helpButtonText3.SetActive(true);

            helpCanvas.SetActive(true);
            
            textShown = true;
        }
        else if (textShown == true)
        {
            backButtonText1.SetActive(false);
            resetButtonText2.SetActive(false);
            helpButtonText3.SetActive(false);

            helpCanvas.SetActive(false);
            
            textShown = false;
        }
    }
}
