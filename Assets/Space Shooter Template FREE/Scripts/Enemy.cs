using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script defines 'Enemy's' health and behavior. 
/// </summary>
public class Enemy : MonoBehaviour {

    #region FIELDS
    [Tooltip("Health points in integer")]
    public int health;

    [Tooltip("Enemy's projectile prefab")]
    public GameObject Projectile;

    [Tooltip("VFX prefab generating after destruction")]
    public GameObject destructionVFX;
    public GameObject hitEffect;
    
    [HideInInspector] public int shotChance; //probability of 'Enemy's' shooting during tha path
    [HideInInspector] public float shotTimeMin, shotTimeMax; //max and min time for shooting from the beginning of the path
    
    [Header("Difficulty Tweaks")]
    [Tooltip("총알이 유도탄일 경우, 발사 확률을 몇 배로 줄일지(나눌지) 결정합니다. (3이면 확률 1/3로 감소)")]
    public float homingMissileNerf = 3f;
    [Header("Visual Override (그림 덮어씌우기)")]
    [Tooltip("에디터에서 만든 애니메이션 프리팹을 여기에 넣으면, 기존 우주선 그림을 지우고 이 그림으로 교체됩니다.")]
    public GameObject overrideVisualPrefab;
    #endregion

    private void Start()
    {
        // 사용자가 새 애니메이션 프리팹을 드래그해서 넣었다면?
        if (overrideVisualPrefab != null)
        {
            // 원래 있던 낡은 이미지는 안 보이게 투명하게 만듭니다.
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;

            // 방금 넣은 새 애니메이션 프리팹을 내 자식(껍데기)으로 스폰시킵니다!
            Instantiate(overrideVisualPrefab, transform.position, transform.rotation, transform);
        }

        Invoke("ActivateShooting", Random.Range(shotTimeMin, shotTimeMax));
    }

    //coroutine making a shot
    void ActivateShooting() 
    {
        float currentChance = shotChance;
        
        // 장착된 총알이 유도탄일 경우 발사 확률을 대폭 깎아서 게임오버를 방지합니다.
        if (Projectile != null)
        {
            var moveScript = Projectile.GetComponent<DirectMoving>();
            if (moveScript != null && moveScript.isHoming)
            {
                currentChance = currentChance / homingMissileNerf;
            }
        }

        if (Random.value < currentChance / 100f)                             //if random value less than shot probability, making a shot
        {                         
            GameObject bullet = Instantiate(Projectile, gameObject.transform.position, Quaternion.identity); 

            // 발사한 것이 '레이저' 라면 (LaserBeam 스크립트를 갖고 있다면)
            if (bullet.GetComponent<LaserBeam>() != null)
            {
                // 레이저가 적 오브젝트를 따라다니도록 자식(Child)으로 설정합니다.
                bullet.transform.SetParent(gameObject.transform);
                
                // 레이저의 위치를 적 우주선의 조금 아래쪽에 맞춥니다.
                bullet.transform.localPosition = new Vector3(0, -3f, 0); 
            }             
        }
    }

    //method of getting damage for the 'Enemy'
    public void GetDamage(int damage) 
    {
        health -= damage;           //reducing health for damage value, if health is less than 0, starting destruction procedure
        if (health <= 0)
            Destruction();
        else
            Instantiate(hitEffect,transform.position,Quaternion.identity,transform);
    }    

    //if 'Enemy' collides 'Player', 'Player' gets the damage equal to projectile's damage value
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (Projectile.GetComponent<Projectile>() != null)
                Player.instance.GetDamage(Projectile.GetComponent<Projectile>().damage);
            else
                Player.instance.GetDamage(1);
        }
    }

    //method of destroying the 'Enemy'
    void Destruction()                           
    {        
        Instantiate(destructionVFX, transform.position, Quaternion.identity); 
        Destroy(gameObject);
    }
}
