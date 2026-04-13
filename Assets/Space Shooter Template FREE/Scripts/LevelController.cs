using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Serializable classes
[System.Serializable]
public class EnemyWaves 
{
    [Tooltip("time for wave generation from the moment the game started")]
    public float timeToStart;

    [Tooltip("Enemy wave's prefab")]
    public GameObject wave;
}

[System.Serializable]
public class LaserSpawnConfig
{
    [Tooltip("이 레이저 비행선이 등장할 시간(초)")]
    public float timeToStart;
    
    [Tooltip("가로(X) 스폰 위치 (0이면 정중앙, 마이너스는 왼쪽, 플러스는 오른쪽)")]
    public float spawnXPosition = 0f;

    [Tooltip("스폰될 레이저 전함(Enemy_LaserShip) 프리팹")]
    public GameObject laserShipPrefab;
}

[System.Serializable]
public class GridStrikeConfig
{
    [Tooltip("이 장판 폭격 패턴이 등장할 시간(초)")]
    public float timeToStart;

    [Tooltip("X: 타일번호(1~16), Y: 각도(-90=아래, 0=오른쪽, 45=대각선 등)")]
    public Vector2[] targetTilesAndAngles;
}
#endregion

public class LevelController : MonoBehaviour {

    //Serializable classes implements
    public EnemyWaves[] enemyWaves; 

    public GameObject powerUp;
    public float timeForNewPowerup;
    public GameObject[] planets;
    public float timeBetweenPlanets;
    public float planetsSpeed;
    List<GameObject> planetsList = new List<GameObject>();

    Camera mainCamera;   

    [Header("🎮 Global Game Settings (전체 난이도 관리)")]
    [Tooltip("모든 적 패턴(장판 폭격 포함)이 시작되기 전, 맨 처음 주어지는 준비 시간(초)입니다! 여기서 조절하세요.")]
    public float globalStartDelay = 5f; 

    [Header("Evasion Spawning System")]
    public bool enableRandomSpawning = true;
    public bool disableOriginalWaves = false; // 기본 웨이브 시스템 끄기 (요청에 따라 기본 웨이브 켬)
    public bool spawnOriginalWavesFromBottom = true; // 기본 웨이브를 뒤집어서 뒤에서도 생성할지 여부
    public GameObject[] customEnemyPrefabs;
    public float spawnInterval = 1.5f;
    public float randomEnemySpeed = 10f;

    [Header("Wall Spawning System")]
    public bool enableWallSpawning = true;
    public float wallSpawnStartDelay = 5f;
    public float wallSpawnInterval = 3f;
    public int wallObstacleCount = 7;
    public float wallObstacleSpeed = 8f;
    public float wallObstacleSpacing = 2.5f; // 장애물 사이의 간격

    [Header("Laser Ship Waves (위치 고정 시스템)")]
    public LaserSpawnConfig[] laserWaves;

    [Header("16-Tile Grid Strike System (장판 폭격기)")]
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
    public float environmentalWarningDuration = 1.0f;


    private void Awake()
    {
        // PC 빌드(exe 파일) 실행 시 해상도를 강제로 720 x 960 세로형 창모드로 고정합니다.
#if UNITY_STANDALONE
        Screen.SetResolution(720, 960, FullScreenMode.Windowed);
#endif
    }

    private void Start()
    {
        mainCamera = Camera.main;
        if (!disableOriginalWaves)
        {
            //for each element in 'enemyWaves' array creating coroutine which generates the wave
            for (int i = 0; i<enemyWaves.Length; i++) 
            {
                float finalDelay = globalStartDelay + enemyWaves[i].timeToStart;
                StartCoroutine(CreateEnemyWave(finalDelay, enemyWaves[i].wave, false));
                if (spawnOriginalWavesFromBottom)
                {
                    // 뒤에서도 정확히 거울처럼 대칭으로 나오게 함
                    StartCoroutine(CreateEnemyWave(finalDelay, enemyWaves[i].wave, true));
                }
            }
        }
        StartCoroutine(PowerupBonusCreation());
        StartCoroutine(PlanetsCreation());

        if (enableRandomSpawning)
        {
            StartCoroutine(RandomEnemySpawning());
        }

        if (enableWallSpawning)
        {
            StartCoroutine(WallSpawning());
        }

        if (enableLaserSpawning)
        {
            StartCoroutine(LaserSpawning());
        }

        // 레이저 웨이브 처리
        if (laserWaves != null)
        {
            foreach (var laserWave in laserWaves)
            {
                StartCoroutine(ProcessSingleLaserWave(laserWave));
            }
        }

        // 장판 폭격(16칸) 처리
        if (gridStrikes != null)
        {
            foreach (var strike in gridStrikes)
            {
                StartCoroutine(ProcessSingleGridStrike(strike));
            }
        }
    }
    
