using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class AgentMovementController : MonoBehaviour {
    
    public float wanderTimer;
    public float time;

    private Agent _agent;
    private GameObject _plant;
    private float _timer;
    private float _distanceToSpawn;
    private List<Vector3> _closePlantsPosition;
    private float _wanderRadius;


    // Use this for initialization
    void OnEnable ()
    {
        Initialize();
    }
 
    // Update is called once per frame
    void Update () {
        _timer += Time.deltaTime;
        _wanderRadius = GameObject.FindWithTag("GameController").GetComponent<GameController>().mapSize;
        _distanceToSpawn = Vector3.Distance(transform.position, _agent.SpawnPosition);
        
        if ((_distanceToSpawn / _agent.Speed + 0.1 > time && _agent.Food == 1) || _agent.Food == 2)
        {
            _agent.NavMeshAgent.enabled = false;
            GetComponent<CapsuleCollider>().enabled = false;
            transform.position = Vector3.MoveTowards(transform.position,   _agent.SpawnPosition, _agent.Speed * Time.deltaTime);
        }
        else if (!_agent.NearPlant && _timer >= wanderTimer) {
            Vector3 newPos = RandomNavSphere(Vector3.zero, _wanderRadius, -1);
            _agent.NavMeshAgent.SetDestination(newPos);
            _timer = 0;
        }
        
        if (time > 0)
        {
            time -= Time.deltaTime;
        }
    }

    public void Initialize()
    {
        _agent = new Agent
        {
            NavMeshAgent = GetComponent<NavMeshAgent>(),
            SpawnPosition = transform.position,
            Speed = GetComponent<NavMeshAgent>().speed,
            Food = 0
        };
        _plant = new GameObject();
        _closePlantsPosition = new List<Vector3>();
        _timer = wanderTimer;
        time = GameObject.FindWithTag("GameController").GetComponent<GameController>().roundTime;
        _agent.NavMeshAgent.enabled = true;
        GetComponent<CapsuleCollider>().enabled = true;
    }
 
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask) {
        Vector3 randDirection = Random.insideUnitSphere * dist;
 
        randDirection += origin;
 
        NavMeshHit navHit;
 
        NavMesh.SamplePosition (randDirection, out navHit, dist, layermask);
 
        return navHit.position;
    }

    private void OnCollisionEnter(Collision collisionInfo)
    {
        _closePlantsPosition.Add(collisionInfo.transform.position);
        
        if (collisionInfo.transform.gameObject.CompareTag("Plant") && !_agent.NearPlant && time > 0)
        {
            StartGathering(collisionInfo.gameObject, collisionInfo.transform.position);
        }

        Debug.Log("Plants:");
        foreach (var plant in _closePlantsPosition)
        {
            Debug.Log(plant);   
        }
    }
    
    private void OnCollisionStay(Collision collisionInfo)
    {
        if (collisionInfo.gameObject.GetInstanceID() == _plant.GetInstanceID() && time > 0)
        {
            if (Vector3.Distance(transform.position,   _agent.PositionToFollow) < 0.5f && !_agent.Stopped)
            {
                _agent.Stopped = true;
                Debug.Log($"Close to plant {collisionInfo.transform.position}");
                StartCoroutine(StartMoving(collisionInfo));
            }
            
            if (!_agent.Stopped)
            {
                transform.position = Vector3.MoveTowards(transform.position,   _agent.PositionToFollow, _agent.Speed * Time.deltaTime);
            }
        }
    }

    public void OnCollisionExit(Collision collisionInfo)
    {
        Debug.Log($"Far from plant {collisionInfo.transform.position}");
        _closePlantsPosition.Remove(collisionInfo.transform.position);
    }

    private IEnumerator StartMoving(Collision collisionInfo)
    {
        Debug.Log($"Collision with plant ended {collisionInfo.transform.position}");
        _closePlantsPosition.Remove(collisionInfo.transform.position);
        Destroy(collisionInfo.gameObject, 1f);
        Debug.Log($"Collision with plant ended {collisionInfo.transform.position}");
        yield return new WaitForSeconds(1);
        _agent.Food += 1;
        var tuple = GetClosestPlant();
        _plant = new GameObject();
        _agent.NavMeshAgent.enabled = true;
        _agent.NearPlant = false;
        _agent.Stopped = false;   
        
        if (tuple.Item2 < Mathf.Infinity && _agent.Food < 2)
        {
            StartGathering(FindAt(tuple.Item1), tuple.Item1);
        }
    }

    private void StartGathering(GameObject gameObject, Vector3 position)
    {
        if (!gameObject.TryGetComponent(typeof(Plant), out _) || gameObject.GetComponent<Plant>().Gathered)
        {
            return;
        }
        
        Debug.Log($"Collision with plant {position}");
        _agent.NavMeshAgent.enabled = false;
        _agent.NearPlant = true;
        _agent.PositionToFollow = position;
        _agent.PositionToFollow.y = transform.position.y;
        _agent.Stopped = false;
        _plant = gameObject;
        gameObject.GetComponent<Plant>().Gathered = true;
    }

    private Tuple<Vector3, float> GetClosestPlant()
    {
        Tuple<Vector3, float> tuple = new Tuple<Vector3, float>(new Vector3(), Mathf.Infinity);
        
        foreach (var plant in _closePlantsPosition)
        {
            var distance = Vector3.Distance(transform.position, plant);
            Debug.Log($"{plant}::{distance}");
            
            if (distance < tuple.Item2)
            {
                tuple = new Tuple<Vector3, float>(plant, distance);
            }
        }

        return tuple;
    }
    
    private GameObject FindAt(Vector3 pos)
    {
        var cols = Physics.OverlapSphere(pos, 0.1f);
        var dist= Mathf.Infinity;
        var nearest = new GameObject();
        foreach (var col in cols)
        {
            var d = Vector3.Distance(pos, col.transform.position);
            if (d < dist)
            {
                dist = d;
                nearest = col.gameObject;
            }
        }
        return nearest;
    }

    public Agent GetAgent()
    {
        return _agent;
    }
    
    public void UpdateSpeed()
    {
        _agent.Speed = GetComponent<NavMeshAgent>().speed;
    }

    public void Mutate(GameObject parent, float[,] modifiers)
    {
        var trait = Random.Range(0, 2);
        var modifier  = modifiers[trait, Random.Range(0, 2)];

        GetComponent<NavMeshAgent>().speed = parent.GetComponent<NavMeshAgent>().speed;
        GetComponent<CapsuleCollider>().radius = parent.GetComponent<CapsuleCollider>().radius;
        
        switch (trait)
        {
            case 0:
                GetComponent<NavMeshAgent>().speed *= modifier;
                break;
            case 1:
                GetComponent<CapsuleCollider>().radius *= modifier;
                break;
        }
    }
}
