using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TextSO", menuName = "TextSO/GameOverText")]
public class GameOverTextSO : ScriptableObject
{
    [System.Serializable]
    public class StageText
    {
        public List<string> gameOverText;
    }
    public List<StageText> StageTexts;
}
