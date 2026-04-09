using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script moves the attached object along the Y-axis with the defined speed
/// </summary>
public class DirectMoving : MonoBehaviour {

    [Tooltip("Moving speed on Y axis in local space")]
    public float speed;

    [Header("Homing Settings")]
    [Tooltip("체크 시 플레이어를 따라가는 유도탄이 됩니다")]
    public bool isHoming = false;
    
    [Tooltip("유도 회전 속도 (클수록 더 예리하게 꺾임)")]
    public float homingRotSpeed = 120f;

    [Tooltip("유도 기능이 유지되는 시간(초)")]
    public float homingDuration = 2f;
    private float homingTimer = 0f;

    private void Start()
    {
        homingTimer = homingDuration;

        // 유도탄인데 기본 속도가 음수(뒤로 날아감)라면, 
        // 머리 방향을 180도 돌려주고 속도를 양수로 바꿔 위화감 없이 추적하게 만듭니다.
        if (isHoming && speed < 0)
        {
            speed = Mathf.Abs(speed);
            transform.Rotate(0, 0, 180f);
        }
    }

    //moving the object with the defined speed
    private void Update()
    {
        // 타이머가 0보다 클 때만 유도 기능 작동
        if (isHoming && Player.instance != null && homingTimer > 0f)
        {
            homingTimer -= Time.deltaTime; // 남은 시간 틱다운

            Vector2 direction = (Vector2)Player.instance.transform.position - (Vector2)transform.position;
            direction.Normalize();
            
            // 스프라이트는 머리(위쪽)를 향하고 있다고 가정 (-90 보정)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            
            // 서서히 목표 각도로 회전
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, homingRotSpeed * Time.deltaTime);
        }

        transform.Translate(Vector3.up * speed * Time.deltaTime); 
    }
}
