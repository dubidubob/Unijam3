using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DiagonalMonster : MonoBehaviour
{
    [SerializeField] GamePlayDefine.DiagonalType diagonalT;
    [SerializeField] Transform targetPos;
    [SerializeField] Sprite outline;
    public GamePlayDefine.DiagonalType DiagonalT => diagonalT;
    
    private bool _isDying = false;
    private int _jumpCnt = 4;

    private Vector3 _originPos;
    private Sprite _originSprite;
    private SpriteRenderer _objectRenderer; // 페이드 아웃을 적용할 객체의 렌더러
    
    private float _duration;
    private Vector3 _stride;
    private float _moveBeat;
    private DG.Tweening.Sequence jumpSequence;

    private int attackValue = 20;
    private int healingValue = 2;

    private void Awake()
    {
        _objectRenderer = GetComponent<SpriteRenderer>();
        _originSprite = _objectRenderer.sprite;
        _originPos = transform.position;
        PauseManager.IsPaused -= PauseForWhile;
        PauseManager.IsPaused += PauseForWhile;
    }

    private void OnEnable()
    {
        _duration = (float)IngameData.BeatInterval;
        _stride = (targetPos.position - _originPos) / _jumpCnt;
        Managers.Sound.Play("SFX/Enemy/Diagonal_V2", Define.Sound.SFX,1f,0.5f);
        Move();
    }

    private void OnDisable()
    {
        transform.DOKill();
        PauseManager.IsPaused -= PauseForWhile;

        var child = GetComponentInChildren<TutorialDiagonalKeyUI>();
        if (child != null)
        {
            child.OnDisableCalledByParent();  // 자식의 OnDisableCalledByParent() 직접 호출
        }
    }
    //TODO : tmp!
    public void SetMovebeat(float moveBeat)
    {
        _moveBeat = (moveBeat/2);
    }

    private void PauseForWhile(bool isStop)
    {
        if (isStop)
            jumpSequence.Pause();
        else 
            jumpSequence.Play();
    }

    private void Move()
    {   
        jumpSequence = DOTween.Sequence();

        // 점프와 대기를 번갈아 가며 설정
        for (int i = 0; i < _jumpCnt; i++)
        {
            Vector3 target = _originPos + _stride * (i + 1);
            jumpSequence.Append(transform.DOJump(
                target,   // 목표 지점
                0.5f,        // 점프 높이
                1,           // 점프 횟수 (한 번만 점프)
                _duration* _moveBeat
            ));

            // 쉬는 시간 (1박자)
            if (i < _jumpCnt - 1) // 마지막 점프는 대기 필요 없음
            {
                jumpSequence.AppendInterval(_duration* _moveBeat);
            }
            else
            {
                jumpSequence.AppendCallback(() =>
                {
                    DoFade();
                });
            }
        }

        jumpSequence.OnComplete(() => {
            SetDead(false);
        });
    }

    private void DoFade()
    {
        _isDying = true;
        _objectRenderer.sprite = outline;
        float delay = 0.2f;
        Invoke("ChangeToOriginal", delay);
        _objectRenderer.material.DOFade(0, _duration- delay).SetDelay(delay); // 마지막 점프 구간에서 페이드 아웃
    }

    private void ChangeToOriginal()
    {
        _objectRenderer.sprite = _originSprite;
    }

    public void SetDead(bool isAttackedByPlayer = true)
    {
        jumpSequence.Kill();
        Managers.Sound.Play("SFX/Enemy/DiagonalSuccess_V2", Define.Sound.SFX,1f,0.2f);
        if (!isAttackedByPlayer)
        {
            Managers.Game.PlayerAttacked(attackValue);
            DoFade();
        }
        else
        {
            if (_isDying) //player attacked, but late, this not count
                return;

            Managers.Game.ComboInc(healingValue);
        }
        _isDying = false;
        gameObject.SetActive(false);
        transform.position = _originPos;
        _objectRenderer.color = new Color(_objectRenderer.color.r, _objectRenderer.color.g, _objectRenderer.color.b, 1);        
    }
}