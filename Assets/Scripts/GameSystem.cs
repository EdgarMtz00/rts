using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    private GameObject[] _objectives;
    public GameObject objectivePrefab;
    public float endTime;
    public int numObjectives = 10;
    
    void Start()
    {
        _objectives = new GameObject[numObjectives];
        for (int i = 0; i < numObjectives; i++)
        {
            float x = Random.Range(-10, 10);
            float z = Random.Range(-10, 10);
            _objectives[i] = GameObject.Instantiate(objectivePrefab, new Vector3(x, 5.0f, z), Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > endTime)
        {
            ScenesManager.LoadLose();
        }
        else
        {
            bool allComplete = true;
            foreach (GameObject objective in _objectives)
            {
                if (objective != null)
                {
                    allComplete = false;
                }
            }
            if (allComplete)
            {
                ScenesManager.LoadWin();
            }
        }
    }
}
