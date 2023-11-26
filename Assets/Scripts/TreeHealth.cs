using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
public class TreeHealth : MonoBehaviour
{
    public float maxHealth = 4;
    public float health;

    public float woodCount;

    public bool cutOff = false;

    private void Start()
    {
        health = maxHealth;
    }
    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            cutOff = true;

            var wood = Random.Range(5, 10);

            woodCount += wood;

            Destroy(gameObject);
        }

    }
}
