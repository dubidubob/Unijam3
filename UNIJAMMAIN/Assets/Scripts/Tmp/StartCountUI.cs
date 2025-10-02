using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartCountUI : MonoBehaviour
{
    [SerializeField] List<Sprite> sprites = new List<Sprite>();

    private SpriteRenderer sp;
    private bool isCounting = false;

    void Awake()
    {
        sp = GetComponent<SpriteRenderer>();
    }

    public IEnumerator Play123Coroutine()
    {
        if (isCounting)
            yield break;

        isCounting = true;
        int count = 0;
        gameObject.SetActive(true);

        long startBeat = BeatClock.CurrentTick;
        long firstCountdownBeat = startBeat + 1;

        for (int i = 0; i < sprites.Count; i++)
        {
            long targetBeat = firstCountdownBeat + i;
            double targetDspTime = BeatClock.GetScheduledTime(targetBeat);

            // ▼▼▼ 핵심 로직: 정박 시간에 오프셋을 '더해서' 목표 시간을 살짝 늦춥니다 ▼▼▼
            double adjustedTime = targetDspTime + BeatClock.GameSettings.CountdownOffset;

            // 목표 시간이 될 때까지 매 프레임마다 현재 시간을 확인하며 대기합니다.
            while (AudioSettings.dspTime < adjustedTime)
            {
                yield return null;
            }
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

            // 정확한 시간이 되면 스프라이트를 교체합니다.
            sp.sprite = sprites[i];
        }

        // 마지막 스프라이트("시작!")가 한 박자 동안 온전히 보이도록 추가로 기다립니다.
        float lastWait = (float)BeatClock.BeatInterval;
        yield return new WaitForSeconds(lastWait);

        // 종료 처리
        gameObject.SetActive(false);
        Managers.Game.currentPlayerState = GameManager.PlayerState.Normal;
        isCounting = false;
    }
}

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class StartCountUI : MonoBehaviour
//{
//    [SerializeField] List<Sprite> sprites = new List<Sprite>();

//    private SpriteRenderer sp;
//    private bool running = false;
//    void OnEnable()
//    {
//        sp = GetComponent<SpriteRenderer>();

//        BeatClock.OnBeat -= Show123Start;
//        BeatClock.OnBeat += Show123Start;
//    }

//    private void OnDisable()
//    {
//        BeatClock.OnBeat -= Show123Start;
//    }

//    public IEnumerator Play123Coroutine()
//    {
//        Start123();
//        // 완료 조건: running이 false가 되거나, cnt가 sprites.Count 이상
//        yield return new WaitUntil(() => !running);
//    }
//    private void Start123()
//    {
//        if (cnt != 0) return;

//        running = true;
//    }

//    int cnt = 0;
//    private void Show123Start(double _, long __)
//    {
//        if (!running) return;
//        if (sprites.Count <= cnt)
//        {
//            running = false;
//            this.enabled = false;
//            this.gameObject.SetActive(false);
//            // State 관리
//            Managers.Game.currentPlayerState = GameManager.PlayerState.Normal;
//            return;
//        }

//        sp.sprite = sprites[cnt++];
//    }
//}
