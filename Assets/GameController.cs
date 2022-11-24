using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    public int agentNumber;
    public int grassNumber;
    public GameObject agentPrefab;
    public GameObject plantPrefab;
    public float size;
    public float time;

    private List<GameObject> _agents;
    private float _timeLeft;
    
    // Start is called before the first frame update
    void Start()
    {
        _agents = new List<GameObject>();
        _timeLeft = time;
        
        for (var i = 0; i < agentNumber; i++)
        {
            /* Get the spawn position */
            var spawnPos = GetSpawnPosition(i, agentNumber);
         
            /* Now spawn */
            var agent = Instantiate(agentPrefab, spawnPos, Quaternion.identity);

            agent.GetComponent<AgentMovementController>().time = time;
         
            /* Rotate the enemy to face towards player */
            agent.transform.LookAt(Vector3.zero);
            
            _agents.Add(agent);
        }

        SpawnPlants();
    }

    // Update is called once per frame
    void Update()
    {
        if (_timeLeft <= 0f)
        {
            foreach (var agent in _agents.ToList())
            {
                var agentInfo = agent.GetComponent<AgentMovementController>().GetAgent();

                if (agent.transform.position != agentInfo.SpawnPosition || agentInfo.Food < 1)
                {
                    _agents.Remove(agent);
                    Destroy(agent);
                }
                else if (agentInfo.Food > 1)
                {
                    var newAgent = Instantiate(agentPrefab, agentInfo.SpawnPosition, Quaternion.identity);

                    newAgent.GetComponent<AgentMovementController>().time = time;
         
                    /* Rotate the enemy to face towards player */
                    newAgent.transform.LookAt(Vector3.zero);
            
                    _agents.Insert(_agents.IndexOf(agent), newAgent);
                }
            }

            _timeLeft = time;
            
            SpawnPlants();

            for (var i = 0; i < _agents.Count; i++)
            {
                var agent = _agents[i];
                agent.transform.position = GetSpawnPosition(i, _agents.Count);
                agent.GetComponent<AgentMovementController>().Initialize();
            }
        }

        _timeLeft -= Time.deltaTime;
    }

    private Vector3 GetSpawnPosition(int position, int numberOfPositions)
    {
        /* Distance around the circle */  
        var radians = 2 * MathF.PI / numberOfPositions * position;
         
        /* Get the vector direction */ 
        var vertical = MathF.Sin(radians);
        var horizontal = MathF.Cos(radians); 
         
        var spawnDir = new Vector3 (horizontal, 0, vertical);
         
        /* Get the spawn position */ 
        return new Vector3 (0, 1, 0) + spawnDir * ((size-1)/2); // Radius is just the distance away from the point
    }

    private void SpawnPlants()
    {
        var plants =  GameObject.FindGameObjectsWithTag ("Plant");

        foreach (var plant in plants)
        {
            Destroy(plant);   
        }

        for (var i = 0; i < grassNumber; i++)
        {
            var position = Random.insideUnitSphere * (size/2 - 5);
            position.y = 0;
            var enemy = Instantiate(plantPrefab, position, Quaternion.identity);

            enemy.transform.LookAt(Vector3.zero);
        }
    }
}
