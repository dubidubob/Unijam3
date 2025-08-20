using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DiagonalMonster : MonoBehaviour
{
    [SerializeField]
    private GamePlayDefine.DiagonalType diagonalT;
    public GamePlayDefine.DiagonalType DiagonalT => diagonalT;

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
        transform.localScale = Vector3.one * 0.37f;

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

    private void DyingAnim() // 살아있는 동안의 액션
    {
        Sequence dyingSequence = DOTween.Sequence();
        dyingSequence.Append(transform.DOScale(Vector3.one * 0.05f, 0.1f).SetEase(Ease.OutBack));
        dyingSequence.Append(transform.DOLocalMove(new Vector3(0, 0, 0), 0.08f));
        dyingSequence.OnComplete(() =>
        {
            isDying = true;
            SetDead(false);
        });

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
        colorTween?.Kill();
        fadeTween?.Kill();
    }
}