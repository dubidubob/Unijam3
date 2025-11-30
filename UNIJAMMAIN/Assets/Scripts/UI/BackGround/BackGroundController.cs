using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundController : MonoBehaviour
{

    private readonly string satPropName = "_Saturation";

    public Material sharedMaterial;


    private void OnDestroy()
    {
        sharedMaterial.SetFloat(satPropName, 1f);
    }
    public void SettingSatuation(float value)
    {
        if (sharedMaterial != null)
        {
            sharedMaterial.SetFloat(satPropName, value);
        }
    }


    private void OnApplicationQuit()
    {
        if (sharedMaterial != null)
        {
            sharedMaterial.SetFloat(satPropName, 1f); // 1.0(원본)으로 복구
        }
    }

}

