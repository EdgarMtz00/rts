using UnityEngine;
using UnityEngine.UI;

namespace Menus
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private GameObject buildMenu;
        [SerializeField] private GameObject unitMenu;
        private bool shouldAddListener = true;
        private void Start()
        {
            buildMenu.SetActive(false);
            unitMenu.SetActive(false);
        }

        public void BuildMenu()
        {
            CloseUnitMenu();
            buildMenu.SetActive(true);
        }

        public void CloseBuildMenu()
        {
            buildMenu.SetActive(false);
        }

        public void UnitMenu(GameObject playerTank, Vector3 position)
        {
            CloseBuildMenu();
            unitMenu.SetActive(true);
            
            if (shouldAddListener)
            {
                shouldAddListener = false;
                unitMenu.GetComponentInChildren<Button>().onClick.AddListener(() =>
                    Instantiate(playerTank, position + new Vector3(0, 0.33f, 0), Quaternion.identity));
            }
        }

        public void CloseUnitMenu()
        {
            unitMenu.SetActive(false);
        }
    }
}