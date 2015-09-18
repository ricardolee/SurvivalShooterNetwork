using UnityEngine;
using System;
using Toolkit;
public class GameOverManager : MonoBehaviour{

    [HideInInspector]
    public float restartDelay = 5f;

    Animator anim;
    float restartTimer;

    void Awake() {
        anim = GetComponent<Animator>();

    }
    
    void Start() {
        InvokeRepeating("CheckGameOver", 0, 1000);
    }

    void CheckGameOver() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        players = Array.FindAll(players, (go) => PlayerHealth.HealthState.Live == go.GetComponent<StateManager>().GetCurrentState<PlayerHealth.HealthState>());
        if (players.Length == 0)
        {
            anim.SetTrigger("GameOver");
            restartTimer += Time.deltaTime;
            if (restartTimer >= restartDelay) {
                Application.LoadLevel(Application.loadedLevel);
            }
        }
    }
}
