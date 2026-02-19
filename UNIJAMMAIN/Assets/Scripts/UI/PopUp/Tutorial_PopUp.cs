using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks; // UniTask 필수
using System.Threading;      // CancellationToken 필수
using UnityEngine.UI;
using DG.Tweening;



public class Tutorial_PopUp : UI_Popup
{
    public enum dir
    {
        left,
        right
    }

    public TMP_Text text;
    public GameObject contents;
    public GameObject keyBoardGuide;

    public float appearSpeed = 2f;
    public float disappearSpeed = 2f;
    public float startOffset = -100f;

    private Vector3 originalPosition;
    private CanvasGroup canvasGroup;

    [SerializeField] private GameObject leftCharacter;
    [SerializeField] private GameObject rightCharacter;
    [SerializeField] private Image leftImage;
    [SerializeField] private Image rightImage;

    [SerializeField] private Sprite default_Image;

    [Header("0장 튜토리얼 이미지 설정")]
    [SerializeField] RectTransform leftHandRect;
    [SerializeField] GameObject rightHand;

    // 실행 중인 작업 취소를 위한 토큰 소스
    private CancellationTokenSource _cts;

    public override void Init()
    {
        base.Init();
    }

    private void Awake()
    {
        // 초기화 시 필요한 컴포넌트 캐싱
        if (contents != null)
        {
            originalPosition = contents.transform.localPosition;
            canvasGroup = contents.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = contents.AddComponent<CanvasGroup>();
            }
        }

        if (keyBoardGuide != null)
        {
            keyBoardGuide.SetActive(false);
        }

