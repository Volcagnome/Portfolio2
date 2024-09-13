using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;


//Respawns guards and patrols. Titan respawns pending balancing.

public class RobotFabricator : MonoBehaviour
{

    //Components in scene and prefabs for each enemy type
    [SerializeField] GameObject fabricatorDoor;
    [SerializeField] Light fabricatorDoorLight;
    [SerializeField] GameObject spawnPosition;
    [SerializeField] GameObject guard;
    [SerializeField] GameObject patrol;

    //Tracks door position
    private Vector3 doorCurrentPosition;
    private Vector3 doorOpenPosition;
    private Vector3 doorClosedPosition;

    private bool spawningRobot;
    private bool doorOpen;
    private bool isReadyToSpawn;
    //private bool isFunctional;

    // Start is called before the first frame update
    //On start will save its closed position and calculates what its position will be when open.
    void Start()
    {
        doorOpen = false;
        //isFunctional = true;
        isReadyToSpawn = true;
        fabricatorDoorLight.GetComponent<Light>().enabled = false;
        doorClosedPosition = fabricatorDoor.transform.localPosition;
        doorOpenPosition = new Vector3(doorClosedPosition.x, doorClosedPosition.y + 3.8f, doorClosedPosition.z);
    }

    // Update is called once per frame
    //Opens door when spawning a robot, once the robot has walked through the door, closes the door.
    void Update()
    {
        if (spawningRobot && !doorOpen)
            OpenDoor();
        else if(!spawningRobot && doorOpen)
            CloseDoor();

    }

    //Turns on the light inside the fabricator and moves the door component towards the open position until it is fully open.
    private void OpenDoor()
    {
        doorCurrentPosition = fabricatorDoor.transform.localPosition;
        fabricatorDoorLight.GetComponent<Light>().enabled = true;

        if (Vector3.Distance(doorCurrentPosition, doorOpenPosition) <= 0.2f)
        {
            doorOpen = true;
            return;
        }

        fabricatorDoor.transform.localPosition = Vector3.MoveTowards(doorCurrentPosition, doorOpenPosition, Time.deltaTime * 4f);

    }


    //Reverses the OpenDoor function.
    private void CloseDoor()
    {
        doorCurrentPosition = fabricatorDoor.transform.localPosition;

        if (Vector3.Distance(doorCurrentPosition, doorClosedPosition) <= 0.2f)
        {
            fabricatorDoorLight.GetComponent<Light>().enabled = false;
            doorOpen = false;
            return;
        }

        fabricatorDoor.transform.localPosition = Vector3.MoveTowards(doorCurrentPosition, doorClosedPosition, Time.deltaTime * 4f);
    }
    

    //Gives the fabricator door time to open, the spawns the passed entity and makes sure it is set to active. Then starts the 
    //spawn cooldown.
    public IEnumerator SpawnRobot(SharedEnemyAI.enemyType entityToSpawn)
    {
        spawningRobot = true;
        isReadyToSpawn = false;

        GameObject newRobot = null;

        yield return new WaitForSeconds(2f);

        
        if(entityToSpawn == SharedEnemyAI.enemyType.Guard)
            newRobot = Instantiate(guard, spawnPosition.transform.position, spawnPosition.transform.localRotation);
        else if(entityToSpawn == SharedEnemyAI.enemyType.Patrol)
            newRobot = Instantiate(patrol, spawnPosition.transform.position, spawnPosition.transform.localRotation);

        newRobot.SetActive(true);

        StartCoroutine(SpawnCooldown());
    }


    //Waits the configured number of seconds from the EnemyManager's minimum spawn interval variable.
    IEnumerator SpawnCooldown()
    {
        yield return new WaitForSeconds(EnemyManager.instance.GetEnemySpawnInterval());

        isReadyToSpawn = true;
    }

    //Once the new robot walks through the door, sets spawningRobot to false so the fabricator knows to close the door.
    private void OnTriggerExit(Collider newRobot)
    {

        if (newRobot.gameObject.tag == "Enemy")
        {
            spawningRobot = false;
        }

    }


    public bool GetIsReadyToSpawn() { return isReadyToSpawn; }

    //public bool GetIsFunctional() { return isFunctional; }
}
