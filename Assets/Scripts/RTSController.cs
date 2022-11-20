using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSController : MonoBehaviour
{
    public GameObject selectedObject;
    private Ray ray;
    private RaycastHit hitData;
    
    // Update is called once per frame
    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hitData, 1000) && Input.GetMouseButtonDown(0))
        {
            selectedObject = hitData.transform.gameObject;
            Debug.Log(selectedObject.name);
        }
    }
}
