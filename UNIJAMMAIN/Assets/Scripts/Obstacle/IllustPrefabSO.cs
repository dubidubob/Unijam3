using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MonsterTypeSO;
using static IllustController;

[CreateAssetMenu(fileName = "IllustPrefabSO", menuName = "SO/IllustPrefabSO")]
public class IllustPrefabSO : ScriptableObject
{
    [System.Serializable]
    public struct IllustData
    {
        public GamePlayDefine.IllustType type;
        public List<Sprite> illustLists;
    }

    [SerializeField] List<IllustData> illusts = new List<IllustData>();

    public IllustData GetIllust(GamePlayDefine.IllustType illustType)
    {
        IllustData go = new IllustData();

        foreach (IllustData illustdata in illusts)
        {
            if (illustdata.type == illustType)
                go = illustdata;
        }

        return go;
    }
}
