using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnCollisionEnter(Collision collisionInfo)
    {
        Debug.Log("Collision");
        if (collisionInfo.transform.gameObject.CompareTag("Agent"))
        {
            Debug.Log("Collision with Agent");
        }
    }
    
    private void OnCollisionStay(Collision collisionInfo)
    {
        if (collisionInfo.transform.gameObject.CompareTag("Agent"))
        {
            Debug.Log("Collision with Agent");
        }
    }

    public void OnCollisionExit(Collision other)
    {
        Debug.Log("Collision exit");
    }
}
