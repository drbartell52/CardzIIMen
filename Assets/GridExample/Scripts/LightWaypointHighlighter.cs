using System.Collections;
using System.Collections.Generic;
using Gameboard.Navigation;
using UnityEngine;

public class LightWaypointHighlighter : MonoBehaviour
{
    private Light mLight;
    public GbWaypoint waypoint;
    // Start is called before the first frame update
    void Start()
    {
        mLight = GetComponent<Light>();
        waypoint.OnHighlightToggleChange += OnWaypointHighlight;
    }

    private void OnDestroy()
    {
        waypoint.OnHighlightToggleChange -= OnWaypointHighlight;
    }
    void OnWaypointHighlight(bool highlighted)
    {
        // Turn on the light when the waypoint is highlithed.
        mLight.enabled = highlighted;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
