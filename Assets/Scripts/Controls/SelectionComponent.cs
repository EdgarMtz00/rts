using UnityEngine;

namespace Controls
{
    public class SelectionComponent : MonoBehaviour
    {
        void Start()
        {
            GetComponent<Renderer>().material.color = Color.red;
        }

        private void OnDestroy()
        {
            GetComponent<Renderer>().material.color = Color.white;
        }
    }
}
