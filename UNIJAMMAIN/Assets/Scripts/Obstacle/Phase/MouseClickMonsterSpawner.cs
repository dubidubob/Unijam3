using UnityEngine;

public class MouseClickMonsterSpawner : MonoBehaviour, ISpawnable
{
    [SerializeField] GameObject LeftOne;
    [SerializeField] GameObject RightOne;

    public Define.MonsterType MonsterType => Define.MonsterType.MouseClick;

    private void Awake()
    {
        LeftOne.SetActive(false);
        RightOne.SetActive(false);

        Managers.Input.MouseAction -= DeactivateMouse;
        Managers.Input.MouseAction += DeactivateMouse;
    }

    private void DeactivateMouse(GamePlayDefine.MouseType mouseType)
    { 
        GameObject deactivateGo = mouseType == GamePlayDefine.MouseType.Left? LeftOne : RightOne;
        deactivateGo.SetActive(false);
    }

    public void Spawn(MonsterData data)
        {
            if (Random.Range(0, 2) == 0)
                LeftOne.SetActive(true);
            else RightOne.SetActive(true);
        }
}

