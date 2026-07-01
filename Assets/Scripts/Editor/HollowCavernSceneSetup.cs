#if UNITY_EDITOR
using Hollow.GameCamera;
using Hollow.Input;
using Hollow.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Hollow.Editor
{
    public static class HollowCavernSceneSetup
    {
        private const string ScenePath = "Assets/Scenes/VerticalCavernTest.unity";
        private const string ConfigPath = "Assets/ScriptableObjects/DefaultHeroConfig.asset";
        private const string PrefabPath = "Assets/Prefabs/Player.prefab";
        private const string AnimatorPath = "Assets/Art/Animations/HeroAnimator.controller";

        private static readonly Color WallColor = new(0.12f, 0.14f, 0.24f);
        private static readonly Color RockColor = new(0.18f, 0.20f, 0.34f);
        private static readonly Color ThinPlatColor = new(0.24f, 0.26f, 0.40f);
        private static readonly Color BgFarColor = new(0.07f, 0.09f, 0.16f, 0.9f);
        private static readonly Color BgMidColor = new(0.09f, 0.11f, 0.19f, 0.85f);
        private static readonly Color GrassColor = new(0.55f, 0.65f, 0.75f, 0.8f);
        private static readonly Color PoleColor = new(0.35f, 0.38f, 0.48f);
        private static readonly Color EnemyGlowColor = new(1f, 0.55f, 0.15f);

        [MenuItem("Hollow/Setup Vertical Cavern Test Scene")]
        public static void SetupVerticalCavernScene()
        {
            HollowSceneSetup.EnsureSharedAssets();
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (playerPrefab == null)
            {
                EditorUtility.DisplayDialog("Hollow Setup", "Player prefab missing. Run Hollow > Setup Movement Test Scene first.", "OK");
                return;
            }

            CreateVerticalCavernScene(playerPrefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!Application.isBatchMode)
                EditorUtility.DisplayDialog("Hollow Setup", "Vertical cavern scene created at Assets/Scenes/VerticalCavernTest.unity", "OK");
        }

        private static void CreateVerticalCavernScene(GameObject playerPrefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Level_Geometry");
            var decorRoot = new GameObject("Level_Decor");
            var bgRoot = new GameObject("Background");

            SetupCameraAndLighting(out var camGo);

            CreateBackgroundLayers(bgRoot.transform);
            CreateShaftWalls(root.transform);
            CreateCavernPlatforms(root.transform);
            CreateDecorations(decorRoot.transform);

            var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.transform.position = new Vector3(-3.5f, -4.2f, 0f);

            EnableVerticalCameraLookAhead(player);
            var cameraTarget = player.transform.Find("CameraTarget");
            HollowSceneSetup.SetupCameraFollowPublic(camGo, cameraTarget != null ? cameraTarget : player.transform);

            HollowSceneSetup.EnsureDirectoryPublic("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            HollowSceneSetup.AddSceneToBuildSettingsPublic(ScenePath);
        }

        private static void SetupCameraAndLighting(out GameObject camGo)
        {
            camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 8f, -10f);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7f;
            cam.backgroundColor = new Color(0.04f, 0.05f, 0.09f);
            camGo.AddComponent<AudioListener>();
            HollowSceneSetup.TryAddCinemachineBrainPublic(camGo);

            var lightGo = new GameObject("Global Light 2D");
            var light = lightGo.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
            light.intensity = 0.65f;
            light.color = new Color(0.65f, 0.75f, 1f);
        }

        private static void EnableVerticalCameraLookAhead(GameObject player)
        {
            var cameraTarget = player.transform.Find("CameraTarget");
            if (cameraTarget == null)
                return;

            var lookAhead = cameraTarget.GetComponent<CameraLookAhead>();
            if (lookAhead == null)
                return;

            var so = new SerializedObject(lookAhead);
            so.FindProperty("useVerticalLookAhead").boolValue = true;
            so.FindProperty("verticalLookAheadDistance").floatValue = 2f;
            so.FindProperty("lookAheadDistance").floatValue = 1.5f;
            so.FindProperty("baseOffset").vector3Value = new Vector3(0f, 1.5f, 0f);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateBackgroundLayers(Transform parent)
        {
            CreateBgPanel(parent, "BG_Far", new Vector3(0f, 16f, 5f), new Vector2(22f, 50f), BgFarColor, -30);
            CreateBgPanel(parent, "BG_Mid", new Vector3(0f, 16f, 3f), new Vector2(18f, 44f), BgMidColor, -20);
            CreateBgPanel(parent, "BG_Spirals_Left", new Vector3(-4f, 14f, 2f), new Vector2(6f, 30f), new Color(0.11f, 0.13f, 0.22f, 0.5f), -15);
            CreateBgPanel(parent, "BG_Spirals_Right", new Vector3(4.5f, 18f, 2f), new Vector2(5f, 24f), new Color(0.10f, 0.12f, 0.20f, 0.45f), -15);
        }

        private static void CreateBgPanel(Transform parent, string name, Vector3 pos, Vector2 size, Color color, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = HollowSceneSetup.CreatePlaceholderSpritePublic($"Cavern_{name}", color, 8);
            sr.sortingOrder = sortingOrder;
        }

        private static void CreateShaftWalls(Transform parent)
        {
            CreateSolidPlatform(parent, "Wall_Left", new Vector3(-6f, 16f, 0f), new Vector2(1.4f, 44f), WallColor, "Wall");
            CreateSolidPlatform(parent, "Wall_Right", new Vector3(6f, 16f, 0f), new Vector2(1.4f, 44f), WallColor, "Wall");
            CreateSolidPlatform(parent, "Floor_Left", new Vector3(-3.8f, -5.2f, 0f), new Vector2(4.8f, 1f), RockColor, "Ground");
            CreateSolidPlatform(parent, "Floor_Right", new Vector3(3.8f, -5.2f, 0f), new Vector2(4.8f, 1f), RockColor, "Ground");
        }

        private static void CreateCavernPlatforms(Transform parent)
        {
            // Bottom / entry
            CreateSolidPlatform(parent, "Entry_Ledge", new Vector3(-3.2f, -3.8f, 0f), new Vector2(2.8f, 0.55f), RockColor);
            CreateSolidPlatform(parent, "Step_01", new Vector3(1.8f, -2.5f, 0f), new Vector2(1.4f, 0.35f), ThinPlatColor, oneWay: true);
            CreateSolidPlatform(parent, "Step_02", new Vector3(-0.8f, -1.2f, 0f), new Vector2(2f, 0.4f), ThinPlatColor);

            // Lower shaft — large left shelf with drop
            CreateSolidPlatform(parent, "Shelf_Large_01", new Vector3(-3.2f, 1.8f, 0f), new Vector2(3.8f, 0.85f), RockColor);
            CreateSpikeDecor(parent, "Spikes_Shelf_01", new Vector3(-3.2f, 1.15f, 0f), 3.2f);
            CreateSolidPlatform(parent, "Step_03", new Vector3(1.5f, 2.8f, 0f), new Vector2(1.2f, 0.32f), ThinPlatColor, oneWay: true);
            CreateSolidPlatform(parent, "Center_04", new Vector3(0f, 4.5f, 0f), new Vector2(2.2f, 0.45f), RockColor);
            CreateSolidPlatform(parent, "Right_Ledge_01", new Vector3(3.8f, 4f, 0f), new Vector2(2.4f, 0.55f), RockColor);

            // Mid climb — zigzag
            CreateSolidPlatform(parent, "Left_Ledge_02", new Vector3(-3.5f, 6.5f, 0f), new Vector2(2.2f, 0.45f), RockColor);
            CreateSolidPlatform(parent, "Step_05", new Vector3(-1.5f, 8.2f, 0f), new Vector2(1.3f, 0.32f), ThinPlatColor, oneWay: true);
            CreateSolidPlatform(parent, "Center_06", new Vector3(1.2f, 9.8f, 0f), new Vector2(2f, 0.42f), RockColor);
            CreateSolidPlatform(parent, "Right_Ledge_02", new Vector3(3.6f, 11.2f, 0f), new Vector2(2.5f, 0.5f), RockColor);
            CreateSolidPlatform(parent, "Shelf_Large_02", new Vector3(-2.8f, 12.8f, 0f), new Vector2(3.4f, 0.75f), RockColor);
            CreateSpikeDecor(parent, "Spikes_Shelf_02", new Vector3(-2.8f, 12.25f, 0f), 2.8f);

            CreateSolidPlatform(parent, "Step_07", new Vector3(2.2f, 14f, 0f), new Vector2(1.2f, 0.32f), ThinPlatColor, oneWay: true);
            CreateSolidPlatform(parent, "Center_08", new Vector3(0f, 15.8f, 0f), new Vector2(2.4f, 0.45f), RockColor);
            CreateSolidPlatform(parent, "Left_Ledge_03", new Vector3(-3.8f, 17.2f, 0f), new Vector2(2.6f, 0.55f), RockColor);
            CreateSolidPlatform(parent, "Right_Ledge_03", new Vector3(3.5f, 18.8f, 0f), new Vector2(2.2f, 0.48f), RockColor);

            // Upper shaft
            CreateSolidPlatform(parent, "Center_09", new Vector3(-1.5f, 20.5f, 0f), new Vector2(2.8f, 0.5f), RockColor);
            CreateSolidPlatform(parent, "Step_10", new Vector3(2.5f, 22f, 0f), new Vector2(1.3f, 0.32f), ThinPlatColor, oneWay: true);
            CreateSolidPlatform(parent, "Shelf_Large_03", new Vector3(-3f, 23.8f, 0f), new Vector2(3.6f, 0.8f), RockColor);
            CreateSolidPlatform(parent, "Center_11", new Vector3(0.5f, 25.8f, 0f), new Vector2(2.2f, 0.42f), RockColor);
            CreateSolidPlatform(parent, "Right_Ledge_04", new Vector3(3.8f, 27.2f, 0f), new Vector2(2.4f, 0.5f), RockColor);
            CreateSolidPlatform(parent, "Step_12", new Vector3(-1.8f, 28.8f, 0f), new Vector2(1.4f, 0.32f), ThinPlatColor, oneWay: true);
            CreateSolidPlatform(parent, "Center_13", new Vector3(1f, 30.5f, 0f), new Vector2(2.6f, 0.48f), RockColor);

            // Top exit
            CreateSolidPlatform(parent, "Top_Left", new Vector3(-2.5f, 32.5f, 0f), new Vector2(3f, 0.65f), RockColor);
            CreateSolidPlatform(parent, "Top_Right", new Vector3(2.8f, 34f, 0f), new Vector2(2.4f, 0.5f), RockColor);
            CreateSolidPlatform(parent, "Top_Cap", new Vector3(0f, 36.2f, 0f), new Vector2(8f, 1f), WallColor);

            // Side wall ledges (inset from main walls for wall-jump practice)
            CreateSolidPlatform(parent, "WallLedge_L_01", new Vector3(-5.1f, 3.5f, 0f), new Vector2(1.2f, 0.4f), ThinPlatColor);
            CreateSolidPlatform(parent, "WallLedge_L_02", new Vector3(-5.1f, 10.5f, 0f), new Vector2(1.2f, 0.4f), ThinPlatColor);
            CreateSolidPlatform(parent, "WallLedge_L_03", new Vector3(-5.1f, 19f, 0f), new Vector2(1.2f, 0.4f), ThinPlatColor);
            CreateSolidPlatform(parent, "WallLedge_L_04", new Vector3(-5.1f, 26.5f, 0f), new Vector2(1.2f, 0.4f), ThinPlatColor);
            CreateSolidPlatform(parent, "WallLedge_R_01", new Vector3(5.1f, 6f, 0f), new Vector2(1.2f, 0.4f), ThinPlatColor);
            CreateSolidPlatform(parent, "WallLedge_R_02", new Vector3(5.1f, 13.5f, 0f), new Vector2(1.2f, 0.4f), ThinPlatColor);
            CreateSolidPlatform(parent, "WallLedge_R_03", new Vector3(5.1f, 21f, 0f), new Vector2(1.2f, 0.4f), ThinPlatColor);
            CreateSolidPlatform(parent, "WallLedge_R_04", new Vector3(5.1f, 29f, 0f), new Vector2(1.2f, 0.4f), ThinPlatColor);

            // Silksong movement validation zones
            CreateSolidPlatform(parent, "SprintJump_Only", new Vector3(4.8f, 8.5f, 0f), new Vector2(2f, 0.4f), new Color(0.55f, 0.38f, 0.58f));
            CreateSolidPlatform(parent, "WallDash_Gap", new Vector3(-4.8f, 15.5f, 0f), new Vector2(2.2f, 0.4f), new Color(0.52f, 0.36f, 0.56f));
            CreateSolidPlatform(parent, "ClimbShaft_Top", new Vector3(4.8f, 24.5f, 0f), new Vector2(2.5f, 0.45f), new Color(0.54f, 0.37f, 0.57f));
            CreateSolidPlatform(parent, "DownDash_ShaftTop", new Vector3(4.5f, 16.5f, 0f), new Vector2(1.8f, 0.35f), new Color(0.5f, 0.35f, 0.54f));
            CreateSolidPlatform(parent, "DownDash_ShaftBot", new Vector3(4.5f, 13f, 0f), new Vector2(1.8f, 0.35f), new Color(0.46f, 0.32f, 0.5f));
            CreateSolidPlatform(parent, "DownDash_ShaftWallL", new Vector3(3.5f, 14.75f, 0f), new Vector2(0.4f, 4f), WallColor, "Wall");
            CreateSolidPlatform(parent, "DownDash_ShaftWallR", new Vector3(5.5f, 14.75f, 0f), new Vector2(0.4f, 4f), WallColor, "Wall");
        }

        private static void CreateDecorations(Transform parent)
        {
            CreateGrass(parent, "Grass_Entry", new Vector3(-3.5f, -3.45f, 0f));
            CreateGrass(parent, "Grass_Shelf01", new Vector3(-4.2f, 2.15f, 0f));
            CreateGrass(parent, "Grass_Center04", new Vector3(-0.5f, 4.75f, 0f));
            CreateGrass(parent, "Grass_Shelf02", new Vector3(-3.8f, 13.2f, 0f));
            CreateGrass(parent, "Grass_Top", new Vector3(-3f, 32.9f, 0f));

            CreatePole(parent, "Pole_Shelf01", new Vector3(-1.8f, 2.15f, 0f));
            CreatePole(parent, "Pole_Center08", new Vector3(0.8f, 16.05f, 0f));
            CreatePole(parent, "Pole_Shelf03", new Vector3(-1.2f, 24.2f, 0f));
            CreatePole(parent, "Pole_Top", new Vector3(2f, 34.25f, 0f));

            CreateSignpost(parent, "Signpost_Right", new Vector3(4.2f, 11.5f, 0f));

            CreateFlyingEnemyMarker(parent, "Flyer_01", new Vector3(0f, 0.5f, 0f));
            CreateFlyingEnemyMarker(parent, "Flyer_02", new Vector3(-1.5f, 7f, 0f));
            CreateFlyingEnemyMarker(parent, "Flyer_03", new Vector3(2f, 12.5f, 0f));
            CreateFlyingEnemyMarker(parent, "Flyer_04", new Vector3(-0.5f, 18f, 0f));
            CreateFlyingEnemyMarker(parent, "Flyer_05", new Vector3(1.5f, 24f, 0f));
            CreateFlyingEnemyMarker(parent, "Flyer_06", new Vector3(-1f, 29f, 0f));

            CreateNpcSilhouette(parent, "NPC_Shelf01", new Vector3(-4.5f, 2.15f, 0f));
            CreateNpcSilhouette(parent, "NPC_Right02", new Vector3(4.5f, 4.3f, 0f));
            CreateNpcSilhouette(parent, "NPC_Center09", new Vector3(-2.5f, 20.8f, 0f));
        }

        private static void CreateSolidPlatform(
            Transform parent,
            string name,
            Vector3 position,
            Vector2 size,
            Color color,
            string layerName = "Ground",
            bool oneWay = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.layer = LayerMask.NameToLayer(layerName);
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = HollowSceneSetup.CreatePlaceholderSpritePublic($"Cavern_{name}", color, 8);
            sr.sortingOrder = 0;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            if (oneWay)
            {
                var effector = go.AddComponent<PlatformEffector2D>();
                effector.useOneWay = true;
                effector.surfaceArc = 160f;
                col.usedByEffector = true;
            }
        }

        private static void CreateSpikeDecor(Transform parent, string name, Vector3 position, float width)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = new Vector3(width, 0.5f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = HollowSceneSetup.CreatePlaceholderSpritePublic($"Cavern_{name}", new Color(0.08f, 0.09f, 0.14f), 8);
            sr.sortingOrder = -1;
        }

        private static void CreateGrass(Transform parent, string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = new Vector3(0.8f, 0.5f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = HollowSceneSetup.CreatePlaceholderSpritePublic($"Decor_{name}", GrassColor, 4);
            sr.sortingOrder = 2;
        }

        private static void CreatePole(Transform parent, string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = new Vector3(0.15f, 1.8f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = HollowSceneSetup.CreatePlaceholderSpritePublic($"Decor_{name}", PoleColor, 4);
            sr.sortingOrder = 3;
        }

        private static void CreateSignpost(Transform parent, string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = new Vector3(0.5f, 0.8f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = HollowSceneSetup.CreatePlaceholderSpritePublic($"Decor_{name}", new Color(0.7f, 0.75f, 0.85f), 8);
            sr.sortingOrder = 4;
        }

        private static void CreateFlyingEnemyMarker(Transform parent, string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = new Vector3(0.55f, 0.55f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = HollowSceneSetup.CreatePlaceholderSpritePublic($"Enemy_{name}", EnemyGlowColor, 16);
            sr.sortingOrder = 6;

            var lightGo = new GameObject("Glow");
            lightGo.transform.SetParent(go.transform);
            lightGo.transform.localPosition = Vector3.zero;
            var light = lightGo.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
            light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point;
            light.intensity = 0.4f;
            light.pointLightOuterRadius = 2f;
            light.color = EnemyGlowColor;
        }

        private static void CreateNpcSilhouette(Transform parent, string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = new Vector3(0.45f, 0.7f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = HollowSceneSetup.CreatePlaceholderSpritePublic($"Decor_{name}", new Color(0.05f, 0.06f, 0.10f), 8);
            sr.sortingOrder = 1;
        }
    }
}
#endif
