using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script defines the borders of ‘Player’s’ movement. Depending on the chosen handling type, it moves the ‘Player’ together with the pointer.
/// </summary>

[System.Serializable]
public class Borders
{
    [Tooltip("offset from viewport borders for player's movement")]
    public float minXOffset = 1.5f, maxXOffset = 1.5f, minYOffset = 1.5f, maxYOffset = 1.5f;
    [HideInInspector] public float minX, maxX, minY, maxY;
}

public class PlayerMoving : MonoBehaviour {

    [Tooltip("offset from viewport borders for player's movement")]
    public Borders borders;
    Camera mainCamera;
    bool controlIsActive = true; 
    [Header("Movement Settings")]
    public float baseSpeed = 15f;

    [Header("Dash Settings")]
    public float dashSpeedMultiplier = 3f;
    public float dashInvincibilityDuration = 3f;
    [HideInInspector] public bool isDashing = false;

    [Header("Dash Charges")]
    public int maxDashCharges = 5;
    public int currentDashCharges = 5;

    [Header("UI Objects (직접 연결해주세요)")]
    public GameObject dashTextObj;
    public GameObject warningTextObj;

    public static PlayerMoving instance; //unique instance of the script for easy access to the script

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        ResizeBorders();                //setting 'Player's' moving borders deending on Viewport's size
        
        // --- [UI 자동 복구 코드] ---
        // 텍스트가 안 보이는 현상(화면 밖 이탈, 캔버스 에러 등)을 
        // 게임 시작 시 코드가 강제로 고쳐버립니다!
        if (dashTextObj != null)
        {
            Canvas canvas = dashTextObj.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                // 무조건 화면 맨 앞(오버레이)에 붙임
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
                if (scaler != null)
                {
                    // 화면 비율에 따라 UI를 줄이고 늘리도록 강제 세팅 (해상도 밖으로 날아감 방지)
                    scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(640, 920); // 캡처본 기준 사이즈
                    scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
                }
            }
            
            // 혹시 Z값이 안드로메다로 가있을 경우를 대비해 위치 원상복구
            RectTransform rt = dashTextObj.GetComponent<RectTransform>();
            if (rt != null) rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0f);
        }
        // ---------------------------

        UpdateDashUI();
        if (warningTextObj != null) 
            warningTextObj.SetActive(false);
    }

    private void Update()
    {
        if (controlIsActive)
        {
            // 스페이스바 또는 우클릭 시 대쉬(무적) 발동
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(1))
            {
                if (currentDashCharges > 0 && !isDashing)
                {
                    PerformDash();
                }
                else if (currentDashCharges <= 0 && warningTextObj != null && !warningTextObj.activeSelf)
                {
                    StartCoroutine(ShowWarningUI());
                }
            }

            // 키보드 WASD 또는 방향키 입력 받기
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 moveDirection = new Vector3(horizontal, vertical, 0).normalized;

            // --- [애니메이션 처리] ---
            Animator anim = GetComponent<Animator>();
            if (anim != null)
            {
                // 현재 이동 중인지 확인
                bool isMoving = moveDirection != Vector3.zero;
                anim.SetBool("isMoving", isMoving);

                if (isMoving)
                {
                    // 이동 중일 때만 방향 파라미터 업데이트 (가만히 있을 때 이전 방향을 바라보게 하기 위함)
                    anim.SetFloat("InputX", horizontal);
                    anim.SetFloat("InputY", vertical);
                }
            }
            // -------------------------

            // 마우스 추적 시절의 30f는 키보드 조작에는 너무 빠를 수 있어 기본 baseSpeed(15f)를 사용
            float currentSpeed = isDashing ? baseSpeed * dashSpeedMultiplier : baseSpeed;

            if (moveDirection != Vector3.zero)
            {
                // 1. 해당 막대 방향으로 이동
                transform.position += moveDirection * currentSpeed * Time.deltaTime;

                // 2. [8방향 애니메이션 사용을 위해 물리적 회전 비활성화]
                // 8방향 스프라이트 이미지가 방향을 나타내므로, 오브젝트 자체를 회전시키면 이미지가 이상하게 돌아갑니다.
                // float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg - 90f;
                // Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
                // transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 1000f * Time.deltaTime);
            }
            transform.position = new Vector3    //if 'Player' crossed the movement borders, returning him back 
                (
                Mathf.Clamp(transform.position.x, borders.minX, borders.maxX),
                Mathf.Clamp(transform.position.y, borders.minY, borders.maxY),
                0
                );
        }
    }

    //setting 'Player's' movement borders according to Viewport size and defined offset
    void ResizeBorders() 
    {
        borders.minX = mainCamera.ViewportToWorldPoint(Vector2.zero).x + borders.minXOffset;
        borders.minY = mainCamera.ViewportToWorldPoint(Vector2.zero).y + borders.minYOffset;
        borders.maxX = mainCamera.ViewportToWorldPoint(Vector2.right).x - borders.maxXOffset;
        borders.maxY = mainCamera.ViewportToWorldPoint(Vector2.up).y - borders.maxYOffset;
    }

    void PerformDash()
    {
        currentDashCharges--;
        UpdateDashUI();
        StartCoroutine(DashRoutine());
        if (Player.instance != null)
        {
            StartCoroutine(Player.instance.DashInvincibility(dashInvincibilityDuration));
        }
    }

    public void AddDashCharge()
    {
        if (currentDashCharges < maxDashCharges)
        {
            currentDashCharges++;
            UpdateDashUI();
        }
    }

    void UpdateDashUI()
    {
        SetTextIfPossible(dashTextObj, "Dash: " + currentDashCharges + " / " + maxDashCharges);
    }

    IEnumerator ShowWarningUI()
    {
        if (warningTextObj != null)
        {
            SetTextIfPossible(warningTextObj, "NO ITEMS!");
            warningTextObj.SetActive(true);
            yield return new WaitForSeconds(1f);
            warningTextObj.SetActive(false);
        }
    }

    void SetTextIfPossible(GameObject obj, string textValue)
    {
        if (obj == null) return;
        
        // 1. Legacy Text 지원
        var legacyText = obj.GetComponent<UnityEngine.UI.Text>();
        if (legacyText != null) 
        {
            legacyText.text = textValue;
            return;
        }
        
        // 2. TextMeshPro 등 모든 Text 지원 (리플렉션 사용)
        Component[] components = obj.GetComponents<Component>();
        foreach(var comp in components)
        {
            if (comp == null) continue;
            if (comp.GetType().Name.Contains("Text"))
            {
                var propInfo = comp.GetType().GetProperty("text");
                if (propInfo != null && propInfo.CanWrite)
                {
                    propInfo.SetValue(comp, textValue, null);
                    return;
                }
            }
        }
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        yield return new WaitForSeconds(dashInvincibilityDuration);
        isDashing = false;
    }
}
