using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class breakableObject : MonoBehaviour, IDamage
{
    [SerializeField] int HP;
    [SerializeField] int MaxHP;

    // Start is called before the first frame update
    void Start()
    {
        MaxHP = HP;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void takeDamage(int dmg)
    {
        HP -= dmg;

        if (HP <= 0) Destroy(gameObject);
    }
}
