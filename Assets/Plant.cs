using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public int Gathered;

    public List<AgentMovementController> Agents;

    public IEnumerator GatherPlant()
    {
        yield return new WaitForSeconds(1);

        while (Gathered > 1 && Agents.Count < 2)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (Gathered > 1)
        {
            if (Agents[0].GetSize() >= Agents[1].GetSize()*2 && Agents[0].IsAggressive())
            {
                Debug.Log($"$Agent {Agents[0].GetSize()} eats {Agents[1].GetSize()}");
                Agents[0].StartMoving(2f);
                Destroy(Agents[1].gameObject);
            }
            else if (Agents[1].GetSize() >= Agents[0].GetSize()*2 && Agents[1].IsAggressive())
            {
                Debug.Log($"$Agent {Agents[1].GetSize()} eats {Agents[0].GetSize()}");
                Agents[1].StartMoving(2f);
                Destroy(Agents[0].gameObject);
            }
            else
            {
                switch (Agents[0].IsAggressive(), Agents[1].IsAggressive())
                {
                    case (true, true):
                        Agents[0].StartMoving(0.35f);
                        Agents[1].StartMoving(0.35f);
                        break;
                    case (true, false):
                        Agents[0].StartMoving(0.75f);
                        Agents[1].StartMoving(0.25f);
                        break;
                    case (false, true):
                        Agents[0].StartMoving(0.25f);
                        Agents[1].StartMoving(0.75f);
                        break;
                    case (false, false):
                        Agents[0].StartMoving(0.5f);
                        Agents[1].StartMoving(0.5f);
                        break;
                }    
            }
        }
        else
        {
            Agents[0].StartMoving(1f);
        }

        Destroy(gameObject);
    }
}
