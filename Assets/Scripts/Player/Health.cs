using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// Health.cs made for Lesson 7 of the Game Architecture course (0h 30 min)

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth;

    public Action<float> OnHealthUpdate;
    public Action OnDeath;

    public bool isDead { get; private set; }
    private float health;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = maxHealth;
        OnHealthUpdate?.Invoke(maxHealth);
    }

    public void DeductHealth(float value)
    {
        if (isDead) return;

        health -= value;
        if (health <= 0)
        {
            
            isDead = true;
            OnDeath();
            health = 0;
        }
        OnHealthUpdate(health);
    }
}

