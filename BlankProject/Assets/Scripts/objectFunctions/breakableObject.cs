using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class breakableObject : MonoBehaviour, IDamage
{
    [SerializeField] float HP;
    [SerializeField] float MaxHP;

    // Start is called before the first frame update
    void Start()
    {
        MaxHP = HP;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void takeDamage(float dmg)
    {
        HP -= dmg;

        if (HP <= 0) Destroy(gameObject);
    }

    public void criticalHit(float amount)
    {

    }
}
