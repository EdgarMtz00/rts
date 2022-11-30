using UnityEngine;

namespace Units.Attack
{
    public class Health : MonoBehaviour
    {
        public float maxHealth = 100f;
        [SerializeField] private float health = 100f;
    
        void Start()
        {
            health = maxHealth;    
        }
    
    

        public void TakeDamage(int i)
        {
            health -= i;
            if (health <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Destroy(gameObject);
        }
    }
}
