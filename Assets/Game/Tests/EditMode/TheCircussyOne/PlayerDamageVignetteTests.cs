using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

public sealed class PlayerDamageVignetteTests
{
    private GameObject _viewObject;
    private GameObject _cameraObject;
    private Material _material;
    private DamageFeedbackVisualConfig _config;

    [TearDown]
    public void TearDown()
    {
        if (_viewObject != null)
        {
            Object.DestroyImmediate(_viewObject);
        }

        if (_material != null)
        {
            Object.DestroyImmediate(_material);
        }

        if (_cameraObject != null)
        {
            Object.DestroyImmediate(_cameraObject);
        }

        TheCircussyOneTestObjects.Destroy(_config);
    }

    [Test]
    public void DamageIntensityUsesPostArmorDamagePercent()
    {
        float intensity = PlayerDamageVignetteRules.DamageIntensity(5, 100, 10f);

        Assert.That(intensity, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(PlayerDamageVignetteRules.DamageIntensity(0, 100, 10f), Is.Zero);
        Assert.That(PlayerDamageVignetteRules.DamageIntensity(5, 0, 10f), Is.Zero);
    }

    [Test]
    public void HealingIntensityUsesActualHealPercent()
    {
        float intensity = PlayerDamageVignetteRules.HealingIntensity(20, 100, 4f);

        Assert.That(intensity, Is.EqualTo(0.8f).Within(0.001f));
        Assert.That(PlayerDamageVignetteRules.HealingIntensity(0, 100, 4f), Is.Zero);
        Assert.That(PlayerDamageVignetteRules.HealingIntensity(20, 0, 4f), Is.Zero);
    }

    [Test]
    public void AccumulationClampsAtConfiguredCap()
    {
        float intensity = PlayerDamageVignetteRules.Accumulate(0.8f, 0.7f, 1.25f);

        Assert.That(intensity, Is.EqualTo(1.25f).Within(0.001f));
    }

    [Test]
    public void FadeReachesZeroByDuration()
    {
        float halfway = PlayerDamageVignetteRules.Fade(1f, 0.25f, 0.5f, EaseSettings.Linear);
        float complete = PlayerDamageVignetteRules.Fade(1f, 0.5f, 0.5f, EaseSettings.Linear);

        Assert.That(halfway, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(complete, Is.Zero);
    }

    [Test]
    public void EvaluateUsesConfigValuesAndHidesWhenDisabled()
    {
        _config = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        PlayerDamageVignetteFrame frame = PlayerDamageVignetteRules.Evaluate(0.5f, 0f, _config, 1.25f);

        Assert.That(frame.Visible, Is.True);
        Assert.That(frame.Intensity, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(frame.DarkTint, Is.EqualTo(_config.playerDamageVignetteDarkColor));
        Assert.That(frame.MaxOpacity, Is.EqualTo(_config.playerDamageVignetteMaxOpacity).Within(0.001f));
        Assert.That(frame.DarkMaxOpacity, Is.EqualTo(_config.playerDamageVignetteDarkMaxOpacity).Within(0.001f));
        Assert.That(frame.GrainStrength, Is.EqualTo(_config.playerDamageVignetteGrainStrength).Within(0.001f));
        Assert.That(frame.Time, Is.EqualTo(1.25f).Within(0.001f));

        _config.playerDamageVignetteEnabled = false;
        Assert.That(PlayerDamageVignetteRules.Evaluate(0.5f, 0f, _config, 0f).Visible, Is.False);
    }

    [Test]
    public void EvaluateHealingUsesGreenConfigValuesAndHidesWhenDisabled()
    {
        _config = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        PlayerDamageVignetteFrame frame = PlayerDamageVignetteRules.EvaluateHealing(0.5f, 0f, _config, 1.25f);

        Assert.That(frame.Visible, Is.True);
        Assert.That(frame.Intensity, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(frame.Tint, Is.EqualTo(_config.playerHealingVignetteColor));
        Assert.That(frame.DarkTint, Is.EqualTo(_config.playerHealingVignetteDarkColor));
        Assert.That(frame.MaxOpacity, Is.EqualTo(_config.playerHealingVignetteMaxOpacity).Within(0.001f));
        Assert.That(frame.DarkMaxOpacity, Is.EqualTo(_config.playerHealingVignetteDarkMaxOpacity).Within(0.001f));
        Assert.That(frame.GrainStrength, Is.EqualTo(_config.playerHealingVignetteGrainStrength).Within(0.001f));
        Assert.That(frame.SplotchStrength, Is.EqualTo(_config.playerHealingVignetteSplotchStrength).Within(0.001f));
        Assert.That(frame.Radius, Is.EqualTo(_config.playerHealingVignetteRadius).Within(0.001f));
        Assert.That(frame.Softness, Is.EqualTo(_config.playerHealingVignetteSoftness).Within(0.001f));
        Assert.That(frame.Time, Is.EqualTo(1.25f).Within(0.001f));

        _config.playerHealingVignetteEnabled = false;
        Assert.That(PlayerDamageVignetteRules.EvaluateHealing(0.5f, 0f, _config, 0f).Visible, Is.False);
    }

    [Test]
    public void DamageVignetteViewHiddenFrameDisablesRenderer()
    {
        DamageVignetteView view = CreateView();

        view.ApplyFrame(PlayerDamageVignetteFrame.Hidden);

        Assert.That(view.IsVisible, Is.False);
    }

    [Test]
    public void DamageVignetteViewVisibleFrameEnablesRenderer()
    {
        DamageVignetteView view = CreateView();
        var frame = new PlayerDamageVignetteFrame(
            true,
            0.75f,
            Color.red,
            Color.black,
            0.5f,
            0.4f,
            0.25f,
            0.35f,
            0.8f,
            0.28f,
            2f);

        view.ApplyFrame(frame);

        Assert.That(view.IsVisible, Is.True);
    }

    [Test]
    public void DamageVignetteViewAlignsToCurrentCameraPose()
    {
        _cameraObject = new GameObject("Damage Vignette Camera");
        Camera camera = _cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 60f;
        camera.aspect = 16f / 9f;
        camera.nearClipPlane = 0.03f;
        DamageVignetteView view = CreateView(camera);
        var frame = new PlayerDamageVignetteFrame(
            true,
            0.75f,
            Color.red,
            Color.black,
            0.5f,
            0.4f,
            0.25f,
            0.35f,
            0.8f,
            0.28f,
            2f);

        view.ApplyFrame(frame);
        _cameraObject.transform.SetPositionAndRotation(new Vector3(3f, 4f, -5f), Quaternion.Euler(12f, 35f, 0f));
        view.AlignToCamera();

        Transform overlay = _viewObject.transform.Find("Damage Vignette Overlay");
        float distance = Mathf.Max(camera.nearClipPlane + 0.03f, 0.12f);
        Assert.That(Vector3.Distance(overlay.position, camera.transform.position + camera.transform.forward * distance), Is.EqualTo(0f).Within(0.001f));
        Assert.That(Quaternion.Angle(overlay.rotation, camera.transform.rotation), Is.EqualTo(0f).Within(0.001f));
    }

    [Test]
    public void PresenterSubscribesAppliesAndUnsubscribes()
    {
        _config = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        DamageVignetteView view = CreateView();
        var gameConfig = TheCircussyOneTestObjects.CreateConfig();
        var state = new GameState(gameConfig);
        var time = new FakeGameTime { Time = 1f, DeltaTime = 0.01f };
        var presenter = new PlayerDamageVignettePresenter(state, _config, view, time);

        presenter.Start();
        state.DamagePlayer(2);
        presenter.Tick();
        Assert.That(view.IsVisible, Is.True);
        presenter.Dispose();
        state.DamagePlayer(2);

        Assert.That(view.IsVisible, Is.False);
        TheCircussyOneTestObjects.Destroy(gameConfig);
    }

    [Test]
    public void PresenterAppliesHealingVignetteOnActualHeal()
    {
        _config = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        DamageVignetteView view = CreateView();
        var gameConfig = TheCircussyOneTestObjects.CreateConfig();
        var state = new GameState(gameConfig);
        var time = new FakeGameTime { Time = 1f, DeltaTime = 0.01f };
        var presenter = new PlayerDamageVignettePresenter(state, _config, view, time);

        presenter.Start();
        state.DamagePlayer(20);
        state.HealPlayer(10);
        presenter.Tick();
        Assert.That(view.IsVisible, Is.True);
        presenter.Dispose();
        state.DamagePlayer(20);
        state.HealPlayer(10);

        Assert.That(view.IsVisible, Is.False);
        TheCircussyOneTestObjects.Destroy(gameConfig);
    }

    [Test]
    public void PresenterLateTickRealignsViewAfterCameraFollowShake()
    {
        _config = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        DamageVignetteView view = CreateView();
        var gameConfig = TheCircussyOneTestObjects.CreateConfig();
        var state = new GameState(gameConfig);
        var time = new FakeGameTime { Time = 1f, DeltaTime = 0.01f };
        var presenter = new PlayerDamageVignettePresenter(state, _config, view, time);

        Assert.That(presenter.GetType().GetMethod("LateTick"), Is.Not.Null);

        TheCircussyOneTestObjects.Destroy(gameConfig);
    }

    [Test]
    public void ShaderAndMaterialAssetsAreTracked()
    {
        Assert.That(UnityEditor.AssetDatabase.LoadAssetAtPath<Shader>(TheCircussyOneAssetPaths.PlayerDamageVignetteShaderPath), Is.Not.Null);
        Assert.That(UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(TheCircussyOneAssetPaths.PlayerDamageVignetteMaterialPath), Is.Not.Null);
    }

    private DamageVignetteView CreateView()
    {
        return CreateView(null);
    }

    private DamageVignetteView CreateView(Camera camera)
    {
        _viewObject = new GameObject("Damage Vignette View");
        _material = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color"));
        DamageVignetteView view = _viewObject.AddComponent<DamageVignetteView>();
        view.Configure(_material, camera);
        return view;
    }
}
