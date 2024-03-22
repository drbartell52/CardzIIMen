using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingSystem : MonoBehaviour
{
    AsyncOperation sceneLoadingData; // An AsyncOperation allows us to load a scene while receiving information on the progress.
    public Image LoadingBarImage; // This is the image we're using for the loading bar.

    void Start()
    {
        StartCoroutine(LoadSceneNow()); // When the loading scene loads, we start a coroutine to load our designated scene.
    }

    IEnumerator LoadSceneNow()
    {
        sceneLoadingData = SceneManager.LoadSceneAsync(LoadingControllerExample.Instance.sceneToLoad); // Pull the scene we're going to load from the LoadingManager.

        while (!sceneLoadingData.isDone)
        {
            float loadProgress = Mathf.Clamp01(sceneLoadingData.progress / .9f); // More accurate way to get the load progress.
            LoadingBarImage.fillAmount = loadProgress; // Fill the loading bar image based on our scene loading progress.
            yield return null;
        }
    }
}
