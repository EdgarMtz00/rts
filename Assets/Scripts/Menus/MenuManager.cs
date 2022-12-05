using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menus
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private GameObject buildMenu;
        [SerializeField] private GameObject unitMenu;
        [SerializeField] private GameObject countdown;
        
        private float _timer = 0f;
        private bool _shouldAddListener = true;
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
            
            if (_shouldAddListener)
            {
                _shouldAddListener = false;
                unitMenu.GetComponentInChildren<Button>().onClick.AddListener(() =>
                    Instantiate(playerTank, position + new Vector3(0, 0.33f, 0), Quaternion.identity));
            }
        }

        public void Update()
        {
            _timer += Time.deltaTime;
            countdown.GetComponentInChildren<TextMeshProUGUI>().text = 60 - (int) _timer + "s"; 
            
        }

        public void CloseUnitMenu()
        {
            unitMenu.SetActive(false);
        }
    }
}