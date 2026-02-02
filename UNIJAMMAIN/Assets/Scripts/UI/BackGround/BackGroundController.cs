using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks; // UniTask 네임스페이스 추가
using System.Threading; // CancellationToken 사용
using System;

public class BackGroundController : MonoBehaviour
{
    private Sequence seq;
    private readonly string satPropName = "_Saturation";

    public Material sharedMaterial;

    // 외부에서 접근 가능하도록 프로퍼티 추가 (getter만 public)
    public float BeatDuration { get; private set; }

    public float beatDuration;
    private int chapterIdx =-100;
    [SerializeField] public Image extraObjectImage;
    [SerializeField] public Image extraObjectImage2;
    [SerializeField] public Image extraObjectImage3;
    [SerializeField] public Image extraObjectImage4;
    [SerializeField] public Image extraObjectImage5;
    [SerializeField] public Image extraObjectImage6;
    [SerializeField] public Image extraObjectImage7;
    [SerializeField] public BackGroundDataSO backGrounddataSO;

    // 외부 스크립트용 프로퍼티(편의상 이름 매핑)
    public Image ExtraImage1 => extraObjectImage;
    public Image ExtraImage2 => extraObjectImage2;
    public Image ExtraImage3 => extraObjectImage3;
    public Image ExtraImage4 => extraObjectImage4;
    public Image ExtraImage5 => extraObjectImage5;
    public BackGroundDataSO DataSO => backGrounddataSO;

    [Header("Layer Control")]
      [SerializeField] Image backGround;
    [SerializeField] Transform bgTransform;

    public int ActionNumberTarget { get; set; } = 4;
    private int nowactionNumber = 1;
    public int actionNumberTarget = 4;


    //---- 각 챕터에 대한 정보 저장용 ---- //
    private Action currentChapterAction;

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

