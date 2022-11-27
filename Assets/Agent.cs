using System.Numerics;
using UnityEditor;
using UnityEngine.AI;
using Vector3 = UnityEngine.Vector3;

namespace DefaultNamespace
{
    public class Agent
    {
        public NavMeshAgent NavMeshAgent;

        public bool NearPlant;
        
        public float Speed;

        public float Size;

        public Vector3 PositionToFollow;

        public Vector3 SpawnPosition;

        public bool Stopped;

        public float Food;

        public bool Aggressive;
    }
}