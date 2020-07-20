using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    [SerializeField]
    private string levelName;

    [SerializeField]
    private bool enterImmediately = false;

    public void Load()
    {
        Debug.Log("Loaded level: " + levelName);
        //SceneManager.LoadScene(levelName);  // Enable when levels are there
    }
}
