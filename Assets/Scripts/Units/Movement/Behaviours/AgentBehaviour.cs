using System;
using System.Collections;
using System.Collections.Generic;
using Units.Movement;
using UnityEngine;

public class AgentBehaviour : MonoBehaviour
{
    public float weight = 1.0f;

    public Vector3 target;
    protected Agent Agent;

    public virtual void Start()
    {
        Agent = gameObject.GetComponent<Agent>();
    }
    
    public virtual void Update()
    {
        Agent.SetSteering(GetSteering(), weight);    
    }

    protected virtual Steering GetSteering()
    {
        return new Steering();
    }
}
