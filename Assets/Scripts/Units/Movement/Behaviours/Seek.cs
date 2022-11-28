using UnityEngine;

namespace Units.Movement.Behaviours
{
    public class Seek : AgentBehaviour
    {
        protected override Steering GetSteering()
        {
            Steering steer = new Steering();
            steer.LinearVelocity = target - transform.position;
            steer.LinearVelocity.Normalize();
            steer.LinearVelocity *= Agent.maxAcceleration;
            return steer;
        }

        public override void Update()
        {
            if (Vector3.Distance(transform.position, target) < 0.5f)
            {
                print("Reached target");
                Agent.velocity = Vector3.zero;
                Destroy(this);
            }

            base.Update();
        }
        
        public void SetTarget(Vector3 target)
        {
            Agent.velocity = Vector3.zero;
            this.target = target;
        }
    }
}
