using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

public sealed class XpGainCounterViewTests
{
    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
    }

    [Test]
    public void ViewAppliesTextColorScaleAndBillboardRotation()
    {
        XpGainCounterVisualConfig config = TheCircussyOneTestObjects.CreateXpGainCounterConfig();
        XpGainCounterView view = TheCircussyOneTestObjects.CreateXpGainCounterPrefab();
        CameraView cameraView = CreateCameraView(new Vector3(0f, 2f, -5f));
        var frame = new XpGainCounterFrame("+7", new Vector3(1f, 1.5f, 0f), 0.9f, config.textColor, true);

        view.Prepare(config);
        view.ApplyFrame(frame, cameraView.Camera);

        Assert.That(view.Text, Is.EqualTo("+7"));
        Assert.That(view.TextColor, Is.EqualTo(config.textColor));
        Assert.That(view.transform.position, Is.EqualTo(frame.Position));
        Assert.That(view.transform.localScale.x, Is.EqualTo(0.9f).Within(0.001f));
        Assert.That(Vector3.Dot(view.transform.forward, (frame.Position - cameraView.transform.position).normalized), Is.GreaterThan(0.99f));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void SystemAggregatesThenHidesAndResetsAfterFade()
    {
        GameConfig gameConfig = TheCircussyOneTestObjects.CreateConfig();
        XpGainCounterVisualConfig config = TheCircussyOneTestObjects.CreateXpGainCounterConfig();
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(gameConfig);
        CameraView cameraView = CreateCameraView(new Vector3(0f, 5f, -8f));
        XpGainCounterView prefab = TheCircussyOneTestObjects.CreateXpGainCounterPrefab();
        Transform root = TheCircussyOneTestObjects.CreateRoot("XP Gain Counter Root").transform;
        var time = new FakeGameTime { Time = 0f };
        var factory = new XpGainCounterFactory(config, prefab, root);
        var system = new XpGainCounterSystem(config, player, cameraView, time, factory);

        system.Add(3);
        Assert.That(system.Amount, Is.EqualTo(3));
        system.LateTick();
        Assert.That(system.IsVisible, Is.True);

        time.Time = 0.35f;
        system.Add(4);
        system.LateTick();
        Assert.That(system.Amount, Is.EqualTo(7));
        Assert.That(root.GetComponentInChildren<XpGainCounterView>().Text, Is.EqualTo("+7"));

        time.Time = 1.86f;
        system.LateTick();
        Assert.That(system.Amount, Is.Zero);
        Assert.That(system.IsVisible, Is.False);

        Object.DestroyImmediate(gameConfig);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void TicketGainCounterShowsPositiveTicketDeltasOnPlayerTopLeft()
    {
        GameConfig gameConfig = TheCircussyOneTestObjects.CreateConfig();
        XpGainCounterVisualConfig config = TheCircussyOneTestObjects.CreateXpGainCounterConfig();
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(gameConfig);
        CameraView cameraView = CreateCameraView(new Vector3(0f, 5f, -8f));
        XpGainCounterView prefab = TheCircussyOneTestObjects.CreateXpGainCounterPrefab();
        Transform root = TheCircussyOneTestObjects.CreateRoot("Ticket Gain Counter Root").transform;
        var time = new FakeGameTime { Time = 0f };
        var currency = new RunCurrencyState();
        var factory = new XpGainCounterFactory(config, prefab, root, "Ticket Gain Counter");
        var system = new TicketGainCounterSystem(config, currency, player, cameraView, time, factory);

        system.Start();
        currency.AddTickets(6);
        system.LateTick();

        XpGainCounterView view = root.GetComponentInChildren<XpGainCounterView>();
        Assert.That(system.Amount, Is.EqualTo(6));
        Assert.That(system.IsVisible, Is.True);
        Assert.That(view, Is.Not.Null);
        Assert.That(view.name, Is.EqualTo("Ticket Gain Counter"));
        Assert.That(view.Text, Is.EqualTo("+6"));
        Assert.That(view.TextColor, Is.EqualTo(config.ticketTextColor));
        Assert.That(view.transform.position.x, Is.LessThan(player.Position.x));

        currency.TrySpendTickets(2);
        system.LateTick();
        Assert.That(system.Amount, Is.EqualTo(6));
        Assert.That(view.Text, Is.EqualTo("+6"));

        system.Dispose();
        Object.DestroyImmediate(gameConfig);
        Object.DestroyImmediate(config);
    }

    private static CameraView CreateCameraView(Vector3 position)
    {
        GameObject cameraObject = new("XP Counter Test Camera");
        cameraObject.transform.position = position;
        cameraObject.transform.rotation = Quaternion.LookRotation(Vector3.zero - position, Vector3.up);
        cameraObject.AddComponent<Camera>();
        return cameraObject.AddComponent<CameraView>();
    }
}
