using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("체력바 (HP Bar)")]
    public Slider healthSlider;
    public Text hpText;
    public Image fillImage; // 체력바 색상 변경용
    public Color highHpColor = Color.green;
    public Color lowHpColor = Color.red;

    [Header("남은 아이템 (대쉬 횟수)")]
    public Text itemText;

    void Update()
    {
        // 1. 체력바 실시간 동기화
        if (Player.instance != null && healthSlider != null)
        {
            healthSlider.maxValue = Player.instance.maxHealth;
            healthSlider.value = Player.instance.health;

            if (hpText != null)
            {
                hpText.text = Player.instance.health + " / " + Player.instance.maxHealth;
            }

            // 체력이 30% 이하로 떨어지면 붉은색, 아니면 녹색으로 시각적 경고
            if (fillImage != null)
            {
                float hpPercent = (float)Player.instance.health / Player.instance.maxHealth;
                fillImage.color = Color.Lerp(lowHpColor, highHpColor, hpPercent);
            }
        }
        else if (Player.instance == null && healthSlider != null)
        {
            // 플레이어가 죽었을 때 표기
            healthSlider.value = 0;
            if (hpText != null) hpText.text = "DESTROYED";
            if (fillImage != null) fillImage.color = Color.gray;
        }

        // 2. 아이템(대쉬) 횟수 실시간 동기화
        if (PlayerMoving.instance != null && itemText != null)
        {
            itemText.text = "DASH: " + PlayerMoving.instance.currentDashCharges + " / " + PlayerMoving.instance.maxDashCharges;
        }
    }
}
