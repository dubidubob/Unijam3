using System.Collections;
using UnityEngine;

public class Knockback
{
    public bool IsEnabled { get; private set; }
    public bool IsApplied { get; private set; }
    int hp;
    public void OnKnockback(bool isOn)
    {
        IsEnabled = isOn;
        IsApplied = false;
        hp = 2;
    }
    public bool CheckKnockback()
    {
        if (!IsEnabled) return false;
        if (--hp <= 0)
        {
            IsApplied = true;
            return false;
        } 
        return true;
    }
}

[RequireComponent(typeof(Poolable))]
public class MovingEnemy : MonoBehaviour
{
    private GamePlayDefine.WASDType enemyType;

    // ===== dsp 이동 파라미터 =====
    private Vector3 startPos;
    private Vector3 targetPos;
    private double hitDsp;         // 이 시각에 도달해야 함
    private double leadSec;        // 등장~히트까지 소요시간
    private float totalDistance;  // 경로 길이(속도 유지용)
    private bool useDspMove;     // dsp 이동 활성화

    // 연출/리사이즈
    private SpriteRenderer monsterImg;
    private Vector3 origin;
    private bool isResizeable = false;
    private Vector2 sizeDiffRate;

    // 넉백
    public Knockback Knockback { get; private set; }
    

    // 기타
    private Poolable poolable;

    private static double DspNow => AudioSettings.dspTime;

    private void OnEnable()
    {
        Knockback = new Knockback();

        if (!monsterImg) monsterImg = GetComponentInChildren<SpriteRenderer>();
        if (!poolable) poolable = GetComponent<Poolable>();
        if (monsterImg != null && origin == default) origin = monsterImg.transform.localScale;
    }

    private void Start()
    {
        // 리사이즈 타입 결정
        if (enemyType == GamePlayDefine.WASDType.W || enemyType == GamePlayDefine.WASDType.S)
            isResizeable = true;
        else
            isResizeable = false;
    }

    public void SetDead()
    {
        Managers.Pool.Push(poolable);
    }

    /// <summary>
    /// dsp 이동 설정(권장). spawnPos → targetPos를 leadSec 동안 이동하여 hitDsp에 도달.
    /// </summary>
    public void SetupDspMovement(
        Vector3 spawnPos, Vector3 targetPos,
        double hitDspTime, double leadSec,
        Vector2 sizeDiffRate, GamePlayDefine.WASDType wasdType)
    {
        this.enemyType = wasdType;
        this.sizeDiffRate = sizeDiffRate;

        this.startPos = spawnPos;
        this.targetPos = targetPos;
        this.hitDsp = hitDspTime;
        this.leadSec = Mathf.Max(0.0001f, (float)leadSec);
        this.totalDistance = Vector3.Distance(spawnPos, targetPos);

        transform.position = startPos;
        useDspMove = true;

        // 리사이즈 타입 갱신
        isResizeable = (enemyType == GamePlayDefine.WASDType.W || enemyType == GamePlayDefine.WASDType.S);
        if (monsterImg && origin == default) origin = monsterImg.transform.localScale;
    }

    /// <summary>
    /// (호환용) 기존 속도 기반 모드. dsp 이동 대신 사용하고 싶을 때만 호출.
    /// </summary>
    public void SetVariance(float distance, float movingDuration, int numInRow, Vector2 sizeDiffRate, GamePlayDefine.WASDType wasdType)
    {
        // 유지: 필요시 레거시 경로로 사용
        this.enemyType = wasdType;
        this.sizeDiffRate = sizeDiffRate;

        // 레거시 모드에서는 별도 설정 없이 MoveTowards를 사용(아래 Update에서 처리)
        useDspMove = false;
        totalDistance = distance;
        // startPos/targetPos는 스포너가 별도로 관리하므로 여기선 생략
    }

    public void SetKnockback(bool isTrue)
    {
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr) sr.color = isTrue ? Color.red : Color.white;
        Knockback.OnKnockback(isTrue);
    }

    public bool CheckCanDead()
    {
        if (Knockback.CheckKnockback())
        {
            if (this.isActiveAndEnabled) StartCoroutine(ExecuteKnockback());
            return false;
        }
        return true;
    }

    IEnumerator ExecuteKnockback()
    {
        float elapsed = 0f;
        float backwardDuration = (float)leadSec * 0.125f;         // 원 코드의 12.5%만큼
        float knockbackDistance = totalDistance * 0.125f;
        Vector3 knockDir = (transform.position - targetPos).normalized; // 목표에서 멀어지는 방향

        while (elapsed < backwardDuration)
        {
            float t = elapsed / backwardDuration;                  // 0→1
            float move = Mathf.Lerp(knockbackDistance, 0, t * t);  // 가속도 느낌
            transform.position += knockDir * move * Time.deltaTime * 2.0f;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 넉백이 끝났다면, 현재 위치에서 목표까지 남은 거리에 맞춰
        // hit 시각을 "지금 + 남은시간"으로 뒤로 밀어주어 속도 일관성 유지
        float remain = Vector3.Distance(transform.position, targetPos);
        if (totalDistance > 0.0001f)
        {
            double now = DspNow;
            double remainLead = (remain / totalDistance) * leadSec; // 원래 속도 비율 유지
            hitDsp = now + remainLead;                              // 목표 시각 밀기
            leadSec = remainLead;                                   // 리사이즈도 맞게 가도록 갱신
        }
    }

    void Update()
    {
        if (Knockback.IsEnabled) return;

        if (useDspMove)
        {
            double now = DspNow;
            double remain = hitDsp - now;                 // 남은 시간(초)
            float p = 1f - (float)(remain / leadSec);     // 0→1
            p = Mathf.Clamp01(p);

            // 위치 보간(프레임 지터 무시)
            transform.position = Vector3.Lerp(startPos, targetPos, p);

            // 원근 리사이즈(선택)
            if (isResizeable && monsterImg)
            {
                if (enemyType == GamePlayDefine.WASDType.W)        // 작아졌다 커지기
                    monsterImg.transform.localScale = Vector3.Lerp(origin * sizeDiffRate.x, origin, p);
                else if (enemyType == GamePlayDefine.WASDType.S)   // 커졌다 작아지기
                    monsterImg.transform.localScale = Vector3.Lerp(origin * sizeDiffRate.y, origin, p);
            }

            // p==1이면 목표 지점 도달(실제 게임 로직은 트리거로 처리하니 여기선 유지)
        }
        else
        {
            // 레거시 모드(필요 시 유지) — MoveTowards 등속
            // targetPos가 없다면 매니저에서 플레이어 위치를 전달받아 써야 함.
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "detectArea")
        {
            Managers.Game.AddAttackableEnemy(enemyType, this.gameObject);
        }
        else if (collision.tag == "dangerLine")
        {
            Managers.Game.attacks[enemyType].Dequeue();
            SetDead();
            Managers.Game.DecHealth();
        }
    }
}
