using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script defines which sprite the 'Player" uses and its health.
/// </summary>

public class Player : MonoBehaviour
{
    public GameObject destructionFX;
    [Tooltip("비행기의 체력 (0이 되면 파괴됨)")]
    public int health = 10;

    public static Player instance; 
    [HideInInspector] public bool isInvincible = false;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        if (instance == null) 
            instance = this;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    //method for damage proceccing by 'Player'
    public void GetDamage(int damage)   
    {
        if (isInvincible) return; // 대쉬 중 무적 처리
        
        health -= damage;
        if (health <= 0)
        {
            Destruction();
        }
        else
        {
            StartCoroutine(DamageFlash());
        }
    }    

    // 피격 시 빨간색으로 깜빡이는 효과 코루틴
    IEnumerator DamageFlash()
    {
        if (spriteRenderer != null) spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        // 대쉬 중(무적)이 아닐 때만 원래 색으로 복구 (대쉬의 노란색 덮어쓰기 방지)
        if (spriteRenderer != null && !isInvincible) spriteRenderer.color = Color.white;
    }

    // 일정 시간 무적 부여 코루틴
    public IEnumerator DashInvincibility(float duration)
    {
        isInvincible = true;
        if (spriteRenderer != null) spriteRenderer.color = Color.yellow;
        yield return new WaitForSeconds(duration);
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
        isInvincible = false;
    }

    //'Player's' destruction procedure
    void Destruction()
    {
        Instantiate(destructionFX, transform.position, Quaternion.identity); //generating destruction visual effect and destroying the 'Player' object
        Destroy(gameObject);
    }
}
















