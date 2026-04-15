using UnityEngine;

/// <summary>
/// 자동으로 사이버펑크 배경을 생성하고 무한 스크롤(Scrolling)해 주는 스크립트입니다.
/// 아무 빈 오브젝트(예: GameManager 등)에 이 스크립트를 붙이기만 하면 됩니다.
/// </summary>
public class CyberpunkBackgroundScroller : MonoBehaviour
{
    [Header("설정")]
    public float scrollSpeed = 3.0f; // 배경이 내려오는 속도
    public string resourcePath = "background"; // Resources 폴더 내 파일명

    private GameObject[] bgObjects = new GameObject[2]; // 무한 루프를 위한 2개의 배경
    private SpriteRenderer[] renderers = new SpriteRenderer[2];
    private float spriteHeight;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        Sprite bgSprite = Resources.Load<Sprite>(resourcePath);

        if (bgSprite == null)
        {
            Debug.LogError($"[BackgroundScroller] '{resourcePath}' 이미지를 Assets/Resources 폴더에서 찾을 수 없습니다!");
            return;
        }

        // 2개의 배경 오브젝트 생성 (서로 이어붙이기 위해)
        for (int i = 0; i < 2; i++)
        {
            bgObjects[i] = new GameObject("Cyberpunk_BG_" + i);
            bgObjects[i].transform.SetParent(this.transform);
            
            renderers[i] = bgObjects[i].AddComponent<SpriteRenderer>();
            renderers[i].sprite = bgSprite;
            renderers[i].sortingOrder = -100; // 가장 뒤에 배치

            // 화면 가로 크기에 맞춰 스케일 조절
            ScaleToFitScreen(bgObjects[i], bgSprite);
            
            spriteHeight = bgSprite.bounds.size.y * bgObjects[i].transform.localScale.y;
            
            // 초기 위치 설정 (첫 번째는 중앙, 두 번째는 바로 위에)
            bgObjects[i].transform.position = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y + (i * spriteHeight), 0);
        }
    }

    void Update()
    {
        for (int i = 0; i < 2; i++)
        {
            // 배경 아래로 이동
            bgObjects[i].transform.Translate(Vector3.down * scrollSpeed * Time.deltaTime);

            // 화면 아래로 완전히 벗어나면 다시 위로 보냄 (무한 루프)
            if (bgObjects[i].transform.position.y <= mainCam.transform.position.y - spriteHeight)
            {
                // 다른 배경의 위에 정확히 붙도록 재배치
                int otherIndex = (i == 0) ? 1 : 0;
                bgObjects[i].transform.position = new Vector3(mainCam.transform.position.x, bgObjects[otherIndex].transform.position.y + spriteHeight, 0);
            }
        }
    }

    void ScaleToFitScreen(GameObject obj, Sprite sprite)
    {
        float worldScreenHeight = mainCam.orthographicSize * 2.0f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        float spriteWidth = sprite.bounds.size.x;
        
        // 가로 길이를 화면에 맞춤
        float scale = worldScreenWidth / spriteWidth;
        obj.transform.localScale = new Vector3(scale, scale, 1);
    }
}
