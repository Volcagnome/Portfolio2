using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] bool levelIncrease;

    private void OnTriggerEnter(Collider other)
    {
        int level = GameManager.instance.GetCurrentLevel();

        Debug.Log(GameManager.instance.GetCurrentLevel());

        if (other.gameObject.CompareTag("Player") && levelIncrease)
        {
            SceneManager.LoadScene(level + 1);
            GameManager.instance.SetCurrentLevel(level + 1);
        }

        else if (level != 0)
            SceneManager.LoadScene(level - 1);
    }
}
