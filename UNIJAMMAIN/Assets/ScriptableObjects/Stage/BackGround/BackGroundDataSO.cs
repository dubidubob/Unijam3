using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BackGroundDatabase", menuName = "BackGround/Database")]
public class BackGroundDataSO : ScriptableObject
{
     [System.Serializable]
     public class BackGroundData
    {
        public List<Sprite> extraBackGroundLists;
    }
    public List<BackGroundData> backGroundDatas;
}
