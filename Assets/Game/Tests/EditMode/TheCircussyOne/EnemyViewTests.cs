using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

public sealed class EnemyViewTests
{
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");
    private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
    private static readonly int GridColorProperty = Shader.PropertyToID("_GridColor");
    private static readonly int EmissionColorProperty = Shader.PropertyToID("_EmissionColor");
    private static readonly int EmissionStrengthProperty = Shader.PropertyToID("_EmissionStrength");

    private GameConfig _config;
    private Material _material;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        _config = TheCircussyOneTestObjects.CreateConfig();
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.Destroy(_config);
        if (_material != null)
        {
            Object.DestroyImmediate(_material);
        }
    }

    [Test]
    public void PrepareConfiguresExplicitHurtboxContactHitboxAndMovementBodyOnly()
    {
        _config.enemyHurtboxRadius = 0.72f;
        _config.enemyHurtboxHeightOffset = 1.05f;
        _config.enemyContactHitboxSize = new Vector3(0.68f, 0.82f, 0.44f);
        _config.enemyContactHitboxOffset = new Vector3(0.03f, 0.62f, 0.41f);
        _config.enemyMovementBodyRadius = 0.49f;
        _config.enemyMovementBodyHeight = 1.37f;
        _config.enemyMovementBodyOffset = new Vector3(0.02f, 0.74f, -0.03f);
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        Transform body = enemy.transform.Find("Body Scale Root/Body") ?? enemy.transform.Find("Body");

        enemy.Prepare(_config, Vector3.zero);

        Assert.That(enemy.Hurtbox, Is.Not.Null);
        Assert.That(enemy.Hurtbox.Role, Is.EqualTo(EnemyHitboxRole.Hurtbox));
        Assert.That(enemy.Hurtbox.Collider.isTrigger, Is.True);
        Assert.That(enemy.Hurtbox.Collider, Is.TypeOf<SphereCollider>());
        SphereCollider hurtboxSphere = (SphereCollider)enemy.Hurtbox.Collider;
        Assert.That(hurtboxSphere.radius, Is.EqualTo(_config.enemyHurtboxRadius).Within(0.0001f));
        Assert.That(enemy.Hurtbox.transform.localPosition, Is.EqualTo(new Vector3(0f, _config.enemyHurtboxHeightOffset, 0f)));
        Assert.That(enemy.Hurtbox.gameObject.layer, Is.EqualTo(GameLayers.EnemyIndex));

        Assert.That(enemy.ContactHitbox, Is.Not.Null);
        Assert.That(enemy.ContactHitbox.Role, Is.EqualTo(EnemyHitboxRole.ContactDamage));
        Assert.That(enemy.ContactHitbox.Collider.isTrigger, Is.True);
        Assert.That(enemy.ContactHitbox.Collider, Is.TypeOf<BoxCollider>());
        BoxCollider contactBox = (BoxCollider)enemy.ContactHitbox.Collider;
        Assert.That(contactBox.size, Is.EqualTo(_config.enemyContactHitboxSize));
        Assert.That(enemy.ContactHitbox.transform.localPosition, Is.EqualTo(_config.enemyContactHitboxOffset));
        Assert.That(enemy.ContactHitbox.gameObject.layer, Is.EqualTo(GameLayers.EnemyIndex));

        Assert.That(enemy.MovementBody, Is.Not.Null);
        Assert.That(enemy.MovementBody.Role, Is.EqualTo(EnemyHitboxRole.MovementBody));
        Assert.That(enemy.MovementBody.Collider.isTrigger, Is.True);
        Assert.That(enemy.MovementBody.Collider, Is.TypeOf<CapsuleCollider>());
        CapsuleCollider movementCapsule = (CapsuleCollider)enemy.MovementBody.Collider;
        Assert.That(movementCapsule.radius, Is.EqualTo(_config.enemyMovementBodyRadius).Within(0.0001f));
        Assert.That(movementCapsule.height, Is.EqualTo(_config.enemyMovementBodyHeight).Within(0.0001f));
        Assert.That(movementCapsule.direction, Is.EqualTo(1));
        Assert.That(enemy.MovementBody.transform.localPosition, Is.EqualTo(_config.enemyMovementBodyOffset));
        Assert.That(enemy.MovementBody.gameObject.layer, Is.EqualTo(GameLayers.EnemyIndex));

        Collider[] bodyColliders = body.GetComponentsInChildren<Collider>(includeInactive: true);
        foreach (Collider collider in bodyColliders)
        {
            Assert.That(collider.GetComponent<EnemyHitbox>() != null || !collider.enabled, Is.True);
        }
    }

    [Test]
    public void TurnTowardsRotatesRootAndForwardContactHitboxAtConfiguredRate()
    {
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        enemy.Prepare(_config, Vector3.zero);

        enemy.TurnTowards(Vector3.right, degreesPerSecond: 90f, deltaTime: 0.5f);

        Assert.That(Quaternion.Angle(Quaternion.identity, enemy.transform.rotation), Is.EqualTo(45f).Within(0.001f));
        Assert.That(Vector3.Dot(enemy.HorizontalForward, new Vector3(1f, 0f, 1f).normalized), Is.GreaterThan(0.999f));
        Assert.That(Vector3.Dot(enemy.ContactHitbox.transform.forward, enemy.HorizontalForward), Is.GreaterThan(0.999f));
    }

    [Test]
    public void MoveTranslatesWithoutChangingFacing()
    {
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        enemy.Prepare(_config, Vector3.zero);
        enemy.TurnTowards(Vector3.right, degreesPerSecond: 90f, deltaTime: 0.5f);
        Quaternion rotation = enemy.transform.rotation;

        enemy.Move(Vector3.right, speed: 2f, deltaTime: 1f);

        Assert.That(enemy.Position.x, Is.EqualTo(2f).Within(0.001f));
        Assert.That(Quaternion.Angle(rotation, enemy.transform.rotation), Is.EqualTo(0f).Within(0.001f));
    }

    [Test]
    public void HitFlashUsesPropertyBlockAndPrepareRestoresMaterialOwnedColor()
    {
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        Transform body = enemy.transform.Find("Body Scale Root/Body") ?? enemy.transform.Find("Body");
        Renderer renderer = body.GetComponent<Renderer>();
        Renderer faceRenderer = AddFaceRenderer(body);
        Color materialColor = new(0.2f, 0.7f, 0.4f, 1f);
        _material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        _material.color = materialColor;
        renderer.sharedMaterial = _material;
        faceRenderer.sharedMaterial = _material;

        enemy.Prepare(_config, Vector3.zero);
        AssertColor(enemy.CurrentBodyColor, materialColor);

        enemy.PlayHit(0.1f, Color.white, 0.08f);

        var propertyBlock = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(propertyBlock);
        AssertColor(enemy.CurrentBodyColor, Color.white);
        AssertColor(propertyBlock.GetColor(ColorProperty), Color.white);
        AssertColor(propertyBlock.GetColor(BaseColorProperty), Color.white);
        AssertColor(propertyBlock.GetColor(GridColorProperty), Color.white);
        AssertColor(propertyBlock.GetColor(EmissionColorProperty), Color.white);
        Assert.That(propertyBlock.GetFloat(EmissionStrengthProperty), Is.EqualTo(4f).Within(0.0001f));
        faceRenderer.GetPropertyBlock(propertyBlock);
        AssertColor(propertyBlock.GetColor(BaseColorProperty), Color.white);
        AssertColor(propertyBlock.GetColor(GridColorProperty), Color.white);
        AssertColor(renderer.sharedMaterial.color, materialColor);
        AssertColor(faceRenderer.sharedMaterial.color, materialColor);

        enemy.Prepare(_config, Vector3.one);
        renderer.GetPropertyBlock(propertyBlock);
        Assert.That(propertyBlock.isEmpty, Is.True);
        faceRenderer.GetPropertyBlock(propertyBlock);
        Assert.That(propertyBlock.isEmpty, Is.True);

        AssertColor(enemy.CurrentBodyColor, materialColor);
        AssertColor(renderer.sharedMaterial.color, materialColor);
    }

    [Test]
    public void HitPulseScalesBodyOnlyAndExpires()
    {
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        DamageFeedbackVisualConfig feedbackConfig = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        Transform body = enemy.transform.Find("Body");
        if (body == null)
        {
            body = enemy.transform.Find("Body Scale Root/Body");
        }

        Transform scaleRoot = body.parent;

        enemy.Prepare(_config, Vector3.zero);
        float baseBefore = BodyBaseWorldY(body);
        enemy.PlayHit(0.1f, feedbackConfig, motionConfig);
        enemy.TickMotionVisuals(motionConfig, motionConfig.enemyHitPulse.seconds * 0.5f);

        Assert.That(enemy.transform.localScale, Is.EqualTo(Vector3.one));
        Assert.That(scaleRoot.localScale.x, Is.GreaterThan(1f));
        Assert.That(scaleRoot.localScale.y, Is.GreaterThan(1f));
        Assert.That(body.localScale, Is.EqualTo(Vector3.one));
        Assert.That(BodyBaseWorldY(body), Is.EqualTo(baseBefore).Within(0.001f));

        enemy.TickMotionVisuals(motionConfig, motionConfig.enemyHitPulse.seconds);

        Assert.That(enemy.transform.localScale, Is.EqualTo(Vector3.one));
        Assert.That(scaleRoot.localScale, Is.EqualTo(Vector3.one));
        Assert.That(body.localScale, Is.EqualTo(Vector3.one));

        TheCircussyOneTestObjects.Destroy(motionConfig);
        TheCircussyOneTestObjects.Destroy(feedbackConfig);
    }

    [Test]
    public void PreparePreservesSpawnHeightAndMovePreservesVerticalMotorHeight()
    {
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();

        enemy.Prepare(_config, new Vector3(1f, 5f, 2f));
        Assert.That(enemy.transform.position.y, Is.EqualTo(5f).Within(0.0001f));

        enemy.transform.position = new Vector3(1f, 4f, 2f);
        enemy.Move(Vector3.forward, 3f, 0.5f);

        Assert.That(enemy.transform.position.y, Is.EqualTo(4f).Within(0.0001f));
    }

    [Test]
    public void GroundedPrepareDoesNotStartIdleHoverAndResetsBodyRoot()
    {
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        DamageFeedbackVisualConfig feedbackConfig = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        Transform scaleRoot = enemy.transform.Find("Body Scale Root");
        scaleRoot.localPosition = new Vector3(0f, 0.5f, 0f);
        enemy.StartIdleHover(feedbackConfig);
        Assert.That(enemy.IsIdleHoverActive, Is.True);

        enemy.Prepare(_config, feedbackConfig, Vector3.zero);

        Assert.That(enemy.IsIdleHoverActive, Is.False);
        Assert.That(scaleRoot.localPosition.y, Is.EqualTo(0f).Within(0.0001f));

        TheCircussyOneTestObjects.Destroy(feedbackConfig);
    }

    [Test]
    public void FloatingPrepareStartsIdleHover()
    {
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        var bodyProfile = new EnemyBodyProfile();
        bodyProfile.ApplyDefaultsFromGameConfig(_config);
        var locomotion = new EnemyLocomotionProfile
        {
            mode = EnemyLocomotionMode.Floating,
            hoverHeight = 0.35f,
            hoverSeconds = 0.6f
        };

        enemy.Prepare(bodyProfile, locomotion, null, Vector3.zero);

        Assert.That(enemy.IsIdleHoverActive, Is.True);
    }

    [Test]
    public void SpawnEmergenceReturnsGroundedEnemyToBodyBase()
    {
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        Transform scaleRoot = enemy.transform.Find("Body Scale Root");

        enemy.Prepare(_config, Vector3.zero);
        enemy.BeginSpawnEmergence(startBodyLocalY: -0.5f);
        Assert.That(scaleRoot.localPosition.y, Is.LessThan(0f));

        enemy.CompleteSpawnEmergence(bodyLocalY: 1f);

        Assert.That(enemy.IsIdleHoverActive, Is.False);
        Assert.That(scaleRoot.localPosition.y, Is.EqualTo(0f).Within(0.0001f));
    }

    [Test]
    public void SetHeightMovesHitboxesAimAndHealthBarWithRoot()
    {
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        enemy.Prepare(_config, Vector3.zero);
        float hurtboxY = enemy.Hurtbox.Collider.bounds.center.y;
        float contactY = enemy.ContactHitboxCollider.bounds.center.y;
        float movementY = enemy.MovementBodyCollider.bounds.center.y;
        float aimY = enemy.AimPosition.y;
        HealthBarView healthBar = enemy.GetComponentInChildren<HealthBarView>(includeInactive: true);
        float healthY = healthBar.transform.position.y;

        enemy.SetHeight(2.25f);
        Physics.SyncTransforms();

        Assert.That(enemy.Hurtbox.Collider.bounds.center.y, Is.EqualTo(hurtboxY + 2.25f).Within(0.001f));
        Assert.That(enemy.ContactHitboxCollider.bounds.center.y, Is.EqualTo(contactY + 2.25f).Within(0.001f));
        Assert.That(enemy.MovementBodyCollider.bounds.center.y, Is.EqualTo(movementY + 2.25f).Within(0.001f));
        Assert.That(enemy.AimPosition.y, Is.EqualTo(aimY + 2.25f).Within(0.001f));
        Assert.That(healthBar.transform.position.y, Is.EqualTo(healthY + 2.25f).Within(0.001f));
    }

    [Test]
    public void DeathFadesHealthBarAndPrepareRestoresItForPooling()
    {
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        DamageFeedbackVisualConfig feedbackConfig = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        HealthBarView healthBar = enemy.GetComponentInChildren<HealthBarView>(includeInactive: true);

        enemy.Prepare(_config, feedbackConfig, Vector3.zero);
        Assert.That(healthBar.gameObject.activeSelf, Is.False);

        enemy.SetHealthNormalized(0.5f);
        Assert.That(healthBar.gameObject.activeSelf, Is.True);

        enemy.SetHealthNormalized(0f);
        enemy.PlayDeath(0.1f, feedbackConfig.enemyDeathEase, feedbackConfig, completed: null);

        Assert.That(healthBar.CurrentAlpha, Is.EqualTo(0f));
        Assert.That(healthBar.gameObject.activeSelf, Is.False);

        enemy.Prepare(_config, feedbackConfig, Vector3.one);

        Assert.That(healthBar.CurrentAlpha, Is.EqualTo(1f));
        Assert.That(healthBar.gameObject.activeSelf, Is.False);

        TheCircussyOneTestObjects.Destroy(feedbackConfig);
    }

    [Test]
    public void DefinitionModelInstantiatesDisablesFallbackAndDoesNotCreateGameplayColliders()
    {
        EnemyDefinition definition = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        definition.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Enemy Model Prefab");
        definition.modelTransform.localPosition = new Vector3(0.2f, 0.3f, 0.4f);
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        Renderer fallbackRenderer = enemy.transform.Find("Body Scale Root/Body").GetComponent<Renderer>();

        enemy.Prepare(definition, null, Vector3.zero);

        Assert.That(enemy.RuntimeModelInstance, Is.Not.Null);
        Assert.That(enemy.RuntimeModelInstance.transform.parent, Is.EqualTo(enemy.transform.Find("Body Scale Root")));
        Assert.That(enemy.RuntimeModelInstance.transform.localPosition, Is.EqualTo(definition.modelTransform.localPosition));
        Assert.That(fallbackRenderer.enabled, Is.False);
        Assert.That(enemy.Hurtbox.Collider.enabled, Is.True);
        Assert.That(enemy.ContactHitboxCollider.enabled, Is.True);
        Assert.That(enemy.MovementBodyCollider.enabled, Is.True);
        foreach (Collider collider in enemy.RuntimeModelInstance.GetComponentsInChildren<Collider>(includeInactive: true))
        {
            Assert.That(collider.enabled, Is.False);
            Assert.That(collider.GetComponent<EnemyHitbox>(), Is.Null);
        }

        TheCircussyOneTestObjects.Destroy(definition);
    }

    [Test]
    public void PrepareClearsOrReplacesPooledRuntimeModel()
    {
        EnemyDefinition firstDefinition = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        firstDefinition.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Enemy First Model Prefab");
        EnemyDefinition secondDefinition = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        secondDefinition.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Enemy Second Model Prefab");
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        Renderer fallbackRenderer = enemy.transform.Find("Body Scale Root/Body").GetComponent<Renderer>();

        enemy.Prepare(firstDefinition, null, Vector3.zero);
        GameObject firstInstance = enemy.RuntimeModelInstance;
        enemy.Prepare(secondDefinition, null, Vector3.one);

        Assert.That(firstInstance == null, Is.True);
        Assert.That(enemy.RuntimeModelInstance, Is.Not.Null);
        Assert.That(enemy.RuntimeModelInstance, Is.Not.SameAs(firstInstance));
        Assert.That(fallbackRenderer.enabled, Is.False);

        enemy.Prepare(_config, Vector3.zero);

        Assert.That(enemy.RuntimeModelInstance, Is.Null);
        Assert.That(fallbackRenderer.enabled, Is.True);

        TheCircussyOneTestObjects.Destroy(firstDefinition);
        TheCircussyOneTestObjects.Destroy(secondDefinition);
    }

    [Test]
    public void HitFlashTargetsRuntimeModelRenderers()
    {
        EnemyDefinition definition = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        definition.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Enemy Model Prefab");
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        Renderer fallbackRenderer = enemy.transform.Find("Body Scale Root/Body").GetComponent<Renderer>();
        _material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        _material.color = new Color(0.25f, 0.45f, 0.75f, 1f);
        Renderer prefabRenderer = definition.worldPrefab.GetComponentInChildren<Renderer>();
        prefabRenderer.sharedMaterial = _material;

        enemy.Prepare(definition, null, Vector3.zero);
        Renderer modelRenderer = enemy.RuntimeModelInstance.GetComponentInChildren<Renderer>();

        enemy.PlayHit(0.1f, Color.white, 0.08f);

        var propertyBlock = new MaterialPropertyBlock();
        modelRenderer.GetPropertyBlock(propertyBlock);
        AssertColor(propertyBlock.GetColor(BaseColorProperty), Color.white);
        fallbackRenderer.GetPropertyBlock(propertyBlock);
        Assert.That(propertyBlock.isEmpty, Is.True);

        TheCircussyOneTestObjects.Destroy(definition);
    }

    [Test]
    public void EnemyAnimationDriverIsSafeWhenClipsAreMissing()
    {
        EnemyDefinition definition = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        definition.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Enemy Model Prefab");
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();

        enemy.Prepare(definition, null, Vector3.zero);
        enemy.SetMotionVisualSpeed(1f);

        Assert.That(HasComponent(enemy.RuntimeModelInstance, "Animancer.AnimancerComponent"), Is.True);
        Assert.DoesNotThrow(() => enemy.TickMotionVisuals(motionConfig, 0.016f));

        TheCircussyOneTestObjects.Destroy(motionConfig);
        TheCircussyOneTestObjects.Destroy(definition);
    }

    [Test]
    public void AudienceMemberContentPlaysAssignedRuntimeAnimation()
    {
        EnemyDefinition definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/Game/ScriptableObjects/Balance/Enemies/NormalEnemy.asset");
        Assume.That(definition, Is.Not.Null);
        Assume.That(definition.worldPrefab, Is.Not.Null);
        Assume.That(definition.animation.idle, Is.Not.Null);
        Assert.That(AssetDatabase.GetAssetPath(definition.worldPrefab), Is.EqualTo("Assets/Game/Art/Actors/Enemies/AudienceMember/Prefabs/AudienceMember_Model.prefab"));
        Assert.That(definition.animation.idle.isLooping, Is.True);
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();

        enemy.Prepare(definition, null, Vector3.zero);
        enemy.SetMotionVisualSpeed(0f);
        enemy.TickMotionVisuals(motionConfig, 0.016f);

        Assert.That(enemy.RuntimeModelInstance, Is.Not.Null);
        Assert.That(enemy.RuntimeAnimationClip, Is.SameAs(definition.animation.idle));
        Assert.That(enemy.RuntimeAnimationPaused, Is.False);

        TheCircussyOneTestObjects.Destroy(motionConfig);
    }

    [Test]
    public void RuntimeModelKeepsPrefabAnimatorControllerFallback()
    {
        const string testRoot = "Assets/__EnemyViewTests";
        const string controllerPath = testRoot + "/ControllerFallback.controller";
        AssetDatabase.DeleteAsset(testRoot);
        AssetDatabase.CreateFolder("Assets", "__EnemyViewTests");
        EnemyDefinition definition = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        definition.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Enemy Controller Model Prefab");
        RuntimeAnimatorController controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        Animator prefabAnimator = definition.worldPrefab.GetComponent<Animator>();
        prefabAnimator.runtimeAnimatorController = controller;
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();

        try
        {
            enemy.Prepare(definition, null, Vector3.zero);

            Animator runtimeAnimator = enemy.RuntimeModelInstance.GetComponentInChildren<Animator>();
            Assert.That(runtimeAnimator.runtimeAnimatorController, Is.SameAs(controller));
        }
        finally
        {
            AssetDatabase.DeleteAsset(testRoot);
            TheCircussyOneTestObjects.Destroy(definition);
        }
    }

    [Test]
    public void EnemyRunAnimationTransitionUsesSmoothedMovementSpeed()
    {
        EnemyDefinition definition = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        definition.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Enemy Transition Model Prefab");
        definition.animation.idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidIdleClipPath);
        definition.animation.run = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidRunClipPath);
        definition.animation.runThreshold01 = 0.5f;
        definition.animation.runExitThreshold01 = 0.2f;
        definition.animation.runSpeedSmoothingSharpness = 1f;
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();

        enemy.Prepare(definition, null, Vector3.zero);
        enemy.SetMotionVisualSpeed(1f);
        enemy.TickMotionVisuals(motionConfig, 0.016f);

        Assert.That(enemy.RuntimeAnimationClip, Is.SameAs(definition.animation.idle));

        enemy.TickMotionVisuals(motionConfig, 1f);

        Assert.That(enemy.RuntimeAnimationClip, Is.SameAs(definition.animation.run));

        TheCircussyOneTestObjects.Destroy(motionConfig);
        TheCircussyOneTestObjects.Destroy(definition);
    }

    [Test]
    public void EnemyRunAnimationUsesExitThresholdDuringDeceleration()
    {
        EnemyDefinition definition = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        definition.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Enemy Run Exit Model Prefab");
        definition.animation.idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidIdleClipPath);
        definition.animation.run = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidRunClipPath);
        definition.animation.runThreshold01 = 0.5f;
        definition.animation.runExitThreshold01 = 0.2f;
        definition.animation.runSpeedSmoothingSharpness = 0f;
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();

        enemy.Prepare(definition, null, Vector3.zero);
        enemy.SetMotionVisualSpeed(1f);
        enemy.TickMotionVisuals(motionConfig, 0.016f);
        Assert.That(enemy.RuntimeAnimationClip, Is.SameAs(definition.animation.run));

        enemy.SetMotionVisualSpeed(0.35f);
        enemy.TickMotionVisuals(motionConfig, 0.016f);
        Assert.That(enemy.RuntimeAnimationClip, Is.SameAs(definition.animation.run));

        enemy.SetMotionVisualSpeed(0.1f);
        enemy.TickMotionVisuals(motionConfig, 0.016f);
        Assert.That(enemy.RuntimeAnimationClip, Is.SameAs(definition.animation.idle));

        TheCircussyOneTestObjects.Destroy(motionConfig);
        TheCircussyOneTestObjects.Destroy(definition);
    }

    [Test]
    public void EnemyRunAnimationSpeedScalesWithMovementSpeed()
    {
        EnemyDefinition definition = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        definition.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Enemy Scaled Run Model Prefab");
        definition.animation.run = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidRunClipPath);
        definition.animation.runSpeed = 2f;
        definition.animation.minRunSpeedMultiplier = 0.5f;
        definition.animation.runSpeedSmoothingSharpness = 0f;
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();

        enemy.Prepare(definition, null, Vector3.zero);
        enemy.SetMotionVisualSpeed(0.25f);
        enemy.TickMotionVisuals(motionConfig, 0.016f);

        Assert.That(enemy.RuntimeAnimationClip, Is.SameAs(definition.animation.run));
        Assert.That(enemy.RuntimeAnimationSpeed, Is.EqualTo(1.25f).Within(0.001f));

        enemy.SetMotionVisualSpeed(1f);
        enemy.TickMotionVisuals(motionConfig, 0.016f);

        Assert.That(enemy.RuntimeAnimationSpeed, Is.EqualTo(2f).Within(0.001f));

        TheCircussyOneTestObjects.Destroy(motionConfig);
        TheCircussyOneTestObjects.Destroy(definition);
    }

    [Test]
    public void EnemyRuntimeAnimationCanPauseAndResume()
    {
        EnemyDefinition definition = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        definition.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Paused Enemy Model Prefab");
        definition.animation.idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidIdleClipPath);
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();

        enemy.Prepare(definition, null, Vector3.zero);
        enemy.TickMotionVisuals(motionConfig, 0.016f);
        enemy.SetWorldAnimationPaused(true);

        Assert.That(enemy.RuntimeAnimationPaused, Is.True);

        enemy.SetWorldAnimationPaused(false);

        Assert.That(enemy.RuntimeAnimationPaused, Is.False);

        TheCircussyOneTestObjects.Destroy(motionConfig);
        TheCircussyOneTestObjects.Destroy(definition);
    }

    [Test]
    public void EnemyFloatingIdleAnimationIgnoresRunMovementScaling()
    {
        EnemyDefinition definition = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        definition.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Enemy Floating Model Prefab");
        definition.locomotion.mode = EnemyLocomotionMode.Floating;
        definition.animation.run = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidRunClipPath);
        definition.animation.floatingIdle = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidIdleClipPath);
        definition.animation.runSpeed = 2f;
        definition.animation.floatingIdleSpeed = 0.65f;
        definition.animation.minRunSpeedMultiplier = 0.5f;
        definition.animation.runSpeedSmoothingSharpness = 0f;
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();

        enemy.Prepare(definition, null, Vector3.zero);
        enemy.SetMotionVisualSpeed(1f);
        enemy.TickMotionVisuals(motionConfig, 0.016f);

        Assert.That(enemy.RuntimeAnimationClip, Is.SameAs(definition.animation.floatingIdle));
        Assert.That(enemy.RuntimeAnimationSpeed, Is.EqualTo(0.65f).Within(0.001f));

        TheCircussyOneTestObjects.Destroy(motionConfig);
        TheCircussyOneTestObjects.Destroy(definition);
    }

    private static void AssertColor(Color actual, Color expected)
    {
        Assert.That(actual.r, Is.EqualTo(expected.r).Within(0.0001f));
        Assert.That(actual.g, Is.EqualTo(expected.g).Within(0.0001f));
        Assert.That(actual.b, Is.EqualTo(expected.b).Within(0.0001f));
        Assert.That(actual.a, Is.EqualTo(expected.a).Within(0.0001f));
    }

    private static float BodyBaseWorldY(Transform body)
    {
        MeshFilter meshFilter = body.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            return body.position.y;
        }

        return body.TransformPoint(new Vector3(0f, meshFilter.sharedMesh.bounds.min.y, 0f)).y;
    }

    private static Renderer AddFaceRenderer(Transform body)
    {
        GameObject facePart = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        facePart.name = "Face Test Eye";
        facePart.transform.SetParent(body, false);
        Object.DestroyImmediate(facePart.GetComponent<Collider>());
        return facePart.GetComponent<Renderer>();
    }

    private static bool HasComponent(GameObject root, string fullName)
    {
        Component[] components = root.GetComponentsInChildren<Component>(includeInactive: true);
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] != null && components[i].GetType().FullName == fullName)
            {
                return true;
            }
        }

        return false;
    }
}
