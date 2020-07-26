using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Enemy : MonoBehaviour
{
    public abstract string Name { get; }
    public abstract Transform Target { get;  }

    public Image healthBar;

    public abstract int MaxHealth { get; }
    private int _currentHealth;

    private void Awake()
    {
        _currentHealth = MaxHealth;
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        float newHealthBarAmount = (float)GetHealth() / (float)MaxHealth;
        healthBar.fillAmount = newHealthBarAmount;
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
