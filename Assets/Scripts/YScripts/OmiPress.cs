using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OmiPress : MonoBehaviour
{
    public float t = 0;

    public bool toRotate;

    public Transform defaultRotation;
    public Transform targetRotation;
    
    // Start is called before the first frame update
    void Start()
    {
        defaultRotation = gameObject.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (toRotate == true)
        {
            t += Time.deltaTime/3;

            if (t >= 1)
            {
                t = 0;

                toRotate = false;

                gameObject.transform.rotation = defaultRotation.rotation;
            }
                
            
            this.gameObject.transform.rotation = Quaternion.Lerp(defaultRotation.rotation,targetRotation.rotation, t);
            
        }
    }

    public void OnOmiPress()
    {
        
    }
}
