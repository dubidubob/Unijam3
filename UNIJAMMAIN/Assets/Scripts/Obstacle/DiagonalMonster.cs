using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DiagonalMonster : MonoBehaviour
{
    [SerializeField] List<float> scales = new List<float>();
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

        baseScale = transform.localScale;
        curCnt = 0;

        BeatClock.OnBeat -= BeatMoving;
        BeatClock.OnBeat += BeatMoving;
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



    private void DyingAnim()
    {
        BeatClock.OnBeat -= BeatMoving;
        if (seq != null && seq.IsActive()) seq.Kill();

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
            Managers.Game.PlayerAttacked();
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
        transform.DOKill();
        transform.localScale = baseScale;
        colorTween?.Kill();
        fadeTween?.Kill();
    }


    int curCnt;
    Vector3 baseScale;
    Sequence seq; 
    void BeatMoving(double __, long _)
    {
        if (curCnt >= scales.Count)
        {
            BeatClock.OnBeat -= BeatMoving;
            return;
        }

        if (seq != null && seq.IsActive() && seq.IsPlaying()) return;

        float beat = (float)IngameData.BeatInterval;

        var targetScale = baseScale * scales[curCnt++];

        if (seq != null && seq.IsActive()) seq.Kill(); 
        seq = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDestroy).SetAutoKill(true);

        
        seq.Append(transform.DOScale(targetScale * 1.15f, beat * 0.3f).SetEase(Ease.OutCubic));
        
        seq.Append(transform.DOScale(targetScale, beat * 0.2f).SetEase(Ease.OutCubic));
        seq.AppendInterval(beat * 0.25f);
    }
}