        Managers.UI.SetCanvasMost(this.gameObject);
    }

    private void OnEnable()
    {
        // 활성화될 때 새 토큰 생성
        RefreshCancellationToken();
    }

    private void OnDisable()
    {
        // 비활성화될 때 진행 중인 작업 취소
        CancelCancellationToken();
    }

    private void OnDestroy()
    {
        CancelCancellationToken();
    }

    // 토큰 갱신
    private void RefreshCancellationToken()
    {
        CancelCancellationToken();
        _cts = new CancellationTokenSource();
    }

    // 토큰 취소 및 정리
    private void CancelCancellationToken()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    // 외부 호출 진입점
    public void StartTutorial(IReadOnlyList<TextInfo> textInfo, int? lastMonsterHitCnt)
    {
        // UniTaskVoid로 실행 (Fire and Forget)
        ShowSequenceOfPopups(textInfo, lastMonsterHitCnt, _cts.Token).Forget();
    }

  

    // 메인 시퀀스 (async UniTaskVoid)
    private async UniTaskVoid ShowSequenceOfPopups(IReadOnlyList<TextInfo> textInfo, int? lastMonsterHitCnt, CancellationToken token)
    {
        SetClear();

        // lastMonsterHitCnt가 null이면 0으로 처리
        int baseHitCnt = lastMonsterHitCnt ?? 0;

        for (int i = 0; i < textInfo.Count; i++)
        {
            // 토큰 취소 확인 (오브젝트 비활성화 등)
            if (token.IsCancellationRequested) return;

            var info = textInfo[i];

            // 이미지 세팅
            ImageSetting(info);


            // IngameData 값 캐싱 (불필요한 접근 최소화)
            float beatInterval = (float)IngameData.BeatInterval;
            float durationSec = beatInterval * (info.delayBeat - 1);

            appearSpeed = beatInterval * 0.5f;
            disappearSpeed = beatInterval * 0.5f;

            // 텍스트 설정 로직
            int curMonsterHitCnt = IngameData.PerfectMobCnt + IngameData.GoodMobCnt;
            bool isFail = (curMonsterHitCnt - baseHitCnt) < info.monsterCutline;

            if (info.textContents != null && info.textContents.Count() > 0)
            {
                text.text = isFail ? info.textContents.Last() : info.textContents.First();
            }

            // 1. 팝업 나타나기
            await SmoothyPopUp(true, token);

            // 2. 대기 (일시정지 고려)
            await WaitForDurationWithPause(durationSec, token);

            // 3. 팝업 사라지기
            await SmoothyPopUp(false, token);
        }

        // 모든 시퀀스 종료 후 비활성화
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 팝업 애니메이션 (UniTask)
    /// </summary>
    private async UniTask SmoothyPopUp(bool isAppearing, CancellationToken token)
    {
        float timer = 0f;
        float duration = isAppearing ? appearSpeed : disappearSpeed;

        // 시작 전 초기 상태 설정
        if (isAppearing)
        {
            contents.transform.localPosition = originalPosition + new Vector3(0, startOffset, 0);
            canvasGroup.alpha = 0f;
            KeyBoardGuideOn(); // 특정 텍스트일 때 가이드 켜기
        }
        else
        {
            // 사라질 때는 현재 상태에서 시작
            // (혹시 중간에 끊겼을 수 있으므로 명시적 설정은 생략하거나 현재값 사용)
        }

        while (timer < 1f)
        {
            // 일시정지 체크 (애니메이션 중에도 멈춰야 한다면)
            if (IngameData.Pause)
            {
                await UniTask.WaitUntil(() => !IngameData.Pause, cancellationToken: token);
            }

            if (token.IsCancellationRequested) return;

            // duration이 0이거나 매우 작으면 즉시 완료 처리
            if (duration <= Mathf.Epsilon)
            {
                timer = 1.1f;
            }
            else
            {
                timer += Time.deltaTime / duration;
            }

            float k = Mathf.Clamp01(timer);

            if (isAppearing)
            {
                contents.transform.localPosition = Vector3.Lerp(originalPosition + new Vector3(0, startOffset, 0), originalPosition, k);
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, k);
            }
            else
            {
                contents.transform.localPosition = Vector3.Lerp(originalPosition, originalPosition + new Vector3(0, startOffset, 0), k);
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, k);
            }

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
        }

        // 최종 상태 보정
        if (isAppearing)
        {
            contents.transform.localPosition = originalPosition;
            canvasGroup.alpha = 1f;
        }
        else
        {
            contents.transform.localPosition = originalPosition + new Vector3(0, startOffset, 0);
            canvasGroup.alpha = 0f;
        }
    }

    /// <summary>
    /// 일시정지를 고려한 대기 (UniTask)
    /// </summary>
    private async UniTask WaitForDurationWithPause(float duration, CancellationToken token)
    {
        float timer = 0f;
        while (timer < duration)
        {
            if (token.IsCancellationRequested) return;

            // 일시정지 상태라면 풀릴 때까지 대기
            if (IngameData.Pause)
            {
                await UniTask.WaitUntil(() => !IngameData.Pause, cancellationToken: token);
            }

            timer += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
        }
    }

    private void SetClear()
    {
        if (keyBoardGuide != null)
            keyBoardGuide.SetActive(false);
    }

    private void ImageSetting(TextInfo textInfo)    
    {
        if(textInfo.characterData ==null)
        {
            leftImage.sprite = default_Image;
            return;
        }

        dir d = textInfo.dir;
        if(d == dir.left) // 왼쪽
        {
            leftCharacter.SetActive(true);
            rightCharacter.SetActive(false);
            leftImage.sprite = textInfo.characterData.CharacterImage;
           
        }
        else // 오른쪽
        {
            leftCharacter.SetActive(false);
            rightCharacter.SetActive(true);
            rightImage.sprite = textInfo.characterData.CharacterImage;
        }
    }
    private void KeyBoardGuideOn()
    {
        if(IngameData.ChapterIdx!=0)
        {
            return;
        }
        // 문자열 비교 최적화를 위해 상수로 관리하거나 ID로 관리하는 것이 좋지만, 
        // 현재 로직을 유지하면서 string.Equals 사용
        if (string.Equals(text.text, "(너를 잠식하려는 혼령이 보이는 순간, 바로 대각선 방향키를 통해 공격해!)"))
        {
            if (keyBoardGuide != null)
                keyBoardGuide.SetActive(true);

            rightHand.SetActive(true);
            Image rightHandimage = rightHand.GetComponent<Image>();
            rightHandimage.DOFade(1f, 0.5f);
            rightHand.GetComponent<RectTransform>().DOAnchorPosY(-293, 1f);
        }
    }
}