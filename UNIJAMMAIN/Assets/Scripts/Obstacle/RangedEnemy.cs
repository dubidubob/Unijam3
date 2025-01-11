using System.Threading.Tasks.Sources;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RangedEnemy : MonoBehaviour
{
    private float lifeDuration;

    private SpriteRenderer spriteRenderer;
    private bool isDying = false;

    private Tween colorTween;
    private Tween fadeTween;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lifeDuration = 3f;
    }

    private void OnEnable()
    {
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
        dyingSequence.Append(transform.DOScale(Vector3.one * 0.05f, 0.1f).SetEase(Ease.OutBack));
        dyingSequence.Append(transform.DOMove(new Vector3(0, 0, 0), 0.05f));
        dyingSequence.OnComplete(() =>
        {
            isDying = true;
            SetDead(false);
        });

        // 시퀀스 시작
        dyingSequence.Play();
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
        // 트윈 정리
        colorTween?.Kill();
        fadeTween?.Kill();
    }
}