    // 16칸 타일 장판 폭격 코루틴
    IEnumerator ProcessSingleGridStrike(GridStrikeConfig config)
    {
        Debug.Log($"[그리드 폭격] {config.timeToStart}초 대기 시작...");
        yield return new WaitForSeconds(globalStartDelay + config.timeToStart);

        if (gridLaserPrefab == null) 
        {
            Debug.LogError("🚨 장판 폭격 실패! LevelController의 Grid Laser Prefab 칸이 비어있습니다. 레이저를 넣어주세요.");
            yield break;
        }

        if (config.targetTilesAndAngles == null || config.targetTilesAndAngles.Length == 0) 
        {
            Debug.LogWarning("🚨 장판 폭격 취소: 타겟 타일 번호가 정해지지 않았습니다.");
            yield break;
        }

        float minX = mainCamera.ViewportToWorldPoint(new Vector2(0, 0)).x;
        float maxX = mainCamera.ViewportToWorldPoint(new Vector2(1, 1)).x;
        float minY = mainCamera.ViewportToWorldPoint(new Vector2(0, 0)).y;
        float maxY = mainCamera.ViewportToWorldPoint(new Vector2(1, 1)).y;

        float cellWidth = (maxX - minX) / 4f;
        float cellHeight = (maxY - minY) / 4f;

        List<GameObject> activeWarnings = new List<GameObject>();

        // 경고 프리팹이 등록되어 있다면 (원래의 2단계 방식)
        if (gridWarningPrefab != null)
        {
            foreach (Vector2 tileData in config.targetTilesAndAngles)
            {
                int tileNumber = (int)tileData.x;
                float laserAngle = tileData.y;
                
                int index = Mathf.Clamp(tileNumber - 1, 0, 15);
                int row = index / 4; int col = index % 4;
                float xPos = minX + (col * cellWidth) + (cellWidth / 2f);
                float yPos = maxY - (row * cellHeight) - (cellHeight / 2f);

                // 경고 장판도 레이저가 나갈 궤적(각도)과 동일하게 회전시켜서 발사합니다!
                // 1칸 크기에 네모낳게 찌그러뜨리던 코드를 제거하여, 유저의 프리팹 원형(길쭉한 궤도 등)을 보존합니다.
                GameObject warning = Instantiate(gridWarningPrefab, new Vector3(xPos, yPos, 0), Quaternion.Euler(0, 0, laserAngle));
                activeWarnings.Add(warning);
            }
            yield return new WaitForSeconds(gridWarningDuration);
        }

        // 실제 레이저 폭격 발사 (경고가 없으면 대기 없이 즉시 발사됨)
        foreach (Vector2 tileData in config.targetTilesAndAngles)
        {
            int tileNumber = (int)tileData.x;
            float laserAngle = tileData.y;

            int index = Mathf.Clamp(tileNumber - 1, 0, 15);
            int row = index / 4; int col = index % 4;
            float xPos = minX + (col * cellWidth) + (cellWidth / 2f);
            float yPos = maxY - (row * cellHeight) - (cellHeight / 2f);

            // 기획자가 설정한 Z축 방향(레이저 각도)를 적용하여 위아래/좌우 혼합 십자 포화 등을 구현!
            Instantiate(gridLaserPrefab, new Vector3(xPos, yPos, 0), Quaternion.Euler(0, 0, laserAngle));
        }

        // 생성되었던 경고판 파괴
        foreach (GameObject w in activeWarnings)
        {
            if (w != null) Destroy(w);
        }
    }


