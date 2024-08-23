using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class RobotFabricator : MonoBehaviour
{
    [SerializeField] GameObject fabricatorDoor;
    [SerializeField] Light fabricatorDoorLight;
    [SerializeField] GameObject entityToSpawn;
    [SerializeField] GameObject spawnPosition;

    private Vector3 doorCurrentPosition;
    private Vector3 doorOpenPosition;
    private Vector3 doorClosedPosition;

    private bool spawningRobot;
    private bool doorOpen;

    // Start is called before the first frame update
    void Start()
    {
        doorOpen = false;
        fabricatorDoorLight.GetComponent<Light>().enabled = false;
        doorClosedPosition = fabricatorDoor.transform.localPosition;
        doorOpenPosition = new Vector3(doorClosedPosition.x, doorClosedPosition.y + 3.8f, doorClosedPosition.z);

        StartCoroutine(MonitorForEmptyPost());
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
    public bool GetDoorOpen()
    {
        return doorOpen;
    }

    private void SpawnEnemy()
    {
        GameObject newEnemy = Instantiate(entityToSpawn, spawnPosition.transform.position, spawnPosition.transform.localRotation);
        EnemyManager.instance.AssignRole(newEnemy);
    }

    IEnumerator MonitorForEmptyPost()
    {
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            if (EnemyManager.instance.GetCurrentNumberRobots() < EnemyManager.instance.maxAllowedRobots)
            {
                spawningRobot = true;

                if (doorOpen)
                {
                    SpawnEnemy();
                }
            }

            yield return new WaitForSeconds(EnemyManager.instance.EnemySpawnInterval);
        }
    }

    private void OnTriggerExit(Collider newRobot)
    {

        if (newRobot.gameObject.tag == "Enemy")
        {
            spawningRobot = false;
        }

    }
}
