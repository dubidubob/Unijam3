using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LocalizeRotationFix : MonoBehaviour
{
    [Header("언어별 Z 각도 직접 입력")]
    public float koreanAngle = -29.2f;
    public float englishAngle = 60f;    // 영어일 때 텍스트 각도
    public float chineseAngle = -27.7f;
    public float japaneseAngle = -29.2f;

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        StartCoroutine(ApplyRotationDelayed(LocalizationSettings.SelectedLocale));
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        StartCoroutine(ApplyRotationDelayed(locale));
    }

    private System.Collections.IEnumerator ApplyRotationDelayed(Locale locale)
    {
        yield return null; // 유니티가 딴짓(버그) 못하게 1프레임 뒤에 강제로 박아버림

        if (locale == null) yield break;

        float targetZ = 0f;
        string code = locale.Identifier.Code;

        if (code.Contains("ko")) targetZ = koreanAngle;
        else if (code.Contains("en")) targetZ = englishAngle;
        else if (code.Contains("zh")) targetZ = chineseAngle;
        else if (code.Contains("ja")) targetZ = japaneseAngle;

        // 사람이 읽는 직관적인 각도(Euler)로 직접 멱살 잡고 고정!
        transform.localEulerAngles = new Vector3(0, 0, targetZ);
    }
}