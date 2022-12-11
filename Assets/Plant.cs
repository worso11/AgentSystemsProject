using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public int Gathered;

    public List<AgentMovementController> Agents;

    private float _foodOnSoloGather;
    private float _foodPacifistWithPacifist;
    private float _foodPacifistWithAggressive;
    private float _foodAggressiveWithPacifist;
    private float _foodAggressiveWithAggressive;
    private float _foodOnEatingOther;
    private float _sizeProportionToEatOther;

    public void Initialize(float foodOnSoloGather, float foodPacifistWithPacifist, float foodPacifistWithAggressive, 
                            float foodAggressiveWithPacifist, float foodAggressiveWithAggressive, float foodOnEatingOther, float sizeProportionToEatOther)
    {
        _foodOnSoloGather = foodOnSoloGather;
        _foodPacifistWithPacifist = foodPacifistWithPacifist;
        _foodPacifistWithAggressive = foodPacifistWithAggressive;
        _foodAggressiveWithPacifist = foodAggressiveWithPacifist;
        _foodAggressiveWithAggressive = foodAggressiveWithAggressive;
        _foodOnEatingOther = foodOnEatingOther;
        _sizeProportionToEatOther = sizeProportionToEatOther;
    }
    
    public IEnumerator GatherPlant()
    {
        yield return new WaitForSeconds(1);

        while (Gathered > 1 && Agents.Count < 2)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (Gathered > 1)
        {
            if (Agents[0].GetSize() >= Agents[1].GetSize()*_sizeProportionToEatOther && Agents[0].IsAggressive())
            {
                Debug.Log($"$Agent {Agents[0].GetSize()} eats {Agents[1].GetSize()}");
                Agents[0].StartMoving(_foodOnEatingOther);
                Destroy(Agents[1].gameObject);
            }
            else if (Agents[1].GetSize() >= Agents[0].GetSize()*_sizeProportionToEatOther && Agents[1].IsAggressive())
            {
                Debug.Log($"$Agent {Agents[1].GetSize()} eats {Agents[0].GetSize()}");
                Agents[1].StartMoving(_foodOnEatingOther);
                Destroy(Agents[0].gameObject);
            }
            else
            {
                switch (Agents[0].IsAggressive(), Agents[1].IsAggressive())
                {
                    case (true, true):
                        Agents[0].StartMoving(_foodAggressiveWithAggressive);
                        Agents[1].StartMoving(_foodAggressiveWithAggressive);
                        break;
                    case (true, false):
                        Agents[0].StartMoving(_foodAggressiveWithPacifist);
                        Agents[1].StartMoving(_foodPacifistWithAggressive);
                        break;
                    case (false, true):
                        Agents[0].StartMoving(_foodPacifistWithAggressive);
                        Agents[1].StartMoving(_foodAggressiveWithPacifist);
                        break;
                    case (false, false):
                        Agents[0].StartMoving(_foodPacifistWithPacifist);
                        Agents[1].StartMoving(_foodPacifistWithPacifist);
                        break;
                }    
            }
        }
        else
        {
            Agents[0].StartMoving(_foodOnSoloGather);
        }

        Destroy(gameObject);
    }
}
