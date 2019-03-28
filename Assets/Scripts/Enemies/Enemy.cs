using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IKillable
{
    public int health = 100;
    public int damage = 25;
   
    public void Kill()
    {
        throw new System.NotImplementedException();
    }
    public void TakeDamage(int damage)
    {
        // Reduce health by damage
        health -= damage;
        // If health is less than or equal to zero
        if(health <= 0)
        {
            // Dead
            Destroy(gameObject);
        }
    }
}
