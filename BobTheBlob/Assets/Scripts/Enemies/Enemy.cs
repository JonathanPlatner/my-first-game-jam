using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public abstract string Name { get; }
    public abstract Transform Target { get;  }

    public abstract int MaxHealth { get; }
    private int _currentHealth;

    private void Awake()
    {
        _currentHealth = MaxHealth;
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
    }

    public int GetHealth()
    {
        return _currentHealth;
    }

    public bool Dead()
    {
        return _currentHealth <= 0;
    }
}
