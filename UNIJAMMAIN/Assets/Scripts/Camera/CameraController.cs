using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CameraController : MonoBehaviour
{
    public static float TargetBaseSize { get; private set; } = 5f;
    public static bool IsLocked { get; private set; } = false; // 몬스터 액션 중인지 여부

    public static void SetMonsterMode(bool isActive, float size = 4f)
    {
        IsLocked = isActive;
        TargetBaseSize = isActive ? size : 5f;
    }
}