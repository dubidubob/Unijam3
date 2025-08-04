using System.Collections.Generic;
using UnityEngine;

public class MouseEnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject LeftOne;
    [SerializeField] GameObject RightOne;
    private List<GameObject> Panels = new List<GameObject>();

    private void Start()
    {
        Panels.Add(LeftOne);
        Panels.Add(RightOne);
    }
    public void ActivateRandomPanel()
    {
        int rand = Random.Range(0, 2);
        Panels[rand].SetActive(true);
    }
}