    // 개별 레이저 비행선 웨이브 스폰
    IEnumerator ProcessSingleLaserWave(LaserSpawnConfig config)
    {
        // 글로벌 딜레이 + 개별 딜레이만큼 대기
        yield return new WaitForSeconds(globalStartDelay + config.timeToStart);

        if (config.laserShipPrefab != null && Player.instance != null)
        {
            // 화면 밖 맨 위(Top) Y 좌표 계산
            float maxY = mainCamera.ViewportToWorldPoint(new Vector2(1, 1)).y + 2f;
            Vector2 spawnPos = new Vector2(config.spawnXPosition, maxY);
            
            // 프리팹이 갖고 있는 고유 회전값 그대로 스폰 (픽셀아트 때문에 이미 -90도가 프리팹에 들어있음)
            Instantiate(config.laserShipPrefab, spawnPos, config.laserShipPrefab.transform.rotation);
        }
    }
    
    //Create a new wave after a delay
    IEnumerator CreateEnemyWave(float delay, GameObject Wave, bool inverted) 
    {
        if (delay != 0)
            yield return new WaitForSeconds(delay);
        if (Player.instance != null)
        {
            GameObject waveInstance = Instantiate(Wave);
            if (inverted)
            {
                // 웨이브 오브젝트를 180도 뒤집어서 위->아래 경로를 아래->위 경로로 거울 반전시킴
                waveInstance.transform.rotation = Quaternion.Euler(0, 0, 180f);
            }
        }
    }

    //endless coroutine generating 'levelUp' bonuses. 
    IEnumerator PowerupBonusCreation() 
    {
        while (true) 
        {
            yield return new WaitForSeconds(timeForNewPowerup);
            Instantiate(
                powerUp,
                //Set the position for the new bonus: for X-axis - random position between the borders of 'Player's' movement; for Y-axis - right above the upper screen border 
                new Vector2(
                    Random.Range(PlayerMoving.instance.borders.minX, PlayerMoving.instance.borders.maxX), 
                    mainCamera.ViewportToWorldPoint(Vector2.up).y + powerUp.GetComponent<Renderer>().bounds.size.y / 2), 
                Quaternion.identity
                );
        }
    }

    IEnumerator PlanetsCreation()
    {
        //Create a new list copying the arrey
        for (int i = 0; i < planets.Length; i++)
        {
            planetsList.Add(planets[i]);
        }
        yield return new WaitForSeconds(10);
        while (true)
        {
            ////choose random object from the list, generate and delete it
            int randomIndex = Random.Range(0, planetsList.Count);
            GameObject newPlanet = Instantiate(planetsList[randomIndex]);
            planetsList.RemoveAt(randomIndex);
            //if the list decreased to zero, reinstall it
            if (planetsList.Count == 0)
            {
                for (int i = 0; i < planets.Length; i++)
                {
                    planetsList.Add(planets[i]);
                }
            }
            newPlanet.GetComponent<DirectMoving>().speed = planetsSpeed;

            yield return new WaitForSeconds(timeBetweenPlanets);
        }
    }

