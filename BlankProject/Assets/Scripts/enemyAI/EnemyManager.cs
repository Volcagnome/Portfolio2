using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] public int EnemySpawnInterval;
    [SerializeField] public int guardUnitsMax;
    public static EnemyManager instance;

    public List<GameObject> guardUnits;
    public List<GameObject> guardPosts;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void NewGuard(GameObject newGuard)
    {
        guardUnits.Add(newGuard);
        newGuard.GetComponent<enemyAI>().SetBehavior(enemyAI.behaviorType.guard);

        for (int guardPostIndex = 0; guardPostIndex < guardPosts.Count; guardPostIndex++)
        {
            GameObject temp = guardPosts[guardPostIndex];

            if (!temp.GetComponent<GuardPost>().CheckIfOccupied())
            {
                newGuard.GetComponent<enemyAI>().SetDefaultPost(temp.gameObject);
                temp.GetComponent<GuardPost>().AssignGuard(newGuard);
                temp.GetComponent<GuardPost>().SetIsOccupied(true);
                break;
            }
        }
    }

    public void AddGuardPostToList(GameObject newGuardPost)
    {
        guardPosts.Add(newGuardPost);
    }

    public void RemoveFromGuardUnits(GameObject deadRobot)
    {
        guardUnits.Remove(deadRobot);  
    }
    
}
