using UnityEngine;

/// <summary>
/// 슬라임 플레이어의 Idle/Run 스프라이트 애니메이션을 코드로 직접 제어합니다.
/// PlayerMoving이 붙어있는 같은 오브젝트에 부착하세요.
/// </summary>
public class PlayerSpriteAnimator : MonoBehaviour
{
    [Header("Idle 애니메이션 프레임 (가만히 있을 때)")]
    public Sprite[] idleFrames;   // idle_frame1, idle_frame2, idle_frame3

    [Header("Run 애니메이션 프레임 (움직일 때)")]
    public Sprite[] runFrames;    // run_frame1, run_frame2, run_frame3

    [Header("애니메이션 속도")]
    [Tooltip("프레임 전환 간격 (초). 작을수록 빠르게 통통 튑니다")]
    public float frameInterval = 0.15f;

    private SpriteRenderer spriteRenderer;
    private float timer;
    private int currentFrame;
    private bool isMoving;

    // 프레임 재생 순서: 0→1→0→2 (기본→찌그러짐→기본→늘어남) 루프
    private int[] playOrder = { 0, 1, 0, 2 };
    private int playIndex;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 이동 입력 감지
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        isMoving = (h != 0 || v != 0);

        // 타이머 기반 프레임 전환
        timer += Time.deltaTime;
        if (timer >= frameInterval)
        {
            timer = 0f;
            playIndex = (playIndex + 1) % playOrder.Length;
            currentFrame = playOrder[playIndex];

            Sprite[] frames = isMoving ? runFrames : idleFrames;
            if (frames != null && frames.Length > currentFrame && frames[currentFrame] != null)
            {
                spriteRenderer.sprite = frames[currentFrame];
            }
        }
    }
}
