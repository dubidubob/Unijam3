using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class BackGroundEffect_11 : MonoBehaviour, IBackGroundEffect
{
    // extraobject에 대한 설명 
    // 2,3 -> 불빛, target2_Container의 불빛의 rotationZ를 0->-15 정도까지 왔다갔다하게할것
    // 2는 오른쪽 불빛 z -> -15로
    // 3은 왼쪽 불빛임 z -> 15로 움직여야할 것임

    private BackGroundController _ctrl;

    // 비트를 계산하기 위한 카운터 변수
    private int beatCount = 0;

    // extraObjectImage(1번)의 켜짐/꺼짐 상태를 추적하기 위한 변수
    private bool isExtraImageOn = true;

    // 컨트롤러의 데이터(이미지, 비트 시간 등)를 가져오기 위한 초기화
    public void Initialize(BackGroundController controller)
    {
        _ctrl = controller;
        SettingBackGround();
    }

    // 박자마다 비트가 떨어질 때 실행할 메인 액션
    public void EffectActionGo()
    {
        if (_ctrl == null) return;

        beatCount++;
        float duration = _ctrl.beatDuration;

        // -------------------------------------------------------
        // 1. extraObjectImage: 2박자마다 알파값 0 <-> 1 (켜졌다 꺼졌다)
        // -------------------------------------------------------
        if (beatCount % 2 == 0) // 2박자마다 실행
        {
            _ctrl.extraObjectImage.DOKill(false); // 진행 중인 페이드 중지

            float targetAlpha = isExtraImageOn ? 0f : 1f;

            // 너무 팍팍 꺼지면 눈이 아플 수 있으므로 비트 시간에 맞춰 살짝 부드럽게 페이드
            _ctrl.extraObjectImage.DOFade(targetAlpha, duration * 0.8f)
                .SetEase(Ease.InOutQuad);

            isExtraImageOn = !isExtraImageOn; // 상태 반전
        }

        // -------------------------------------------------------
        // 2. extraObjectImage2 & 3: 8박자 단위로 움직이는 불빛 모션
        // -------------------------------------------------------
        if (beatCount % 8 == 0) // 8박자마다 실행 (8~9박자 느낌)
        {
            RectTransform lightRight = _ctrl.target2_Container;
            RectTransform lightLeft = _ctrl.target3_Container;

            lightRight.DOKill(true);
            lightLeft.DOKill(true);

            // 시선이 불편하지 않도록, 타겟 각도에 살짝 랜덤값을 주어 약간 불규칙하게 만듬
            float rightTargetZ = Random.Range(-13f, -17f); // 약 -15도 언저리
            float leftTargetZ = Random.Range(13f, 17f);    // 약 15도 언저리

            // 왕복(Yoyo) 애니메이션이므로, 편도 이동 시간을 길게(비트의 3.5배) 잡아서 서서히 움직이게 함
            float sweepDuration = duration * 3.5f;

            // 오른쪽 불빛 애니메이션 (Z축 회전 후 원래 자리로)
            lightRight.DORotate(new Vector3(0, 0, rightTargetZ), sweepDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(2, LoopType.Yoyo); // 타겟 각도로 갔다가 다시 0으로 돌아옴

            // 왼쪽 불빛 애니메이션
            // 오른쪽과 완전히 똑같이 움직이면 인위적이므로 duration에 아주 살짝(1.1배) 차이를 줌
            lightLeft.DORotate(new Vector3(0, 0, leftTargetZ), sweepDuration * 1.1f)
                .SetEase(Ease.InOutSine)
                .SetLoops(2, LoopType.Yoyo);
        }
    }

    private void SettingBackGround()
    {
        _ctrl.actionNumberTarget = 1;
        _ctrl.extraObjectImage.sprite = _ctrl.backGrounddataSO.extraBackGroundDatas[2].extraBackGroundLists[0];
        _ctrl.extraObjectImage2.sprite = _ctrl.backGrounddataSO.extraBackGroundDatas[2].extraBackGroundLists[1];
        _ctrl.extraObjectImage3.sprite = _ctrl.backGrounddataSO.extraBackGroundDatas[2].extraBackGroundLists[2];
        _ctrl.extraObjectImage4.sprite = _ctrl.backGrounddataSO.extraBackGroundDatas[2].extraBackGroundLists[3];

        _ctrl.extraObjectImage.gameObject.SetActive(true);
        _ctrl.extraObjectImage2.gameObject.SetActive(true);
        _ctrl.extraObjectImage3.gameObject.SetActive(true);
        _ctrl.extraObjectImage4.gameObject.SetActive(true);

        _ctrl.UpdateRectPosition(_ctrl.extraObjectImage.rectTransform, 0, 268);
        _ctrl.UpdateRectPosition(_ctrl.extraObjectImage2.rectTransform, 200, 470);
        _ctrl.UpdateRectPosition(_ctrl.extraObjectImage3.rectTransform, -200, 470);
        _ctrl.UpdateRectMargin(_ctrl.extraObjectImage4.rectTransform, 0);

        _ctrl.extraObjectImage2.GetComponent<Transform>().SetParent(_ctrl.target2_Container.GetComponent<Transform>());
        _ctrl.extraObjectImage3.GetComponent<Transform>().SetParent(_ctrl.target3_Container.GetComponent<Transform>());

        // 초기 알파값 및 회전값 세팅
        Color c = _ctrl.extraObjectImage.color;
        c.a = 1f;
        _ctrl.extraObjectImage.color = c;
        isExtraImageOn = true;

        _ctrl.extraObjectImage2.rectTransform.localRotation = Quaternion.identity;
        _ctrl.extraObjectImage3.rectTransform.localRotation = Quaternion.identity;
    }

    private void OnDestroy()
    {
        if (_ctrl != null)
        {
            _ctrl.extraObjectImage?.DOKill();
            _ctrl.extraObjectImage2?.DOKill();
            _ctrl.extraObjectImage3?.DOKill(); // 3번도 DOKill 추가
            _ctrl.extraObjectImage4?.DOKill();
            _ctrl.extraObjectImage5?.DOKill();
            _ctrl.extraObjectImage6?.DOKill();
            _ctrl.extraObjectImage7?.DOKill();
        }
    }
}