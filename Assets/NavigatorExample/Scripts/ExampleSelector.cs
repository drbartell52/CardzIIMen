using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameboard.Navigation;

public class ExampleSelector : MonoBehaviour
{

    public Material defaultMaterial;
    public Material selectionMaterial;

    // Waypoint that this will respond to when highlighted.
    public GbWaypoint attachedWaypoint;

    private MeshRenderer mRenderToBeControlled;

    // Start is called before the first frame update
    void Start()
    {
        if (attachedWaypoint == null)
        {
            return;
        }
        attachedWaypoint.OnHighlightToggleChange += OnWaypointToggleChanged;
        mRenderToBeControlled = GetComponent<MeshRenderer>();
    }

    private void OnDestroy()
    {
        if (attachedWaypoint == null)
        {
            return;
        }

        attachedWaypoint.OnHighlightToggleChange -= OnWaypointToggleChanged;
    }

    void OnWaypointToggleChanged(bool selected)
    {
        if (attachedWaypoint == null)
        {
            return;
        }

        if (selected)
        {
            Material[] mats = mRenderToBeControlled.materials;
            mats[0] = selectionMaterial;
            mRenderToBeControlled.materials = mats;
        }
        else
        {
            //mRenderToBeControlled.materials[0].shader = defaultMaterial.shader;
            Material[] mats = mRenderToBeControlled.materials;
            mats[0] = defaultMaterial;
            mRenderToBeControlled.materials = mats;
        }
    }
}
