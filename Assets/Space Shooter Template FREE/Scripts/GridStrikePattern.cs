using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 에디터 패턴 메이커로 구워진 레이저 프리팹 전용 코어 엔진입니다.
/// LevelController의 Enemy Waves 시스템에 의해 게임 내에 생성(Instantiate)되는 즉시 스스로 발동됩니다.
/// </summary>
public class GridStrikePattern : MonoBehaviour
{
    [Header("패턴 데이터 (툴에서 자동 기입됨)")]
    [Tooltip("체크된 타일 번호(X)와 레이저 각도(Y) 목록")]
    public Vector2[] targetTilesAndAngles;
    
    [Header("프리팹 셋팅")]
    public GameObject warningPrefab;
    public GameObject laserPrefab;
    public float warningDuration = 1.0f;

    IEnumerator Start()
    {
        if (targetTilesAndAngles == null || targetTilesAndAngles.Length == 0)
        {
            Destroy(gameObject);
            yield break;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null) yield break;

        float minX = mainCamera.ViewportToWorldPoint(new Vector2(0, 0)).x;
        float maxX = mainCamera.ViewportToWorldPoint(new Vector2(1, 1)).x;
        float minY = mainCamera.ViewportToWorldPoint(new Vector2(0, 0)).y;
        float maxY = mainCamera.ViewportToWorldPoint(new Vector2(1, 1)).y;

        float cellWidth = (maxX - minX) / 4f;
        float cellHeight = (maxY - minY) / 4f;

        List<GameObject> activeWarnings = new List<GameObject>();

        // 1단계: 경고 마크 생성
        if (warningPrefab != null)
        {
            foreach (Vector2 tileData in targetTilesAndAngles)
            {
                int tileNumber = (int)tileData.x;
                float laserAngle = tileData.y;
                
                int index = Mathf.Clamp(tileNumber - 1, 0, 15);
                int row = index / 4; 
                int col = index % 4;
                
                float xPos = minX + (col * cellWidth) + (cellWidth / 2f);
                float yPos = maxY - (row * cellHeight) - (cellHeight / 2f);

                GameObject warning = Instantiate(warningPrefab, new Vector3(xPos, yPos, 0), Quaternion.Euler(0, 0, laserAngle));
                warning.transform.SetParent(this.transform); // 깔끔한 정리를 위해 부모 지정
                activeWarnings.Add(warning);
            }
            yield return new WaitForSeconds(warningDuration);
        }

        // 2단계: 실제 레이저 폭격 발사
        foreach (Vector2 tileData in targetTilesAndAngles)
        {
            int tileNumber = (int)tileData.x;
            float laserAngle = tileData.y;
            
            int index = Mathf.Clamp(tileNumber - 1, 0, 15);
            int row = index / 4; 
            int col = index % 4;
            
            float xPos = minX + (col * cellWidth) + (cellWidth / 2f);
            float yPos = maxY - (row * cellHeight) - (cellHeight / 2f);

            if (laserPrefab != null)
            {
                GameObject laser = Instantiate(laserPrefab, new Vector3(xPos, yPos, 0), Quaternion.Euler(0, 0, laserAngle));
                laser.transform.SetParent(this.transform);
            }
        }

        // 생성되었던 경고판 파괴
        foreach (GameObject w in activeWarnings)
        {
            if (w != null) Destroy(w);
        }

        // 모든 레이저 연출이 끝나고 스스로 소멸하여 메모리 낭비를 방지 (10초 뒤 파괴)
        Destroy(gameObject, 10f);
    }
}
