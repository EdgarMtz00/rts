using UnityEngine;

namespace Units.Movement
{
    public class Steering
    {
        public float AngularRotation;
        public Vector3 LinearVelocity;

        public Steering()
        {
            AngularRotation = 0.0f;
            LinearVelocity = new Vector3();
        }
    }
}
