using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class SplashScreenControllerExample : MonoBehaviour
{
    public GameObject[] splashScreens;
    public int secondsDelay = 3;

    void Start()
    {
        foreach(GameObject splashScreen in splashScreens)
        {
            splashScreen.SetActive(false);
        }
    }

    public async Task ShowSplashScreens()
    {
        foreach (GameObject splashScreen in splashScreens)
        {
            splashScreen.SetActive(true);

            await Task.Delay(secondsDelay * 1000);

            splashScreen.SetActive(false);
        }
    }
}
