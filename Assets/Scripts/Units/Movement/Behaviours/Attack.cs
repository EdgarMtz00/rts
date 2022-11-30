using Units.Attack;
using UnityEngine;

namespace Units.Movement.Behaviours
{
    public class Attack : AgentBehaviour
    {
        protected override Steering GetSteering()
        {
            Steering steer = new Steering();
            steer.LinearVelocity = target.transform.position - transform.position;
            steer.LinearVelocity.Normalize();
            steer.LinearVelocity *= Agent.maxAcceleration;
            return steer;
        }

        public override void Update()
        {
            if (target == null)
            {
                Destroy(this);
            }
            else
            {
                transform.LookAt(target.transform);

                if (Vector3.Distance(transform.position, target.transform.position) < Agent.range)
                {
                    // wait for the next attack
                    if (Time.time > Agent.nextAttack)
                    {
                        // attack
                        Agent.nextAttack = Time.time + Agent.fireRate;
                        Missile.SpawnMissile(transform.position, target, Agent.missile);
                    }
                }
                else
                {
                    base.Update();
                }
            }
        }
        
        public void SetTarget(GameObject target)
        {
            if (Agent != null)
            {
                Agent.velocity = Vector3.zero;   
            }
            this.target = target;
        }
    }
}
