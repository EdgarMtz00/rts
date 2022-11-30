using UnityEngine;
using UnityEngine.UI;

namespace Menus
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private GameObject buildMenu;
        
        private void Start()
        {
            buildMenu.SetActive(false);
        }
        
        public void BuildMenu()
        {
            buildMenu.SetActive(true);
        }

        public void CloseMenu()
        {
            buildMenu.SetActive(false);
        }
    }
}
