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


    // [핵심 1] 애니메이션 실행을 위한 캐싱된 Hash ID 변수들
    private int _animID_Start;
    private int _animID_Idle;
    private int _animID_GameOver;

    // TODO : 나중에 이미지가 더 바뀔일이 있다면, 이곳에 추가하고 리팩토링
    public CharacterSpriteSO characterSpriteSO;
    void Start()
    {   
        Managers.Game.actionUI = this;
        

        Managers.Input.InputWasd -= ChangePlayerSprite;
        Managers.Input.InputWasd += ChangePlayerSprite;

        Managers.Input.InputDiagonal -= ChangePlayerSprite_Arrow;
        Managers.Input.InputDiagonal += ChangePlayerSprite_Arrow;

    }

    public void PlayerSpriteInit(int _chapterIdx)
    {
        sp = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
       
        animator.enabled = false;
        animator.speed = 0;

        // [핵심 2] 챕터에 따라 애니메이션 이름 결정 및 ID 캐싱
        // 한 번만 실행되므로 여기서 if문을 쓰는 것은 전혀 문제되지 않습니다.
        string startName = "Start";
        string idleName = "NormalPosing";
        string gameOverName = "GameOver";

        // 크리스마스 챕터 (예시)
        if (_chapterIdx == 9)
        {
            // 이미지 교체
            
            actionImgs = characterSpriteSO.actionImgs;
            sp.sprite = actionImgs[8].sprite;
            Debug.Log(sp.sprite.name);

            // 애니메이션 이름 교체
            startName = "Santa_Start";       // Animator에 등록된 State 이름
            idleName = "Santa_Idle";
            gameOverName = "Santa_GameOver";
        }
        else
        {
            // 기본 챕터는 기존 리스트 사용
            // actionImgs는 인스펙터에 할당된 기본값 사용됨
        }
        origin = sp.sprite;

        // [핵심 3] String을 Int(Hash)로 변환하여 저장 (성능 최적화)
        _animID_Start = Animator.StringToHash(startName);
        _animID_Idle = Animator.StringToHash(idleName);
        _animID_GameOver = Animator.StringToHash(gameOverName);

        // 딕셔너리 세팅
        _actionImgsDic = new Dictionary<GamePlayDefine.AllType, Sprite>();
        foreach (var a in actionImgs)
        {
            _actionImgsDic[a.type] = a.sprite;
        }
    }

    private void OnDestroy()
    {
        Managers.Input.InputWasd -= ChangePlayerSprite;
        Managers.Input.InputDiagonal -= ChangePlayerSprite_Arrow;
    }

    public void StartMonkAnimAfter123Count()
    {
        animator.enabled = true;

        animator.speed = 0.6f;
        // [핵심 4] 저장해둔 ID를 사용하여 재생 (if문 불필요)
        animator.Play(_animID_Start, -1, 0f);

        Invoke("OnAnimationEnd", 1.7f);
    }

    private void OnAnimationEnd()
    {
        animator.speed = 0.2f;
        animator.Play(_animID_Idle, -1, 0.3f); // ID 사용

        this.transform.position = new Vector2(START_POSITION.x, START_POSITION.y);
    }

    private void NormalPosing()
    {
        if (animator != null && !isActioning)
        {
            animator.Play(_animID_Idle, -1, 0.3f); // ID 사용
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
            animator.speed = 0.3f; //  애니메이터 속도를 다시 1로 되돌립니다.
        }
        NormalPosing();
    }

    public void GameOverAnimation()
    {
        CancelInvoke();
        this.transform.position = new Vector2(0.4f, 0.7f);
        isActioning = true;
        animator.enabled = true;
        animator.speed = 0.5f;

        animator.Play(_animID_GameOver, -1, 0f); // ID 사용
    }


    #region Sound

    
    #endregion
}
