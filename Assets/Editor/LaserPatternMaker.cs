using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class LaserPatternMaker : EditorWindow
{
    private GameObject warningPrefab;
    private GameObject laserPrefab;
    private float warningDuration = 1.0f;
    private string patternName = "LaserPattern_NewWave";

    private List<Vector2> patternNodes = new List<Vector2>();

    private Vector2 scrollPos;

    [MenuItem("Tools/3단계: 레이저 패턴 메이커 (LASER Wave 굽기)")]
    public static void ShowWindow()
    {
        EditorWindow window = GetWindow<LaserPatternMaker>("LASER Pattern Maker");
        window.minSize = new Vector2(400, 600);
    }

    private void OnGUI()
    {
        GUILayout.Label("배치할 패턴 이름", EditorStyles.boldLabel);
        patternName = EditorGUILayout.TextField("프리팹 이름", patternName);

        GUILayout.Space(10);
        GUILayout.Label("1. 시각적 타일 추가 (클릭하면 아래 목록에 추가됨)", EditorStyles.boldLabel);
        
        // 4x4 바둑판 모양의 버튼 렌더링
        GUILayout.BeginVertical("box");
        for (int row = 0; row < 4; row++)
        {
            GUILayout.BeginHorizontal();
            for (int col = 0; col < 4; col++)
            {
                int tileNum = (row * 4) + col + 1; // 1번부터 16번
                if (GUILayout.Button($"[{tileNum}]", GUILayout.Height(50)))
                {
                    // 버튼을 누를 때마다 기본 설정(-90 위에서 아래로)으로 추가됨
                    patternNodes.Add(new Vector2(tileNum, -90f));
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        GUILayout.Space(10);
        GUILayout.Label("2. 상세 타일 및 각도 설정 목록", EditorStyles.boldLabel);
        
        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(150));
        for (int i = 0; i < patternNodes.Count; i++)
        {
            GUILayout.BeginHorizontal();
            
            GUILayout.Label($"항목 {i+1} - 타일:", GUILayout.Width(80));
            float tile = EditorGUILayout.FloatField(patternNodes[i].x, GUILayout.Width(40));
            
            GUILayout.Label("각도:", GUILayout.Width(40));
            float angle = EditorGUILayout.FloatField(patternNodes[i].y, GUILayout.Width(60));
            
            patternNodes[i] = new Vector2(tile, angle);

            if (GUILayout.Button("X", GUILayout.Width(30)))
            {
                patternNodes.RemoveAt(i);
                i--; // 인덱스 보정
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();

        if (GUILayout.Button("목록 모두 지우기"))
        {
            patternNodes.Clear();
        }

        GUILayout.Space(10);
        GUILayout.Label("3. 프리팹 조립 부품 세팅", EditorStyles.boldLabel);
        warningPrefab = (GameObject)EditorGUILayout.ObjectField("Warning Prefab", warningPrefab, typeof(GameObject), false);
        laserPrefab = (GameObject)EditorGUILayout.ObjectField("Laser Prefab", laserPrefab, typeof(GameObject), false);
        warningDuration = EditorGUILayout.FloatField("경고 시간 (초)", warningDuration);

        GUILayout.Space(20);
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("🚀 [Create Laser Wave Prefab] (굽기!)", GUILayout.Height(50)))
        {
            CreatePrefab();
        }
        GUI.backgroundColor = Color.white;
    }

    private void CreatePrefab()
    {
        if (patternNodes.Count == 0)
        {
            EditorUtility.DisplayDialog("경고", "타일 패턴이 최소 하나는 있어야 합니다!", "확인");
            return;
        }

        if (laserPrefab == null)
        {
            EditorUtility.DisplayDialog("경고", "Laser Prefab을 빈칸으로 둘 수 없습니다!", "확인");
            return;
        }

        // 폴더 생성
        string folderPath = "Assets/LaserWaves";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "LaserWaves");
        }

        // 임시 씬 오브젝트 생성
        GameObject rootObj = new GameObject(patternName);
        GridStrikePattern patternScript = rootObj.AddComponent<GridStrikePattern>();

        patternScript.targetTilesAndAngles = patternNodes.ToArray();
        patternScript.warningPrefab = warningPrefab;
        patternScript.laserPrefab = laserPrefab;
        patternScript.warningDuration = warningDuration;

        // 프리팹 파일로 저장
        string localPath = $"{folderPath}/{patternName}.prefab";
        
        // 이름 겹치면 유니크하게 만들기
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

        bool prefabSuccess;
        PrefabUtility.SaveAsPrefabAssetAndConnect(rootObj, localPath, InteractionMode.UserAction, out prefabSuccess);

        // 씬에서 잡동사니 제거
        DestroyImmediate(rootObj);

        if (prefabSuccess)
        {
            EditorUtility.DisplayDialog("성공!", $"Assets/LaserWaves 폴더에 '{patternName}' 프리팹이 성공적으로 저장되었습니다!\n\n이제 LevelController의 'Enemy Waves' 배열에 드래그 앤 드롭해서 사용하세요!", "확인");
        }
        else
        {
            EditorUtility.DisplayDialog("실패", "프리팹 생성에 실패했습니다.", "확인");
        }
    }
}
