using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.EventSystems;

public class BlurController : MonoBehaviour
{
    public Image damageImage;
    bool isCoolDown;
    public Camera camera; // 흔들릴 카메라 Transform
    public float shakeStrength = 0.2f; // 흔들림 강도
    public float shakeDuration = 0.2f; // 흔들림 지속 시간

    // --- 콤보 이펙트 관련 --- //
    private bool IsComboEffectOn = false;
    public Image rightCombo1;
    public Image rightCombo2;
    public Image leftCombo1;
    public Image leftCombo2;
    [Header("Scroll Settings")]
    public float scrollSpeedWeight = 2f; // 초당 이동 속도 (픽셀 단위)

    private float imageHeight;


    #region Combo 관련 
    void Update()
    {
        if (IsComboEffectOn)
        {
            float move = scrollSpeedWeight * Time.deltaTime*(float)IngameData.GameBpm;

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
    public void ComboEffectOn()
    {
        IsComboEffectOn = true;
    }
    #endregion

    private void Start()
    {
        ComboEffectOn();
        if (camera == null)
        { 
            camera = Camera.main;
        }
        damageImage.color = new Color(damageImage.color.r, damageImage.color.g, damageImage.color.b, 0); // 초기 알파 0
        /*
        Debug.Log("테스트용 pitch1.3f");
        Time.timeScale = 1.3f;
        Managers.Sound.Play("BGM/84bpm_64_V1", Define.Sound.BGM, 1.3f);
        */
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
    }

    public Image[] blurImages; // Blur1 ~ BlurN (Inspector에 넣어줌)
    public Image goGrayBackGround;
    public Image gameOverBlack;
    public TMP_Text gameOverText;
    public TMP_Text gameOverDownText;
    public float fadeDuration = 0.5f; // 전환 시간 (초)

    private int currentIndex = 0;
    private Coroutine fadeCoroutine;
    public Image lastBlacking;

    public int[] hpBoundaryWeight = new int[7];

    /// <summary>
    /// 체력에 따라 Blur 상태 업데이트
    /// </summary>
    public void SetBlur (float currentHp, float maxHp)
    { 

        // Debug.Log($"currentHP : {currentHp} - maxHP : {maxHp}");
        if (blurImages.Length == 0) return;

        // 몇 번째 Blur인지 계산
        float lostHp = maxHp - currentHp;
        float cumulativeHpBoundary = 0f;
        int newIndex=0;
        for (int i = 0; i < hpBoundaryWeight.Length; i++)
        {
            cumulativeHpBoundary += hpBoundaryWeight[i];
            if (lostHp < cumulativeHpBoundary)
            {
                newIndex = i;
                break; // 현재 체력에 해당하는 구간을 찾았으므로 루프 종료
            }
            // 만약 잃은 체력이 모든 가중치 합보다 크거나 같다면 마지막 인덱스로 설정됩니다.
            newIndex = i;
        }
        // 안전을 위해 Clamp 처리 (가중치 설정 오류 방지)
        newIndex = Mathf.Clamp(newIndex, 0, blurImages.Length - 1);

        if (newIndex != currentIndex) // 새로운 이미지로의 변환 
        {
            // 전환 시작
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeTransition(currentIndex, newIndex));
            currentIndex = newIndex;

            if (newIndex >= blurImages.Length - 2) // 마지막 2단계부터 새로 추가되는 Layer BackGround.LasBlacking
            {
                lastBlacking.DOFade(1f, 0.8f); // 어두워지기
            }
            else
            {
                lastBlacking.DOFade(0f, 0.8f); // 밝아지기
            }

        }
        
        
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
    private IEnumerator FadeTransition(int oldIndex, int newIndex)
    {
        float time = 0f;

        Image oldImg = blurImages[oldIndex];
        Image newImg = blurImages[newIndex];

        // 시작 알파값

        float oldStartAlpha = oldImg.color.a;
        float newStartAlpha = newImg.color.a;

        while (time < fadeDuration)
        {
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

            yield return null;
        }

        // 최종 보정
        Color endOld = oldImg.color;
        endOld.a = 0f;
        oldImg.color = endOld;

        Color endNew = newImg.color;
        endNew.a = 1f;
        newImg.color = endNew;
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

        // blurImages 개수 기준으로 goGrayBackGround alpha 계산
        // currentIndex는 SetBlur에서 갱신됨, 0 ~ blurImages.Length-1
        float targetAlpha = (currentIndex + 1) / (float)blurImages.Length; // 1/6, 2/6, ... 비율

        // 배경 goGrayBackGround 투명도 점점 진해지도록 추가
        seq.Join(goGrayBackGround.DOFade(targetAlpha, 0.3f)); // damageImage와 동시에 페이드

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
        // InCirc 천천히 어두워지다가 갑자기 어두워지기
        gameOverBlack.DOFade(1 / 255f * 248f, 1f)
            .SetEase(Ease.InCirc)
            .SetUpdate(UpdateType.Normal, true);
        gameOverText.DOFade(1 / 255f * 248f, 1f)
            .SetEase(Ease.InCirc)
            .SetUpdate(UpdateType.Normal, true);
        gameOverDownText.DOFade(1 / 255f * 248f, 1f )
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
        float defaultSize = camera.orthographicSize;

        // 줌인
        camera.DOOrthoSize(defaultSize * 0.9f, 0.4f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
            // 원래 크기로 복귀
            camera.DOOrthoSize(defaultSize, 0.4f)
                    .SetEase(Ease.InOutQuad);
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
}
