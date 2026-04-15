using UnityEngine;

/// <summary>
/// 플레이어와 접촉하면 활성화되는 버튼 스크립트입니다.
/// </summary>
public class InteractionPoint : MonoBehaviour
{
    public LevelController controller;
    private bool isActivated = false;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
            
            // Resources에서 'button' 가오름
            Sprite loadedSprite = Resources.Load<Sprite>("button");
            if (loadedSprite != null)
            {
                sr.sprite = loadedSprite;
            }
            
            sr.color = Color.white; // 이미지가 있으므로 흰색(기본)으로 설정
        }

        // 인스펙터에서 이미지를 수동으로 넣을 수도 있으므로, 이미지가 없다면 빨간색으로 경고
        if (sr.sprite == null) sr.color = Color.red;

        // 콜라이더가 없으면 추가
        if (GetComponent<Collider2D>() == null)
        {
            var col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
        }

        // 총알을 막지 않도록 'Ignore Raycast' 레이어 설정 (필요시)
        gameObject.layer = 2; 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isActivated)
        {
            Activate();
        }
    }

    void Activate()
    {
        isActivated = true;
        sr.color = Color.yellow; // 활성화되면 노란색으로 변경
        
        // 크기 연출 (유니티 인스펙터 설정을 존중하기 위해 강제 배수 제거 가능)

        if (controller != null)
        {
            controller.OnButtonActivated();
        }
        
        // 사운드나 이펙트가 있다면 여기서 재생
        Debug.Log("[Interaction] 버튼 활성화!");
    }
}
