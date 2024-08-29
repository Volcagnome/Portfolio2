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
            && guardRobot.GetComponent<SharedEnemyAI>().GetDefaultPost() == gameObject.GetComponent<GuardPost>().gameObject
            && guardRobot.GetComponent<SharedEnemyAI>().CheckIfOnDuty() == true)
        {   
            guardRobot.gameObject.transform.rotation = gameObject.transform.rotation;
        }
    }

}
