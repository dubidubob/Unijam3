using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 해당 페이즈에 대한 정보를 갖고 있다.
/// </summary>
public class PhaseInfo : MonoBehaviour
{
    [SerializeField] private float duration; // 페이즈 지속 기간(초)
    public float GetDuration() => duration;

    [SerializeField]
    private List<MonsterData> monsterDatas;
    public IReadOnlyList<MonsterData> MonsterDatas => monsterDatas;
}
