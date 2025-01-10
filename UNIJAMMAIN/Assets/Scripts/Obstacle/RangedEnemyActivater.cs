using System.Collections.Generic;
using UnityEngine;
using static GamePlayDefine;

public class RangedEnemyActivater : MonoBehaviour
{
    private struct RangedEnemyInfo
    {
        public RangedAttackType attackType;
        public GameObject go;
    }

    [SerializeField] private float boundaryOffset = 0.5f; // 화면 가장자리로부터 얼마나 안쪽으로 스폰할지 결정하는 값
    [SerializeField] List<RangedEnemyInfo> deactivatedEnemies;
    private List<RangedEnemyInfo> activatedEnemies = new List<RangedEnemyInfo>();

    float xMin = 0;
    float xMax = 0;
    float yMin = 0;
    float yMax = 0;

    float randX = 0f;
    float randY = 0f;

    private void Awake()
    {
        if (deactivatedEnemies == null)
        {
            Debug.LogAssertion("List 채우세요");
        }

        Managers.Input.KeyArrowcodeAction -= DeActivateEnemy;
        Managers.Input.KeyArrowcodeAction += DeActivateEnemy;

        InvestScreenSize();
    }

    private void DeActivateEnemy(RangedAttackType attackType)
    {
        bool isExist = false;
        foreach (RangedEnemyInfo enemy in activatedEnemies)
        {
            if (enemy.attackType == attackType)
            {
                RangedEnemy rangedEnemy = enemy.go.GetComponent<RangedEnemy>();
                if (rangedEnemy != null) { rangedEnemy.SetDead(); }
                isExist = true;
                break;
            }
        }

        if (!isExist) 
        {
            Managers.Game.DecHealth();
        }
    }

    private void InvestScreenSize()
    {
        Camera cam = Camera.main;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        xMin = -halfWidth;
        xMax = halfWidth;
        yMin = -halfHeight;
        yMax = halfHeight;
    }

    private void ActivateEnemy()
    {
        CheckActivated();
        int randomIndex = Random.Range(0, deactivatedEnemies.Count);

        if (randomIndex == 0) return; //전부 활성화되어 있으면

        var chosenEnemy = deactivatedEnemies[randomIndex]; // 미리 적을 변수에 담아둔다.

        PosAndActivateNode(chosenEnemy);

        activatedEnemies.Add(chosenEnemy); // 활성 리스트로 이동
        deactivatedEnemies.RemoveAt(randomIndex); // 비활성 리스트에서 제거
    }

    private void CheckActivated()
    {
        if (activatedEnemies.Count > 0)
        {
            foreach (RangedEnemyInfo enemy in activatedEnemies)
            {
                if (enemy.go.activeSelf)
                {
                    activatedEnemies.Remove(enemy);
                    deactivatedEnemies.Add(enemy);
                }
            }
        }
    }

    private void PosAndActivateNode(RangedEnemyInfo enemy)
    {
        switch (enemy.attackType)
        {
            case RangedAttackType.LeftUp:
                randX = Random.Range(xMin + boundaryOffset, -boundaryOffset);
                randY = Random.Range(boundaryOffset, yMax - boundaryOffset);
                break;


            case RangedAttackType.LeftDown:
                randX = Random.Range(xMin + boundaryOffset, -boundaryOffset);
                randY = Random.Range(yMin + boundaryOffset, -boundaryOffset);
                break;

            case RangedAttackType.RightUp:
                randX = Random.Range(boundaryOffset, xMax - boundaryOffset);
                randY = Random.Range(boundaryOffset, yMax - boundaryOffset);
                break;

            case RangedAttackType.RightDown:
                randX = Random.Range(boundaryOffset, xMax - boundaryOffset);
                randY = Random.Range(yMin + boundaryOffset, -boundaryOffset);
                break;
        }

        enemy.go.transform.position = new Vector3(randX, randY, 0f);
        enemy.go.SetActive(true);
    }
}