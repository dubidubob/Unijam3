using DG.Tweening;
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

    private void Awake()
    {
        IngameData.BeatInterval = 84 / 60;
        _objectRenderer = GetComponent<SpriteRenderer>();
        _originSprite = _objectRenderer.sprite;
        _originPos = transform.position;
    }

    private void OnEnable()
    {
        _duration = (float)IngameData.BeatInterval;
        _stride = (targetPos.position - _originPos) / _jumpCnt;
        Move();
    }

    private void OnDisable()
    {
        DOTween.Kill(transform);
    }

    private void Move()
    {   
        Sequence jumpSequence = DOTween.Sequence();

        // ������ ��⸦ ������ ���� ����
        for (int i = 0; i < _jumpCnt; i++)
        {
            Vector3 target = _originPos + _stride * (i + 1);
            jumpSequence.Append(transform.DOJump(
                target,   // ��ǥ ����
                0.5f,        // ���� ����
                1,           // ���� Ƚ�� (�� ���� ����)
                _duration
            ));

            // ���� �ð� (1����)
            if (i < _jumpCnt - 1) // ������ ������ ��� �ʿ� ����
            {
                jumpSequence.AppendInterval(_duration);
            }
            else
            {
                jumpSequence.AppendCallback(() =>
                {
                    DoFade();
                });
            }
        }

        jumpSequence.OnKill(() => {
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
        if (!isAttackedByPlayer)
        {
            Managers.Game.PlayerAttacked();
            DoFade();
        }
        else
        {
            if (_isDying) //player attacked, but late, this not count
                return;

            Managers.Game.ComboInc();
        }

        _isDying = false;
        gameObject.SetActive(false);
        transform.position = _originPos;
        _objectRenderer.color = new Color(_objectRenderer.color.r, _objectRenderer.color.g, _objectRenderer.color.b, 1);        
    }
}