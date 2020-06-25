using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUnits : MonoBehaviour
{

    public float HP = 100;
    public float Attack = 10;
    public float AttackInterval = 10;

    private float CurrentHp = 100;

    public bool isDead => CurrentHp <= 0;

    // Update is called once per fram
    public void DoDamage(float damage)
    {
        CurrentHp -= damage;
        if (CurrentHp <= 0)
        {
            Destroy(gameObject);
        }
    }
}
