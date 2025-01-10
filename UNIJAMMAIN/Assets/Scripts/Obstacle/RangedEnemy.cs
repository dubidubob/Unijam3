using DG.Tweening;
using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    [SerializeField] private float lifeDuration = 1f;
    SpriteRenderer spriteRenderer;
    bool isDying = false;

    private void Awake()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        spriteRenderer.color = Color.white;

        spriteRenderer
            .DOColor(Color.red, lifeDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                DyingAnim();
            });

        transform.localScale = Vector3.one; 
    }

    private void DyingAnim()
    {
        //dying animation

        //scale animation
        transform.DOScale(Vector3.one * 0.8f, 0.2f)
            .OnComplete(() => 
            transform.DOScale(Vector3.one * 1.7f, 0.5f));

        // after fade, set active false
        spriteRenderer
            .DOFade(0f, 0.7f)
            .OnComplete(() =>
            {
                isDying = true;
                SetDead(false);
            } 
        );
    }

    public void SetLifetime(float lifetime)
    {
        lifeDuration = lifetime;
    }

    public void SetDead(bool isAttackedByPlayer = true) 
    {
        if (!isAttackedByPlayer)
        {
            Managers.Game.DecHealth();
        }
        else //player attacked, but late, this not count
        {
            if (isDying)
                return;
        }

        isDying = false;
        this.gameObject.SetActive(false);
        return;
    }
}