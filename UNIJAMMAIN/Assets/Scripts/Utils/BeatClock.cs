using System;
using UnityEngine;

public class BeatClock : MonoBehaviour
{
    [SerializeField] private double bpm = 84.0;
    private double beatInterval;
    private double nextBeatTime;
    private double startTime;

    public static Action OnBeat;

    void Awake()
    {
        SetBPM(bpm);
    }

    public void SetBPM(double newBpm)
    {
        bpm = newBpm;
        beatInterval = 60f / bpm;
        IngameData.BeatInterval = beatInterval;

        startTime = AudioSettings.dspTime;
        nextBeatTime = startTime + beatInterval;
    }

    void Update()
    {
        double currentTime = AudioSettings.dspTime;

        // �� �����ӿ� ���� ���ڰ� �������� ��� ó��
        while (currentTime >= nextBeatTime)
        {
            OnBeat?.Invoke();
            
            nextBeatTime += beatInterval;
        }
    }
}