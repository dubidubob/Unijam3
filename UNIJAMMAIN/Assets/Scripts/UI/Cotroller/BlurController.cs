using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks; // UniTask 추가
using System.Threading;      // CancellationToken 추가

public class BlurController : MonoBehaviour
{
    [SerializeField] GameOverTextSO gameOverTextSO;  

    public Image damageImage;
    bool isCoolDown;
    public Camera camera; // 흔들릴 카메라 Transform
    public float shakeStrength = 0.2f; // 흔들림 강도
    public float shakeDuration = 0.2f; // 흔들림 지속 시간

    // --- 콤보 이펙트 관련 --- //
    private bool IsComboEffectOn = false;
    public Image comboEffectbase;
    public Image rightCombo1;
    public Image rightCombo2;
    public Image leftCombo1;
    public Image leftCombo2;
    [Header("Scroll Settings")]
    public float scrollSpeedWeight = 2f; // 초당 이동 속도 (픽셀 단위)

    private float imageHeight;

    [SerializeField] BackGroundController backGroundController;

    // UniTask 제어용 토큰 소스
    private CancellationTokenSource _scrollCts;
    private CancellationTokenSource _shineCts;
    private CancellationTokenSource _fadeCts;
    private CancellationTokenSource _destroyCts;

    private void Awake()
    {
        _destroyCts = new CancellationTokenSource();
    }

    private void Start()
    {
        // 이 변수를 클래스 멤버 변수나 루프 밖 지역 변수로 선언
        invLength = 1f / blurImages.Length;

        if (camera == null)
        {
            camera = Camera.main;
        }
        damageImage.color = new Color(damageImage.color.r, damageImage.color.g, damageImage.color.b, 0); // 초기 알파 0

        Managers.Game.blur = this;

        // 콤보 이펙트 관련 
        // 기준 이미지의 높이 측정 (두 세트 모두 같은 높이라고 가정)
        imageHeight = rightCombo1.rectTransform.rect.height;

        // 초기 배치 - 오른쪽 세트 (위→아래)
        rightCombo1.rectTransform.anchoredPosition = new Vector2(rightCombo1.rectTransform.anchoredPosition.x, 0);
        rightCombo2.rectTransform.anchoredPosition = new Vector2(rightCombo2.rectTransform.anchoredPosition.x, imageHeight);

        // 초기 배치 - 왼쪽 세트 (아래→위)
        leftCombo1.rectTransform.anchoredPosition = new Vector2(leftCombo1.rectTransform.anchoredPosition.x, 0);
        leftCombo2.rectTransform.anchoredPosition = new Vector2(leftCombo2.rectTransform.anchoredPosition.x, -imageHeight);

        // 누적 경계값 캐싱
        if (hpBoundaryWeight != null && hpBoundaryWeight.Length > 0)
        {
            _cachedCumulativeBoundaries = new float[hpBoundaryWeight.Length];
            float sum = 0f;
            for (int i = 0; i < hpBoundaryWeight.Length; i++)
            {
                sum += hpBoundaryWeight[i];
                _cachedCumulativeBoundaries[i] = sum;
            }
        }
    }

    private void OnDestroy()
    {
        transform.DOKill();

        // 실행 중인 모든 토큰 취소 및 정리
        CancelAndDispose(ref _scrollCts);
        CancelAndDispose(ref _shineCts);
        CancelAndDispose(ref _fadeCts);
        CancelAndDispose(ref _destroyCts);
    }

