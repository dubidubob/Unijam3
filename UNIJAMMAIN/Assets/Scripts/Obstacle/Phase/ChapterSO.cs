using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 해당 페이즈에 대한 정보를 갖고 있다.
/// </summary>
[CreateAssetMenu(fileName = "Chapter", menuName = "SO/Chapter")]
public class ChapterSO : ScriptableObject
{
    [SerializeField]
    [SerializeReference]
    private List<GameEvent> phases = new();
    public IReadOnlyList<GameEvent> Phases => phases;
    public string MusicPath;
}
