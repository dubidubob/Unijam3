using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class BackGroundEffect_10 : MonoBehaviour, IBackGroundEffect
{
    /* 설명
    EDM 스테이지
    클럽, 반짝거림.

    Image 0 -> 왼쪽 오른쪽 손

    Image 3 -> 앞쪽 손 

    Image 4 -> 미러볼

    Image 5 -> 미러볼의 불빛

    Image 6 -> 개

    Image 7-> ColorEffect

   

    */

    public static Color CurrentClubColor = Color.white;

    private BackGroundController _ctrl;

    private bool isSwayLeft = true; // 손 흔드는 방향 토글 변수

    [Header("Settings")]
    [Range(0f, 1f)] float image6MaxAlpha = 0.03f;

    // 인터페이스 구현: 초기화
    public void Initialize(BackGroundController controller)
    {
        _ctrl = controller;

       
        // 필요한 이미지 활성화 및 스프라이트 세팅
        if (_ctrl.ExtraImage1 != null)
        {
            SettingBackGround();

            StartSmoothColorLoop(_ctrl.extraObjectImage4, 1.0f);
            StartSmoothColorLoop(_ctrl.extraObjectImage5, 1.0f);
            StartSmoothColorLoop(_ctrl.extraObjectImage7, image6MaxAlpha); // Color Effect
        }
    }

    private bool oneTwo = false;

    // 인터페이스 구현: 비트 액션
    public void EffectActionGo()
    {
        if (_ctrl == null) return;

        float duration = _ctrl.beatDuration; // 컨트롤러의 비트 간격 가져오기

        // -------------------------------------------------------
        // 1. Image 0: 군중 손 (Hands Up + 좌우 흔들기)
        // -------------------------------------------------------
        if (_ctrl.extraObjectImage != null)
        {
            RectTransform rect0 = _ctrl.extraObjectImage.rectTransform;
            rect0.DOKill(true); // 이전 트윈 완료 후 제거

            // A. 위로 점프 (Hands Up)
            // Y축으로 50만큼 솟구쳤다가 돌아옴 (Elastic으로 탄력감 부여)
            rect0.DOPunchAnchorPos(new Vector2(0, 50f), duration, 1, 0)
                 .SetEase(Ease.OutQuad);

            // B. 좌우 흔들기 (Wave)
            // 이번엔 왼쪽(-5도), 다음엔 오른쪽(5도)
            float targetAngle = isSwayLeft ? 5f : -5f;
            rect0.DORotate(new Vector3(0, 0, targetAngle), duration)
                 .SetEase(Ease.InOutSine); // 부드럽게 기우뚱

            isSwayLeft = !isSwayLeft; // 방향 전환
        }

        // -------------------------------------------------------
        // 2. Image 2: 앞쪽 손 (약한 바운스)
        // -------------------------------------------------------
        if (_ctrl.extraObjectImage3 != null)
        {
            RectTransform rect2 = _ctrl.extraObjectImage3.rectTransform;
            rect2.DOKill(true);

            // Image 0보다 약하게(30) 짧게 튀어오름
            rect2.DOPunchAnchorPos(new Vector2(0, 30f), duration, 1, 0)
                 .SetEase(Ease.OutQuad);
        }

        // -------------------------------------------------------
        // 3. Image 4, 5, 6: 조명 (색은 자동, 여기선 밝기만 펀치)
        // -------------------------------------------------------
        // 비트가 칠 때마다 불빛이 "번쩍(Flash)" 하는 느낌만 추가
        PunchAlpha(_ctrl.extraObjectImage4, 0.3f, 1.0f, duration);
        PunchAlpha(_ctrl.extraObjectImage5, 0.5f, 1.0f, duration);
        float minAlpha = image6MaxAlpha * 0.3f;

        if(_ctrl.extraObjectImage6!=null)
        {
            if (oneTwo)
            {
                _ctrl.extraObjectImage6.sprite = _ctrl.backGrounddataSO.extraBackGroundDatas[1].animationBackGroundSprites[0];
                oneTwo = false;
            }
            else
            {
                _ctrl.extraObjectImage6.sprite = _ctrl.backGrounddataSO.extraBackGroundDatas[1].animationBackGroundSprites[1];
                oneTwo = true;
            }
        }



        PunchAlpha(_ctrl.extraObjectImage7, minAlpha, image6MaxAlpha, duration);

    }


    private void SettingBackGround()
    {
        _ctrl.actionNumberTarget = 1;
        _ctrl.extraObjectImage.sprite = _ctrl.backGrounddataSO.extraBackGroundDatas[1].extraBackGroundLists[0];
        _ctrl.extraObjectImage2.sprite = _ctrl.backGrounddataSO.extraBackGroundDatas[1].extraBackGroundLists[1];
        _ctrl.extraObjectImage3.sprite = _ctrl.backGrounddataSO.extraBackGroundDatas[1].extraBackGroundLists[2];
        _ctrl.extraObjectImage4.sprite = _ctrl.backGrounddataSO.extraBackGroundDatas[1].extraBackGroundLists[3];
        _ctrl.extraObjectImage5.sprite = _ctrl.backGrounddataSO.extraBackGroundDatas[1].extraBackGroundLists[4];
        _ctrl.extraObjectImage6.sprite = _ctrl.backGrounddataSO.extraBackGroundDatas[1].extraBackGroundLists[5];
        _ctrl.extraObjectImage7.sprite = _ctrl.backGrounddataSO.extraBackGroundDatas[1].extraBackGroundLists[6];


        _ctrl.extraObjectImage.gameObject.SetActive(true);
        _ctrl.extraObjectImage2.gameObject.SetActive(true);
        _ctrl.extraObjectImage3.gameObject.SetActive(true);
        _ctrl.extraObjectImage4.gameObject.SetActive(true);
        _ctrl.extraObjectImage5.gameObject.SetActive(true);
        _ctrl.extraObjectImage6.gameObject.SetActive(true);
        _ctrl.extraObjectImage7.gameObject.SetActive(true);



        _ctrl.UpdateRectMargin(_ctrl.extraObjectImage.rectTransform, 0);
        _ctrl.UpdateRectMargin(_ctrl.extraObjectImage2.rectTransform, 0);
        _ctrl.UpdateRectMargin(_ctrl.extraObjectImage3.rectTransform, 0);
        _ctrl.UpdateRectMargin(_ctrl.extraObjectImage4.rectTransform, 0);
        _ctrl.UpdateRectMargin(_ctrl.extraObjectImage5.rectTransform, 0);
        _ctrl.UpdateRectMargin(_ctrl.extraObjectImage6.rectTransform, 0);
        _ctrl.UpdateRectMargin(_ctrl.extraObjectImage7.rectTransform, 0);

        _ctrl.extraObjectImage.rectTransform.anchoredPosition += new Vector2(0, -100);
    
    }

    // -----------------------------------------------------------------------
    // [Helper] 무지개색 무한 루프 함수
    // -----------------------------------------------------------------------
    private void StartSmoothColorLoop(Image targetImg, float targetAlpha)
    {
        if (targetImg == null) return;
        targetImg.DOKill();

        Gradient rainbow = new Gradient();
        GradientColorKey[] colors = new GradientColorKey[7];
        colors[0] = new GradientColorKey(Color.red, 0f);
        colors[1] = new GradientColorKey(new Color(1, 0.5f, 0), 0.15f);
        colors[2] = new GradientColorKey(Color.yellow, 0.3f);
        colors[3] = new GradientColorKey(Color.green, 0.5f);
        colors[4] = new GradientColorKey(Color.cyan, 0.65f);
        colors[5] = new GradientColorKey(Color.blue, 0.8f);
        colors[6] = new GradientColorKey(Color.magenta, 1f);

        // [핵심] 알파 키 값을 전달받은 targetAlpha로 설정
        GradientAlphaKey[] alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(targetAlpha, 0);
        alphas[1] = new GradientAlphaKey(targetAlpha, 1);

        rainbow.SetKeys(colors, alphas);

        DOVirtual.Float(0f, 1f, 3f, (t) =>
        {
            Color calculatedColor = rainbow.Evaluate(t);

            if (targetImg != null)
                targetImg.color = rainbow.Evaluate(t);

            calculatedColor.a = 1.0f;
            CurrentClubColor = calculatedColor;
        })
        .SetLoops(-1, LoopType.Restart)
        .SetEase(Ease.Linear)
        .SetTarget(targetImg.gameObject);
    }

    // -----------------------------------------------------------------------
    // [Helper] 비트에 맞춰 투명도(Alpha)를 펀치 주는 함수
    // -----------------------------------------------------------------------
    private void PunchAlpha(Image img, float minAlpha, float maxAlpha, float duration)
    {
        if (img == null) return;

        // minAlpha로 흐려졌다가 -> maxAlpha로 복구
        img.DOFade(minAlpha, duration * 0.1f).OnComplete(() =>
        {
            img.DOFade(maxAlpha, duration * 0.9f);
        });
    }

    // 오브젝트 파괴 시 트윈 정리 (메모리 누수 방지)
    private void OnDestroy()
    {
        if (_ctrl != null)
        {
            _ctrl.extraObjectImage?.DOKill();
            _ctrl.extraObjectImage2?.DOKill();
            _ctrl.extraObjectImage4?.DOKill();
            _ctrl.extraObjectImage5?.DOKill();
            _ctrl.extraObjectImage6?.DOKill();
            _ctrl.extraObjectImage7?.DOKill();
        }
        CurrentClubColor = Color.white;
    }

}