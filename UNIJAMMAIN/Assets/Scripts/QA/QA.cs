using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PhaseManager))]
[RequireComponent(typeof(WASDMonsterSpawner))]
public class QA : MonoBehaviour
{
    [SerializeField] GameObject QAPanel;

    [Header("PhaseManager 설정용 Input Fields")]
    [SerializeField] private TMP_InputField startDelayField;
    [SerializeField] private TMP_InputField phaseDurationField;
    [SerializeField] private TMP_InputField spawnDurationField;
    [SerializeField] private TMP_InputField moveToHolderField;

    [Header("WASDMonsterSpawner 설정용 Input Fields")]
    [SerializeField] private TMP_InputField SsizeField;
    [SerializeField] private TMP_InputField WsizeField;
    [SerializeField] private TMP_InputField maxCountField;
    [SerializeField] private Toggle[] keys = new Toggle[4];

    PhaseManager phaseManager;
    WASDMonsterSpawner WASDMonsterSpawner;

    // phase Manager, 변경 버튼 누르면
    private float startDelay = 0.12f, phaseDuration = 1000f;
    private MonsterData monsterData =
        new MonsterData {
            isIn = true,
            monsterType = Define.MonsterType.WASD,
            numInRow = 1,
            spawnDuration = 1f,
            moveToHolderDuration = 1f,
            speedUpRate = 1f
        };

    // wasdspawncontroller, 변경 버튼 누르면
    private Vector2 sizeDiffRate = new Vector2(0.8f, 1.2f);
    private List<int> idxList = new List<int> { 0, 1, 2, 3 };
    private int[] idx => idxList.ToArray();
    private int maxCnt = 3;

    private void Awake()
    {
        phaseManager = GetComponent<PhaseManager>();
        WASDMonsterSpawner = GetComponent<WASDMonsterSpawner>();
        
        QAPanel.SetActive(isPopup);

        Managers.Input.SettingpopAction -= QAPanelPopup;
        Managers.Input.SettingpopAction += QAPanelPopup;

        // 각 InputField 콜백 등록
        startDelayField.onEndEdit.AddListener(SetStartDelay);
        phaseDurationField.onEndEdit.AddListener(SetPhaseDuration);
        spawnDurationField.onEndEdit.AddListener(SetSpawnDuration);
        moveToHolderField.onEndEdit.AddListener(SetMoveToHolderDuration);

        SsizeField.onEndEdit.AddListener(SetSizeS);
        WsizeField.onEndEdit.AddListener(SetSizeW);
        
        maxCountField.onEndEdit.AddListener(SetMaxCnt);

        for(int i=0; i<keys.Length; i++)
        {
            int keyIndex = i;  // 람다 안에서 올바르게 캡처되도록 로컬에 복사
            keys[keyIndex].onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    if (!idxList.Contains(keyIndex))
                        idxList.Add(keyIndex);
                }
                else
                {
                    idxList.Remove(keyIndex);
                }
            });
        }
    }

    bool isPopup = false;
    private void QAPanelPopup()
    {
        isPopup = !isPopup;
        QAPanel.SetActive(isPopup);
    }

    private void Start()
    {
        ApplySettings();
    }

    public void ReloadScene() { ReloadScene(); }

    public void ApplySettings()
    {
        phaseManager.QAPhaseVariance(startDelay, phaseDuration, monsterData);
        WASDMonsterSpawner.QAUpdateVariables(sizeDiffRate, idx, maxCnt);
    }

    #region — InputField 콜백 메서드들 —
    private void SetStartDelay(string txt)
        => float.TryParse(txt, out startDelay);

    private void SetPhaseDuration(string txt)
        => float.TryParse(txt, out phaseDuration);

    private void SetSpawnDuration(string txt)
        => float.TryParse(txt, out monsterData.spawnDuration);

    private void SetMoveToHolderDuration(string txt)
        => float.TryParse(txt, out monsterData.moveToHolderDuration);

    private void SetSpeedUpRate(string txt)
        => float.TryParse(txt, out monsterData.speedUpRate);

    private void SetSizeW(string txt)
        => float.TryParse(txt, out sizeDiffRate.x);

    private void SetSizeS(string txt)
        => float.TryParse(txt, out sizeDiffRate.y);

    private void SetMaxCnt(string txt)
        => int.TryParse(txt, out maxCnt);
    #endregion

}
