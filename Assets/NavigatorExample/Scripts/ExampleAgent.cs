using System.Collections;
using System.Collections.Generic;
using Gameboard.Navigation.Pathfinding;
using UnityEngine;
using Gameboard.Navigation;
using Gameboard;

public class ExampleAgent : MonoBehaviour
{
    GbWaypointNavAgent mNavAgent;

    private List<GbGraphEdge> mPrevPath;
    private GbPathOptions mPrevOptions;

    void Start()
    {
        mNavAgent = GetComponent<GbWaypointNavAgent>();

        // Register to know when the agent enters a waypoint.
        mNavAgent.OnWaypointEntered += OnWaypointEnteredReceiver;

        // Register to know when the agent exits a waypoint.
        mNavAgent.OnWaypointExited += OnWaypointExitedReceiver;

        // Register to know when the pathfinder is complete. Usuful if the search is handed off to a task.
        mNavAgent.OnPathFindingComplete += OnShortestPathSearchComplete;

        // Register to know when the pathfinder is done looking for paths of at most X cost.
        mNavAgent.OnPathOptionsSearchComplete += OnPathsOfAtMostCostComplete;

        // Register to return if a waypoint is valid for this particular agent
        mNavAgent.IsValidWaypointEvaluator += IsValidWaypoint;

        //Register to return if an edge is allowed to be traversed by the agent
        mNavAgent.IsValidWaypointPathEvaluator += IsValidWaypointPath;

        //Register to override the cost of an edge based on the agent
        mNavAgent.GetCostToWaypointEvaluator += GetCostToWaypoint;
    }

    private void OnDestroy()
    {
        mNavAgent.OnWaypointEntered -= OnWaypointEnteredReceiver;
        mNavAgent.OnWaypointExited -= OnWaypointExitedReceiver;
        mNavAgent.OnPathFindingComplete -= OnShortestPathSearchComplete;
        mNavAgent.OnPathOptionsSearchComplete -= OnPathsOfAtMostCostComplete;
        mNavAgent.IsValidWaypointEvaluator -= IsValidWaypoint;
        mNavAgent.GetCostToWaypointEvaluator -= GetCostToWaypoint;
    }

    void OnWaypointEnteredReceiver(GbWaypoint waypoint)
    {
        List<GbGraphEdge> pathFound = mNavAgent.FindPath(mNavAgent.end);
    }

    void OnWaypointExitedReceiver(GbWaypoint waypoint)
    {
        // Agent left waypoint
    }

    void OnShortestPathSearchComplete(List<GbGraphEdge> path)
    {
        if (mPrevPath != null)
        {
            SetPathHighlight(mPrevPath, false);
        }

        mPrevPath = new List<GbGraphEdge>(path);
        SetPathHighlight(mPrevPath, true);
    }

    void OnPathsOfAtMostCostComplete(GbPathOptions options)
    {
        if (mPrevOptions != null)
        {
            foreach (List<GbGraphEdge> path in mPrevOptions)
            {
                SetPathHighlight(path, false);
            }
        }


        mPrevOptions = options;

        foreach (List<GbGraphEdge> path in mPrevOptions)
        {
            SetPathHighlight(path, true);
        }
    }

    bool IsValidWaypoint(GbWaypoint node)
    {
        // Here you can check if this navigiator can go to the node
        // for example, if this navigator can't navigate water, return false if the annotation is 'water'
        // if(node.annotation == "water") return false;

        return true;
    }

    bool IsValidWaypointPath(GbWaypoint current, GbWaypoint node)
    {
        // Here you can check if this navigiator can go from one node to another, potentially checking the edge's annotation
        // for example, if this agent can't fly, it wouldn't be able to navigate a cliff edge

        // var edge = mNavAgent.levelGraph.GetWaypointEdge(current, node);

        // return edge.annotation != "cliff-edge";

        return true;
    }

    int GetCostToWaypoint(GbWaypoint current, GbWaypoint node)
    {
        // Here you can modify the cost to navigate an edge based on the agents abilities
        // for example, if this agent can't navigate the forest quickly, you can change it to cost more to traverse a "forest" edge.

        var edge = mNavAgent.levelGraph.GetWaypointEdge(current, node);

        GameboardLogging.Warning($"edge: {edge}");

        // return edge.annotation == "forest" ? edge.cost*2 : edge.cost;

        return edge.cost;
    }

    private void SetPathHighlight(List<GbGraphEdge> path, bool highlight)
    {
        foreach (GbGraphEdge edge in path)
        {
            if (edge.sourceNode != null)
            {
                edge.sourceNode.ToggleHighlight(highlight);
            }

            if (edge.targetNode != null)
            {
                edge.targetNode.ToggleHighlight(highlight);
            }
        }
    }
}
