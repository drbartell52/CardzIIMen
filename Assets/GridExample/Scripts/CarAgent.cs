using System.Collections;
using System.Collections.Generic;
using Gameboard;
using Gameboard.Navigation;
using UnityEngine;

public class CarAgent : MonoBehaviour
{
    GbWaypointNavAgent mNavAgent;
    private GbWaypoint start;
    List<GbGraphEdge> mPrevPath;
    private GbWaypointMatrix mGrid;

    void Start()
    {
        mPrevPath = new List<GbGraphEdge>();
        mNavAgent = GetComponent<GbWaypointNavAgent>();
        // Listen for path finding results.
        mNavAgent.OnPathFindingComplete += OnShortestPathFound;
        // Get notified when the car enters a new waypoint.
        mNavAgent.OnWaypointEntered += OnWaypointEntered;
        // Override the edge validation check so we may add the car-specific logic.
        mNavAgent.IsValidWaypointPathEvaluator += IsValidWaypointPath;

        mGrid = mNavAgent.GetOwnerMatrix();
        mGrid.OnGridChanged += OnGridChanged;
    }

    private void OnDestroy()
    {
        mNavAgent.OnPathFindingComplete -= OnShortestPathFound;
        mGrid.OnGridChanged -= OnGridChanged;
    }

    void OnShortestPathFound(List<GbGraphEdge> path)
    {
        if (mPrevPath.Count > 0)
        {
            HighLightTitlePath(mPrevPath, false);

        }

        HighLightTitlePath(path, true);
        mPrevPath.AddRange(path);
    }

    void OnWaypointEntered(GbWaypoint waypoint)
    {
        if (start == null)
        {
            // Start of the agent.
            start = waypoint;
            mNavAgent.FindPath(mNavAgent.end);
        }
    }

    /// <summary>
    /// Sample for updating the validity of an egent.
    /// In this case our car only travels through roads that are paved.
    /// No offroading for this card so two tiles must have an overlap in their
    /// paths for the edge to be valid.
    /// </summary>
    /// <param name="sourceWaypoint"></param>
    /// <param name="targetWaypoint"></param>
    /// <returns></returns>
    bool IsValidWaypointPath(GbWaypoint sourceWaypoint, GbWaypoint targetWaypoint)
    {
        // Paths are only valid for this agent if there is a road between the two tiles there.

        Vector2Int sourceTileIndex = mGrid.GetWaypointIndex(sourceWaypoint.transform.position);
        Vector2Int targetTileIndex = mGrid.GetWaypointIndex(targetWaypoint.transform.position);

        // Find the source tile.
        GbTile sourceTile = mGrid.GetTileAt(sourceTileIndex);
        // Find the target tile.
        GbTile targetTile = mGrid.GetTileAt(targetTileIndex);

        // If either is null, it isn't a valid path
        if (sourceTile == null || targetTile == null) return false;

        // Check if there is an overlap in their shape.
        bool valid = sourceTile.PathsOverlap(targetTile);

        return valid;
    }

    void OnGridChanged(GbWaypointMatrix grid, GbTile tile, int row, int col)
    {
        // When grid changes, you can do something
        mNavAgent.FindPath(mNavAgent.end);
    }

    private void HighLightTitlePath(List<GbGraphEdge> path, bool highlight)
    {
        foreach (GbGraphEdge edge in path)
        {
            if (edge.targetNode != null)
            {
                edge.targetNode.ToggleHighlight(highlight);
            }
            if (edge.sourceNode != null)
            {
                edge.sourceNode.ToggleHighlight(highlight);
            }
        }
    }
}
