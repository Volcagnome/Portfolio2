using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class buttonFunctions : MonoBehaviour
{
    public bool isPressed;

    public void resume()
    {
        GameManager.instance.stateUnpaused();
    }
    
    public void ok()
    {
        GameManager.instance.hintWindow.SetActive(false);
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        GameManager.instance.stateUnpaused();
    }

    public void quit()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    public void respawn()
    {
        GameManager.instance.stateUnpaused();
        StartCoroutine(GameManager.instance.RespawnBuffer());
        //GameManager.instance.player.GetComponent<playerDamage>().spawnPlayer();
        GameManager.instance.player.GetComponent<playerDamage>().setHP(GameManager.instance.player.GetComponent<playerDamage>().getMaxHP());
        GameManager.instance.player.GetComponent<playerMovement>().setStamina(GameManager.instance.player.GetComponent<playerMovement>().getMaxStamina());
        GameManager.instance.player.GetComponent<playerDamage>().adjustHPBar();
        GameManager.instance.player.GetComponent<playerMovement>().enabled = false;
        GameManager.instance.player.transform.position = GameManager.instance.currentSpawn.transform.position;
        GameManager.instance.player.GetComponent<playerMovement>().enabled = true;

    }
}
