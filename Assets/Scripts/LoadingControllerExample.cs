using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingControllerExample : MonoBehaviour
{
    public static LoadingControllerExample Instance;

    public string sceneToLoad { get; private set; } // This is the scene our Loading system will load into.

    void Awake()
    {
        if (Instance is null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // When a new scene loads, keep our loading manager active.
        }
        else
        {
            Destroy(gameObject); // A loading manager has already been set up, so remove this one.
            return;
        }
    }

    public void LoadScene(string sceneWeWillLoadInto) // To load to a new scene, just call LoadingManager.Instance.LoadScene(sceneName) anywhere in your game.
    {
        sceneToLoad = sceneWeWillLoadInto; // Storing the scene we will load into so that the LoadingBar scene can use it.
        SceneManager.LoadScene("LoadingSystemScene"); // Instantly load our loading bar scene without a coroutine. Keep this scene small so it loads ASAP!
    }
}
