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
    private SpriteRenderer _objectRenderer; // ���̵� �ƿ��� ������ ��ü�� ������
    
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
            child.OnDisableCalledByParent();  // �ڽ��� OnDisableCalledByParent() ���� ȣ��
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

        // ������ ��⸦ ������ ���� ����
        for (int i = 0; i < _jumpCnt; i++)
        {
            Vector3 target = _originPos + _stride * (i + 1);
            jumpSequence.Append(transform.DOJump(
                target,   // ��ǥ ����
                0.5f,        // ���� ����
                1,           // ���� Ƚ�� (�� ���� ����)
                _duration* _moveBeat
            ));

            // ���� �ð� (1����)
            if (i < _jumpCnt - 1) // ������ ������ ��� �ʿ� ����
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
        _objectRenderer.material.DOFade(0, _duration- delay).SetDelay(delay); // ������ ���� �������� ���̵� �ƿ�
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