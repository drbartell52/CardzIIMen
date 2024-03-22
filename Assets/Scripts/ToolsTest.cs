using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameboard.Tools;

public class ToolsTest : MonoBehaviour
{
    public GameboardSettingsOverlay settings;
    public SDKTestSceneSelector sceneSelector;

    // Start is called before the first frame update
    void Start()
    {
        settings.OnCloseGamePressed += OnExitTools;
    }

    void OnExitTools()
    {
        sceneSelector.Button_GameboardControllersTest();
    }
}
