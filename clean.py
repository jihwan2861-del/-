import re

with open('Assets/Space Shooter Template FREE/Scripts/LevelController.cs', 'r', encoding='utf-8') as f:
    text = f.read()

# 1. GridStrikeConfig 클래스 제거
text = re.sub(
    r'\n\[System\.Serializable\]\npublic class GridStrikeConfig\n\{[^}]+\}\n',
    '\n',
    text
)

# 2. 16-Tile Grid Strike 변수 블록을 Endless Laser Patterns으로 교체
old_vars = '''    [Header("16-Tile Grid Strike System (장판 폭격기)")]
    [Tooltip("원하는 시간대에 원하는 타일 좌표를 적어 넣으세요.")]
    public GridStrikeConfig[] gridStrikes;
    [Tooltip("에디터 Tools에서 만든 경고 타일(GridStrike_Warning)을 넣으세요")]
    public GameObject gridWarningPrefab;
    [Tooltip("에디터 Tools에서 만든 폭발 레이저(GridStrike_Laser)를 넣으세요")]
    public GameObject gridLaserPrefab;
    [Tooltip("경고가 뜬 후 폭격까지 걸리는 시간(초)")]
    public float gridWarningDuration = 1.0f;

    [Header("Environmental Laser Pattern System (옵션)")]
    public bool enableLaserSpawning = false;
    [Tooltip("경고 마크 프리팹 (빈칸이면 경고 없이 즉시 발사)")]
    public GameObject environmentalWarningPrefab;
    [Tooltip("에디터에서 만든 레이저 프리팹을 여기에 넣어주세요")]
    public GameObject environmentalLaserPrefab;
    public float laserSpawnStartDelay = 6f;
    public float laserSpawnInterval = 4f;
    [Tooltip("경고 후 레이저가 떨어지기까지의 시간(초)")]
    public float environmentalWarningDuration = 1.0f;'''

new_vars = '''    [Header("Endless Laser Patterns (레이저 패턴 무한 랜덤)")]
    [Tooltip("패턴 메이커 툴로 구워낸 레이저 프리팹을 넣으세요. 랜덤으로 나옵니다!")]
    public GameObject[] laserPatternPrefabs;
    [Tooltip("패턴 랜덤 스폰 시작 대기 시간 (초)")]
    public float laserPatternStartDelay = 10f;
    [Tooltip("다음 패턴까지의 간격 (초)")]
    public float laserPatternInterval = 4f;'''

text = text.replace(old_vars, new_vars)

# 3. Start()에서 enableLaserSpawning + gridStrikes 블록을 교체
old_start = '''        if (enableLaserSpawning)
        {
            StartCoroutine(LaserSpawning());
        }



        // 장판 폭격(16칸) 처리
        if (gridStrikes != null)
        {
            foreach (var strike in gridStrikes)
            {
                StartCoroutine(ProcessSingleGridStrike(strike));
            }
        }'''

new_start = '''        if (laserPatternPrefabs != null && laserPatternPrefabs.Length > 0)
        {
            StartCoroutine(EndlessLaserPatternSpawning());
        }'''

text = text.replace(old_start, new_start)

# 4. ProcessSingleGridStrike 코루틴 전체 제거
text = re.sub(
    r'    // 16칸 타일 장판 폭격 코루틴\n    IEnumerator ProcessSingleGridStrike\(GridStrikeConfig config\).*?    \}\n',
    '',
    text,
    flags=re.DOTALL
)

# 5. LaserSpawning + SpawnSingleEnvironmentalLaser 코루틴 제거, EndlessLaserPatternSpawning으로 교체
old_laser = '''    IEnumerator LaserSpawning()
    {
        yield return new WaitForSeconds(laserSpawnStartDelay);
        
        while (true)
        {
            if (environmentalLaserPrefab != null)
            {
                float minX = mainCamera.ViewportToWorldPoint(new Vector2(0, 0)).x + 1f;
                float maxX = mainCamera.ViewportToWorldPoint(new Vector2(1, 1)).x - 1f;
                
                float targetX = Random.Range(minX, maxX);
                if (Player.instance != null && Random.value > 0.5f) 
                {
                    targetX = Player.instance.transform.position.x;
                }

                float centerY = (mainCamera.ViewportToWorldPoint(new Vector2(0, 0)).y + mainCamera.ViewportToWorldPoint(new Vector2(1, 1)).y) / 2f;
                Vector2 spawnPos = new Vector2(targetX, centerY);
                
                // 동시에 여러 레이저가 겹쳐서 진행될 수 있도록 별도 코루틴으로 분리 (간격 대기에 영향 안 주게)
                StartCoroutine(SpawnSingleEnvironmentalLaser(spawnPos));
            }
            
            yield return new WaitForSeconds(laserSpawnInterval);
        }
    }

    IEnumerator SpawnSingleEnvironmentalLaser(Vector2 spawnPos)
    {
        GameObject warning = null;

        // 경고 시스템이 셋팅되어 있다면 먼저 발동!
        if (environmentalWarningPrefab != null)
        {
            warning = Instantiate(environmentalWarningPrefab, spawnPos, Quaternion.Euler(0, 0, -90f));
            yield return new WaitForSeconds(environmentalWarningDuration);
            if (warning != null) Destroy(warning);
        }

        // 지연 시간 후 진짜 레이저 투하
        GameObject spawnedLaser = Instantiate(environmentalLaserPrefab, spawnPos, Quaternion.Euler(0, 0, -90f));
        
        DirectMoving mover = spawnedLaser.GetComponent<DirectMoving>();
        if (mover != null) Destroy(mover);
    }'''

new_laser = '''    // 패턴 메이커 툴로 만든 프리팹을 랜덤으로 계속 소환하는 코루틴
    IEnumerator EndlessLaserPatternSpawning()
    {
        yield return new WaitForSeconds(globalStartDelay + laserPatternStartDelay);
        
        while (true)
        {
            int rIndex = Random.Range(0, laserPatternPrefabs.Length);
            GameObject selectedPrefab = laserPatternPrefabs[rIndex];
            
            if (selectedPrefab != null)
            {
                Instantiate(selectedPrefab, Vector3.zero, Quaternion.identity);
            }
            
            yield return new WaitForSeconds(laserPatternInterval);
        }
    }'''

text = text.replace(old_laser, new_laser)

with open('Assets/Space Shooter Template FREE/Scripts/LevelController.cs', 'w', encoding='utf-8') as f:
    f.write(text)

print('Done!')
