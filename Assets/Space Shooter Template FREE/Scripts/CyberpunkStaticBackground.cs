using UnityEngine;

/// <summary>
/// 사이버펑크 이미지를 화면 전체 배경으로 고정시켜 주는 스크립트입니다.
/// 아무 빈 오브젝트에 이 스크립트를 추가하면 됩니다.
/// </summary>
public class CyberpunkStaticBackground : MonoBehaviour
{
    public string resourcePath = "background";

    void Start()
    {
        Camera mainCam = Camera.main;
        Sprite bgSprite = Resources.Load<Sprite>(resourcePath);

        if (bgSprite == null)
        {
            Debug.LogError($"[StaticBackground] '{resourcePath}' 이미지를 Assets/Resources 폴더에서 찾을 수 없습니다!");
            return;
        }

        // 배경 오브젝트 생성
        GameObject bg = new GameObject("Static_Background");
        bg.transform.SetParent(this.transform);

        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = bgSprite;
        sr.sortingOrder = -100; // 가장 뒤로 보내기

        // 화면 크기에 맞게 스케일 조절
        float worldScreenHeight = mainCam.orthographicSize * 2.0f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;
        
        float spriteWidth = bgSprite.bounds.size.x;
        float spriteHeight = bgSprite.bounds.size.y;

        // 화면을 꽉 채우도록 스케일 계산 (가로/세로 비율 유지)
        float scaleX = worldScreenWidth / spriteWidth;
        float scaleY = worldScreenHeight / spriteHeight;
        float finalScale = Mathf.Max(scaleX, scaleY);
        
        bg.transform.localScale = new Vector3(finalScale, finalScale, 1);
        
        // 카메라 중앙에 배치
        bg.transform.position = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, 0);
    }
}
