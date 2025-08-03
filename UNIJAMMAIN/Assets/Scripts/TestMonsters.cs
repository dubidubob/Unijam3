using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TestMonsters : MonoBehaviour
{
    [SerializeField] Define.MonsterType monsterType = Define.MonsterType.Knockback;

    private float currentSpeed = 1f;
    private Vector3 playerPos = Vector3.zero;

    public void SetDead()
    {
        Poolable poolable = GetComponent<Poolable>();
        Managers.Pool.Push(poolable);
    }

    public void SetSpeed(float defaultSpeed)
    {
        currentSpeed = defaultSpeed;
    }

    //private void Update()
    //{
    //    Move();
    //}

    //private void Move()
    //{
    //    Vector3 newPosition = Vector3.MoveTowards(transform.position, playerPos, currentSpeed * Time.deltaTime);
    //    transform.position = newPosition;
    //}
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.tag == "detectArea")
    //    {
    //        Managers.Game.AddAttackableEnemy(enemyType.ToString(), this.gameObject);
    //    }
    //    else if (collision.tag == "Player")
    //    {
    //        Managers.Game.DecHealth();
    //        SetDead();
    //    }
    //}
}
