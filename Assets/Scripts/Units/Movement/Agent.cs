using UnityEngine;

namespace Units.Movement
{
    public class Agent : MonoBehaviour
    {
        public float maxSpeed = 3.0f;
        public float maxAcceleration = 0.5f;
        public float range = 10.0f;
        public float speedLimit;
        public float fireRate = 0.5f;

        public float orientation;
        public float rotation;
        public Vector3 velocity;
        protected Steering Steering;
        public GameObject missile;
        public float nextAttack;

        void Start()
        {
            velocity = Vector3.zero;
            Steering = new Steering();
            speedLimit = maxSpeed;
        }

        public void SetSteering(Steering steering, float weight)
        {
            this.Steering.LinearVelocity += weight * steering.LinearVelocity;
            this.Steering.AngularRotation += weight * steering.AngularRotation;
        }
    
        public void Update()
        {
            Vector3 displacement = velocity * Time.deltaTime;
            displacement.y = 0;

            orientation += rotation * Time.deltaTime;

            if (orientation < 0.0f)
            {
                orientation += 360.0f;
            }else if (orientation <= 360.0f)
            {
                orientation -= 360.0f;
            }
        
            transform.Translate(displacement, Space.World);
            transform.rotation = new Quaternion();
            transform.Rotate(Vector3.up, orientation);
        }

        public void LateUpdate()
        {
            velocity += Steering.LinearVelocity * Time.deltaTime;
            rotation += Steering.AngularRotation * Time.deltaTime;
            if (velocity.magnitude < speedLimit)
            {
                velocity.Normalize();
                velocity *= speedLimit;
            }

            if (Steering.LinearVelocity.magnitude == 0.0f)
            {
                velocity = Vector3.zero;
            }

            Steering = new Steering();
        }

        public void ResetSpeedLimit()
        {
            speedLimit = maxSpeed;
        }
    }
}
