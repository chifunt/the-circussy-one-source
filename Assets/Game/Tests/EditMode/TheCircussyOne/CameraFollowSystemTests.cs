using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

public sealed class CameraFollowSystemTests
{
    private GameObject _wall;
    private GameObject _cameraObject;
    private CameraConfig _cameraConfig;
    private GameConfig _gameConfig;

    [TearDown]
    public void TearDown()
    {
        if (_wall != null)
        {
            Object.DestroyImmediate(_wall);
        }

        if (_cameraObject != null)
        {
            Object.DestroyImmediate(_cameraObject);
        }

        TheCircussyOneTestObjects.Destroy(_cameraConfig);
        TheCircussyOneTestObjects.Destroy(_gameConfig);
        TheCircussyOneTestObjects.DestroyAll();
    }

    [Test]
    public void ObstructedCameraAppliesResolvedPositionWithoutSmoothingThroughWall()
    {
        _gameConfig = TheCircussyOneTestObjects.CreateConfig();
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_gameConfig);
        player.Teleport(Vector3.zero);
        _cameraConfig = TheCircussyOneTestObjects.CreateCameraConfig();
        _cameraConfig.followSharpness = 0.01f;
        _cameraConfig.orbitDistance = 10f;
        _cameraConfig.orbitYaw = 0f;
        _cameraConfig.orbitPitch = 0f;
        _cameraConfig.minPitch = -30f;
        _cameraConfig.maxPitch = 60f;
        _cameraConfig.targetHeight = 1f;
        _cameraConfig.lookAheadDistance = 0f;
        _cameraConfig.topDownLookAheadDistance = 0f;
        _cameraConfig.preventCameraClipping = true;
        _cameraConfig.cameraObstructionMask = 1;
        _cameraConfig.cameraObstructionRadius = 0.1f;
        _cameraConfig.cameraObstructionPadding = 0.05f;

        _wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _wall.name = "Camera Obstruction Wall";
        _wall.layer = 0;
        _wall.transform.position = new Vector3(0f, 1f, -4f);
        _wall.transform.localScale = new Vector3(8f, 4f, 1f);

        _cameraObject = new GameObject("Camera Follow Test Camera");
        _cameraObject.transform.position = new Vector3(0f, 1f, -4f);
        _cameraObject.AddComponent<Camera>();
        CameraView cameraView = _cameraObject.AddComponent<CameraView>();
        Physics.SyncTransforms();

        var system = new CameraFollowSystem(
            _cameraConfig,
            player,
            cameraView,
            new FakeGameTime { DeltaTime = 0.016f },
            new FakeInputService(),
            new CameraOrbitState());

        system.LateTick();

        Assert.That(cameraView.transform.position.z, Is.GreaterThan(-3.5f));
    }

    [Test]
    public void CameraAlreadyInsideWallSnapsToClearDesiredPose()
    {
        _gameConfig = TheCircussyOneTestObjects.CreateConfig();
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_gameConfig);
        player.Teleport(Vector3.zero);
        _cameraConfig = TheCircussyOneTestObjects.CreateCameraConfig();
        _cameraConfig.followSharpness = 0.01f;
        _cameraConfig.orbitDistance = 10f;
        _cameraConfig.orbitYaw = 0f;
        _cameraConfig.orbitPitch = 0f;
        _cameraConfig.minPitch = -30f;
        _cameraConfig.maxPitch = 60f;
        _cameraConfig.targetHeight = 1f;
        _cameraConfig.lookAheadDistance = 0f;
        _cameraConfig.topDownLookAheadDistance = 0f;
        _cameraConfig.preventCameraClipping = true;
        _cameraConfig.cameraObstructionMask = 1;
        _cameraConfig.cameraObstructionRadius = 0.1f;
        _cameraConfig.cameraObstructionPadding = 0.05f;

        _wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _wall.name = "Camera Current Position Wall";
        _wall.layer = 0;
        _wall.transform.position = new Vector3(3f, 1f, 0f);
        _wall.transform.localScale = new Vector3(1f, 4f, 8f);

        _cameraObject = new GameObject("Camera Follow Test Camera");
        _cameraObject.transform.position = new Vector3(3f, 1f, 0f);
        _cameraObject.AddComponent<Camera>();
        CameraView cameraView = _cameraObject.AddComponent<CameraView>();
        Physics.SyncTransforms();

        var system = new CameraFollowSystem(
            _cameraConfig,
            player,
            cameraView,
            new FakeGameTime { DeltaTime = 0.016f },
            new FakeInputService(),
            new CameraOrbitState());

        system.LateTick();

        Assert.That(cameraView.transform.position.x, Is.EqualTo(0f).Within(0.01f));
        Assert.That(cameraView.transform.position.z, Is.EqualTo(-10f).Within(0.01f));
    }

    [Test]
    public void TriggerOrbitAxisChangesCameraYaw()
    {
        _gameConfig = TheCircussyOneTestObjects.CreateConfig();
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_gameConfig);
        player.Teleport(Vector3.zero);
        _cameraConfig = TheCircussyOneTestObjects.CreateCameraConfig();
        _cameraConfig.orbitYaw = 0f;
        _cameraConfig.orbitPitch = 36f;
        _cameraConfig.preventCameraClipping = false;
        _cameraConfig.gamepadTriggerYawSpeed = 130f;
        _cameraConfig.gamepadTriggerDeadzone = 0.08f;

        _cameraObject = new GameObject("Camera Trigger Orbit Test Camera");
        _cameraObject.AddComponent<Camera>();
        CameraView cameraView = _cameraObject.AddComponent<CameraView>();
        var orbit = new CameraOrbitState();

        var system = new CameraFollowSystem(
            _cameraConfig,
            player,
            cameraView,
            new FakeGameTime { DeltaTime = 0.5f },
            new FakeInputService { CameraOrbitTriggerAxis = 1f },
            orbit);

        system.LateTick();

        Assert.That(orbit.Yaw, Is.EqualTo(65f).Within(0.001f));
        Assert.That(orbit.Pitch, Is.EqualTo(36f).Within(0.001f));
    }
}
