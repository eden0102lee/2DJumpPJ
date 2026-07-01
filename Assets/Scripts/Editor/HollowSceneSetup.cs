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
    public static class HollowSceneSetup
    {
        private const string ConfigPath = "Assets/ScriptableObjects/DefaultHeroConfig.asset";
        private const string ScenePath = "Assets/Scenes/MovementTest.unity";
        private const string PrefabPath = "Assets/Prefabs/Player.prefab";
        private const string AnimatorPath = "Assets/Art/Animations/HeroAnimator.controller";

        [MenuItem("Hollow/Setup Movement Test Scene")]
        public static void SetupMovementTestScene()
        {
            EnsureSharedAssets();
            var playerPrefab = EnsurePlayerPrefab();
            CreateMovementTestScene(playerPrefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!Application.isBatchMode)
                EditorUtility.DisplayDialog("Hollow Setup", "Movement test scene created at Assets/Scenes/MovementTest.unity", "OK");
        }

        public static void EnsureSharedAssets()
        {
            EnsureHeroConfig();
            EnsureAnimatorController();
        }

        private static void EnsureHeroConfig()
        {
            if (AssetDatabase.LoadAssetAtPath<HeroControllerConfig>(ConfigPath) != null)
                return;

            var config = ScriptableObject.CreateInstance<HeroControllerConfig>();
            AssetDatabase.CreateAsset(config, ConfigPath);
        }

        private static void EnsureAnimatorController()
        {
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(AnimatorPath) != null)
                return;

            EnsureDirectory("Assets/Art/Animations");
            var controller = AnimatorController.CreateAnimatorControllerAtPath(AnimatorPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("VelocityY", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsWallSliding", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsDashing", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsSprinting", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsWallClimbing", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsDownDashing", AnimatorControllerParameterType.Bool);

            var root = controller.layers[0].stateMachine;
            root.AddState("Idle", new Vector3(300, 0, 0));
            root.AddState("Run", new Vector3(300, 80, 0));
            root.AddState("Sprint", new Vector3(300, 160, 0));
            root.AddState("Jump", new Vector3(500, 0, 0));
            root.AddState("Fall", new Vector3(500, 80, 0));
            root.AddState("WallSlide", new Vector3(500, 160, 0));
            root.AddState("WallClimb", new Vector3(500, 240, 0));
            root.AddState("Dash", new Vector3(700, 0, 0));
            root.AddState("DownDash", new Vector3(700, 80, 0));
            root.defaultState = root.states[0].state;
        }

        private static GameObject EnsurePlayerPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existing != null)
                return existing;

            EnsureDirectory("Assets/Prefabs");
            var player = BuildPlayerObject();
            var prefab = PrefabUtility.SaveAsPrefabAsset(player, PrefabPath);
            Object.DestroyImmediate(player);
            return prefab;
        }

        private static GameObject BuildPlayerObject()
        {
            var config = AssetDatabase.LoadAssetAtPath<HeroControllerConfig>(ConfigPath);
            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            var animatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AnimatorPath);

            var player = new GameObject("Player");
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Player");

            var cameraTarget = new GameObject("CameraTarget");
            cameraTarget.transform.SetParent(player.transform);
            cameraTarget.transform.localPosition = new Vector3(0f, 1f, 0f);
            var lookAhead = cameraTarget.AddComponent<CameraLookAhead>();
            var lookAheadSo = new SerializedObject(lookAhead);
            lookAheadSo.FindProperty("lookAheadDistance").floatValue = 2f;
            lookAheadSo.FindProperty("lookAheadSmooth").floatValue = 5f;
            lookAheadSo.FindProperty("sprintLookAheadMultiplier").floatValue = 1.6f;
            lookAheadSo.ApplyModifiedPropertiesWithoutUndo();

            var visual = new GameObject("Visual");
            visual.transform.SetParent(player.transform);
            visual.transform.localPosition = Vector3.zero;
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite("PlayerPlaceholder", new Color32(180, 80, 200, 255), 32);
            sr.sortingOrder = 10;

            var rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = config.gravityScale;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            var capsule = player.AddComponent<CapsuleCollider2D>();
            capsule.size = new Vector2(0.6f, 1f);

            var sensor = player.AddComponent<GroundWallSensor>();
            SetSensorGroundLayer(sensor);

            var playerInput = player.AddComponent<PlayerInput>();
            playerInput.actions = inputActions;
            playerInput.defaultActionMap = "Player";
            playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;

            var inputReader = player.AddComponent<PlayerInputReader>();
            lookAheadSo = new SerializedObject(lookAhead);
            lookAheadSo.FindProperty("inputReader").objectReferenceValue = inputReader;
            lookAheadSo.ApplyModifiedPropertiesWithoutUndo();

            var hero = player.AddComponent<HeroController>();
            var heroSo = new SerializedObject(hero);
            heroSo.FindProperty("config").objectReferenceValue = config;
            heroSo.ApplyModifiedPropertiesWithoutUndo();

            lookAheadSo = new SerializedObject(lookAhead);
            lookAheadSo.FindProperty("heroController").objectReferenceValue = hero;
            lookAheadSo.ApplyModifiedPropertiesWithoutUndo();

            var animator = player.AddComponent<Animator>();
            if (animatorController != null)
                animator.runtimeAnimatorController = animatorController;

            player.AddComponent<HeroAnimator>();

            return player;
        }

        private static void SetSensorGroundLayer(GroundWallSensor sensor)
        {
            var so = new SerializedObject(sensor);
            so.FindProperty("groundLayer").intValue = LayerMask.GetMask("Ground", "Wall");
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Sprite CreatePlaceholderSprite(string name, Color32 color, int size)
        {
            return CreatePlaceholderSprite(name, (Color)color, size);
        }

        public static Sprite CreatePlaceholderSpritePublic(string name, Color color, int size)
        {
            return CreatePlaceholderSprite(name, color, size);
        }

        private static Sprite CreatePlaceholderSprite(string name, Color color, int size)
        {
            var path = $"Assets/Art/Sprites/{name}.png";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null)
                return existing;

            EnsureDirectory("Assets/Art/Sprites");
            var tex = new Texture2D(size, size);
            var c32 = (Color32)color;
            var pixels = new Color32[size * size];
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = c32;
            tex.SetPixels32(pixels);
            tex.Apply();
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path);

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = size;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void CreateMovementTestScene(GameObject playerPrefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camGo.AddComponent<UnityEngine.Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            camGo.AddComponent<AudioListener>();
            TryAddCinemachineBrain(camGo);

            var lightGo = new GameObject("Global Light 2D");
            lightGo.AddComponent<UnityEngine.Rendering.Universal.Light2D>();

            CreatePlatform("Ground", new Vector3(0f, -3f, 0f), new Vector2(30f, 1f), new Color(0.35f, 0.3f, 0.45f));
            CreatePlatform("Platform_Left", new Vector3(-6f, 0f, 0f), new Vector2(4f, 0.5f), new Color(0.4f, 0.35f, 0.5f));
            CreatePlatform("Platform_Right", new Vector3(6f, 1.5f, 0f), new Vector2(4f, 0.5f), new Color(0.4f, 0.35f, 0.5f));
            CreatePlatform("Platform_High", new Vector3(0f, 4f, 0f), new Vector2(3f, 0.5f), new Color(0.45f, 0.4f, 0.55f));
            CreatePlatform("Wall_Left", new Vector3(-10f, 2f, 0f), new Vector2(1f, 8f), new Color(0.3f, 0.28f, 0.4f));
            CreatePlatform("Wall_Right", new Vector3(10f, 2f, 0f), new Vector2(1f, 8f), new Color(0.3f, 0.28f, 0.4f));
            CreatePlatform("DashHall", new Vector3(0f, -1f, 0f), new Vector2(20f, 0.5f), new Color(0.38f, 0.32f, 0.48f));
            CreatePlatform("Pit_Left", new Vector3(-4f, -5f, 0f), new Vector2(3f, 1f), new Color(0.32f, 0.28f, 0.42f));
            CreatePlatform("Pit_Right", new Vector3(4f, -5f, 0f), new Vector2(3f, 1f), new Color(0.32f, 0.28f, 0.42f));

            // Silksong movement validation zones
            CreatePlatform("SprintJump_Start", new Vector3(12f, -1f, 0f), new Vector2(4f, 0.5f), new Color(0.5f, 0.35f, 0.55f));
            CreatePlatform("SprintJump_Target", new Vector3(18f, 2.5f, 0f), new Vector2(3f, 0.5f), new Color(0.55f, 0.4f, 0.6f));
            CreatePlatform("WallJump_Left", new Vector3(-14f, 0f, 0f), new Vector2(1f, 10f), new Color(0.28f, 0.26f, 0.38f));
            CreatePlatform("WallJump_Right", new Vector3(-8f, 0f, 0f), new Vector2(1f, 10f), new Color(0.28f, 0.26f, 0.38f));
            CreatePlatform("WallJump_Target", new Vector3(-11f, 5f, 0f), new Vector2(2.5f, 0.5f), new Color(0.5f, 0.38f, 0.58f));
            CreatePlatform("ClimbWall_Left", new Vector3(14f, 0f, 0f), new Vector2(1f, 12f), new Color(0.26f, 0.24f, 0.36f));
            CreatePlatform("ClimbWall_Right", new Vector3(16.5f, 0f, 0f), new Vector2(1f, 12f), new Color(0.26f, 0.24f, 0.36f));
            CreatePlatform("ClimbWall_Top", new Vector3(15.25f, 7f, 0f), new Vector2(3f, 0.5f), new Color(0.52f, 0.36f, 0.56f));
            CreatePlatform("DownDash_Top", new Vector3(-18f, 6f, 0f), new Vector2(3f, 0.5f), new Color(0.48f, 0.34f, 0.52f));
            CreatePlatform("DownDash_Bottom", new Vector3(-18f, -1f, 0f), new Vector2(3f, 0.5f), new Color(0.42f, 0.3f, 0.48f));
            CreatePlatform("DownDash_WallL", new Vector3(-19.5f, 2.5f, 0f), new Vector2(0.5f, 8f), new Color(0.3f, 0.28f, 0.4f));
            CreatePlatform("DownDash_WallR", new Vector3(-16.5f, 2.5f, 0f), new Vector2(0.5f, 8f), new Color(0.3f, 0.28f, 0.4f));

            var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.transform.position = new Vector3(-8f, -1.5f, 0f);

            var cameraTarget = player.transform.Find("CameraTarget");
            SetupCameraFollow(camGo, cameraTarget != null ? cameraTarget : player.transform);

            EnsureDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
        }

        public static void TryAddCinemachineBrainPublic(GameObject cameraGo) => TryAddCinemachineBrain(cameraGo);

        public static void SetupCameraFollowPublic(GameObject camGo, Transform target) => SetupCameraFollow(camGo, target);

        public static void AddSceneToBuildSettingsPublic(string scenePath) => AddSceneToBuildSettings(scenePath);

        public static void EnsureDirectoryPublic(string path) => EnsureDirectory(path);

        private static void TryAddCinemachineBrain(GameObject cameraGo)
        {
            var brainType = System.Type.GetType("Unity.Cinemachine.CinemachineBrain, Unity.Cinemachine");
            if (brainType != null)
                cameraGo.AddComponent(brainType);
        }

        private static void SetupCameraFollow(GameObject camGo, Transform target)
        {
            var cmCameraType = System.Type.GetType("Unity.Cinemachine.CinemachineCamera, Unity.Cinemachine");
            if (cmCameraType != null)
            {
                var cmGo = new GameObject("CM_PlayerFollow");
                var cmCamera = cmGo.AddComponent(cmCameraType);

                var targetProp = cmCameraType.GetProperty("Target");
                var targetSettingsType = System.Type.GetType("Unity.Cinemachine.CinemachineCamera+TargetSettings, Unity.Cinemachine");
                if (targetProp != null && targetSettingsType != null)
                {
                    var settings = System.Activator.CreateInstance(targetSettingsType);
                    targetSettingsType.GetField("TrackingTarget")?.SetValue(settings, target);
                    targetSettingsType.GetField("LookAtTarget")?.SetValue(settings, target);
                    targetProp.SetValue(cmCamera, settings);
                }

                var followType = System.Type.GetType("Unity.Cinemachine.CinemachineFollow, Unity.Cinemachine");
                if (followType != null)
                    cmGo.AddComponent(followType);
            }
            else
            {
                var follow = camGo.AddComponent<SmoothCameraFollow>();
                var so = new SerializedObject(follow);
                so.FindProperty("target").objectReferenceValue = target;
                so.FindProperty("smoothTime").floatValue = 0.15f;
                so.FindProperty("offset").vector3Value = new Vector3(0f, 0f, -10f);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            foreach (var s in EditorBuildSettings.scenes)
            {
                if (s.path == scenePath)
                    return;
            }

            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes)
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            EditorBuildSettings.scenes = list.ToArray();
        }

        private static void CreatePlatform(string name, Vector3 position, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.layer = LayerMask.NameToLayer("Ground");
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite($"Platform_{name}", color, 8);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
        }

        private static void EnsureDirectory(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var parts = path.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
