using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RangedEnemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField]
    private float defaultLifeDuration = 2f;

    private float lifeDuration;
    private SpriteRenderer spriteRenderer;
    private bool isDying = false;

    private Tween colorTween;
    private Tween fadeTween;

    public void SetLifetime(float lifetime)
    {
        Debug.Log($"this setlifetime {lifetime}");
        lifeDuration = lifetime;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lifeDuration = defaultLifeDuration;
    }

    private void OnEnable()
    {
        Debug.Log($"this lifeDuration {lifeDuration}");
        Init();
    }
    private void Init()
    {
        isDying = false;

        spriteRenderer.color = Color.white;
        transform.localScale = Vector3.one;

        colorTween?.Kill();
        fadeTween?.Kill();

        colorTween = spriteRenderer
            .DOColor(Color.red, lifeDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                DyingAnim();
            });

     
    }

    private void DyingAnim()
    {
        // 크기 변화 시퀀스 생성
        Sequence dyingSequence = DOTween.Sequence();
        dyingSequence.Append(transform.DOScale(Vector3.one * 0.8f, 0.2f).SetEase(Ease.OutBack));
        dyingSequence.Append(transform.DOScale(Vector3.one * 1.7f, 0.5f).SetEase(Ease.InBack));
        dyingSequence.Play();

        // 페이드 아웃 트윈 시작
        fadeTween = spriteRenderer.DOFade(0f, 0.7f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                isDying = true;
                SetDead(false);
            });
    }



    public void SetDead(bool isAttackedByPlayer = true)
    {
        if (!isAttackedByPlayer)
        {
            Managers.Game.DecHealth();
        }
        else 
        {
            if (isDying) //player attacked, but late, this not count
                return;

            Managers.Game.ComboInc();
        }

        isDying = false;
        this.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        lifeDuration = defaultLifeDuration;

        // 트윈 정리
        colorTween?.Kill();
        fadeTween?.Kill();
    }
}