        chapterIdx = IngameData.ChapterIdx;
        SetCurrentChapterAction(chapterIdx);
        
    }

    
    public void SetCurrentChapterAction(int chapterIndex)
    {
        chapterIdx = chapterIndex; // 변수 동기화

        switch (chapterIdx)
        {
            case 0: currentChapterAction = ChapterAction_0; break;
            case 1: currentChapterAction = ChapterAction_1; break;
            case 2: currentChapterAction = ChapterAction_2; break;
            case 3: currentChapterAction = ChapterAction_3; break;
            case 4: currentChapterAction = ChapterAction_4; break;
            case 5: currentChapterAction = ChapterAction_5; break;
            case 6: currentChapterAction = ChapterAction_6; break;
            case 7: currentChapterAction = ChapterAction_7; break;
            case 10:
                ConnectExternalEffect<BackGroundEffect_10>(); // 이안에서 currentChapterAction 구독
                break;
            default: currentChapterAction = null; break;
        }
    }


    private void BeatOnBackGroundAction(long _)
    {
        // 카운터 체크 로직 (기존과 동일)
        if (nowactionNumber < actionNumberTarget)
        {
            nowactionNumber++;
            return;
        }

        // 타겟에 도달했으므로 액션 실행 후 리셋
        nowactionNumber = 1;

        // [핵심] 스위치문 없이 바로 실행
        // Invoke()는 null 체크(?.)와 함께 사용하여 안전하게 실행
        currentChapterAction?.Invoke();
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
                extraObjectImage2.sprite = backGrounddataSO.backGroundDatas[0].extraBackGroundLists[1];
                UpdateRectPosition(extraObjectImage2.rectTransform, -700f, 300);


                for (int i = 0; i < 3; i++)
                {
                    birdSprites.Add(backGrounddataSO.backGroundDatas[chapterIdx].extraBackGroundLists[i+1]);
                }

                extraObjectImage2.gameObject.SetActive(true);

                UpdateRectMargin(extraObjectImage.rectTransform, -80f);
                break;
            case 1:
                actionNumberTarget = 2;
                extraObjectImage.sprite = backGrounddataSO.backGroundDatas[chapterIdx].extraBackGroundLists[0];
                extraObjectImage2.sprite = backGrounddataSO.backGroundDatas[chapterIdx].extraBackGroundLists[1];
                extraObjectImage3.sprite = backGrounddataSO.backGroundDatas[chapterIdx].extraBackGroundLists[2];
                extraObjectImage.gameObject.SetActive(true);
                extraObjectImage2.gameObject.SetActive(true);
                extraObjectImage3.gameObject.SetActive(true);
                UpdateRectPosition(extraObjectImage.rectTransform, -590f, -380f);
                UpdateRectPosition(extraObjectImage2.rectTransform, 800f, -400f);
                UpdateRectPosition(extraObjectImage3.rectTransform, -500f, 250f);


              
                for(int i =0;i<3;i++)
                {
                    birdSprites.Add(backGrounddataSO.backGroundDatas[chapterIdx].extraBackGroundLists[i+3]);
                }

                extraObjectImage4.sprite = backGrounddataSO.backGroundDatas[chapterIdx].extraBackGroundLists[3];
                UpdateRectPosition(extraObjectImage4.rectTransform, -700f, 300);
                extraObjectImage4.gameObject.SetActive(true);
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
            case 4:
                actionNumberTarget = 2;
                extraObjectImage.sprite = backGrounddataSO.backGroundDatas[4].extraBackGroundLists[0];
                extraObjectImage2.sprite = backGrounddataSO.backGroundDatas[4].extraBackGroundLists[1];
                extraObjectImage.gameObject.SetActive(true);
                extraObjectImage2.gameObject.SetActive(true);
                UpdateRectMargin(extraObjectImage.rectTransform, -80);
                UpdateRectMargin(extraObjectImage2.rectTransform, 0);

                break;
            case 5:
                actionNumberTarget = 2;
                extraObjectImage.sprite = backGrounddataSO.backGroundDatas[chapterIdx].extraBackGroundLists[0];
                extraObjectImage2.sprite = backGrounddataSO.backGroundDatas[chapterIdx].extraBackGroundLists[1];
                extraObjectImage.gameObject.SetActive(true);
                extraObjectImage2.gameObject.SetActive(true);
                UpdateRectMargin(extraObjectImage.rectTransform, 50);
                UpdateRectMargin(extraObjectImage2.rectTransform, 0);

                break;
            case 6:
                actionNumberTarget = 1;
                extraObjectImage.sprite = backGrounddataSO.backGroundDatas[chapterIdx].extraBackGroundLists[0];
                extraObjectImage.gameObject.SetActive(true);
                UpdateRectMargin(extraObjectImage.rectTransform, 0);   
                break;
            case 7:
                actionNumberTarget = 4;
                extraObjectImage.sprite = backGrounddataSO.backGroundDatas[chapterIdx].extraBackGroundLists[0];
                extraObjectImage2.sprite = backGrounddataSO.backGroundDatas[chapterIdx].extraBackGroundLists[1];
                extraObjectImage.gameObject.SetActive(true);
                extraObjectImage2.gameObject.SetActive(true);
                UpdateRectMargin(extraObjectImage.rectTransform, 0);
                UpdateRectMargin(extraObjectImage2.rectTransform, 0);

                break;

                break;
            default:
                break;

        }
    }

    // [추가] 새가 왼쪽을 볼 차례인지 확인하는 변수
    private bool isBirdTurnLeft = true;
    private List<Sprite> birdSprites = new List<Sprite>();

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
            float startMargin = -50f;
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

        // 새에 관한것
        // -------------------------------------------------------------
        // [새 리듬 로직]
        // Front(3), Left(4), Right(5) 이미지를 번갈아 보여줌
        // -------------------------------------------------------------
        if (extraObjectImage2 != null && backGrounddataSO != null)
        {
            // 1. 이전 예약된 행동 취소 (박자가 빨라질 때 꼬임 방지)
            extraObjectImage2.DOKill();


            Sprite frontSprite = birdSprites[0];
            Sprite leftSprite = birdSprites[1];
            Sprite rightSprite = birdSprites[2];

            // 3. 이번 박자에 보여줄 스프라이트 결정 (좌/우 교대)
            Sprite targetSprite = isBirdTurnLeft ? leftSprite : rightSprite;

            // 4. 즉시 고개 돌리기 (Kick)
            extraObjectImage2.sprite = targetSprite;

            // [연출 디테일] 고개만 돌리면 심심하니 살짝 튀어오르는 느낌 추가 (선택사항)
            extraObjectImage2.rectTransform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), beatDuration * 0.2f, 1, 0);

            // 5. 일정 시간 후 정면으로 복귀 (Rest)
            // 전체 박자 시간의 60% 지점에서 정면을 봅니다. (박자가 빠르면 더 빨리 돌아옴)
            float returnTime = beatDuration * 0.6f;

            DOVirtual.DelayedCall(returnTime, () =>
            {
                if (extraObjectImage2 != null)
                {
                    extraObjectImage2.sprite = frontSprite;
                }
            })
            .SetTarget(extraObjectImage2.gameObject); // 안전하게 gameObject 타겟 설정

            // 6. 다음 박자를 위해 방향 토글 (Left -> Right -> Left...)
            isBirdTurnLeft = !isBirdTurnLeft;
        }
    }

    private void ChapterAction_1()
    {
         if (extraObjectImage != null)
        {
            RectTransform[] rectLists = { extraObjectImage.rectTransform, extraObjectImage2.rectTransform, extraObjectImage3.rectTransform };

            foreach (var rect in rectLists)
            {
                // 1. 이전 애니메이션 제거
                rect.DOKill();

                // 2. 크기 초기화 (항상 1.0에서 시작하도록 보정)
                // 비트가 빨라질 때 크기가 계속 커지는 현상을 방지합니다.
                rect.localScale = Vector3.one;

                // 3. 시퀀스 생성
                Sequence seq = DOTween.Sequence();
                seq.SetTarget(extraObjectImage.gameObject);

                // -------------------------------------------------------
                // [설정]
                // Base Scale: 1.0 (평소)
                // Target Scale: 1.5 (커졌을 때)
                // -------------------------------------------------------
                Vector3 targetScale = Vector3.one * 1.3f;

                // [단계 A] Kick: 1.0 -> 1.5 (팍! 하고 커짐)
                // beatDuration의 15% 시간 동안 빠르게 팽창
                seq.Append(rect.DOScale(targetScale, beatDuration * 0.15f)
                    .SetEase(Ease.OutQuad));

                // [단계 B] Rest: 1.5 -> 1.0 (바운스하며 복귀)
                // beatDuration의 85% 시간 동안 원래대로 돌아옴
                // Ease.OutBack: 목표값(1.0)을 살짝 지나쳤다가 돌아오는 "쫀득한" 느낌
                // Ease.OutElastic: "띠요옹" 하고 스프링처럼 튕기는 느낌 (취향에 따라 선택)
                seq.Append(rect.DOScale(Vector3.one*0.8f, beatDuration * 0.85f)
                    .SetEase(Ease.OutBack));
            }
        }
        if (extraObjectImage4 != null && backGrounddataSO != null)
        {
            // 1. 이전 예약된 행동 취소 (박자가 빨라질 때 꼬임 방지)
            extraObjectImage4.DOKill();

   
            Sprite frontSprite = birdSprites[0];
            Sprite leftSprite = birdSprites[1];
            Sprite rightSprite = birdSprites[2];

            // 3. 이번 박자에 보여줄 스프라이트 결정 (좌/우 교대)
            Sprite targetSprite = isBirdTurnLeft ? leftSprite : rightSprite;

            // 4. 즉시 고개 돌리기 (Kick)
            extraObjectImage4.sprite = targetSprite;

            // [연출 디테일] 고개만 돌리면 심심하니 살짝 튀어오르는 느낌 추가 (선택사항)
            extraObjectImage4.rectTransform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), beatDuration * 0.2f, 1, 0);

            // 5. 일정 시간 후 정면으로 복귀 (Rest)
            // 전체 박자 시간의 60% 지점에서 정면을 봅니다. (박자가 빠르면 더 빨리 돌아옴)
            float returnTime = beatDuration * 0.6f;

            DOVirtual.DelayedCall(returnTime, () =>
            {
                if (extraObjectImage4 != null)
                {
                    extraObjectImage4.sprite = frontSprite;
                }
            })
            .SetTarget(extraObjectImage4.gameObject); // 안전하게 gameObject 타겟 설정

            // 6. 다음 박자를 위해 방향 토글 (Left -> Right -> Left...)
            isBirdTurnLeft = !isBirdTurnLeft;
        }
    }

    private bool isChapter2Looping = false;
    private List<Sprite> spriteList = new List<Sprite>();
    private void ChapterAction_2()
    {
        if (spriteList == null || spriteList.Count == 0) return;

        // % (나머지 연산)을 쓰면 0, 1, 2, 3, 0, 1, 2, 3... 자동으로 무한 반복됩니다.
        int index = actionNumberTarget % spriteList.Count; // 혹은 % 4
        extraObjectImage.sprite = spriteList[index];

        actionNumberTarget++;
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

    private void ChapterAction_4()
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
            float startMargin = -70f;
            float shrinkMargin = 0f;

            // 2. 시작 상태 강제 초기화 (0에서 시작)
            UpdateRectMargin(rect, startMargin);

            // 3. 애니메이션 실행
            // BeatClockUI처럼 "팍!(Kick)" 하고 커졌다가 "스르륵(Rest)" 돌아오는 구조입니다.

            // [단계 A] Kick: 0 -> 100 (아주 빠르게! 0.05초)
            // 타겟을 rect로 명시(.SetTarget)해야 DOKill이 먹힙니다.
            DOVirtual.Float(startMargin, shrinkMargin, 0.2f, (x) =>
            {
                UpdateRectMargin(rect, x);
            })
            .SetTarget(rect) // [중요] 타겟을 설정해야 DOKill로 멈출 수 있음
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // [단계 B] Rest: 100 -> 0 (나머지 시간 동안 천천히 복구)
                DOVirtual.Float(shrinkMargin, startMargin, beatDuration * 0.8f, (x) =>
                {
                    UpdateRectMargin(rect, x);
                })
                .SetTarget(rect) // [중요] 여기도 타겟 설정
                .SetEase(Ease.OutQuad);
            });
        }
    }


    private bool isTiltRight = true;
    private void ChapterAction_5()
    {
        if (extraObjectImage == null) return;
        RectTransform rect = extraObjectImage.rectTransform;

        rect.DOKill();
        rect.localRotation = Quaternion.identity;

        // 방향 토글 (이번엔 오른쪽? 다음엔 왼쪽)
        float targetAngle = isTiltRight ? 7f : -7f;
        isTiltRight = !isTiltRight;

        Sequence seq = DOTween.Sequence();
        seq.SetTarget(extraObjectImage.gameObject);

        // 둠 (기울이기)
        seq.Append(rect.DOLocalRotate(new Vector3(0, 0, targetAngle), beatDuration * 0.2f)
            .SetEase(Ease.OutQuad));

        // 칫 (복구)
        seq.Append(rect.DOLocalRotate(Vector3.zero, beatDuration * 0.8f)
            .SetEase(Ease.OutBack));
    }

    // [추가] 스프링 상태 관리 변수 (true: 수축 차례, false: 발사 차례)
    private bool isSpringCompressPhase = true;

    private void ChapterAction_6()
    {
        if (extraObjectImage == null) return;

        RectTransform rect = extraObjectImage.rectTransform;

        // 1. 이전 애니메이션 제거 및 타겟 설정
        rect.DOKill();

        // -------------------------------------------------------
        // [설정] Inspector Top 값 기준  
        // -------------------------------------------------------
        float normalTop = 40f;      // 평소 상태
        float compressedTop = 80f; // 1차 박자: 꾹 눌린 상태 (값이 클수록 아래로 내려감)
        float stretchedTop = -30f;  // 2차 박자: 띠용하고 튀어 오른 상태 (음수일수록 위로 올라감)

        if (isSpringCompressPhase)
        {
            // ====================================================
            // Phase 1: 수축 (에너지 모으기)
            // Normal -> Compressed 상태로 이동 후 대기
            // ====================================================

            // 혹시 모를 위치 어긋남 방지를 위해 시작점 강제 설정
            UpdateRectTop(rect, normalTop);

            // 꾹 누르는 애니메이션 (빠르고 단단하게)
            DOVirtual.Float(normalTop, compressedTop, beatDuration * 0.4f, (val) =>
            {
                UpdateRectTop(rect, val);
            })
            .SetTarget(rect)
            .SetEase(Ease.OutCubic); // OutCubic: 묵직하게 눌리는 느낌

            // 다음 박자는 발사
            isSpringCompressPhase = false;
        }
        else
        {
            // ====================================================
            // Phase 2: 발사! (띠용~)
            // Compressed -> Stretched(튀어오름) -> Normal(복귀)
            // ====================================================

            // 압축된 상태에서 시작한다고 가정 (안전장치로 강제 설정)
            UpdateRectTop(rect, compressedTop);

            Sequence seq = DOTween.Sequence();
            seq.SetTarget(rect);

            // A. 발사!: 압축 상태에서 가장 높은 곳으로 순식간에 이동 (전체 시간의 30%)
            seq.Append(DOVirtual.Float(compressedTop, stretchedTop, beatDuration * 0.3f, (val) =>
            {
                UpdateRectTop(rect, val);
            }).SetEase(Ease.OutQuad)); // OutQuad: 빠르게 치고 나감

            // B. 띠요옹 복귀: 높은 곳에서 원래 위치로 탄력있게 돌아옴 (전체 시간의 70%)
            seq.Append(DOVirtual.Float(stretchedTop, normalTop, beatDuration * 0.7f, (val) =>
            {
                UpdateRectTop(rect, val);
            })
            // Ease.OutElastic: 고무줄이나 스프링이 튕기는 느낌
            // 매개변수 (반동 크기, 횟수) 조절 가능: (1.2f, 0.4f) 추천
            .SetEase(Ease.OutElastic, 1.2f, 0.4f));

            // 다음 박자는 다시 수축
            isSpringCompressPhase = true;
        }
    }

    private void ChapterAction_7()
    {
        if (extraObjectImage2 == null) return;

        RectTransform rect = extraObjectImage2.rectTransform;
        rect.DOKill();

        // -------------------------------------------------------
        // [설정] 위치 값 (화면 해상도에 따라 조절 필요)
        // baseY: 평소 대기 위치 (손끝만 살짝 보이는 위치)
        // reachY: 박자를 탈 때 올라오는 최고점 (손목까지 보이는 위치)
        // -------------------------------------------------------
        float baseY = -180f;  // 화면 아래 숨겨진 위치
        float reachY = -40f; // 위로 솟구치는 목표 위치

        // 안전장치: 시작 위치가 너무 엉뚱하면 안 되니 baseY 근처로 보정
        // (너무 딱딱하게 고정하지 않고, 현재 위치에서 시작하되 튀지 않게)
        if (rect.anchoredPosition.y > reachY)
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, baseY);

        Sequence seq = DOTween.Sequence();
        seq.SetTarget(extraObjectImage2.gameObject);

        // A. Snatch! (낚아채기): 아주 빠르게 위로 솟구침 (전체 시간의 25%)
        // Ease.OutCirc: 시작이 폭발적이고 끝에서 급격히 감속 (가장 "손" 같은 느낌)
        seq.Append(rect.DOAnchorPosY(reachY, beatDuration * 0.1f*actionNumberTarget)
            .SetEase(Ease.OutCirc));

        // B. Slip (미끄러짐): 천천히, 무겁게 아래로 떨어짐 (전체 시간의 75%)
        // Ease.InOutSine: 끈적하게 미끄러지는 느낌 (단순 선형보다 훨씬 기괴함)
        seq.Append(rect.DOAnchorPosY(baseY, beatDuration * 0.9f* actionNumberTarget)
            .SetEase(Ease.InOutSine));

        // [디테일 추가] 손이 올라올 때 살짝 회전도 주면 더 리얼합니다.
        // 솟구칠 때 각도를 살짝 틀었다가 돌아오기
        rect.localRotation = Quaternion.Euler(0, 0, -1f); // 살짝 기울인 상태 시작
        rect.DORotate(new Vector3(0, 0, 1f), beatDuration * 0.2f* actionNumberTarget).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                rect.DORotate(new Vector3(0, 0, -1f), beatDuration * 0.8f* actionNumberTarget).SetEase(Ease.InOutSine);
            });
    }

    private void ChapterAction_10()
    {

    }
    #endregion

    // [헬퍼 함수] 상하좌우 여백(Margin)을 한 번에 조절하는 함수
    // margin이 0이면 stretch 꽉 채움, 50이면 사방에서 50씩 들어옴
    public void UpdateRectMargin(RectTransform rect, float margin)
    {
        // Left, Bottom은 정수값 그대로
        rect.offsetMin = new Vector2(margin, margin);
        // Right, Top은 음수값이어야 안쪽으로 들어옴
        rect.offsetMax = new Vector2(-margin, -margin);
    }

    public void UpdateRectPosition(RectTransform rect, float X, float Y)
    {
        if (rect == null) return;

        Image img = rect.GetComponent<Image>();

        // [중요] 앵커와 피벗을 먼저 '중앙'으로 잡아야 계산이 깔끔합니다.

        // 1. 앵커를 부모의 정중앙(Center)으로 설정
        // Min과 Max가 둘 다 (0.5, 0.5)면 Stretch(늘리기)가 풀리고 중앙 점이 됩니다.
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);

        // 2. 피벗(중심축)도 이미지의 정중앙으로 설정
        rect.pivot = new Vector2(0.5f, 0.5f);

        // 3. 사이즈를 원본 크기로 맞춤 (앵커/피벗 세팅 후에 하는 것이 안전)
        if (img != null)
        {
            img.SetNativeSize();
        }

        // 4. 스케일 초기화 (혹시 찌그러져 있을 수 있으므로)
        rect.localScale = Vector3.one;

        // 5. 위치 적용 (이제 (0,0)은 화면 정중앙입니다)
        rect.anchoredPosition = new Vector2(X, Y);
    }

    // 외부 효과 스크립트를 연결하는 제네릭 함수
    private void ConnectExternalEffect<T>() where T : Component, IBackGroundEffect
    {
        // 컴포넌트 선언
        T effectComponent = null;

        if (effectComponent == null)
        {
            Debug.LogWarning($"BackGroundController: {typeof(T).Name} 컴포넌트가 없습니다! 추가합니다.");
            effectComponent = gameObject.AddComponent<T>();
        }

        // 2. 초기화 실행 (컨트롤러 권한 부여)
        effectComponent.Initialize(this);
        // 3. 액션 연결 (델리게이트에 인터페이스 메서드 등록)
        currentChapterAction = effectComponent.EffectActionGo;


        Debug.Log($"Connected External Effect: {typeof(T).Name}");
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

    // [헬퍼 함수] Inspector의 "Top" 값을 변경하는 함수
    // 기존의 Right(offsetMax.x) 값은 건드리지 않고 Top만 바꿉니다.
    private void UpdateRectTop(RectTransform rect, float top)
    {
        // Unity UI 구조상 Inspector의 Top 값은 offsetMax.y의 '음수' 값입니다.
        // 예: Top 40 => offsetMax.y = -40
        rect.offsetMax = new Vector2(rect.offsetMax.x, -top);
    }
}

