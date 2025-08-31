using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ActionImg 
{
    public GamePlayDefine.AllType type;
    public Sprite sprite;
}
public class PlayerActionUI : MonoBehaviour
{
    [SerializeField] List<ActionImg> actionImgs = new List<ActionImg>();
    private Dictionary<GamePlayDefine.AllType, Sprite> _actionImgsDic;
    private SpriteRenderer sp;
    private Sprite origin;
    private Animator animator;
    private bool isActioning = false;
    void Start()
    {
        sp = GetComponent<SpriteRenderer>();
        origin = sp.sprite;

        animator = GetComponent<Animator>();
        animator.speed = 0; // ���� �� ����

        _actionImgsDic = new Dictionary<GamePlayDefine.AllType, Sprite>();
        foreach (var a in actionImgs)
        {
            _actionImgsDic[a.type] = a.sprite;
        }

        Managers.Input.InputWasd -= ChangePlayerSprite;
        Managers.Input.InputWasd += ChangePlayerSprite;

        Managers.Input.InputDiagonal -= ChangePlayerSprite_Arrow;
        Managers.Input.InputDiagonal += ChangePlayerSprite_Arrow;
    }

    private void OnDestroy()
    {
        Managers.Input.InputWasd -= ChangePlayerSprite;
        Managers.Input.InputDiagonal -= ChangePlayerSprite_Arrow;
    }

    public void StartAnimation()
    {
        animator.speed = 1;
        animator.Play("MonkStarting", -1, 0f); // ���ϴ� stateName ����
        Invoke("OnAnimationEnd", 1.7f);
    }
    private void OnAnimationEnd()
    {
        animator.Play("NormalPosing", -1, 0f);
    }

    private void NormalPosing()
    {
        // _isActioning�� �ƴ� ���� �븻 ��¡�� ����մϴ�.
        if (animator != null && !isActioning)
        {
            animator.Play("NormalPosing", -1, 0f);
        }
    }

    private void StopNormalPosing()
    {
        //  �׼��� ���۵� �� ���¸� �����ϰ� �ִϸ����͸� ������ŵ�ϴ�.
        isActioning = true;
        if (animator != null)
        {
            animator.speed = 0;
        }
    }
    private void ChangePlayerSprite(GamePlayDefine.WASDType type)
    {
        StopNormalPosing();
        // �ִϸ����� ��Ȱ��ȭ
        animator.enabled = false;

        sp.sprite = _actionImgsDic[Util.WASDTypeChange(type)];
        Invoke("Recover", 0.4f);
    }

    private void ChangePlayerSprite_Arrow(GamePlayDefine.DiagonalType type)
    {
        StopNormalPosing();
        // �ִϸ����� ��Ȱ��ȭ
        animator.enabled = false;

        sp.sprite = _actionImgsDic[Util.DiagonalTypeChange(type)];
        Invoke("Recover", 0.4f);
    }

    private void Recover()
    {
        sp.sprite = origin;
        isActioning = false;
        animator.enabled = true;
        if (animator != null)
        {
            animator.speed = 1; //  �ִϸ����� �ӵ��� �ٽ� 1�� �ǵ����ϴ�.
        }
        NormalPosing();
    }
        
    public void GameOverAnimation()
    {
        isActioning = true;
        animator.enabled = true;

        animator.Play("Die");
    }


    #region Sound

    private void SetAttackSound()
    {
        // Generates a random integer from 0 (inclusive) to 4 (exclusive), so the result is 0, 1, 2, or 3.
        int random = UnityEngine.Random.Range(0, 4);

        switch (random)
        {
            case 0:
                Managers.Sound.Play("SFX/Accuracy/Perfect1_V1", Define.Sound.SFX);
                break;
            case 1:
                Managers.Sound.Play("SFX/Accuracy/Perfect2_V1", Define.Sound.SFX);
                break;
            case 2:
                Managers.Sound.Play("SFX/Accuracy/Perfect3_V1", Define.Sound.SFX);
                break;
            case 3:
                Managers.Sound.Play("SFX/Accuracy/Perfect4_V1", Define.Sound.SFX);
                break;
        }
    }
    #endregion
}
