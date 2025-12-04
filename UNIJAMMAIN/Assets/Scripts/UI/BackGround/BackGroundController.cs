using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks; // UniTask 네임스페이스 추가
using System.Threading; // CancellationToken 사용

public class BackGroundController : MonoBehaviour
{
    private Sequence seq;
    private readonly string satPropName = "_Saturation";

    public Material sharedMaterial;
    private float beatDuration;
    private int chapterIdx =-100;
    [SerializeField] Image extraObjectImage;
    [SerializeField] BackGroundDataSO backGrounddataSO;

    [Header("Layer Control")]
      [SerializeField] Image backGround;
    [SerializeField] Transform bgTransform;
  


    //---- 각 챕터에 대한 정보 저장용 ---- //


    private void Start()
    {
        Managers.Game.backGroundController = this;
    }
    private void OnDestroy()
    {
        IngameData.ChangeBpm -= Init;
        BeatClock.OnBeat -= BeatOnBackGroundAction;
        seq?.Kill();
        sharedMaterial.SetFloat(satPropName, 1f);
    }
    private void Awake()
    {

        IngameData.ChangeBpm -= Init;
        IngameData.ChangeBpm += Init;
        chapterIdx = IngameData.ChapterIdx;
    }
   
    private void Init()
    {
        beatDuration = (float)IngameData.BeatInterval;
        seq?.Kill();
        seq = DOTween.Sequence()
           .SetAutoKill(false)
           .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
           .Pause();

        BeatClock.OnBeat -= BeatOnBackGroundAction;
        BeatClock.OnBeat += BeatOnBackGroundAction;
    }

    private int actionNumberTarget = 4;
    private int nowactionNumber=1;
    private void BeatOnBackGroundAction(long _)
    {
       
        Debug.Log(chapterIdx);

        if (nowactionNumber == actionNumberTarget)
        {
            nowactionNumber = 1;
        }
        else
        {
            nowactionNumber++;
            return;
        }


        switch (chapterIdx)
        {
            case 0:
 
                ChapterAction_0();
                break;
            case 1:
                
                break;
            case 2:
                ChapterAction_2();
                break;
            case 3:
                ChapterAction_3();
                break;
        }

       
    }

    #region 챕터별 backGround 액션
    public void ChapterBackGroundInitialize()
    {
        chapterIdx = IngameData.ChapterIdx;
        RectTransform rect = extraObjectImage.rectTransform;
        switch (chapterIdx)
        {
            case 0:
                actionNumberTarget = 2;
                extraObjectImage.sprite = backGrounddataSO.backGroundDatas[0].extraBackGroundLists[0];
                extraObjectImage.gameObject.SetActive(true);
                UpdateRectMargin(extraObjectImage.rectTransform, -80f);

                break;
            case 1:
                extraObjectImage.gameObject.SetActive(true);
                break;
            case 2:
                
                actionNumberTarget = 1;
                extraObjectImage.sprite = backGrounddataSO.backGroundDatas[2].extraBackGroundLists[0];
                extraObjectImage.gameObject.SetActive(true);
                // 데이터 리스트 가져오기
                spriteList = backGrounddataSO.backGroundDatas[2].extraBackGroundLists;
                UpdateRectMargin(extraObjectImage.rectTransform, 0);
                break;
            case 3:
                if (bgTransform != null)
                {
                    // BG의 현재 인덱스를 알아냄
                    int bgIndex = bgTransform.GetSiblingIndex();

                    // ExtraImage를 BG의 인덱스 자리로 보냄 
                    // (원래 있던 BG는 한 칸 아래로 밀려나면서 ExtraImage가 BG보다 위에 위치 -> 화면상 뒤쪽)
                    rect.SetSiblingIndex(bgIndex);
                }
                // 비오는 색깔 조정
               
                extraObjectImage.color = new Color(127f / 255f, 133f / 255f, 145f / 255f);

                actionNumberTarget = 4;
                extraObjectImage.sprite = backGrounddataSO.backGroundDatas[3].extraBackGroundLists[0];
                extraObjectImage.gameObject.SetActive(true);
                UpdateRectMargin(extraObjectImage.rectTransform, 0);

                break;

                break;
        }
    }
    
