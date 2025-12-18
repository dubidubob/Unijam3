using UnityEngine;
using DG.Tweening;

public class BackGroundEffect_10 : MonoBehaviour, IBackGroundEffect
{
    private BackGroundController _ctrl;

    // 인터페이스 구현: 초기화
    public void Initialize(BackGroundController controller)
    {
        _ctrl = controller;

        // 챕터 10에 필요한 초기 설정 (예: 타겟 카운트, 이미지 활성화 등)
        _ctrl.ActionNumberTarget = 1; // 1박자마다 실행

        // 필요한 이미지 활성화 및 스프라이트 세팅
        // (Controller에 public 프로퍼티를 열어두어야 접근 가능합니다)
        if (_ctrl.ExtraImage1 != null)
        {
            _ctrl.ExtraImage1.gameObject.SetActive(true);
            // _ctrl.ExtraImage1.sprite = ... (필요시 데이터 접근)
            _ctrl.UpdateRectMargin(_ctrl.ExtraImage1.rectTransform, 0);
        }
        // 나머지 이미지들도 필요하면 세팅...
    }

    // 인터페이스 구현: 비트 액션
    public void EffectActionGo()
    {
        if (_ctrl == null || _ctrl.ExtraImage1 == null) return;

        // 여기에 챕터 10만의 연출 로직 작성
        RectTransform rect = _ctrl.ExtraImage1.rectTransform;

        rect.DOKill();

        // 예시: 회전 효과
        Sequence seq = DOTween.Sequence();
        seq.SetTarget(rect.gameObject);

        seq.Append(rect.DOPunchRotation(new Vector3(0, 0, 15f), _ctrl.BeatDuration * 0.5f, 10, 1));
    }
}