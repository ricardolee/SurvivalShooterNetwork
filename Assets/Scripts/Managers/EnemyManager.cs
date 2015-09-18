using UnityEngine;
using UnityEngine.Networking;
using Toolkit;

public class EnemyManager : NetworkBehaviour
{
    public GameObject enemyFrefab;
    public float spawnTime = 3f;
    public Transform[] spawnPoints;


    void Start () {
        if (isServer)
        {
            InvokeRepeating ("Spawn", spawnTime, spawnTime);
        }
    }


    void Spawn () {


        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        bool isAlivePlayer = false;
        foreach (GameObject go in players)
        {
            PlayerHealth.HealthState current = go.GetComponent<StateManager>().GetCurrentState<PlayerHealth.HealthState>();
            if (PlayerHealth.HealthState.Live == current)
            {
                isAlivePlayer = true;
            }
        }

        if(!isAlivePlayer) {
            return;
        }
        
        int spawnPointIndex = Random.Range (0, spawnPoints.Length);
        GameObject enem = GameObject.Instantiate(enemyFrefab, spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation) as GameObject;
        NetworkServer.Spawn(enem);
    }
}
