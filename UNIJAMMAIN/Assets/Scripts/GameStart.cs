using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GameStart : MonoBehaviour
{
    public void ButtonClicked()
    {
        DoTweenScaleUp(this.transform);
    }
    void Start()
    {
        
    }

    public void DoTweenScaleUp(Transform transform, float upScaleAmount = 6.5f)
    {
        // transform.DOScale(Vector3.one * upScaleAmount, 0.2f);
        //   .OnComplete(() => transform.DOScale(Vector3.one, 0.2f));

        transform.DOScaleY(upScaleAmount, 0.5f);
        transform.DOLocalMoveY(0,0.5f);
    }

}
