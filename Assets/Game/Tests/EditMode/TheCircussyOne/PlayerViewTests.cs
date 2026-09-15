using System.Reflection;
using MoreMountains.Feedbacks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEngine.TestTools;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

public sealed class PlayerViewTests
{
    private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
    }

    [Test]
    public void MoveDoesNotCallInactiveCharacterController()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(TheCircussyOneTestObjects.CreateConfig());
        player.gameObject.SetActive(false);

        Assert.That(player.CanMove, Is.False);
        Assert.DoesNotThrow(() => player.Move(Vector3.forward, Vector3.forward, 0.016f));
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void MovementSystemSkipsInactivePlayer()
    {
        var config = TheCircussyOneTestObjects.CreateConfig();
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(config);
        player.gameObject.SetActive(false);

        var system = new PlayerMovementSystem(
            config,
            player,
            new GameState(config),
            new FakeInputService { Movement = Vector2.up },
            new FakeGameTime { DeltaTime = 0.016f },
            new CameraOrbitState());

        Assert.DoesNotThrow(system.Tick);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void MovementSystemDrivesMotionVisualsFromInputIntent()
    {
        var config = TheCircussyOneTestObjects.CreateConfig();
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(config);
        CreateGroundCollider();
        var time = new FakeGameTime { DeltaTime = 0.05f };
        var input = new FakeInputService { Movement = Vector2.right };
        var state = new GameState(config);
        var system = new PlayerMovementSystem(
            config,
            player,
            state,
            input,
            time,
            new CameraOrbitState());

        system.Tick();

        Assert.That(player.MotionVisualTargetSpeed01, Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void MovementSystemSuppressesGroundMoveSquashWhileAirborne()
    {
        var config = TheCircussyOneTestObjects.CreateConfig();
        config.playerGroundProbeDistance = 0f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(config);
        CreateGroundCollider();
        var time = new FakeGameTime { DeltaTime = 0.016f };
        var input = new FakeInputService
        {
            Movement = Vector2.zero,
            JumpPressedThisFrame = false,
            JumpHeld = false
        };
        var state = new GameState(config);
        var system = new PlayerMovementSystem(
            config,
            player,
            state,
            input,
            time,
            new CameraOrbitState());

        system.Tick();
        input.Movement = Vector2.right;
        input.JumpPressedThisFrame = true;
        input.JumpHeld = true;
        system.Tick();

        Assert.That(player.IsAirborneForVfx, Is.True);
        Assert.That(player.MotionVisualTargetSpeed01, Is.EqualTo(0f).Within(0.0001f));
    }

    [Test]
    public void MovementSystemReadsJumpInputAndEmitsTakeoffVfx()
    {
        var config = TheCircussyOneTestObjects.CreateConfig();
        config.playerGroundProbeDistance = 0f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(config);
        CreateGroundCollider();
        var time = new FakeGameTime { DeltaTime = 0.016f };
        var input = new FakeInputService();
        var state = new GameState(config);
        var motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        var vfx = new FakeVfxSpawner();
        var system = new PlayerMovementSystem(
            config,
            player,
            state,
            input,
            time,
            new CameraOrbitState(),
            motionConfig,
            vfx);

        system.Tick();
        float startY = player.transform.position.y;
        input.JumpPressedThisFrame = true;
        input.JumpHeld = true;
        system.Tick();

        Assert.That(player.transform.position.y, Is.GreaterThan(startY));
        Assert.That(player.IsAirborneForVfx, Is.True);
        Assert.That(vfx.Calls, Has.Count.EqualTo(1));
        Assert.That(vfx.Calls[0].effectId, Is.EqualTo(VfxEffectId.PlayerJumpTakeoff));

        TheCircussyOneTestObjects.Destroy(motionConfig);
    }

    [Test]
    public void ApplyConfigSetsCharacterControllerStepOffsetAndSlopeLimit()
    {
        var config = TheCircussyOneTestObjects.CreateConfig();
        config.playerControllerStepOffset = 0.48f;
        config.playerControllerSlopeLimit = 52f;
        config.playerGroundProbeDistance = 0.42f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(config);

        var controller = player.ContactCollider as CharacterController;

        Assert.That(controller, Is.Not.Null);
        Assert.That(controller.stepOffset, Is.EqualTo(0.48f).Within(0.0001f));
        Assert.That(controller.slopeLimit, Is.EqualTo(52f).Within(0.0001f));
    }

    [Test]
    public void MoveReportsVisuallyGroundedWhenWalkableGroundIsInsideProbeDistance()
    {
        var config = TheCircussyOneTestObjects.CreateConfig();
        config.playerGroundProbeDistance = 0.5f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(config);
        CreateGroundCollider();
        player.transform.position = Vector3.up * 0.2f;
        Physics.SyncTransforms();

        PlayerMoveResult result = player.MoveDisplacement(Vector3.zero);

        Assert.That(result.IsVisuallyGrounded, Is.True);
        Assert.That(Vector3.Angle(result.GroundNormal, Vector3.up), Is.LessThan(0.1f));
    }

    [Test]
    public void MoveReportsSteepGroundAsVisualOnlyGround()
    {
        var config = TheCircussyOneTestObjects.CreateConfig();
        config.playerControllerSlopeLimit = 45f;
        config.playerVisualGroundProbeMaxSlope = 75f;
        config.playerGroundProbeDistance = 0.7f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(config);
        CreateSlopedGround(60f);
        player.transform.position = Vector3.up * 0.25f;
        Physics.SyncTransforms();

        PlayerMoveResult result = player.MoveDisplacement(Vector3.zero);

        Assert.That(result.IsVisuallyGrounded, Is.True);
        Assert.That(result.IsWalkableGrounded, Is.False);
        Assert.That(Vector3.Angle(result.GroundNormal, Vector3.up), Is.EqualTo(60f).Within(1f));
    }

    [Test]
    public void GroundTiltRotatesOnlyBodyScaleRootTowardSurfaceNormal()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(TheCircussyOneTestObjects.CreateConfig());
        Transform scaleRoot = player.Body.parent;
        Vector3 groundNormal = Quaternion.AngleAxis(20f, Vector3.right) * Vector3.up;

        player.GetComponent<ActorBodyView>().ApplyGroundTilt(groundNormal, 18f, 0f, 0.016f);

        Assert.That(player.transform.localRotation, Is.EqualTo(Quaternion.identity));
        Assert.That(scaleRoot.localRotation, Is.Not.EqualTo(Quaternion.identity));
        Assert.That(Quaternion.Angle(Quaternion.identity, scaleRoot.localRotation), Is.LessThanOrEqualTo(18.05f));
    }

    [Test]
    public void MotionVisualsScaleBodyOnly()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(TheCircussyOneTestObjects.CreateConfig());
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        Transform body = player.Body;
        Transform scaleRoot = body.parent;
        float baseBefore = BodyBaseWorldY(body);

        player.SetMotionVisualSpeed(1f);
        player.TickMotionVisuals(motionConfig, 0.105f);

        Assert.That(player.transform.localScale, Is.EqualTo(Vector3.one));
        Assert.That(scaleRoot.localScale, Is.Not.EqualTo(Vector3.one));
        Assert.That(body.localScale, Is.EqualTo(Vector3.one));
        Assert.That(BodyBaseWorldY(body), Is.EqualTo(baseBefore).Within(0.001f));

        TheCircussyOneTestObjects.Destroy(motionConfig);
    }

    [Test]
    public void PerformerMotionOverrideCanDisableIdleAndMoveBop()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(TheCircussyOneTestObjects.CreateConfig());
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        motionConfig.playerIdle = new ActorSquashStretchCycleSettings(0.12f, 0.25f, 0.5f);
        motionConfig.playerMove = new ActorSquashStretchCycleSettings(0.12f, 0.25f, 0.5f);
        var performer = ScriptableObject.CreateInstance<PerformerDefinition>();
        performer.motionVisuals.overrideGlobalBop = true;
        performer.motionVisuals.proceduralBopEnabled = false;
        player.ApplyPerformerVisuals(performer);

        player.SetMotionVisualSpeed(1f, immediate: true);
        player.TickMotionVisuals(motionConfig, 0.25f);

        Assert.That(player.Body.parent.localScale, Is.EqualTo(Vector3.one));

        TheCircussyOneTestObjects.Destroy(motionConfig);
        TheCircussyOneTestObjects.Destroy(performer);
    }

    [Test]
    public void JumpTakeoffPulseNarrowsAndStretchesBodyScaleRoot()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(TheCircussyOneTestObjects.CreateConfig());
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        Transform body = player.Body;
        Transform scaleRoot = body.parent;

        player.PlayJumpTakeoffPulse(motionConfig);
        player.TickMotionVisuals(motionConfig, motionConfig.playerJumpTakeoffPulse.seconds * 0.25f);

        Assert.That(scaleRoot.localScale.x, Is.LessThan(1f));
        Assert.That(scaleRoot.localScale.z, Is.EqualTo(scaleRoot.localScale.x).Within(0.0001f));
        Assert.That(scaleRoot.localScale.y, Is.GreaterThan(1f));
        Assert.That(body.localScale, Is.EqualTo(Vector3.one));

        TheCircussyOneTestObjects.Destroy(motionConfig);
    }

    [Test]
    public void JumpHoldStretchSustainsOnBodyScaleRootAndClearsOnRelease()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(TheCircussyOneTestObjects.CreateConfig());
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        motionConfig.playerIdle = new ActorSquashStretchCycleSettings(0f, 1.35f, 0.5f);
        motionConfig.playerMove = new ActorSquashStretchCycleSettings(0f, 0.42f, 0.55f);
        Transform body = player.Body;
        Transform scaleRoot = body.parent;

        player.SetJumpHoldVisual(jumpHeld: true, rising: true, airborne: true);
        player.TickMotionVisuals(motionConfig, motionConfig.jumpHoldMaxSeconds);
        Vector3 heldScale = scaleRoot.localScale;

        Assert.That(heldScale.x, Is.LessThan(1f));
        Assert.That(heldScale.z, Is.EqualTo(heldScale.x).Within(0.0001f));
        Assert.That(heldScale.y, Is.GreaterThan(1f));
        Assert.That(player.transform.localScale, Is.EqualTo(Vector3.one));
        Assert.That(body.localScale, Is.EqualTo(Vector3.one));

        player.SetJumpHoldVisual(jumpHeld: false, rising: true, airborne: true);
        player.TickMotionVisuals(motionConfig, motionConfig.jumpHoldReleaseSeconds);

        Assert.That(scaleRoot.localScale, Is.EqualTo(Vector3.one));

        TheCircussyOneTestObjects.Destroy(motionConfig);
    }

    [Test]
    public void MovementSystemFeedsHeldJumpStretchVisualState()
    {
        var config = TheCircussyOneTestObjects.CreateConfig();
        config.playerGroundProbeDistance = 0f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(config);
        CreateGroundCollider();
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        motionConfig.playerIdle = new ActorSquashStretchCycleSettings(0f, 1.35f, 0.5f);
        motionConfig.playerMove = new ActorSquashStretchCycleSettings(0f, 0.42f, 0.55f);
        var input = new FakeInputService();
        var system = new PlayerMovementSystem(
            config,
            player,
            new GameState(config),
            input,
            new FakeGameTime { DeltaTime = 0.016f },
            new CameraOrbitState(),
            motionConfig);
        Transform scaleRoot = player.Body.parent;

        system.Tick();
        input.JumpPressedThisFrame = true;
        input.JumpHeld = true;
        system.Tick();
        player.TickMotionVisuals(motionConfig, motionConfig.jumpHoldMaxSeconds);

        Assert.That(scaleRoot.localScale.x, Is.LessThan(1f));
        Assert.That(scaleRoot.localScale.y, Is.GreaterThan(1f));

        input.JumpPressedThisFrame = false;
        input.JumpHeld = false;
        system.Tick();
        player.TickMotionVisuals(motionConfig, motionConfig.jumpHoldReleaseSeconds);

        Assert.That(scaleRoot.localScale, Is.EqualTo(Vector3.one));

        TheCircussyOneTestObjects.Destroy(motionConfig);
    }

    [Test]
    public void JumpLandingPulseSquashesAndCanRebound()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(TheCircussyOneTestObjects.CreateConfig());
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        Transform scaleRoot = player.Body.parent;

        player.PlayJumpLandPulse(motionConfig);
        player.TickMotionVisuals(motionConfig, motionConfig.playerJumpLandPulse.seconds * 0.25f);

        Assert.That(scaleRoot.localScale.x, Is.GreaterThan(1f));
        Assert.That(scaleRoot.localScale.y, Is.LessThan(1f));

        player.TickMotionVisuals(motionConfig, motionConfig.playerJumpLandPulse.seconds * 0.5f);

        Assert.That(scaleRoot.localScale.x, Is.LessThan(1f));
        Assert.That(scaleRoot.localScale.y, Is.GreaterThan(1f));

        TheCircussyOneTestObjects.Destroy(motionConfig);
    }

    [Test]
    public void ContactShadowStaysProjectedOnGroundWhilePlayerIsAirborne()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(TheCircussyOneTestObjects.CreateConfig());
        GameObject shadow = GameObject.CreatePrimitive(PrimitiveType.Quad);
        shadow.name = "Ground Shadow";
        shadow.transform.SetParent(player.transform, false);
        shadow.transform.localPosition = new Vector3(0f, 0.035f, 0f);
        shadow.transform.localScale = Vector3.one;

        player.transform.position = Vector3.up * 2f;
        player.Move(Vector3.zero, Vector3.forward, 0f);

        Assert.That(shadow.transform.position.y, Is.EqualTo(0.035f).Within(0.001f));
        Assert.That(player.transform.localScale, Is.EqualTo(Vector3.one));
    }

    [Test]
    public void MovementVfxStopsDustAndStartsJumpTrailWhenAirborne()
    {
        var config = TheCircussyOneTestObjects.CreateConfig();
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(config);
        VfxVisualConfig vfxConfig = TheCircussyOneTestObjects.CreateVfxVisualConfig();
        AddParticleChild(player.transform, "Player Move Dust");
        AddParticleChild(player.transform, "Player Jump Trail");
        var state = new GameState(config);
        var system = new PlayerMovementVfxSystem(vfxConfig, player, state);

        player.SetMotionVisualSpeed(1f);
        player.SetAirborneVisual(false);
        system.Tick();
        Assert.That(player.MoveDust.isPlaying, Is.True);
        Assert.That(player.JumpTrail.isPlaying, Is.False);

        player.SetAirborneVisual(true);
        system.Tick();
        Assert.That(player.MoveDust.isPlaying, Is.False);
        Assert.That(player.JumpTrail.isPlaying, Is.True);
        Assert.That(player.JumpTrail.transform.localPosition.y, Is.EqualTo(0.12f).Within(0.001f));
        Assert.That(player.JumpTrail.transform.localPosition.z, Is.EqualTo(-0.08f).Within(0.001f));

        TheCircussyOneTestObjects.Destroy(vfxConfig);
    }

    [Test]
    public void GeneratedFeelFeedbacksDoNotContainScaleFeedback()
    {
        MethodInfo method = typeof(TheCircussyOneSceneBuilder).GetMethod(
            "CreateFeelFeedbacks",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(method, Is.Not.Null);

        object[] args = { null, null, null };
        method.Invoke(null, args);

        var shoot = (MMF_Player)args[0];
        var hit = (MMF_Player)args[1];
        var playerDamage = (MMF_Player)args[2];

        Assert.That(shoot.FeedbacksList.Exists(feedback => feedback is MMF_Scale), Is.False);
        Assert.That(hit.FeedbacksList.Exists(feedback => feedback is MMF_Scale), Is.False);
        Assert.That(playerDamage.FeedbacksList.Exists(feedback => feedback is MMF_Scale), Is.False);

        Object.DestroyImmediate(shoot.transform.parent.gameObject);
    }

    [Test]
    public void DamageFlashUsesRedPropertyBlockTemporarily()
    {
        var config = TheCircussyOneTestObjects.CreateConfig();
        var feedbackConfig = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        feedbackConfig.playerDamageFlashColor = Color.red;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(config);
        Renderer renderer = player.Body.GetComponent<Renderer>();
        Renderer faceRenderer = AddFaceRenderer(player.Body);
        player.ApplyConfig(config, feedbackConfig);
        var propertyBlock = new MaterialPropertyBlock();

        renderer.GetPropertyBlock(propertyBlock);
        Assert.That(propertyBlock.isEmpty, Is.True);
        faceRenderer.GetPropertyBlock(propertyBlock);
        Assert.That(propertyBlock.isEmpty, Is.True);

        player.PlayDamageFlash(feedbackConfig);
        renderer.GetPropertyBlock(propertyBlock);

        Assert.That(propertyBlock.isEmpty, Is.False);
        Assert.That(propertyBlock.GetColor(BaseColorProperty), Is.EqualTo(Color.red));
        faceRenderer.GetPropertyBlock(propertyBlock);
        Assert.That(propertyBlock.isEmpty, Is.False);
        Assert.That(propertyBlock.GetColor(BaseColorProperty), Is.EqualTo(Color.red));
        Assert.That(player.CurrentBodyColor.r, Is.EqualTo(Color.red.r).Within(0.001f));
        Assert.That(player.CurrentBodyColor.g, Is.EqualTo(Color.red.g).Within(0.001f));
        Assert.That(player.CurrentBodyColor.b, Is.EqualTo(Color.red.b).Within(0.001f));

        player.ApplyConfig(config);
        renderer.GetPropertyBlock(propertyBlock);
        Assert.That(propertyBlock.isEmpty, Is.True);
        faceRenderer.GetPropertyBlock(propertyBlock);
        Assert.That(propertyBlock.isEmpty, Is.True);

        TheCircussyOneTestObjects.Destroy(config);
        TheCircussyOneTestObjects.Destroy(feedbackConfig);
    }

    [Test]
    public void PlayerHealthVisualPresenterUpdatesWorldHealthBar()
    {
        var config = TheCircussyOneTestObjects.CreateConfig();
        config.playerMaxHealth = 100;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(config);
        var state = new GameState(config);
        var presenter = new PlayerHealthVisualPresenter(config, state, player);

        presenter.Start();
        HealthBarView healthBar = player.GetComponentInChildren<HealthBarView>(includeInactive: true);
        Assert.That(healthBar.gameObject.activeSelf, Is.False);

        state.DamagePlayer(55);

        Assert.That(healthBar.gameObject.activeSelf, Is.True);
        Assert.That(healthBar.NormalizedValue, Is.EqualTo(0.45f).Within(0.001f));
        Assert.That(healthBar.FillLength / healthBar.BackgroundLength, Is.EqualTo(0.45f).Within(0.001f));

        presenter.Dispose();
        TheCircussyOneTestObjects.Destroy(config);
    }

    [Test]
    public void PlayerWorldHealthBarAppliesConfigurableOffsetAndRenderThrough()
    {
        var config = TheCircussyOneTestObjects.CreateConfig();
        var feedbackConfig = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        feedbackConfig.playerWorldHealthBarLocalOffset = new Vector3(0.15f, 0.34f, -0.08f);
        feedbackConfig.playerWorldHealthBarWidth = 2.1f;
        feedbackConfig.playerWorldHealthBarVisibleThroughPlayer = true;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(config);

        player.ApplyWorldHealthBarConfig(feedbackConfig);

        HealthBarView healthBar = player.GetComponentInChildren<HealthBarView>(includeInactive: true);
        Assert.That(healthBar.gameObject.activeSelf, Is.False);
        Assert.That(healthBar.transform.localPosition.x, Is.EqualTo(0.15f).Within(0.001f));
        Assert.That(healthBar.transform.localPosition.y, Is.EqualTo(0.34f).Within(0.001f));
        Assert.That(healthBar.transform.localPosition.z, Is.EqualTo(-0.08f).Within(0.001f));

        Assert.That(healthBar.ZTest, Is.EqualTo(CompareFunction.Always));
        Assert.That(healthBar.BackgroundLength, Is.EqualTo(2.1f).Within(0.001f));

        TheCircussyOneTestObjects.Destroy(config);
        TheCircussyOneTestObjects.Destroy(feedbackConfig);
    }

    [Test]
    public void PlayerWorldHealthBarCanBeDisabledFromConfig()
    {
        var config = TheCircussyOneTestObjects.CreateConfig();
        var feedbackConfig = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        feedbackConfig.playerWorldHealthBarEnabled = false;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(config);

        player.ApplyWorldHealthBarConfig(feedbackConfig);

        HealthBarView healthBar = player.GetComponentInChildren<HealthBarView>(includeInactive: true);
        Assert.That(healthBar.gameObject.activeSelf, Is.False);

        player.SetHealthNormalized(0.5f);
        Assert.That(healthBar.gameObject.activeSelf, Is.False);

        TheCircussyOneTestObjects.Destroy(config);
        TheCircussyOneTestObjects.Destroy(feedbackConfig);
    }

    [Test]
    public void ApplyPerformerVisualsInstantiatesModelDisablesFallbackRendererAndKeepsController()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(TheCircussyOneTestObjects.CreateConfig());
        Renderer fallbackRenderer = player.Body.GetComponent<Renderer>();
        var performer = ScriptableObject.CreateInstance<PerformerDefinition>();
        performer.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Performer Model Prefab");
        performer.modelTransform.localPosition = new Vector3(0.1f, 0.2f, 0.3f);
        performer.modelTransform.localEulerAngles = new Vector3(0f, 45f, 0f);
        performer.modelTransform.localScale = Vector3.one * 1.2f;

        player.ApplyPerformerVisuals(performer);

        Assert.That(player.RuntimeModelInstance, Is.Not.Null);
        Assert.That(player.RuntimeModelInstance.transform.parent, Is.EqualTo(player.Body.parent));
        Assert.That(player.RuntimeModelInstance.transform.localPosition, Is.EqualTo(performer.modelTransform.localPosition));
        Assert.That(player.RuntimeModelInstance.transform.localScale, Is.EqualTo(performer.modelTransform.localScale));
        Assert.That(fallbackRenderer.enabled, Is.False);
        Assert.That(player.ContactCollider, Is.Not.Null);
        foreach (Collider collider in player.RuntimeModelInstance.GetComponentsInChildren<Collider>(includeInactive: true))
        {
            Assert.That(collider.enabled, Is.False);
        }

        TheCircussyOneTestObjects.Destroy(performer);
    }

    [Test]
    public void ApplyPerformerVisualsClearsModelWhenNoPrefabIsAssigned()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(TheCircussyOneTestObjects.CreateConfig());
        Renderer fallbackRenderer = player.Body.GetComponent<Renderer>();
        var performer = ScriptableObject.CreateInstance<PerformerDefinition>();
        performer.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Performer Model Prefab");
        player.ApplyPerformerVisuals(performer);

        player.ApplyPerformerVisuals(null);

        Assert.That(player.RuntimeModelInstance, Is.Null);
        Assert.That(fallbackRenderer.enabled, Is.True);

        TheCircussyOneTestObjects.Destroy(performer);
    }

    [Test]
    public void PerformerVisualsAddAnimancerComponentAndMissingClipsAreSafe()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(TheCircussyOneTestObjects.CreateConfig());
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        var performer = ScriptableObject.CreateInstance<PerformerDefinition>();
        performer.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Performer Model Prefab");

        player.ApplyPerformerVisuals(performer);

        Assert.That(HasComponent(player.RuntimeModelInstance, "Animancer.AnimancerComponent"), Is.True);
        Assert.DoesNotThrow(() => player.TickMotionVisuals(motionConfig, 0.016f));

        TheCircussyOneTestObjects.Destroy(motionConfig);
        TheCircussyOneTestObjects.Destroy(performer);
    }

    [Test]
    public void PerformerAnimationSpeedsApplyToIdleAndRunClips()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(TheCircussyOneTestObjects.CreateConfig());
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        var performer = ScriptableObject.CreateInstance<PerformerDefinition>();
        performer.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Animated Performer Prefab");
        performer.animation.idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidIdleClipPath);
        performer.animation.run = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidRunClipPath);
        performer.animation.idleSpeed = 0.5f;
        performer.animation.runSpeed = 1.75f;
        performer.animation.scaleRunSpeedWithMovement = false;
        performer.animation.runSpeedSmoothingSharpness = 0f;
        player.ApplyPerformerVisuals(performer);

        player.SetMotionVisualSpeed(0f, immediate: true);
        player.TickMotionVisuals(motionConfig, 0.016f);

        Assert.That(player.RuntimeAnimationClip, Is.SameAs(performer.animation.idle));
        Assert.That(player.RuntimeAnimationSpeed, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(player.RuntimeAnimationBlend01, Is.EqualTo(0f).Within(0.001f));

        player.SetMotionVisualSpeed(1f, immediate: true);
        player.TickMotionVisuals(motionConfig, 0.016f);

        Assert.That(player.RuntimeAnimationClip, Is.SameAs(performer.animation.run));
        Assert.That(player.RuntimeAnimationSpeed, Is.EqualTo(1.75f).Within(0.001f));
        Assert.That(player.RuntimeAnimationBlend01, Is.EqualTo(1f).Within(0.001f));

        TheCircussyOneTestObjects.Destroy(motionConfig);
        TheCircussyOneTestObjects.Destroy(performer);
    }

    [Test]
    public void PerformerRuntimeAnimationCanPauseAndResume()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(TheCircussyOneTestObjects.CreateConfig());
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        var performer = ScriptableObject.CreateInstance<PerformerDefinition>();
        performer.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Paused Performer Prefab");
        performer.animation.idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidIdleClipPath);
        player.ApplyPerformerVisuals(performer);

        player.TickMotionVisuals(motionConfig, 0.016f);
        player.SetWorldAnimationPaused(true);

        Assert.That(player.RuntimeAnimationPaused, Is.True);

        player.SetWorldAnimationPaused(false);

        Assert.That(player.RuntimeAnimationPaused, Is.False);

        TheCircussyOneTestObjects.Destroy(motionConfig);
        TheCircussyOneTestObjects.Destroy(performer);
    }

    [Test]
    public void PerformerRunAnimationTransitionUsesSmoothedMovementSpeed()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(TheCircussyOneTestObjects.CreateConfig());
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        var performer = ScriptableObject.CreateInstance<PerformerDefinition>();
        performer.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Transition Performer Prefab");
        performer.animation.idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidIdleClipPath);
        performer.animation.run = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidRunClipPath);
        performer.animation.runThreshold01 = 0.5f;
        performer.animation.runExitThreshold01 = 0.2f;
        performer.animation.runSpeedSmoothingSharpness = 1f;
        player.ApplyPerformerVisuals(performer);

        player.SetMotionVisualSpeed(1f, immediate: true);
        player.TickMotionVisuals(motionConfig, 0.016f);

        Assert.That(player.RuntimeAnimationClip, Is.SameAs(performer.animation.idle));
        Assert.That(player.RuntimeAnimationBlend01, Is.GreaterThan(0f));
        Assert.That(player.RuntimeAnimationBlend01, Is.LessThan(0.5f));

        player.TickMotionVisuals(motionConfig, 1f);

        Assert.That(player.RuntimeAnimationClip, Is.SameAs(performer.animation.run));
        Assert.That(player.RuntimeAnimationBlend01, Is.GreaterThan(0.5f));

        TheCircussyOneTestObjects.Destroy(motionConfig);
        TheCircussyOneTestObjects.Destroy(performer);
    }

    [Test]
    public void PerformerRunAnimationBlendDeceleratesTowardIdle()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(TheCircussyOneTestObjects.CreateConfig());
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        var performer = ScriptableObject.CreateInstance<PerformerDefinition>();
        performer.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Run Exit Performer Prefab");
        performer.animation.idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidIdleClipPath);
        performer.animation.run = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidRunClipPath);
        performer.animation.runSpeedSmoothingSharpness = 8f;
        player.ApplyPerformerVisuals(performer);

        player.SetMotionVisualSpeed(1f, immediate: true);
        player.TickMotionVisuals(motionConfig, 1f);
        Assert.That(player.RuntimeAnimationClip, Is.SameAs(performer.animation.run));
        Assert.That(player.RuntimeAnimationBlend01, Is.GreaterThan(0.9f));

        player.SetMotionVisualSpeed(0f, immediate: true);
        player.TickMotionVisuals(motionConfig, 0.016f);
        Assert.That(player.RuntimeAnimationClip, Is.SameAs(performer.animation.run));
        Assert.That(player.RuntimeAnimationBlend01, Is.GreaterThan(0.5f));

        player.TickMotionVisuals(motionConfig, 1f);
        Assert.That(player.RuntimeAnimationClip, Is.SameAs(performer.animation.idle));
        Assert.That(player.RuntimeAnimationBlend01, Is.LessThan(0.5f));

        TheCircussyOneTestObjects.Destroy(motionConfig);
        TheCircussyOneTestObjects.Destroy(performer);
    }

    [Test]
    public void PerformerRunAnimationSpeedScalesWithMovementSpeed()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(TheCircussyOneTestObjects.CreateConfig());
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        var performer = ScriptableObject.CreateInstance<PerformerDefinition>();
        performer.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Scaled Run Performer Prefab");
        performer.animation.idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidIdleClipPath);
        performer.animation.run = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidRunClipPath);
        performer.animation.runSpeed = 2f;
        performer.animation.minRunSpeedMultiplier = 0.5f;
        performer.animation.runSpeedSmoothingSharpness = 0f;
        player.ApplyPerformerVisuals(performer);

        player.SetMotionVisualSpeed(0.75f, immediate: true);
        player.TickMotionVisuals(motionConfig, 0.016f);

        Assert.That(player.RuntimeAnimationClip, Is.SameAs(performer.animation.run));
        Assert.That(player.RuntimeAnimationSpeed, Is.EqualTo(1.75f).Within(0.001f));

        player.SetMotionVisualSpeed(1f, immediate: true);
        player.TickMotionVisuals(motionConfig, 0.016f);

        Assert.That(player.RuntimeAnimationSpeed, Is.EqualTo(2f).Within(0.001f));

        TheCircussyOneTestObjects.Destroy(motionConfig);
        TheCircussyOneTestObjects.Destroy(performer);
    }

    [Test]
    public void PerformerRunAnimationSpeedSmoothsMovementScale()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(TheCircussyOneTestObjects.CreateConfig());
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        var performer = ScriptableObject.CreateInstance<PerformerDefinition>();
        performer.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Smoothed Run Performer Prefab");
        performer.animation.run = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidRunClipPath);
        performer.animation.runSpeed = 2f;
        performer.animation.minRunSpeedMultiplier = 0.5f;
        performer.animation.runSpeedSmoothingSharpness = 4f;
        player.ApplyPerformerVisuals(performer);

        player.SetMotionVisualSpeed(1f, immediate: true);
        player.TickMotionVisuals(motionConfig, 0.1f);
        float firstSpeed = player.RuntimeAnimationSpeed;
        player.TickMotionVisuals(motionConfig, 0.1f);
        float secondSpeed = player.RuntimeAnimationSpeed;

        Assert.That(firstSpeed, Is.GreaterThan(1f));
        Assert.That(firstSpeed, Is.LessThan(2f));
        Assert.That(secondSpeed, Is.GreaterThan(firstSpeed));
        Assert.That(secondSpeed, Is.LessThan(2f));

        TheCircussyOneTestObjects.Destroy(motionConfig);
        TheCircussyOneTestObjects.Destroy(performer);
    }

    [Test]
    public void PerformerAnimationSpeedsApplyToJumpClip()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(TheCircussyOneTestObjects.CreateConfig());
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        var performer = ScriptableObject.CreateInstance<PerformerDefinition>();
        performer.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Jump Performer Prefab");
        performer.animation.idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidIdleClipPath);
        performer.animation.jump = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidRunClipPath);
        performer.animation.jumpSpeed = 1.35f;
        player.ApplyPerformerVisuals(performer);
        player.SetJumpHoldVisual(jumpHeld: true, rising: true, airborne: true);
        player.SetJumpAnimationVisual(true);

        player.TickMotionVisuals(motionConfig, 0.016f);

        Assert.That(player.RuntimeAnimationClip, Is.SameAs(performer.animation.jump));
        Assert.That(player.RuntimeAnimationSpeed, Is.EqualTo(1.35f).Within(0.001f));

        TheCircussyOneTestObjects.Destroy(motionConfig);
        TheCircussyOneTestObjects.Destroy(performer);
    }

    [Test]
    public void BriefAirborneGroundProbeLossKeepsPerformerLocomotionAnimation()
    {
        var config = TheCircussyOneTestObjects.CreateConfig();
        config.playerGroundProbeDistance = 0f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(config);
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        var performer = ScriptableObject.CreateInstance<PerformerDefinition>();
        performer.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Ramp Descent Performer Prefab");
        performer.animation.idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidIdleClipPath);
        performer.animation.run = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidRunClipPath);
        performer.animation.jump = performer.animation.idle;
        performer.animation.runSpeedSmoothingSharpness = 0f;
        player.ApplyPerformerVisuals(performer);

        player.Move(Vector3.zero, Vector3.forward, 0.016f);
        player.SetMotionVisualSpeed(1f, immediate: true);
        player.SetJumpHoldVisual(jumpHeld: false, rising: false, airborne: true);
        player.SetJumpAnimationVisual(false);
        player.TickMotionVisuals(motionConfig, 0.016f);

        Assert.That(player.RuntimeAnimationClip, Is.SameAs(performer.animation.run));
        Assert.That(player.RuntimeAnimationClip, Is.Not.SameAs(performer.animation.jump));

        TheCircussyOneTestObjects.Destroy(motionConfig);
        TheCircussyOneTestObjects.Destroy(performer);
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

    private static ParticleSystem AddParticleChild(Transform parent, string name)
    {
        var child = new GameObject(name);
        child.transform.SetParent(parent, false);
        return child.AddComponent<ParticleSystem>();
    }

    private static Renderer AddFaceRenderer(Transform body)
    {
        GameObject facePart = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        facePart.name = "Face Test Eye";
        facePart.transform.SetParent(body, false);
        Object.DestroyImmediate(facePart.GetComponent<Collider>());
        return facePart.GetComponent<Renderer>();
    }

    private static void CreateGroundCollider()
    {
        GameObject ground = TheCircussyOneTestObjects.CreateRoot("Ground");
        var collider = ground.AddComponent<BoxCollider>();
        collider.center = new Vector3(0f, -0.05f, 0f);
        collider.size = new Vector3(20f, 0.1f, 20f);
        Physics.SyncTransforms();
    }

    private static void CreateSlopedGround(float angleDegrees)
    {
        GameObject slope = TheCircussyOneTestObjects.CreateRoot("Steep Ground");
        var collider = slope.AddComponent<BoxCollider>();
        collider.size = new Vector3(20f, 0.1f, 20f);
        slope.transform.rotation = Quaternion.AngleAxis(angleDegrees, Vector3.right);
        Vector3 normal = slope.transform.TransformDirection(Vector3.up);
        slope.transform.position = -normal.normalized * 0.05f;
        Physics.SyncTransforms();
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
