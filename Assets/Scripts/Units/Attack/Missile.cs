using UnityEngine;

namespace Units.Attack
{
    public class Missile : MonoBehaviour
    {
        public GameObject target;
        public float speed = 10f;

        public static void SpawnMissile(Vector3 position, GameObject target, GameObject projectile)
        {
            position = Vector3.MoveTowards(position, target.transform.position, 1f);
            var missileObject = Instantiate(projectile, position, Quaternion.identity);
            var missileComponent = missileObject.AddComponent<Missile>();
            missileComponent.target = target;
        }
        
        void Update()
        {
            if(target == null)
            {
                Destroy(gameObject);
                return;
            }
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            other.gameObject.GetComponent<Health>().TakeDamage(10);
            Destroy(gameObject);
        }
    }
}
