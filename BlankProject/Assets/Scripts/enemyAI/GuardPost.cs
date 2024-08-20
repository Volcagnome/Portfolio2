using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardPost : MonoBehaviour
{

    [SerializeField] GameObject guard;

    private bool isOccupied;

    // Start is called before the first frame update
    void Start()
    {
        EnemyManager.instance.guardPosts_List.Add(gameObject);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetIsOccupied(bool status)
    {
        isOccupied = status;
    }

    public bool CheckIfOccupied()
    {
        return isOccupied;
    }

    public void AssignGuard(GameObject newGuard)
    {
        guard = newGuard;
    }

    private void OnTriggerEnter(Collider guardRobot)
    {

        if (guardRobot.CompareTag("Enemy")
            && guardRobot.GetComponent<enemyAI>().GetDefaultPost() == gameObject.GetComponent<GuardPost>().gameObject
            && guardRobot.GetComponent<enemyAI>().CheckIfOnDuty() == true)
        {   
            guardRobot.gameObject.transform.rotation = gameObject.transform.rotation;
        }
    }

}