    IEnumerator RandomEnemySpawning()
    {
        yield return new WaitForSeconds(globalStartDelay); // 시작 딜레이 통합
        
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            
            GameObject prefabToSpawn = null;
            if (customEnemyPrefabs != null && customEnemyPrefabs.Length > 0)
            {
                prefabToSpawn = customEnemyPrefabs[Random.Range(0, customEnemyPrefabs.Length)];
            }
            else if (enemyWaves != null && enemyWaves.Length > 0 && enemyWaves[0].wave != null)
            {
                // 프리팹 지정을 까먹었다면, 기존 웨이브 설정에서 첫번째 적을 몰래 훔쳐옵니다 (Fallback)
                var waveComp = enemyWaves[0].wave.GetComponent<Wave>();
                if (waveComp != null) prefabToSpawn = waveComp.enemy;
            }

            if (prefabToSpawn == null) continue;

            int edge = Random.Range(0, 4);
            Vector2 spawnPos = Vector2.zero;
            float rotZ = 0;

            // 카메라 경계를 바탕으로 화면 밖 위치 계산
            float minX = mainCamera.ViewportToWorldPoint(new Vector2(0, 0)).x - 2f;
            float maxX = mainCamera.ViewportToWorldPoint(new Vector2(1, 1)).x + 2f;
            float minY = mainCamera.ViewportToWorldPoint(new Vector2(0, 0)).y - 2f;
            float maxY = mainCamera.ViewportToWorldPoint(new Vector2(1, 1)).y + 2f;

            switch(edge)
            {
                case 0: // Top (아래로 비행)
                    spawnPos = new Vector2(Random.Range(minX, maxX), maxY);
                    rotZ = 180f;
                    break;
                case 1: // Bottom (위로 비행)
                    spawnPos = new Vector2(Random.Range(minX, maxX), minY);
                    rotZ = 0f;
                    break;
                case 2: // Left (오른쪽으로 비행)
                    spawnPos = new Vector2(minX, Random.Range(minY, maxY));
                    rotZ = -90f;
                    break;
                case 3: // Right (왼쪽으로 비행)
                    spawnPos = new Vector2(maxX, Random.Range(minY, maxY));
                    rotZ = 90f;
                    break;
            }

            GameObject newEnemy = Instantiate(prefabToSpawn, spawnPos, Quaternion.Euler(0, 0, rotZ));
            
            // 기존 길따라가기 스크립트 제거 (직진만 하도록)
            var follow = newEnemy.GetComponent<FollowThePath>();
            if (follow != null) Destroy(follow);
            
            var directMove = newEnemy.GetComponent<DirectMoving>();
            if (directMove == null) directMove = newEnemy.AddComponent<DirectMoving>();
            directMove.speed = randomEnemySpeed;
        }
    }

    IEnumerator WallSpawning()
    {
        // 처음 5초 딜레이 (globalStartDelay 적용)
        yield return new WaitForSeconds(globalStartDelay);
        
        while (true)
        {
            GameObject prefabToSpawn = null;
            if (customEnemyPrefabs != null && customEnemyPrefabs.Length > 0)
            {
                prefabToSpawn = customEnemyPrefabs[Random.Range(0, customEnemyPrefabs.Length)];
            }
            else if (enemyWaves != null && enemyWaves.Length > 0 && enemyWaves[0].wave != null)
            {
                var waveComp = enemyWaves[0].wave.GetComponent<Wave>();
                if (waveComp != null) prefabToSpawn = waveComp.enemy;
            }

            if (prefabToSpawn != null)
            {
                int edge = Random.Range(0, 4);
                float rotZ = 0;

                // 카메라 경계 계산
                float minX = mainCamera.ViewportToWorldPoint(new Vector2(0, 0)).x - 1f;
                float maxX = mainCamera.ViewportToWorldPoint(new Vector2(1, 1)).x + 1f;
                float minY = mainCamera.ViewportToWorldPoint(new Vector2(0, 0)).y - 1f;
                float maxY = mainCamera.ViewportToWorldPoint(new Vector2(1, 1)).y + 1f;

                float centerX = (minX + maxX) / 2f;
                float centerY = (minY + maxY) / 2f;

                for (int i = 0; i < wallObstacleCount; i++)
                {
                    Vector2 spawnPos = Vector2.zero;

                    // 화면 중앙을 기준으로 좌우/상하로 퍼지도록 오프셋 계산
                    float totalLength = (wallObstacleCount - 1) * wallObstacleSpacing;
                    float offset = (-totalLength / 2f) + (i * wallObstacleSpacing);

                    switch(edge)
                    {
                        case 0: // Top (가로로 일렬 배열, 아래로 비행)
                            spawnPos = new Vector2(centerX + offset, maxY + 2f);
                            rotZ = 180f;
                            break;
                        case 1: // Bottom (가로로 일렬 배열, 위로 비행)
                            spawnPos = new Vector2(centerX + offset, minY - 2f);
                            rotZ = 0f;
                            break;
                        case 2: // Left (세로로 일렬 배열, 오른쪽 비행)
                            spawnPos = new Vector2(minX - 2f, centerY + offset);
                            rotZ = -90f;
                            break;
                        case 3: // Right (세로로 일렬 배열, 왼쪽 비행)
                            spawnPos = new Vector2(maxX + 2f, centerY + offset);
                            rotZ = 90f;
                            break;
                    }

                    GameObject newEnemy = Instantiate(prefabToSpawn, spawnPos, Quaternion.Euler(0, 0, rotZ));
                    
                    var follow = newEnemy.GetComponent<FollowThePath>();
                    if (follow != null) Destroy(follow);
                    
                    var directMove = newEnemy.GetComponent<DirectMoving>();
                    if (directMove == null) directMove = newEnemy.AddComponent<DirectMoving>();
                    directMove.speed = wallObstacleSpeed;
                }
            }

            // 첫 스폰 이후로는 3초마다 반복
            yield return new WaitForSeconds(wallSpawnInterval);
        }
    }

    IEnumerator LaserSpawning()
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
    }
}

