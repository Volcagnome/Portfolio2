using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//All Patrol Waypoints hold a reference to the next waypoint on the route.
//MaxRobotsOnThisRoute and robotAssgnedToRoute will only be accessed if the object is a PatrolWaypointStart. 


public class PatrolWaypoint : MonoBehaviour
{
    //Next waypoint on the route
    [SerializeField] GameObject nextWaypoint;

    //Max number of robots allowed on this route  (only need to set if waypoint is a WaypointStart
    [SerializeField] int MaxRobotsOnThisRoute;

    //Roster of robots assigned to route (will auto-populate with any robots who have this PatrolWaypointStart assigned as 
    //their default post)
    [SerializeField] List<GameObject> robotsAssignedToRoute;


    public int GetNumberRobotsOnThisRoute(){ return robotsAssignedToRoute.Count;}

    public int GetMaxRobotsOnThisRoute() { return MaxRobotsOnThisRoute; }

    public void AddRobotToRoute(GameObject newPatrolBot) { robotsAssignedToRoute.Add(newPatrolBot);}

    public void RemoveRobotFromRoute(GameObject deadPatrolBot) { robotsAssignedToRoute.Remove(deadPatrolBot); }

    public GameObject GetNextWaypoint() { return nextWaypoint; }

}
