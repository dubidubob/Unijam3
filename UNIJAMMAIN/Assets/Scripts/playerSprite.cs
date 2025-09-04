using System.Collections;
using UnityEngine;

public class playerSprite : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void Start()
    {
        StartCoroutine(endAnim());
    }

    private IEnumerator endAnim()
    {
        yield return new WaitForSeconds(3f);
        animator.enabled = false;
        Debug.Log(animator.enabled = false);
    }
}
