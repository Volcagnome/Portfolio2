using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFight : MonoBehaviour
{
    [SerializeField] bossAI boss;
    [SerializeField] GameObject bossDefaultPost2;

    bool bossFightBegin = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player") && bossFightBegin == false)
        {
            bossFightBegin = true;
            BossExitMainframe();
        }
    }

    private void BossExitMainframe()
    {
        boss.GetComponent<SharedEnemyAI>().SetDefaultPost(bossDefaultPost2);
    }

}
