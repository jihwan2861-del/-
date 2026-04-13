using UnityEngine;

public class BlinkEffect : MonoBehaviour
{
    [Tooltip("숫자가 클수록 경고가 더 빠르게 깜빡거립니다!")]
    public float blinkSpeed = 15f; 
    
    private SpriteRenderer sr;
    private Color originalColor;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            originalColor = sr.color;
        }
    }

    void Update()
    {
        if (sr != null)
        {
            Color c = originalColor;
            // 시간에 따라 투명도(Alpha)를 0 ~ 원본 알파값 사이로 빠르게 부드럽게 왕복시킵니다.
            c.a = originalColor.a * (0.5f + 0.5f * Mathf.Sin(Time.time * blinkSpeed));
            sr.color = c;
        }
    }
}
