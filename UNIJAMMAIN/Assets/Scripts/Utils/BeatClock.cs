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

            // �� �����ӿ� ���� ���ڰ� �������� ��� ó��
            while (currentTime >= nextBeatTime)
            {
                OnBeat?.Invoke(nextBeatTime);

                nextBeatTime += beatInterval;
            }
        }
    }
}