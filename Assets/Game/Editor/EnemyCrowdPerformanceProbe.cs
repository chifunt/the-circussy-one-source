using System;
using System.Collections.Generic;
using System.Diagnostics;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public static class EnemyCrowdPerformanceProbe
{
    private const string EnemyPrefabPath = "Assets/Game/Prefabs/Enemy.prefab";
    private const float FixedDeltaTime = 1f / 60f;

    [MenuItem("Tools/The Circussy One/Diagnostics/Run Enemy Crowd Performance Probe")]
    public static void RunDefaultProbe()
    {
        RunProbe(new[] { 50, 150, 300 }, 45, 120);
    }

    [MenuItem("Tools/The Circussy One/Diagnostics/Run Enemy Crowd Performance Probe (Fast)")]
    public static void RunFastProbe()
    {
        RunProbe(new[] { 50, 150 }, 20, 60);
    }

    public static void RunProbe(int[] enemyCounts, int warmupFrames, int sampleFrames)
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("[PERF-ENEMY] Exit Play Mode before running the editor enemy crowd probe.");
            return;
        }

        if (!TryLoadAssets(out ProbeAssets assets))
        {
            return;
        }

        enemyCounts ??= new[] { 50, 150, 300 };
        warmupFrames = Mathf.Max(0, warmupFrames);
        sampleFrames = Mathf.Max(1, sampleFrames);

        Scene previousScene = SceneManager.GetActiveScene();
        Scene probeScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        EditorSceneManager.SetActiveScene(probeScene);

        try
        {
            Debug.Log($"[PERF-ENEMY] Starting enemy crowd performance probe. counts={string.Join(",", enemyCounts)} warmupFrames={warmupFrames} sampleFrames={sampleFrames}");
            for (int i = 0; i < enemyCounts.Length; i++)
            {
                int enemyCount = Mathf.Max(1, enemyCounts[i]);
                EditorUtility.DisplayProgressBar(
                    "Enemy Crowd Performance Probe",
                    $"Sampling {enemyCount} enemies...",
                    (float)i / Mathf.Max(1, enemyCounts.Length));
                RunScenariosForCount(assets, enemyCount, warmupFrames, sampleFrames);
            }

            Debug.Log("[PERF-ENEMY] Probe complete. Compare baseline against no-motion-visuals, movement-only, and idle-visuals-only rows before choosing an optimization target.");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            if (previousScene.IsValid())
            {
                EditorSceneManager.SetActiveScene(previousScene);
            }

            if (probeScene.IsValid())
            {
                EditorSceneManager.CloseScene(probeScene, removeScene: true);
            }
        }
    }

    private static void RunScenariosForCount(ProbeAssets assets, int enemyCount, int warmupFrames, int sampleFrames)
    {
        Scenario[] scenarios =
        {
            new("baseline", runMovement: true, runClimb: true, runMotionVisuals: true, movingVisuals: true),
            new("no-motion-visuals", runMovement: true, runClimb: true, runMotionVisuals: false, movingVisuals: false),
            new("movement-only", runMovement: true, runClimb: false, runMotionVisuals: false, movingVisuals: false),
            new("idle-visuals-only", runMovement: false, runClimb: false, runMotionVisuals: true, movingVisuals: false),
        };

        for (int i = 0; i < scenarios.Length; i++)
        {
            Scenario scenario = scenarios[i];
            using ProbeContext context = ProbeContext.Create(assets, enemyCount, scenario.MovingVisuals);
            ScenarioResult result = Measure(context, scenario, warmupFrames, sampleFrames);
            Debug.Log(result.Format(enemyCount, scenario.Name));
        }
    }

    private static ScenarioResult Measure(ProbeContext context, Scenario scenario, int warmupFrames, int sampleFrames)
    {
        for (int frame = 0; frame < warmupFrames; frame++)
        {
            Tick(context, scenario, null);
        }

        var result = new ScenarioResult();
        var stopwatch = new Stopwatch();
        for (int frame = 0; frame < sampleFrames; frame++)
        {
            Tick(context, scenario, stopwatch, ref result);
        }

        result.SampleFrames = sampleFrames;
        return result;
    }

    private static void Tick(ProbeContext context, Scenario scenario, Stopwatch stopwatch)
    {
        var ignored = new ScenarioResult();
        Tick(context, scenario, stopwatch, ref ignored);
    }

    private static void Tick(ProbeContext context, Scenario scenario, Stopwatch stopwatch, ref ScenarioResult result)
    {
        context.Time.Advance(FixedDeltaTime);

        if (scenario.RunMovement)
        {
            MeasureStage(stopwatch, context.EnemyDirector.Tick, ref result.MovementTotalMs, ref result.MovementMaxMs);
        }

        if (scenario.RunClimb)
        {
            MeasureStage(stopwatch, context.EnemyClimb.Tick, ref result.ClimbTotalMs, ref result.ClimbMaxMs);
        }

        if (scenario.RunMotionVisuals)
        {
            MeasureStage(stopwatch, context.MotionVisuals.Tick, ref result.MotionTotalMs, ref result.MotionMaxMs);
        }
    }

    private static void MeasureStage(Stopwatch stopwatch, Action action, ref double totalMs, ref double maxMs)
    {
        if (stopwatch == null)
        {
            action();
            return;
        }

        stopwatch.Restart();
        action();
        stopwatch.Stop();
        double ms = stopwatch.Elapsed.TotalMilliseconds;
        totalMs += ms;
        maxMs = Math.Max(maxMs, ms);
    }

    private static bool TryLoadAssets(out ProbeAssets assets)
    {
        GameConfig gameConfig = AssetDatabase.LoadAssetAtPath<GameConfig>(TheCircussyOneAssetPaths.GameConfigPath);
        EnemyDefinition enemyDefinition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(TheCircussyOneAssetPaths.NormalEnemyPath);
        DamageFeedbackVisualConfig damageFeedback = AssetDatabase.LoadAssetAtPath<DamageFeedbackVisualConfig>(TheCircussyOneAssetPaths.DamageFeedbackVisualConfigPath);
        ActorMotionVisualConfig motionVisuals = AssetDatabase.LoadAssetAtPath<ActorMotionVisualConfig>(TheCircussyOneAssetPaths.ActorMotionVisualConfigPath);
        GameObject enemyPrefabObject = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);
        EnemyView enemyPrefab = enemyPrefabObject != null ? enemyPrefabObject.GetComponentInChildren<EnemyView>(true) : null;

        bool ok = true;
        ok &= RequireAsset(gameConfig, TheCircussyOneAssetPaths.GameConfigPath);
        ok &= RequireAsset(enemyDefinition, TheCircussyOneAssetPaths.NormalEnemyPath);
        ok &= RequireAsset(damageFeedback, TheCircussyOneAssetPaths.DamageFeedbackVisualConfigPath);
        ok &= RequireAsset(motionVisuals, TheCircussyOneAssetPaths.ActorMotionVisualConfigPath);
        ok &= RequireAsset(enemyPrefab, EnemyPrefabPath);

        assets = ok
            ? new ProbeAssets(gameConfig, enemyDefinition, damageFeedback, motionVisuals, enemyPrefab)
            : null;
        return ok;
    }

    private static bool RequireAsset(Object asset, string path)
    {
        if (asset != null)
        {
            return true;
        }

        Debug.LogError($"[PERF-ENEMY] Missing required asset: {path}");
        return false;
    }

    private sealed class ProbeAssets
    {
        public ProbeAssets(
            GameConfig gameConfig,
            EnemyDefinition enemyDefinition,
            DamageFeedbackVisualConfig damageFeedback,
            ActorMotionVisualConfig motionVisuals,
            EnemyView enemyPrefab)
        {
            GameConfig = gameConfig;
            EnemyDefinition = enemyDefinition;
            DamageFeedback = damageFeedback;
            MotionVisuals = motionVisuals;
            EnemyPrefab = enemyPrefab;
        }

        public GameConfig GameConfig { get; }
        public EnemyDefinition EnemyDefinition { get; }
        public DamageFeedbackVisualConfig DamageFeedback { get; }
        public ActorMotionVisualConfig MotionVisuals { get; }
        public EnemyView EnemyPrefab { get; }
    }

    private sealed class ProbeContext : IDisposable
    {
        private ProbeContext(
            GameObject root,
            ProbeGameTime time,
            EnemyDirectorSystem enemyDirector,
            EnemyClimbSystem enemyClimb,
            ActorMotionVisualSystem motionVisuals)
        {
            Root = root;
            Time = time;
            EnemyDirector = enemyDirector;
            EnemyClimb = enemyClimb;
            MotionVisuals = motionVisuals;
        }

        private GameObject Root { get; }
        public ProbeGameTime Time { get; }
        public EnemyDirectorSystem EnemyDirector { get; }
        public EnemyClimbSystem EnemyClimb { get; }
        public ActorMotionVisualSystem MotionVisuals { get; }

        public static ProbeContext Create(ProbeAssets assets, int enemyCount, bool movingVisuals)
        {
            GameObject root = new($"[Perf Probe] {enemyCount} Enemies");
            BuildFloor(root.transform);
            BuildBlockers(root.transform);

            PlayerView player = BuildPlayer(root.transform);
            ActorRegistry registry = new();
            GameState state = new(assets.GameConfig, new RunStats(assets.GameConfig));
            var time = new ProbeGameTime();
            Transform enemyRoot = new GameObject("Enemies").transform;
            enemyRoot.SetParent(root.transform, false);
            var enemyFactory = new EnemyFactory(assets.GameConfig, assets.DamageFeedback, registry, assets.EnemyPrefab, enemyRoot);

            for (int i = 0; i < enemyCount; i++)
            {
                EnemyRuntime enemy = enemyFactory.Spawn(assets.EnemyDefinition, SpawnPosition(i, enemyCount), level: 1, elapsedSeconds: 0f);
                if (movingVisuals)
                {
                    enemy.View.SetMotionVisualSpeed(1f);
                }
            }

            Physics.SyncTransforms();
            return new ProbeContext(
                root,
                time,
                new EnemyDirectorSystem(assets.GameConfig, player, registry, state, time),
                new EnemyClimbSystem(assets.GameConfig, registry, time),
                new ActorMotionVisualSystem(assets.MotionVisuals, null, registry, time));
        }

        public void Dispose()
        {
            if (Root != null)
            {
                Object.DestroyImmediate(Root);
            }
        }

        private static void BuildFloor(Transform parent)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Probe Floor";
            floor.transform.SetParent(parent, false);
            floor.transform.position = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(150f, 1f, 150f);
            Object.DestroyImmediate(floor.GetComponent<Renderer>());
        }

        private static void BuildBlockers(Transform parent)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 2f / 8f;
                Vector3 position = new(Mathf.Cos(angle) * 10f, 1.1f, Mathf.Sin(angle) * 10f);
                GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blocker.name = $"Probe Climb Blocker {i + 1}";
                blocker.transform.SetParent(parent, false);
                blocker.transform.position = position;
                blocker.transform.rotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 0f);
                blocker.transform.localScale = new Vector3(2.5f, 2.2f, 7f);
                Object.DestroyImmediate(blocker.GetComponent<Renderer>());
            }
        }

        private static PlayerView BuildPlayer(Transform parent)
        {
            GameObject playerObject = new("Probe Player");
            playerObject.transform.SetParent(parent, false);
            playerObject.transform.position = Vector3.zero;
            CharacterController controller = playerObject.AddComponent<CharacterController>();
            controller.radius = 0.45f;
            controller.height = 1.8f;
            return playerObject.AddComponent<PlayerView>();
        }

        private static Vector3 SpawnPosition(int index, int count)
        {
            float goldenAngle = Mathf.PI * (3f - Mathf.Sqrt(5f));
            float normalized = (index + 0.5f) / Mathf.Max(1, count);
            float radius = Mathf.Lerp(18f, 56f, Mathf.Sqrt(normalized));
            float angle = index * goldenAngle;
            return new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }
    }

    private sealed class ProbeGameTime : IGameTime
    {
        public float Time { get; private set; }
        public float DeltaTime { get; private set; }

        public void Advance(float deltaTime)
        {
            DeltaTime = Mathf.Max(0f, deltaTime);
            Time += DeltaTime;
        }
    }

    private readonly struct Scenario
    {
        public Scenario(string name, bool runMovement, bool runClimb, bool runMotionVisuals, bool movingVisuals)
        {
            Name = name;
            RunMovement = runMovement;
            RunClimb = runClimb;
            RunMotionVisuals = runMotionVisuals;
            MovingVisuals = movingVisuals;
        }

        public string Name { get; }
        public bool RunMovement { get; }
        public bool RunClimb { get; }
        public bool RunMotionVisuals { get; }
        public bool MovingVisuals { get; }
    }

    private struct ScenarioResult
    {
        public int SampleFrames;
        public double MovementTotalMs;
        public double ClimbTotalMs;
        public double MotionTotalMs;
        public double MovementMaxMs;
        public double ClimbMaxMs;
        public double MotionMaxMs;

        public string Format(int enemyCount, string scenarioName)
        {
            double divisor = Math.Max(1, SampleFrames);
            double moveAvg = MovementTotalMs / divisor;
            double climbAvg = ClimbTotalMs / divisor;
            double motionAvg = MotionTotalMs / divisor;
            double totalAvg = moveAvg + climbAvg + motionAvg;
            double totalMax = MovementMaxMs + ClimbMaxMs + MotionMaxMs;
            return $"[PERF-ENEMY] count={enemyCount} scenario={scenarioName} "
                + $"avgTotalMs={totalAvg:F3} maxStageSumMs={totalMax:F3} "
                + $"moveAvgMs={moveAvg:F3} moveMaxMs={MovementMaxMs:F3} "
                + $"climbAvgMs={climbAvg:F3} climbMaxMs={ClimbMaxMs:F3} "
                + $"motionAvgMs={motionAvg:F3} motionMaxMs={MotionMaxMs:F3}";
        }
    }
}
