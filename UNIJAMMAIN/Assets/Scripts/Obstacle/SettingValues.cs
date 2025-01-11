using UnityEngine;
using TMPro;

public class SettingValues : MonoBehaviour
{
    [SerializeField] private SpawnController spawnController;

    [Header("Overall")]
    [SerializeField] private TMP_InputField initialInterval;
    [SerializeField] private TMP_InputField rangeDebuf;
    [SerializeField] private TMP_InputField updownDebuf;

    [Header("Phase1")]
    [SerializeField] private TMP_InputField phase1Duration;
    [SerializeField] private TMP_InputField phase1DefaultSpeed;
    [SerializeField] private TMP_InputField phase1TargetSpeed;
    [SerializeField] private TMP_InputField phase1IntervalDecRate;

    [Header("Phase2")]
    [SerializeField] private TMP_InputField phase2Duration;
    [SerializeField] private TMP_InputField phase2DefaultSpeed;
    [SerializeField] private TMP_InputField phase2TargetSpeed;
    [SerializeField] private TMP_InputField phase2IntervalDecRate;

    [SerializeField] private TMP_InputField phase2DefaultLifetime;
    [SerializeField] private TMP_InputField phase2TargetLifetime;
    [SerializeField] private TMP_InputField phase2RangedIntervalDecRate;

    [Header("Phase3")]
    [SerializeField] private TMP_InputField phase3Duration;
    [SerializeField] private TMP_InputField phase3DefaultSpeed;
    [SerializeField] private TMP_InputField phase3TargetSpeed;
    [SerializeField] private TMP_InputField phase3IntervalDecRate;

    [SerializeField] private TMP_InputField phase3DefaultLifetime;
    [SerializeField] private TMP_InputField phase3TargetLifetime;
    [SerializeField] private TMP_InputField phase3RangedIntervalDecRate;

    [SerializeField] private TMP_InputField phase3CntMouseInputTwo;
    [SerializeField] private TMP_InputField phase3MouseIntervalDecRate;

    public void Confirm()
    {
        //// == Overalll ==
        //UpdateFloatValue(ref spawnController.initialInterval, initialInterval.text);
        //UpdateFloatValue(ref spawnController.rangeDebuf, rangeDebuf.text);
        //UpdateFloatValue(ref spawnController.updownDebuf, updownDebuf.text);

        //// === Phase 1 ===
        //UpdateFloatValue(ref spawnController.phase1.movingPhaseDuration, phase1Duration.text);
        //UpdateFloatValue(ref spawnController.phase1.movingDefaultSpeed, phase1DefaultSpeed.text);
        //UpdateFloatValue(ref spawnController.phase1.movingTargetSpeed, phase1TargetSpeed.text);
        //UpdateFloatValue(ref spawnController.phase1.movingIntervalDecRate, phase1IntervalDecRate.text);

        //// === Phase 2 ===
        //UpdateFloatValue(ref spawnController.phase2.movingPhaseDuration, phase2Duration.text);
        //UpdateFloatValue(ref spawnController.phase2.movingDefaultSpeed, phase2DefaultSpeed.text);
        //UpdateFloatValue(ref spawnController.phase2.movingTargetSpeed, phase2TargetSpeed.text);
        //UpdateFloatValue(ref spawnController.phase2.movingIntervalDecRate, phase2IntervalDecRate.text);

        //UpdateFloatValue(ref spawnController.phase2.rangedDefaultLifetime, phase2DefaultLifetime.text);
        //UpdateFloatValue(ref spawnController.phase2.rangedTargetLifetime, phase2TargetLifetime.text);
        //UpdateFloatValue(ref spawnController.phase2.rangedIntervalDecRate, phase2RangedIntervalDecRate.text);

        //// === Phase 3 ===
        //UpdateFloatValue(ref spawnController.phase3.movingPhaseDuration, phase3Duration.text);
        //UpdateFloatValue(ref spawnController.phase3.movingDefaultSpeed, phase3DefaultSpeed.text);
        //UpdateFloatValue(ref spawnController.phase3.movingTargetSpeed, phase3TargetSpeed.text);
        //UpdateFloatValue(ref spawnController.phase3.movingIntervalDecRate, phase3IntervalDecRate.text);

        //UpdateFloatValue(ref spawnController.phase3.rangedDefaultLifetime, phase3DefaultLifetime.text);
        //UpdateFloatValue(ref spawnController.phase3.rangedTargetLifetime, phase3TargetLifetime.text);
        //UpdateFloatValue(ref spawnController.phase3.rangedIntervalDecRate, phase3RangedIntervalDecRate.text);

        //UpdateIntValue(ref spawnController.phase3.cntMouseInputTwo, phase3CntMouseInputTwo.text);

        //UpdateFloatValue(ref spawnController.phase3.mouseIntervalDecRate, phase3MouseIntervalDecRate.text);

        //Debug.Log("설정값이 적용되었습니다!");

        //spawnController.InitAgain();
    }

    private void UpdateFloatValue(ref float field, string input)
    {
        if (float.TryParse(input, out float result))
        {
            field = result;
            Debug.Log($"field saved : {field}");
        }
        else
        {
            //Debug.LogWarning($"Invalid float input: {input}. Field remains unchanged.");
        }
    }

    private void UpdateIntValue(ref int field, string input)
    {
        if (int.TryParse(input, out int result))
        {
            field = result;
            Debug.Log($"field saved : {field}");
        }
        else
        {
            //ebug.LogWarning($"Invalid int input: {input}. Field remains unchanged.");
        }
    }
}
