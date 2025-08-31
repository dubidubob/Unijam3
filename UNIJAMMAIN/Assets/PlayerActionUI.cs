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
    // 모든 애니메이션이 시작될 위치를 정의합니다.
    private static readonly Vector3 START_POSITION = new Vector3(-0.2f, 0.7f, 0f);


    [SerializeField] List<ActionImg> actionImgs = new List<ActionImg>();
    private Dictionary<GamePlayDefine.AllType, Sprite> _actionImgsDic;
    private SpriteRenderer sp;
    private Sprite origin;
    private Animator animator;
    private bool isActioning = false;
    void Start()
    {   
        Managers.Game.actionUI = this;
        sp = GetComponent<SpriteRenderer>();
        origin = sp.sprite;

        animator = GetComponent<Animator>();
        animator.speed = 0; // 시작 시 정지

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
        animator.speed = 0.6f;
        animator.Play("Start", -1, 0f); // 원하는 stateName 실행
        this.transform.position = new Vector2(-0.2f, 0.7f);
        Invoke("OnAnimationEnd", 1.7f); 
    }
    private void OnAnimationEnd()
    {
        animator.speed = 0.2f;
        animator.Play("NormalPosing", -1, 0.2f);
        this.transform.position = new Vector2(START_POSITION.x, START_POSITION.y);
    }

    private void NormalPosing()
    {
        // _isActioning이 아닐 때만 노말 포징을 재생합니다.
        if (animator != null && !isActioning)
        {
            animator.Play("NormalPosing", -1, 0.2f);
            animator.speed = 0.2f;
        }
    }

    private void StopNormalPosing()
    {
        //  액션이 시작될 때 상태를 변경하고 애니메이터를 정지시킵니다.
        isActioning = true;
        if (animator != null)
        {
            animator.speed = 0;
        }
    }
    private void ChangePlayerSprite(GamePlayDefine.WASDType type)
    {
        if (Managers.Game.currentPlayerState == GameManager.PlayerState.Die)
        {
            CancelInvoke("Recover"); // 게임오버 상태일 때 Invoke를 취소합니다.
            return;
        }

        StopNormalPosing();
        // 애니메이터 비활성화
        animator.enabled = false;

        sp.sprite = _actionImgsDic[Util.WASDTypeChange(type)];
        Invoke("Recover", 0.4f);
    }



    private void ChangePlayerSprite_Arrow(GamePlayDefine.DiagonalType type)
    {
        if (Managers.Game.currentPlayerState == GameManager.PlayerState.Die)
        {
            CancelInvoke("Recover"); // 게임오버 상태일 때 Invoke를 취소합니다.
            return;
        }

        StopNormalPosing();
        // 애니메이터 비활성화
        animator.enabled = false;

        sp.sprite = _actionImgsDic[Util.DiagonalTypeChange(type)];
        Invoke("Recover", 0.4f);
    }

    private void Recover()
    {

        if (Managers.Game.currentPlayerState == GameManager.PlayerState.Die)
        {
            return; // 죽은 상태에서는 복구하지 않습니다.
        }
        sp.sprite = origin;
        isActioning = false;
        animator.enabled = true;
        if (animator != null)
        {
            animator.speed = 0.2f; //  애니메이터 속도를 다시 1로 되돌립니다.
        }
        NormalPosing();
    }

    public void GameOverAnimation()
    {
        // 게임 오버 애니메이션 시작 시, 다른 모든 Invoke를 취소합니다.
        CancelInvoke();
        this.transform.position = new Vector2(0.4f, 0.7f); 
        isActioning = true;
        animator.enabled = true;
        animator.speed = 0.5f;
        animator.Play("GameOver", -1, 0f);
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
