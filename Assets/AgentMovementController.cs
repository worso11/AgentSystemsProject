using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class AgentMovementController : MonoBehaviour {
 
    public float wanderRadius;
    public float wanderTimer;

    private Transform _target;
    private NavMeshAgent _agent;
    private float _timer;
    private bool _nearPlant;
    private float _speed;
    private Vector3 _position;
    private bool _stopped;
    
    // Use this for initialization
    void OnEnable () {
        _agent = GetComponent<NavMeshAgent>();
        _timer = wanderTimer;
        _speed = GetComponent<NavMeshAgent>().speed*1.5f;
    }
 
    // Update is called once per frame
    void Update () {
        _timer += Time.deltaTime;

        if (!_nearPlant && _timer >= wanderTimer) {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            _agent.SetDestination(newPos);
            _timer = 0;
        }
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
        if (collisionInfo.transform.gameObject.CompareTag("Plant"))
        {
            Debug.Log("Collision with plant");
            _agent.enabled = false;
            _nearPlant = true;
            _position = collisionInfo.transform.position;
            _position.y = transform.position.y;
            _stopped = false;
        }

        if (collisionInfo.transform.gameObject.CompareTag("Agent"))
        {
            Debug.Log("Agents collided");
        }
    }
    
    private void OnCollisionStay(Collision collisionInfo)
    {
        if (collisionInfo.transform.gameObject.CompareTag("Plant"))
        {
            if (Vector3.Distance(transform.position, _position) < 1f && !_stopped)
            {
                _stopped = true;
                StartCoroutine(StartMoving(collisionInfo.gameObject));
            }
            
            if (!_stopped)
            {
                transform.position = Vector3.MoveTowards(transform.position, _position, _speed * Time.deltaTime);
            }
        }
    }
    
    private IEnumerator StartMoving(GameObject plant)
    {
        yield return new WaitForSeconds(1);
        Destroy(plant);
        Debug.Log("Collision with plant ended");
        _agent.enabled = true;
        _nearPlant = false;
        _stopped = false;
    }
}
