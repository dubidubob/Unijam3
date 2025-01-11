using System.Collections.Generic;
using UnityEngine;

public class MouseEnemyActivater : MonoBehaviour
{
    [SerializeField] GameObject LeftOne;
    [SerializeField] GameObject RightOne;
    private List<GameObject> Panels = new List<GameObject>();

    private void Start()
    {
        Panels.Add(LeftOne);
        Panels.Add(RightOne);
    }
    public void ActivateRandomPanel(bool canTwo)
    {
        int endIdx = 2;
        if (canTwo)
            endIdx++;

        int rand = Random.Range(0, endIdx);
        if (rand == 3)
        {
            LeftOne.SetActive(true);
            RightOne.SetActive(true);
        }
        else
        {
            foreach (GameObject go in Panels)
            { 
                go.SetActive(true);
            }
        }
    }
}

