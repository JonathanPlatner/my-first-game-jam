using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    private GameObject[] enemies;

    [SerializeField]
    private int levelNum;
    private void Start()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        foreach(GameObject enemy in enemies)
        {
            if(enemy != null)
            {
                Debug.Log("All enemies must be destroyed before leaving level");
                return;
            }
        }
        if (collision.gameObject.tag == "Player")
        {
            PersistentData.maxLevel++;
            PersistentData.LevelIndex = levelNum;
            SceneManager.LoadScene("LevelSelection");
        }
    }
}
