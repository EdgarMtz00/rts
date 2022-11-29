using UnityEngine;

namespace Units.Movement.Behaviours
{
    public class Seek : AgentBehaviour
    {
        protected override Steering GetSteering()
        {
            Steering steer = new Steering();
            steer.LinearVelocity = targetPosition - transform.position;
            steer.LinearVelocity.Normalize();
            steer.LinearVelocity *= Agent.maxAcceleration;
            return steer;
        }

        public override void Update()
        {
            if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
            {
                Agent.velocity = Vector3.zero;
                Destroy(this);
            }

            base.Update();
        }
        
        public void SetTargetPosition(Vector3 position)
        {
            if (Agent != null)
            {
                Agent.velocity = Vector3.zero;
            }
            this.targetPosition = position;
        }
    }
}
