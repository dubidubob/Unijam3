using DG.Tweening;
using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    private float lifeDuration;
    SpriteRenderer spriteRenderer;
    bool isDying = false;
    bool isDurationLocked = false;

    private void Awake()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        lifeDuration = 2f;
        isDurationLocked = false;
    }

    private void OnEnable()
    {
        Debug.Log($"this lifeDuration {lifeDuration}");
        if(transform.gameObject.activeSelf)
            isDurationLocked = true;
        Init();
    }

    private void OnDisable()
    {
        isDurationLocked = false;
    }

    private void Init()
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
        Debug.Log($"this setlifetime {lifetime}, {isDurationLocked}");
        if(!isDurationLocked)
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
            Managers.Game.ComboInc();
            if (isDying)
                return;
        }

        isDying = false;
        this.gameObject.SetActive(false);
        return;
    }
}