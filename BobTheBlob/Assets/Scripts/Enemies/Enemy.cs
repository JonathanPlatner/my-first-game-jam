using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer[] MainSprites;
    [SerializeField]
    private SpriteRenderer[] WhiteSprites;
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
        StartCoroutine(Flash());
    }

    public int GetHealth()
    {
        return _currentHealth;
    }

    public bool Dead()
    {
        return _currentHealth <= 0;
    }

    public float GetHealthPercentage()
    {
        return (float)_currentHealth / MaxHealth;
    }

    public IEnumerator Flash()
    {
        int flashCount = 4;
        float flashInterval = 0.125f;
        bool flashState = false;

        for(int i=0;i<flashCount;i++)
        {
            if(flashState)
            {
                foreach (SpriteRenderer sr in MainSprites)
                {
                    sr.enabled = true;
                }
                foreach(SpriteRenderer sr in WhiteSprites)
                {
                    sr.enabled = false;
                }
            }
            else
            {
                foreach(SpriteRenderer sr in MainSprites)
                {
                    sr.enabled = false;
                }
                foreach(SpriteRenderer sr in WhiteSprites)
                {
                    sr.enabled = true;
                }
            }
            flashState = !flashState;
            yield return new WaitForSeconds(flashInterval);
        }
        foreach(SpriteRenderer sr in MainSprites)
        {
            sr.enabled = true;
        }
        foreach(SpriteRenderer sr in WhiteSprites)
        {
            sr.enabled = false;
        }
    }
}
