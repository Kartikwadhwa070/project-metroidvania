using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public string transitionedFromScene;

    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if(Instance != null && Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }
}
