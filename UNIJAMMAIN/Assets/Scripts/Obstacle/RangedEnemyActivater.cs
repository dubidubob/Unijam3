using System.Collections.Generic;
using UnityEngine;
using static GamePlayDefine;

public class RangedEnemyActivater : MonoBehaviour
{
    [System.Serializable]
    private struct RangedEnemyInfo
    {
        public RangedAttackType attackType;
        public GameObject go;
    
    }

    [SerializeField] private float boundaryOffset = 1f;
    [SerializeField] List<RangedEnemyInfo> deactivatedEnemies;
    [SerializeField] private SpriteRenderer spriteRenderer;
    public Sprite LU, LD, RU, RD;

    private List<RangedEnemyInfo> activatedEnemies = new List<RangedEnemyInfo>();
    public DiyongManager Diyong;

    private void Awake()
    {
        if (deactivatedEnemies == null)
        {
            Debug.LogAssertion("List ä�켼��");
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
                if (enemy.go != null)
                {
                    RangedEnemy rangedEnemy = enemy.go.GetComponent<RangedEnemy>();
                    if (rangedEnemy != null) { rangedEnemy.SetDead(); Debug.Log("대체 왜?"); }
                    else 
                    {
                        isExist = true;
                        break;
                    }
                    switch (attackType)
                    {
                        case RangedAttackType.LeftUp:
                            spriteRenderer.sprite = LU;
                            break;
                        case RangedAttackType.LeftDown:
                            spriteRenderer.sprite = LD; ;
                            break;
                        case RangedAttackType.RightUp:
                            spriteRenderer.sprite = RU;
                            break;
                        case RangedAttackType.RightDown:
                            spriteRenderer.sprite = RD;
                            break;
                    }
                    Managers.Sound.Play("Range_Monster_Death_SFX");
                    isExist = true;
                    break;
                }
                else 
                { 
                    Debug.LogWarning("Missing 어쩌고 에러");
                    isExist = false;
                }
            }
        }

        if (!isExist && Managers.Game.GetPhase() > 1) 
        {
            Managers.Game.DecHealth();
            Managers.Tracker.MissedKeyPress(attackType.ToString());
            Diyong.MissedKeyPressedArrow(attackType.ToString());
        }
    }


    float xMin = 0;
    float xMax = 0;
    float yMin = 0;
    float yMax = 0;

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

    public void ActivateEnemy()
    {
        CheckActivated();

        if (deactivatedEnemies.Count == 0) //���� Ȱ��ȭ �Ǿ�������
        {
            return;
        }

        int randomIndex = Random.Range(0, deactivatedEnemies.Count);

        var chosenEnemy = deactivatedEnemies[randomIndex]; // �̸� ���� ������ ��Ƶд�.
        RangedEnemy rangedEnemy = chosenEnemy.go.GetComponent<RangedEnemy>();

        PosAndActivateNode(chosenEnemy);

        activatedEnemies.Add(chosenEnemy); 
        deactivatedEnemies.RemoveAt(randomIndex);
    }

    private void CheckActivated()
    {
        for (int i = activatedEnemies.Count - 1; i >= 0; i--)
        {
            if (!activatedEnemies[i].go.activeSelf)
            {
                RangedEnemyInfo enemy = activatedEnemies[i];
                activatedEnemies.RemoveAt(i);
                deactivatedEnemies.Add(enemy);
            }
        }
    }

    private void PosAndActivateNode(RangedEnemyInfo enemy)
    {
        float randX = 0f;
        float randY = 0f;

        switch (enemy.attackType)
        {
            case RangedAttackType.LeftUp:
                randX = UnityEngine.Random.Range(xMin + boundaryOffset, -boundaryOffset);
                randY = UnityEngine.Random.Range(boundaryOffset, yMax - boundaryOffset);
                break;

            case RangedAttackType.LeftDown:
                randX = UnityEngine.Random.Range(xMin + boundaryOffset, -boundaryOffset);
                randY = UnityEngine.Random.Range(yMin + boundaryOffset, -boundaryOffset);
                break;

            case RangedAttackType.RightUp:
                randX = UnityEngine.Random.Range(boundaryOffset, xMax - boundaryOffset);
                randY = UnityEngine.Random.Range(boundaryOffset, yMax - boundaryOffset);
                break;

            case RangedAttackType.RightDown:
                randX = UnityEngine.Random.Range(boundaryOffset, xMax - boundaryOffset);
                randY = UnityEngine.Random.Range(yMin + boundaryOffset, -boundaryOffset);
                break;
        }

        enemy.go.transform.position = new Vector3(randX, randY, 0f);
        enemy.go.SetActive(true);
    }
}