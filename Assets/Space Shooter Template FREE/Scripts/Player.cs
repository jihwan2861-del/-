using UnityEngine;
using System.Collections;

/// <summary>
/// 비행기의 체력과 무적 상태를 관리하는 스크립트입니다.
/// </summary>
public class Player : MonoBehaviour
{
    public GameObject destructionFX;
    
    [Header("Health Settings")]
    public int maxHealth = 10;
    public int health = 10;

    public static Player instance; 
    
    [Header("Invincibility Flags")]
    [HideInInspector] public bool isInvincible = false;      // 대쉬 등 일반 무적
    [HideInInspector] public bool safeZoneInvincible = false; // 세이프존 무적

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        if (instance == null) 
            instance = this;
        spriteRenderer = GetComponent<SpriteRenderer>();
        health = maxHealth;
    }

    public void GetDamage(int damage)   
    {
        // 대쉬 중이거나 세이프존 안에 있으면 무시함
        if (isInvincible || safeZoneInvincible) return; 
        
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

    IEnumerator DamageFlash()
    {
        if (spriteRenderer != null) spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        if (spriteRenderer != null && !isInvincible && !safeZoneInvincible) 
            spriteRenderer.color = Color.white;
    }

    public IEnumerator DashInvincibility(float duration)
    {
        isInvincible = true;
        if (spriteRenderer != null) spriteRenderer.color = Color.yellow;
        yield return new WaitForSeconds(duration);
        if (spriteRenderer != null && !safeZoneInvincible) 
            spriteRenderer.color = Color.white;
        isInvincible = false;
    }

    void Destruction()
    {
        if (destructionFX != null)
            Instantiate(destructionFX, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
