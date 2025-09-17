    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

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
        public Sprite backGroundSprite;
        public Sprite backGroundGraySprite;
        public Color colorPalette;



#if UNITY_EDITOR
    private void OnValidate()
        {
            if (phases == null) return;
            foreach (var e in phases)
                if (e is TutorialEvent te) te.RecalculateDuration();
        }
    #endif
    }
