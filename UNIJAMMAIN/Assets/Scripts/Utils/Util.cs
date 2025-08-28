using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util
{
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
        {
            component = go.AddComponent<T>();
        }
        return component;
    }
    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false) // ���ӿ�����Ʈ ��ü�� ��ȯ
    {
        Transform transfrom = FindChild<Transform>(go, name, recursive);
        if (transfrom == null)
        {
            return null;
        }
        return transfrom.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object // ���� ������Ʈ�� �ڽĵ��� T�� ������Ʈ ��ȯ
    {
        if (go == null)
            return null;
        if(recursive == false) 
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                    {
                        return component;
                    }
                }
            }

        }
        else
        {
            foreach(T component in go.GetComponentsInChildren<T>())
            {
                if(string.IsNullOrEmpty(name) || component.name == name)
                {
                    return component;
                }

            }
        }
        return null;
    }

    public static GamePlayDefine.AllType WASDTypeChange(GamePlayDefine.WASDType type)
    {
        GamePlayDefine.AllType allType = type switch
        {
            GamePlayDefine.WASDType.W => GamePlayDefine.AllType.W,
            GamePlayDefine.WASDType.A => GamePlayDefine.AllType.A,
            GamePlayDefine.WASDType.S => GamePlayDefine.AllType.S,
            GamePlayDefine.WASDType.D => GamePlayDefine.AllType.D,
            _ => GamePlayDefine.AllType.Idle
        };
        return allType;
    }

    public static GamePlayDefine.AllType DiagonalTypeChange(GamePlayDefine.DiagonalType type)
    {
        GamePlayDefine.AllType allType = type switch
        {
            GamePlayDefine.DiagonalType.LeftDown => GamePlayDefine.AllType.LeftDown,
            GamePlayDefine.DiagonalType.LeftUp => GamePlayDefine.AllType.LeftUp,
            GamePlayDefine.DiagonalType.RightDown => GamePlayDefine.AllType.RightDown,
            GamePlayDefine.DiagonalType.RightUp => GamePlayDefine.AllType.RightUp,
            _ => GamePlayDefine.AllType.Idle
        };
        return allType;
    }

}
