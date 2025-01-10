using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TestEnemy : MonoBehaviour
{
    public Transform playerTransform;
    public float moveSpeed = 2f;  // 적의 이동 속도


    private void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform; // 플레이어의 현재위치받기
    }
    void Update()
    {
        // 플레이어를 향한 방향 계산
        Vector3 direction = (playerTransform.position - transform.position).normalized;

        // 적의 새로운 위치 계산
        Vector3 newPosition = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);

        // 적을 새로운 위치로 이동
        transform.position = newPosition;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "detectArea")
        {
            Debug.Log("탐지범위");
        }
        else if (collision.tag == "Player")
        {
            Debug.Log("Player와 닿음");
        }
    }

}
