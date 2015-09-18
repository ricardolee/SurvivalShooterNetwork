using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using Toolkit;

public class EnemyMovement : NetworkBehaviour
{
    GameObject targetPlayer;

    NavMeshAgent nav;


    public Animator anim;
    
    void Awake()
    {
        anim = GetComponent<Animator>();
        nav = GetComponent<NavMeshAgent>();
    }

    IEnumerator Start()
    {
        // InvokeRepeating("SearchPlayer", 0, 3000);
        while(true) {
            SearchPlayer();
            yield return new WaitForSeconds(3);
        }
    }

    void Update()
    {
        if (targetPlayer != null && nav.isActiveAndEnabled)
        {
            nav.SetDestination(targetPlayer.transform.position);
        }
    }

    void SearchPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        players = Array.FindAll(players, (go) => PlayerHealth.HealthState.Live == go.GetComponent<StateManager>().GetCurrentState<PlayerHealth.HealthState>());

        if (players.Length == 0)
        {
            targetPlayer = null;
            if (IsArrive(nav))
            {
                anim.SetTrigger("Idle");
            }
            return;
        }

        Array.Sort(players, comparison);
        targetPlayer = players[0];
    }

    int comparison(GameObject a, GameObject b)
    {
        float t = (transform.position - a.transform.position).sqrMagnitude - (transform.position - b.transform.position).sqrMagnitude;
        if (t < 0)
        {
            return -1;
        }
        else if (t == 0)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }

    public bool IsArrive(NavMeshAgent nav)
    {
        if (!nav.pathPending)
        {
            if (nav.remainingDistance <= nav.stoppingDistance)
            {
                if (!nav.hasPath || nav.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
