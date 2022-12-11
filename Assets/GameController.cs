using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    [Header("Initial parameters")]
    public GameObject agentPrefab;
    public GameObject plantPrefab;
    public GameObject floor;
    public int pacificAgentNumber;
    public int aggressiveAgentNumber;
    public float mapSize;
    public float initSpeed;
    public float initRange;
    public float initSize;
    public bool grouped;
    
    [Header("Modifiable per round")]
    public int grassNumber;
    public float roundTime;
    
    [Header("Traits options")]
    public float speedModifier;
    public float rangeModifier;
    public float sizeModifier;
    public bool balanceTraits;

    [Header("Food parameters")] 
    public float foodOnSoloGather;
    public float foodPacifistWithPacifist;
    public float foodPacifistWithAggressive;
    public float foodAggressiveWithPacifist;
    public float foodAggressiveWithAggressive;
    public float foodOnEatingOther;
    public float sizeProportionToEatOther;

    [Header("Statistics")]
    public float averageSpeed;
    public float averageRange;
    public float averageSize;
    public float pacifistNumber;
    public float aggressiveNumber;
    
    private List<GameObject> _agents;
    private float _timeLeft;
    
    // Start is called before the first frame update
    void Start()
    {
        _agents = new List<GameObject>();
        _timeLeft = roundTime;
        
        floor.transform.localScale = new Vector3(mapSize, 0.1f, mapSize);
        floor.GetComponent<NavMeshSurface>().BuildNavMesh();
        GameObject.FindWithTag("MainCamera").transform.position = new Vector3(0, 7*mapSize/10, -7*mapSize/10);
        
        for (var i = 0; i < pacificAgentNumber + aggressiveAgentNumber; i++)
        {
            /* Get the spawn position */
            var spawnPos = GetSpawnPosition(i, pacificAgentNumber + aggressiveAgentNumber, initSize);
         
            /* Now spawn */
            var agent = Instantiate(agentPrefab, spawnPos, Quaternion.identity);
            var agentInfo = agent.GetComponent<AgentMovementController>();

            agentInfo.time = roundTime;
            agent.GetComponent<CapsuleCollider>().radius = initRange;
            agent.GetComponent<NavMeshAgent>().speed = initSpeed;
            agent.transform.localScale = new Vector3(1,initSize, 1);
            agentInfo.UpdateSpeed();
            agentInfo.UpdateSize();

            /* Rotate the enemy to face towards player */
            agent.transform.LookAt(Vector3.zero);
            
            if (grouped && i < aggressiveAgentNumber 
                || (!grouped && ((aggressiveAgentNumber >= pacificAgentNumber 
                                  && (i < aggressiveAgentNumber - pacificAgentNumber 
                                      || i % 2 == 0)) 
                                 || (pacificAgentNumber > aggressiveAgentNumber 
                                     && i > pacificAgentNumber - aggressiveAgentNumber 
                                     && i % 2 == 0))))
            {
                agentInfo.GetAgent().Aggressive = true;
                agent.GetComponent<Renderer>().material.color = Color.red;
            }
            
            _agents.Add(agent);
        }

        SpawnPlants();
        UpdateStatistics();
    }

    // Update is called once per frame
    void Update()
    {
        if (_timeLeft <= 0f)
        {
            foreach (var agent in _agents.ToList())
            {
                if (agent.gameObject == null)
                {
                    _agents.Remove(agent);
                    continue;
                }
                
                var agentInfo = agent.GetComponent<AgentMovementController>().GetAgent();

                if (agent.transform.position != agentInfo.SpawnPosition || agentInfo.Food < 1f)
                {
                    _agents.Remove(agent);
                    Destroy(agent);
                }
                else if (agentInfo.Food >= 2f)
                {
                    var newAgent = Instantiate(agentPrefab, agentInfo.SpawnPosition, Quaternion.identity);

                    var newAgentInfo = newAgent.GetComponent<AgentMovementController>();
                    
                    newAgentInfo.time = roundTime;
                    newAgentInfo.Mutate(agent, new [,]{{1-speedModifier, 1+speedModifier}, {1-rangeModifier, 1+rangeModifier}, {1-sizeModifier, 1+sizeModifier}}, balanceTraits);

                    /* Rotate the enemy to face towards player */
                    newAgent.transform.LookAt(Vector3.zero);
            
                    if (agentInfo.Aggressive)
                    {
                        newAgent.GetComponent<AgentMovementController>().GetAgent().Aggressive = true;
                        newAgent.GetComponent<Renderer>().material.color = Color.red;
                    }
                    
                    _agents.Insert(_agents.IndexOf(agent), newAgent);
                }
            }

            _timeLeft = roundTime;
            
            SpawnPlants();
            UpdateStatistics();

            for (var i = 0; i < _agents.Count; i++)
            {
                var agent = _agents[i];
                var isAggressive = agent.GetComponent<AgentMovementController>().GetAgent().Aggressive;
                agent.transform.position = GetSpawnPosition(i, _agents.Count, agent.transform.localScale.y);
                agent.GetComponent<AgentMovementController>().Initialize(isAggressive);
            }
        }

        _timeLeft -= Time.deltaTime;
    }

    private Vector3 GetSpawnPosition(int position, int numberOfPositions, float size)
    {
        /* Distance around the circle */  
        var radians = 2 * MathF.PI / numberOfPositions * position;
         
        /* Get the vector direction */ 
        var vertical = MathF.Sin(radians);
        var horizontal = MathF.Cos(radians); 
         
        var spawnDir = new Vector3 (horizontal, 0, vertical);
         
        /* Get the spawn position */ 
        return new Vector3 (0, size, 0) + spawnDir * ((mapSize-1)/2); // Radius is just the distance away from the point
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
            var position = Random.insideUnitSphere * (mapSize/2 - 5);
            position.y = 0;
            var plant = Instantiate(plantPrefab, position, Quaternion.identity);

            plant.transform.LookAt(Vector3.zero);
            plant.GetComponent<Plant>().Initialize(foodOnSoloGather, foodPacifistWithPacifist, foodPacifistWithAggressive,
                                                    foodAggressiveWithPacifist, foodAggressiveWithAggressive, foodOnEatingOther, sizeProportionToEatOther);
        }
    }

    private void UpdateStatistics()
    {
        var totalSpeed = 0f;
        var totalRange = 0f;
        var totalSize = 0f;
        var aggr = 0;
        var pac = 0;
        
        foreach (var agent in _agents.ToList())
        {
            totalSpeed += agent.GetComponent<NavMeshAgent>().speed;
            totalRange += agent.GetComponent<CapsuleCollider>().radius;
            totalSize += agent.transform.localScale.y;
            if (agent.GetComponent<AgentMovementController>().GetAgent().Aggressive)
            {
                aggr += 1;
            }
            else
            {
                pac += 1;
            }
        }

        averageSpeed = totalSpeed / _agents.Count;
        averageRange = totalRange / _agents.Count;
        averageSize = totalSize / _agents.Count;
        pacifistNumber = pac;
        aggressiveNumber = aggr;
        Debug.Log($"Pacifist: {pac}");
        Debug.Log($"Aggressive: {aggr}");
    }
}
