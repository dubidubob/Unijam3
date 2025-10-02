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

            // ���� �ٽ� ����: ���� �ð��� �������� '���ؼ�' ��ǥ �ð��� ��¦ ����ϴ� ����
            double adjustedTime = targetDspTime + BeatClock.GameSettings.CountdownOffset;

            // ��ǥ �ð��� �� ������ �� �����Ӹ��� ���� �ð��� Ȯ���ϸ� ����մϴ�.
            while (AudioSettings.dspTime < adjustedTime)
            {
                yield return null;
            }
            // �������������������������������������������

            // ��Ȯ�� �ð��� �Ǹ� ��������Ʈ�� ��ü�մϴ�.
            sp.sprite = sprites[i];
        }

        // ������ ��������Ʈ("����!")�� �� ���� ���� ������ ���̵��� �߰��� ��ٸ��ϴ�.
        float lastWait = (float)BeatClock.BeatInterval;
        yield return new WaitForSeconds(lastWait);

        // ���� ó��
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
//        // �Ϸ� ����: running�� false�� �ǰų�, cnt�� sprites.Count �̻�
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
//            // State ����
//            Managers.Game.currentPlayerState = GameManager.PlayerState.Normal;
//            return;
//        }

//        sp.sprite = sprites[cnt++];
//    }
//}
