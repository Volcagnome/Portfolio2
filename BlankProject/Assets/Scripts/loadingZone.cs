using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class loadingZone : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] int nextSceneAddress;
    int currentSceneAddress = -1;

    private void Start()
    {
         if (currentSceneAddress == -1)currentSceneAddress = SceneManager.GetActiveScene().buildIndex;
    }


    //Done with semi-extensive research
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StartCoroutine(loadScene());

            GameManager.instance.player.GetComponent<playerDamage>().spawnPlayer();
        }
    }

    IEnumerator loadScene()
    {
        //Following along with an example from https://stackoverflow.com/questions/45798666/move-transfer-gameobject-to-another-scene
        //Found another at https://stackoverflow.com/questions/44727881/how-to-use-scenemanager-unloadsceneasync

        //Some information was missing, notably about how to unload a scene
        AsyncOperation nextScene = SceneManager.LoadSceneAsync(nextSceneAddress, LoadSceneMode.Additive);
        nextScene.allowSceneActivation = false;

        while (nextScene.progress < .9F) yield return null;

        nextScene.allowSceneActivation = true;

        while (!nextScene.isDone) yield return null;
        

        SceneManager.MoveGameObjectToScene(GameManager.instance.player, SceneManager.GetSceneByBuildIndex(nextSceneAddress));
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(nextSceneAddress));

        AsyncOperation lastScene = SceneManager.UnloadSceneAsync(currentSceneAddress);
        while (lastScene.progress < .9F) yield return null;


        
        GameManager.instance.player.GetComponent<playerDamage>().spawnPlayer();
    }
}
