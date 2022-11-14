using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Models;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

public class Main : MonoBehaviour
{
    private List<Agent> _agents = new List<Agent>();
    private int _agentNumber = 20;
    private int _foodNumber = 500;
    private int _mapSizeX = 100;
    private int _mapSizeY = 100;
    private int[,] _map;
    private int _iteration;
    
    // Start is called before the first frame update
    void Start()
    {
        _map = new int[_mapSizeX, _mapSizeY];
        for (var i = 0; i < _agentNumber; i++)
        {
            var border = new[] { 0, 99 };

            if (Random.Range(0, 2) < 1)
            {
                SpawnAgent(border[Random.Range(0, 2)], Random.Range(0, _mapSizeX));
            }
            else
            {
                SpawnAgent(Random.Range(0, _mapSizeY), border[Random.Range(0, 2)]);
            }
        }
        
        StartCoroutine("UpdateMap");
    }

    private void SpawnAgent(int x, int y)
    {
        var position = new Tuple<int, int>(x, y);
        var number = _agents.Count;

        _agents.Add(new Agent
        {
            Number = number,
            StartingCords = position,
            Position = position
        });
        
        Debug.Log($"Spawning agent {number} on ({position.Item1}, {position.Item2})");
    }

    private void SpawnFood()
    {
        for (int i = 0; i < _foodNumber; i++)
        {
            bool spawned = false;

            while (!spawned)
            {
                var posX = Random.Range(1, _mapSizeX - 1);
                var posY = Random.Range(1, _mapSizeY - 1);

                if (_map[posX, posY] == 0)
                {
                    _map[posX, posY] = 1;
                    spawned = true;
                    //Debug.Log($"Food spawned at ({posX}, {posY})");
                }
            }
        }
    }

    private void Evolve()
    {
        foreach (var agent in _agents.ToList())
        {
            if (agent.Position.Equals(agent.StartingCords))
            {
                if (agent.Food == 0)
                {
                    _agents.Remove(agent);
                    Debug.Log($"Agent {agent.Number} died");
                }
                else if (agent.Food > 1)
                {
                    SpawnAgent(agent.Position.Item1, agent.Position.Item2);
                    Debug.Log($"Agent {agent.Number} replicated");
                }
            }
            else
            {
                _agents.Remove(agent);
                Debug.Log($"Agent {agent.Number} died - wrong position");
            }
        }
    }
    
    // Update is called once per frame
    private void Update()
    {
    }

    IEnumerator UpdateMap()
    {
        while (true)
        {
            Debug.Log($"Iteration {_iteration}");
            //Debug.Log($"Position of 0: {_agents[0].Position}");
            if (_iteration % 50 == 0)
            {
                SpawnFood();
                if (_iteration > 0)
                {
                    Evolve();
                }
            }

            foreach (var agent in _agents)
            {
                if (agent.Position.Equals(agent.StartingCords) && _iteration % 50 != 0)
                {
                    continue;
                }

                var posX = agent.Position.Item1;
                var posY = agent.Position.Item2;

                if (agent.Food > 1 ||
                    Math.Abs(agent.Position.Item1 - agent.StartingCords.Item1) == 49 - _iteration % 50 ||
                    Math.Abs(agent.Position.Item2 - agent.StartingCords.Item2) == 49 - _iteration % 50)
                {
                    if (agent.Position.Item1 > agent.StartingCords.Item1)
                    {
                        posX -= 1;
                    }

                    if (agent.Position.Item1 < agent.StartingCords.Item1)
                    {
                        posX += 1;
                    }

                    if (agent.Position.Item2 > agent.StartingCords.Item2)
                    {
                        posY -= 1;
                    }

                    if (agent.Position.Item2 < agent.StartingCords.Item2)
                    {
                        posY += 1;
                    }
                }
                else
                {
                    var dir = new[] { -1, 0, 1 };
                    posX = agent.Position.Item1 + dir[Random.Range(0, 3)];
                    posX = posX > 0 ? posX : posX + 1;
                    posX = posX < 99 ? posX : posX - 1;
                    posY = agent.Position.Item2 + dir[Random.Range(0, 3)];
                    posY = posY > 0 ? posY : posY + 1;
                    posY = posY < 99 ? posY : posY - 1;

                    foreach (var x in dir)
                    {
                        foreach (var y in dir)
                        {
                            var tempPosX = agent.Position.Item1 + x;
                            var tempPosY = agent.Position.Item2 + y;
                            if (tempPosX > 0 && tempPosX < 99 && tempPosY > 0 && tempPosY < 99)
                            {
                                if (_map[tempPosX, tempPosY] == 1)
                                {
                                    posX = tempPosX;
                                    posY = tempPosY;
                                }
                            }
                        }
                    }
                }

                agent.Position = new Tuple<int, int>(posX, posY);

                if (_map[posX, posY] == 1)
                {
                    agent.Food += 1;
                    _map[posX, posY] = 0;
                    Debug.Log($"Agent {agent.Number} found food");
                }
            }

            _iteration += 1;
            yield return new WaitForSeconds(0.1F);
        }
    }
}
