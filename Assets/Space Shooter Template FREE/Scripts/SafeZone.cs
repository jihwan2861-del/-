using UnityEngine;

/// <summary>
/// 모든 버튼이 활성화되면 나타나는 세이프존 스크립트입니다. 
/// 플레이어가 이 안에 있으면 무적이 되며, 일정 시간 버티면 승리합니다.
/// </summary>
public class SafeZone : MonoBehaviour
{
    public float winDelay = 10.0f; // 세이프존에서 버텨야 하는 시간 (10초로 변경)
    private float timer = 0f;
    private bool isPlayerInside = false;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
            
            // Resources에서 'safezone' 가오름
            Sprite loadedSprite = Resources.Load<Sprite>("safezone");
            if (loadedSprite != null)
            {
                sr.sprite = loadedSprite;
            }
            sr.color = new Color(1, 1, 1, 0.5f); // 이미지가 있으므로 적절한 투명도 설정
            sr.sortingOrder = 100; // 가장 앞에 보이도록 설정 (적/플레이어보다 높게)
        }

        // 기존에 만들어둔 BlinkEffect 컴포넌트 추가
        BlinkEffect blink = gameObject.AddComponent<BlinkEffect>();
        blink.blinkSpeed = 10f; // 깜빡임 속도 조절

        // 총알을 막지 않도록 'Ignore Raycast' 레이어 설정
        gameObject.layer = 2;

        // 콜라이더가 없으면 원형 콜라이더 추가 (기본 세팅만 수행)
        if (GetComponent<Collider2D>() == null)
        {
            var col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = true;
            if (Player.instance != null) Player.instance.safeZoneInvincible = true; // 무적 활성화
            Debug.Log("[SafeZone] 플레이어 보호 시작! 무적 상태입니다.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (Player.instance != null) Player.instance.safeZoneInvincible = false; // 무적 해제
            timer = 0f;
            Debug.Log("[SafeZone] 플레이어 보호 종료! 공격에 취약합니다.");
        }
    }

    private void Update()
    {
        if (isPlayerInside)
        {
            timer += Time.deltaTime;
            if (timer >= winDelay)
            {
                TriggerVictory();
            }
        }
    }

    void TriggerVictory()
    {
        isPlayerInside = false; // 중복 호출 방지
        Debug.Log("🏆 [VICTORY] 스테이지 클리어!");
        
        if (PlayerUI.instance != null)
        {
            PlayerUI.instance.ShowVictory();
        }

        // 게임을 완전히 멈춥니다.
        Time.timeScale = 0f; 
    }
}


