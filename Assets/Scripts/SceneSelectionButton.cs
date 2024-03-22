using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class SceneSelectionButton : MonoBehaviour
{
    private Button button;

    public void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(TransitionScene);
    }

    public void OnDestroy()
    {
        button.onClick.RemoveListener(TransitionScene);
    }

    private void TransitionScene()
    {
        if (LoadingControllerExample.Instance != null)
        {
            LoadingControllerExample.Instance.LoadScene(this.name);
        }
        else
        {
            SceneManager.LoadScene(this.name);
        }
    }
}