    private void ChapterAction_0()
    {
        if (extraObjectImage != null)
        {
            
           
            RectTransform rect = extraObjectImage.rectTransform;

            // 1. 이 오브젝트에 걸린 모든 트윈을 확실하게 제거
            // rect(컴포넌트)가 아니라 gameObject를 기준으로 킬을 해야 안전합니다.
            rect.DOKill();

            // -------------------------------------------------------
            // [설정]
            // startMargin: 0 (꽉 찬 상태)
            // shrinkMargin: 100 (안쪽으로 쪼그라든 상태)
            // -------------------------------------------------------
            float startMargin = -80f;
            float shrinkMargin = 0f;

            // 2. 시작 상태 강제 초기화 (0에서 시작)
            UpdateRectMargin(rect, startMargin);

            // 3. 애니메이션 실행
            // BeatClockUI처럼 "팍!(Kick)" 하고 커졌다가 "스르륵(Rest)" 돌아오는 구조입니다.

            // [단계 A] Kick: 0 -> 100 (아주 빠르게! 0.05초)
            // 타겟을 rect로 명시(.SetTarget)해야 DOKill이 먹힙니다.
            DOVirtual.Float(startMargin, shrinkMargin, 0.05f, (x) =>
            {
                UpdateRectMargin(rect, x);
            })
            .SetTarget(rect) // [중요] 타겟을 설정해야 DOKill로 멈출 수 있음
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // [단계 B] Rest: 100 -> 0 (나머지 시간 동안 천천히 복구)
                DOVirtual.Float(shrinkMargin, startMargin, beatDuration * 0.9f, (x) =>
                {
                    UpdateRectMargin(rect, x);
                })
                .SetTarget(rect) // [중요] 여기도 타겟 설정
                .SetEase(Ease.OutQuad);
            });
        }
    }


    private bool isChapter2Looping = false;
    private List<Sprite> spriteList = new List<Sprite>();
    private void ChapterAction_2()
    {
        
        // 예외 처리: 리스트가 없거나 비었으면 리턴
        if (spriteList == null || spriteList.Count == 0) return;

        // 1. 현재 인덱스(actionNumberTarget)에 해당하는 스프라이트 적용
        // 안전 장치: 인덱스가 범위를 벗어나지 않게 Clamp
        int safeIndex = Mathf.Clamp(actionNumberTarget, 0, spriteList.Count - 1);
        extraObjectImage.sprite = spriteList[safeIndex];

        // 2. 다음 비트를 위해 인덱스 증가 및 로직 처리
        actionNumberTarget++;

        if (!isChapter2Looping)
        {
            // [Phase 1] 처음: 1번부터 끝까지 재생
            // 리스트의 끝에 도달했는지 확인
            if (actionNumberTarget >= spriteList.Count)
            {
                isChapter2Looping = true; // 이제부터 루프 모드 진입
                actionNumberTarget = 0;   // 0번으로 초기화
            }
        }
        else
        {
            // [Phase 2] 반복: 0번부터 4번까지만 반복
            // 4번을 넘어가면(5가 되면) 다시 0으로
            if (actionNumberTarget > 4)
            {
                actionNumberTarget = 0;
            }
        }
    }

    private void ChapterAction_3()
    {
        if (extraObjectImage == null) return;

        RectTransform rect = extraObjectImage.rectTransform;

        // 1. 이전 비트의 애니메이션 즉시 중단
        rect.DOKill();

        // 2. 크기 초기화 (혹시 커져 있는 상태에서 시작할 수 있으므로 1.0으로 리셋)
        rect.localScale = Vector3.one;

        // 3. 시퀀스 생성
        Sequence seq = DOTween.Sequence();
        seq.SetTarget(extraObjectImage.gameObject); // GameObject를 타겟으로 설정하여 안전하게 관리

        // [연출 설정]
        // 1.0 -> 1.09로 커졌다가(Kick), 다시 1.0으로 돌아옴(Rest)

        // A. 팽창 (Kick): 아주 빠르게 (전체 시간의 15%)
        // Ease.OutQuad: 시작할 때 빠르고 끝날 때 느려짐 (타격감)
        seq.Append(rect.DOScale(new Vector3(1.07f, 1.07f, 1f), beatDuration * 0.3f)
            .SetEase(Ease.OutQuad));

        // B. 수축 (Rest): 천천히 원래대로 (전체 시간의 85%)
        // Ease.OutQuad: 부드럽게 돌아옴
        seq.Append(rect.DOScale(Vector3.one, beatDuration * 0.7f)
            .SetEase(Ease.OutQuad));
    }
    #endregion

    // [헬퍼 함수] 상하좌우 여백(Margin)을 한 번에 조절하는 함수
    // margin이 0이면 stretch 꽉 채움, 50이면 사방에서 50씩 들어옴
    private void UpdateRectMargin(RectTransform rect, float margin)
    {
        // Left, Bottom은 정수값 그대로
        rect.offsetMin = new Vector2(margin, margin);
        // Right, Top은 음수값이어야 안쪽으로 들어옴
        rect.offsetMax = new Vector2(-margin, -margin);
    }

    public void SettingSatuation(float value)
    {
        if (sharedMaterial != null)
        {
            sharedMaterial.SetFloat(satPropName, value);
        }
    }


    private void OnApplicationQuit()
    {
        if (sharedMaterial != null)
        {
            sharedMaterial.SetFloat(satPropName, 1f); // 1.0(원본)으로 복구
        }
    }

}

