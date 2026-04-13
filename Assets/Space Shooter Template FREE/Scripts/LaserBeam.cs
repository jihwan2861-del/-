using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 지속적으로 닿아있을 때 플레이어에게 데미지를 주는 레이저 스크립트입니다.
/// </summary>
public class LaserBeam : MonoBehaviour
{
    [Tooltip("레이저가 주는 데미지")]
    public int damage = 1;

    [Tooltip("레이저가 발사되어 유지되는 시간")]
    public float lifeTime = 2f;

    [Tooltip("몇 초 간격으로 데미지가 계속 들어갈지 결정")]
    public float damageTickRate = 0.5f;

    private float nextDamageTime;

    private void Start()
    {
        // lifeTime 이후에 자동으로 파괴됩니다.
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // OnTriggerStay2D는 콜라이더가 겹쳐있는 동안 계속 호출됩니다.
        // 플레이어이고, 틱 시간이 지났을 때만 데미지를 줍니다.
        if (collision.CompareTag("Player") && Time.time >= nextDamageTime)
        {
            Player.instance.GetDamage(damage);
            nextDamageTime = Time.time + damageTickRate;
        }
    }
}
