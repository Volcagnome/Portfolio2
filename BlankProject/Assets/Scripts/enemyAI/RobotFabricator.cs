using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class RobotFabricator : MonoBehaviour
{
    [SerializeField] GameObject fabricatorDoor;
    [SerializeField] Light fabricatorDoorLight;
    [SerializeField] GameObject guard;
    [SerializeField] GameObject patrol;
    [SerializeField] GameObject titan;
    [SerializeField] GameObject spawnPosition;

    private Vector3 doorCurrentPosition;
    private Vector3 doorOpenPosition;
    private Vector3 doorClosedPosition;

    private bool spawningRobot;
    private bool doorOpen;
    private bool isReadyToSpawn;
    private bool isFunctional;

    // Start is called before the first frame update
    void Start()
    {
        doorOpen = false;
        isFunctional = true;
        isReadyToSpawn = true;
        fabricatorDoorLight.GetComponent<Light>().enabled = false;
        doorClosedPosition = fabricatorDoor.transform.localPosition;
        doorOpenPosition = new Vector3(doorClosedPosition.x, doorClosedPosition.y + 3.8f, doorClosedPosition.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (spawningRobot && !doorOpen)
            OpenDoor();
        else if(!spawningRobot && doorOpen)
            CloseDoor();

    }


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
    

    public IEnumerator SpawnRobot(SharedEnemyAI.enemyType entityToSpawn)
    {
        spawningRobot = true;
        isReadyToSpawn = false;

        GameObject newRobot = null;

        yield return new WaitForSeconds(2f);

        if (entityToSpawn == SharedEnemyAI.enemyType.Guard)
        { 
           newRobot = Instantiate(guard, spawnPosition.transform.position, spawnPosition.transform.localRotation);
        }

        else if (entityToSpawn == SharedEnemyAI.enemyType.Patrol)
        {
           newRobot =  Instantiate(patrol, spawnPosition.transform.position, spawnPosition.transform.localRotation);
        }

        newRobot.SetActive(true);

        StartCoroutine(SpawnCooldown());
    }

    IEnumerator SpawnCooldown()
    {
        yield return new WaitForSeconds(EnemyManager.instance.GetEnemySpawnInterval());

        isReadyToSpawn = true;
    }

    private void OnTriggerExit(Collider newRobot)
    {

        if (newRobot.gameObject.tag == "Enemy")
        {
            spawningRobot = false;
        }

    }

    public bool GetIsReadyToSpawn() { return isReadyToSpawn; }

    public bool GetIsFunctional() { return isFunctional; }
}
