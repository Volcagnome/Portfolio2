using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolWaypoint : MonoBehaviour
{
    [SerializeField] PatrolWaypoint nextWaypoint;
    [SerializeField] int MaxRobotsOnThisRoute;
    private List<GameObject> robotsAssignedToRoute;

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.name == "Patrol Waypoint Start")
            EnemyManager.instance.patrolRoutes_List.Add(gameObject);

    }

// Update is called once per frame
void Update()
    {

    }

    public int GetNumberRobotsOnThisRoute()
    {
        return robotsAssignedToRoute.Count;
    }

    public int GetMaxRobotsOnThisRoute()
    {
        return MaxRobotsOnThisRoute;
    }

    public void AddRobotToRoute(GameObject newRobot)
    {
        robotsAssignedToRoute.Add(newRobot);
    }

    public void RemoveRobotFromRoute(GameObject deadRobot)
    {
        robotsAssignedToRoute.Remove(deadRobot);
    }

}
