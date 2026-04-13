using UnityEditor;
using UnityEngine;

public class AutoGridSystemBuilder
{
    [MenuItem("Tools/16칸 그리드 타일 폭격 무기 생성!")]
    public static void BuildGridWeapons()
    {
        string warningPath = "Assets/Space Shooter Template FREE/Prefabs/Projectiles/GridStrike_Warning.prefab";
        string laserPath = "Assets/Space Shooter Template FREE/Prefabs/Projectiles/GridStrike_Laser.prefab";

        Sprite squareSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // 1. 사전 경고 타일 바탕 만들기
        if (AssetDatabase.LoadAssetAtPath<GameObject>(warningPath) == null)
        {
            GameObject warningObj = new GameObject("GridStrike_Warning");
            SpriteRenderer sr = warningObj.AddComponent<SpriteRenderer>();
            sr.sprite = squareSprite;
            // 반투명한 노란-빨간색으로 위험 표시!
            sr.color = new Color(1f, 0.3f, 0f, 0.4f); 
            // 타일 기본 크기를 충분히 크게 (나중에 코드로 화면 계산해서 다시 맞춥니다)
            warningObj.transform.localScale = new Vector3(2f, 2f, 1f);
            
            PrefabUtility.SaveAsPrefabAsset(warningObj, warningPath);
            Object.DestroyImmediate(warningObj);
        }

        // 2. 실제 타일 레이저 폭격 장판 만들기
        if (AssetDatabase.LoadAssetAtPath<GameObject>(laserPath) == null)
        {
            GameObject laserObj = new GameObject("GridStrike_Laser");
            SpriteRenderer sr = laserObj.AddComponent<SpriteRenderer>();
            sr.sprite = squareSprite;
            // 완전 새빨간 치명적인 빛!!
            sr.color = new Color(1f, 0f, 0f, 0.9f); 
            laserObj.transform.localScale = new Vector3(2f, 2f, 1f);

            BoxCollider2D col = laserObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            
            Rigidbody2D rb = laserObj.AddComponent<Rigidbody2D>();
            rb.isKinematic = true;

            // 이미 존재하는 레이저 데미지 스크립트 붙이기
            LaserBeam beam = laserObj.AddComponent<LaserBeam>();
            beam.lifeTime = 0.5f; // 폭격 유지 시간 0.5초 매우 짧고 굵게!
            beam.damage = 1;
            beam.damageTickRate = 0.5f; // 한 번 맞으면 끝

            PrefabUtility.SaveAsPrefabAsset(laserObj, laserPath);
            Object.DestroyImmediate(laserObj);
        }

        Debug.Log("🎉 [완료] 타일 폭격용 경고/폭발 장판 프리팹들이 성공적으로 구워졌습니다!");
    }
}
