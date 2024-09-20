using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsRobots : MonoBehaviour
{
    bool nextMove;

    private void Start()
    {
        nextMove = true;
    }

    private void NextMove()
    {
        GetComponent<Animator>().SetBool("nextMove", nextMove);
        nextMove = !nextMove;
    }

}
