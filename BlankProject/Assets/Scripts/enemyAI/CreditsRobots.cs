using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsRobots : MonoBehaviour
{
    bool nextMove;

    private void Start()
    {
        nextMove = true;
        StartCoroutine(StartDancing());
    }

    private void NextMove()
    {
        GetComponent<Animator>().SetBool("nextMove", nextMove);
        nextMove = !nextMove;
    }

    IEnumerator StartDancing()
    {
        yield return new WaitForSeconds(1.75f);

        GetComponent<Animator>().SetBool("nextMove", true);
    }

}
