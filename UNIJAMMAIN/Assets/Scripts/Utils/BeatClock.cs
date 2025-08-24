using System;
using UnityEngine;

public class BeatClock : MonoBehaviour
{
    private double beatInterval;
    private double nextBeatTime;
    private double startTime;

    public static Action<double> OnBeat;

    private bool isClockStart = false;
    private void Start()
    {
        IngameData.ChangeBpm -= Init;
        IngameData.ChangeBpm += Init;
    }
    private void Init()
    {
        startTime = AudioSettings.dspTime;
        beatInterval = IngameData.BeatInterval;
        nextBeatTime = startTime + beatInterval;

        isClockStart = true;
    }

    void Update()
    {
        if (isClockStart)
        {
            double currentTime = AudioSettings.dspTime;

            // 한 프레임에 여러 박자가 지나갔을 경우 처리
            while (currentTime >= nextBeatTime)
            {
                OnBeat?.Invoke(nextBeatTime);

                nextBeatTime += beatInterval;
            }
        }
    }
}