    // 토큰 정리 헬퍼 함수
    private void CancelAndDispose(ref CancellationTokenSource cts)
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }
    }

    #region Combo 관련 (Update -> UniTask Loop)

    public void ComboEffectOn()
    {
        IsComboEffectOn = true;
        rightCombo1.gameObject.SetActive(true);
        rightCombo2.gameObject.SetActive(true);
        leftCombo1.gameObject.SetActive(true);
        leftCombo2.gameObject.SetActive(true);
        comboEffectbase.gameObject.SetActive(true);

        // 기존 스크롤 작업 취소 후 새로 시작
        CancelAndDispose(ref _scrollCts);
        _scrollCts = new CancellationTokenSource();
        ScrollComboImages(_scrollCts.Token).Forget();

        PlayComboEffect();
    }

    public void ComboEffectOff()
    {
        IsComboEffectOn = false;

        // 스크롤 작업 중단
        CancelAndDispose(ref _scrollCts);

        rightCombo1.gameObject.SetActive(false);
        rightCombo2.gameObject.SetActive(false);
        leftCombo1.gameObject.SetActive(false);
        leftCombo2.gameObject.SetActive(false);
        comboEffectbase.gameObject.SetActive(false);

        StopComboEffect();
    }

    // Update() 대신 비동기 루프 사용
    private async UniTaskVoid ScrollComboImages(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            float move = scrollSpeedWeight * Time.deltaTime * (float)IngameData.GameBpm;

            // 오른쪽 (아래로)
            MoveImageDown(rightCombo1, move);
            MoveImageDown(rightCombo2, move);
            ResetIfOffScreenDown(rightCombo1, rightCombo2);
            ResetIfOffScreenDown(rightCombo2, rightCombo1);

            // 왼쪽 (위로)
            MoveImageUp(leftCombo1, move);
            MoveImageUp(leftCombo2, move);
            ResetIfOffScreenUp(leftCombo1, leftCombo2);
            ResetIfOffScreenUp(leftCombo2, leftCombo1);

            // 다음 프레임 대기 (Update 타이밍)
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
        }
    }

    // ===== 오른쪽 세트 (아래로 스크롤) =====
    void MoveImageDown(Image img, float move)
    {
        var rt = img.rectTransform;
        rt.anchoredPosition -= new Vector2(0, move);
    }

    void ResetIfOffScreenDown(Image current, Image other)
    {
        var rt = current.rectTransform;
        if (rt.anchoredPosition.y <= -imageHeight)
        {
            // 아래로 사라지면 위로 재배치
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, other.rectTransform.anchoredPosition.y + imageHeight);
        }
    }

    // ===== 왼쪽 세트 (위로 스크롤) =====
    void MoveImageUp(Image img, float move)
    {
        var rt = img.rectTransform;
        rt.anchoredPosition += new Vector2(0, move);
    }

    void ResetIfOffScreenUp(Image current, Image other)
    {
        var rt = current.rectTransform;
        if (rt.anchoredPosition.y >= imageHeight)
        {
            // 위로 사라지면 아래로 재배치
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, other.rectTransform.anchoredPosition.y - imageHeight);
        }
    }

    #endregion

    float invLength;

    public Image[] blurImages; // Blur1 ~ BlurN (Inspector에 넣어줌)
    public Image BackGroundImage;
    public Image gameOverBlack;
    public TMP_Text gameOverText;
    public TMP_Text gameOverDownText;
    public float fadeDuration = 0.5f; // 전환 시간 (초)

    private int currentIndex = 0;

    public Image lastBlacking;

    public int[] hpBoundaryWeight = new int[7];

    // 누적 경계값 미리 계산
    private float[] _cachedCumulativeBoundaries;
    // 중복 호출 방지를 위한 캐싱 변수
    private float _lastCurrentHp = -1f;
    private float _lastMaxHp = -1f;


    /// <summary>
    /// 체력에 따라 Blur 상태 업데이트
    /// </summary>
    public void SetBlur(float currentHp, float maxHp)
    {
        if (blurImages.Length == 0) return;

        //이전값과 같다면 즉시 종료
        if (Mathf.Approximately(currentHp, _lastCurrentHp) && Mathf.Approximately(maxHp, _lastMaxHp))
        {
            return;
        }

        // 값 업데이트
        _lastCurrentHp = currentHp;
        _lastMaxHp = maxHp;

        float lostHp = maxHp - currentHp;
        int newIndex = 0;

        for (int i = 0; i < _cachedCumulativeBoundaries.Length; i++)
        {
            if (lostHp < _cachedCumulativeBoundaries[i])
            {
                newIndex = i;
                break;
            }
            newIndex = i;
        }

        newIndex = Mathf.Clamp(newIndex, 0, blurImages.Length - 1);

        if (newIndex != currentIndex)
        {
            // 기존 페이드 작업 취소
            CancelAndDispose(ref _fadeCts);
            _fadeCts = new CancellationTokenSource();

            // UniTask 실행
            FadeTransition(currentIndex, newIndex, _fadeCts.Token).Forget();

            currentIndex = newIndex;

            // 채도 조절 함수 호출
            SettingSaturation();

            // 검은 배경 처리
            bool isDarkPhase = newIndex >= blurImages.Length - 2;
            lastBlacking.DOFade(isDarkPhase ? 1f : 0f, 0.8f);
        }
    }

    // IEnumerator -> async UniTask
    private async UniTaskVoid FadeTransition(int oldIndex, int newIndex, CancellationToken token)
    {
        float time = 0f;

        Image oldImg = blurImages[oldIndex];
        Image newImg = blurImages[newIndex];

        float oldStartAlpha = oldImg.color.a;
        float newStartAlpha = newImg.color.a;

        while (time < fadeDuration)
        {
            // 토큰 취소 확인
            if (token.IsCancellationRequested) return;

            time += Time.deltaTime;
            float t = time / fadeDuration;

            // old → 투명
            Color c1 = oldImg.color;
            c1.a = Mathf.Lerp(oldStartAlpha, 0f, t);
            oldImg.color = c1;

            // new → 불투명
            Color c2 = newImg.color;
            c2.a = Mathf.Lerp(newStartAlpha, 1f, t);
            newImg.color = c2;

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
        }

        // 최종 보정
        Color endOld = oldImg.color;
        endOld.a = 0f;
        oldImg.color = endOld;

        Color endNew = newImg.color;
        endNew.a = 1f;
        newImg.color = endNew;
    }

    bool isSaturationSeqCool = false;
    private void SettingSaturation()
    {
        if (isSaturationSeqCool) return;
        isSaturationSeqCool = true;

        Sequence seq = DOTween.Sequence();

        // 사용처
        float targetAlpha = 1f - ((currentIndex + 1) * invLength);

        seq.Join(backGroundController.sharedMaterial.DOFloat(targetAlpha, "_Saturation", 0.3f)); // damageImage와 동시에 페이드

        seq.OnComplete(() => isSaturationSeqCool = false);
    }

    public void ShowDamageEffect()
    {
        if (isCoolDown) return;

        isCoolDown = true;
        damageImage.DOKill();
        camera.transform.DOKill(); // 이전 흔들림 중단

        // 피해 효과 UI 페이드 
        Sequence seq = DOTween.Sequence();
        seq.Append(damageImage.DOFade(1f, 0.15f));
        seq.Append(damageImage.DOFade(0f, 0.15f));


        PlayRandomHurtSound();

        // 카메라 흔들림 효과 추가
        camera.transform.DOShakePosition(
            duration: shakeDuration,
            strength: shakeStrength,
            vibrato: 8, // 흔들리는 횟수
            randomness: 90,
            snapping: false,
            fadeOut: true
        );

        seq.OnComplete(() => isCoolDown = false);
    }

    public void GameOverBlurEffect()
    {
        // 게임오버 텍스트 설정 랜덤으로 출력
        if (gameOverTextSO.StageTexts[IngameData.ChapterIdx].gameOverText.Count!=0)
        {
            gameOverText.text = gameOverTextSO.StageTexts[IngameData.ChapterIdx].gameOverText
                [Random.Range(0, gameOverTextSO.StageTexts[IngameData.ChapterIdx].gameOverText.Count)];
        }
        else
        {
            gameOverText.text = "중요한 것은 흔들리지 않는 마음이오.";
        }


        // InCirc 천천히 어두워지다가 갑자기 어두워지기
        gameOverBlack.DOFade(1 / 255f * 248f, 1f)
            .SetEase(Ease.InCirc)
            .SetUpdate(UpdateType.Normal, true);
        gameOverText.DOFade(1 / 255f * 248f, 1f)
            .SetEase(Ease.InCirc)
            .SetUpdate(UpdateType.Normal, true);
        gameOverDownText.DOFade(1 / 255f * 248f, 1f)
            .SetEase(Ease.InCirc)
            .SetUpdate(UpdateType.Normal, true);
    }

    public void WaitForGameOver()
    {
        // 클릭 이벤트를 정의합니다.
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((eventData) =>
        {
            // 클릭 시 스테이지 씬으로 이동
            SceneLoadingManager.Instance.LoadScene("StageScene");
            Time.timeScale = 1f; // 타임스케일 원상 복구
        });

        // 화면에 클릭 가능한 EventTrigger 컴포넌트를 추가합니다.
        var eventTrigger = gameOverBlack.gameObject.GetOrAddComponent<EventTrigger>();
        // EventTrigger에 이벤트를 추가합니다.
        eventTrigger.triggers.Add(entry);
    }
    public void ComboEffect()
    {
        // 몬스터 액션 중이라면 줌인만 하고, 복귀는 몬스터가 정한 TargetBaseSize로 한다.
        Camera.main.DOKill(false);

        float currentBase = Camera.main.orthographicSize; // 현재 사이즈에서
        float punchSize = currentBase * 0.9f; // 살짝만 펀치

        Camera.main.DOOrthoSize(punchSize, 0.4f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // [해결책] 무조건 5f로 가는 게 아니라, 현재 카메라가 잠겨있다면 잠긴 값을 유지!
            float recoverSize = CameraController.IsLocked ? CameraController.TargetBaseSize : 5f;
                Camera.main.DOOrthoSize(recoverSize, 0.4f).SetEase(Ease.OutSine);
            });
    }

    #region tool

    private void PlayRandomHurtSound()
    {
        // 0 또는 1을 무작위로 선택
        int randomIndex = Random.Range(0, 2);

        if (randomIndex == 0)
        {
            Managers.Sound.Play("SFX/Damaged/Hurt1_V1");
        }
        else
        {
            Managers.Sound.Play("SFX/Damaged/Hurt2_V1");
        }
    }
    #endregion

    #region ComboEffect (Shining)

    // [개선] 이미지를 배열로 관리합니다. 인스펙터에서 원하는 만큼 추가할 수 있습니다.
    [Header("반짝일 이미지들")]
    public Image[] shiningImages;

    [Header("사용할 스프라이트들")]
    public Sprite[] comboShiningSprites;

    [Tooltip("애니메이션의 프레임 간 시간 (초)")]
    [SerializeField] private float animationFrameDelay = 0.08f;

    public void PlayComboEffect()
    {
        if (shiningImages == null || shiningImages.Length == 0 || comboShiningSprites == null || comboShiningSprites.Length == 0)
        {
            Debug.LogWarning("콤보 이펙트 설정이 누락되었습니다.");
            return;
        }

        // [개선] 배열을 순회하며 모든 이미지를 활성화
        foreach (Image img in shiningImages)
        {
            if (img != null) img.gameObject.SetActive(true);
        }

        // 기존 샤인 애니메이션 취소 및 새 시작
        CancelAndDispose(ref _shineCts);
        _shineCts = new CancellationTokenSource();
        AnimateComboShine(_shineCts.Token).Forget();
    }

    public void StopComboEffect()
    {
        // 샤인 애니메이션 중단
        CancelAndDispose(ref _shineCts);

        // [개선] 배열을 순회하며 모든 이미지를 비활성화
        if (shiningImages != null)
        {
            foreach (Image img in shiningImages)
            {
                if (img != null) img.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 실제 애니메이션을 처리하는 UniTaskVoid
    /// </summary>
    private async UniTaskVoid AnimateComboShine(CancellationToken token)
    {
        // TimeSpan 캐싱 (GC 감소)
        var delay = System.TimeSpan.FromSeconds(animationFrameDelay);

        while (!token.IsCancellationRequested)
        {
            // [개선] 배열의 모든 이미지를 순회하며 각각 무작위 스프라이트 할당
            foreach (Image img in shiningImages)
            {
                if (img != null)
                {
                    int randomIndex = Random.Range(0, comboShiningSprites.Length);
                    img.sprite = comboShiningSprites[randomIndex];
                }
            }

            // WaitForSeconds -> UniTask.Delay
            await UniTask.Delay(delay, cancellationToken: token);
        }
    }

    #endregion
}