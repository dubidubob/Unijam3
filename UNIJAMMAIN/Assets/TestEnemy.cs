using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TestEnemy : MonoBehaviour
{
    public Transform playerTransform;
    public float moveSpeed = 2f;  // ���� �̵� �ӵ�


    private void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform; // �÷��̾��� ������ġ�ޱ�
    }
    void Update()
    {
        // �÷��̾ ���� ���� ���
        Vector3 direction = (playerTransform.position - transform.position).normalized;

        // ���� ���ο� ��ġ ���
        Vector3 newPosition = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);

        // ���� ���ο� ��ġ�� �̵�
        transform.position = newPosition;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "detectArea")
        {
            Debug.Log("Ž������");
        }
        else if (collision.tag == "Player")
        {
            Debug.Log("Player�� ����");
        }
    }

}
