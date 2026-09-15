using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;
using UnityEngine;

public sealed class WorldPropPlacementTests
{
    private const int TestSurfaceLayer = 30;
    private const int TestSurfaceMask = 1 << TestSurfaceLayer;

    [SetUp]
    public void SetUp()
    {
        PrototypeArenaVisibility.SetVisible(true);
        TheCircussyOneTestObjects.DestroyAll();
        SceneInteractableRegistry.Clear();
        InteractableSelectionOutlineRegistry.Clear();
    }

    [TearDown]
    public void TearDown()
    {
        PrototypeArenaVisibility.SetVisible(true);
        TheCircussyOneTestObjects.DestroyAll();
        SceneInteractableRegistry.Clear();
        InteractableSelectionOutlineRegistry.Clear();
    }

    [Test]
    public void PlacementGroundsBlockingPropOnRaisedSurface()
    {
        CreateSurface("TCO Test Raised Ground", new Vector3(0f, 0.5f, 0f), new Vector3(8f, 1f, 8f));
        Physics.SyncTransforms();
        var service = TestPlacementService();
        WorldPropPlacementProfile profile = WorldPropPlacementProfile.ChestDefault();

        bool placed = service.TryPlace(new Vector3(0f, 0f, 0f), profile, out WorldPropPlacementResult result);

        Assert.That(placed, Is.True);
        Assert.That(result.FoundSurface, Is.True);
        Assert.That(result.Position.y, Is.EqualTo(1f + profile.RootHeightOffset).Within(0.001f));
        Assert.That(result.GroundNormal, Is.EqualTo(Vector3.up));
    }

    [Test]
    public void PlacementOffsetsRootAlongSlopeNormal()
    {
        Vector3 expectedNormal = CreateSlopedSurface(30f, Vector3.zero, new Vector3(12f, 0.1f, 12f));
        var service = TestPlacementService();
        WorldPropPlacementProfile profile = WorldPropPlacementProfile.ChestDefault();

        bool placed = service.TryPlace(Vector3.zero, profile, out WorldPropPlacementResult result);

        Assert.That(placed, Is.True);
        Assert.That(Vector3.Angle(result.Position.normalized, expectedNormal), Is.LessThan(1f));
        Assert.That(result.Position.magnitude, Is.EqualTo(profile.RootHeightOffset).Within(0.001f));
    }

    [Test]
    public void PlacementRotationAlignsToSlopeNormal()
    {
        Vector3 expectedNormal = CreateSlopedSurface(30f, Vector3.zero, new Vector3(12f, 0.1f, 12f));
        var service = TestPlacementService();
        WorldPropPlacementProfile profile = WorldPropPlacementProfile.ChestDefault();

        bool placed = service.TryPlace(Vector3.zero, profile, out WorldPropPlacementResult result);

        Assert.That(placed, Is.True);
        Assert.That(Vector3.Angle(result.GroundNormal, expectedNormal), Is.LessThan(1f));
        Assert.That(Vector3.Angle(result.Rotation * Vector3.up, expectedNormal), Is.LessThan(1f));
    }

    [Test]
    public void TicketDepositVisualScaleKeepsBottomAnchoredToRamp()
    {
        Vector3 expectedNormal = CreateSlopedSurface(30f, Vector3.zero, new Vector3(12f, 0.1f, 12f));
        var service = TestPlacementService();
        WorldPropPlacementProfile profile = WorldPropPlacementProfile.TicketDepositDefault(TicketDepositTier.Small);
        TicketDepositDefinition deposit = TheCircussyOneTestObjects.CreateTicketDepositDefinition(
            "small_ticket_stack",
            "TCO Test Small Ticket Stack");
        deposit.visualScale = 0.75f;

        bool placed = service.TryPlace(Vector3.zero, profile, out WorldPropPlacementResult result);

        Assert.That(placed, Is.True);

        GameObject root = new("TCO Test Scaled Ticket Deposit");
        root.name = "TCO Test Scaled Ticket Deposit";
        root.transform.SetPositionAndRotation(result.Position, result.Rotation);

        GameObject visualRoot = new(VisualGroundAnchorUtility.VisualRootName);
        visualRoot.transform.SetParent(root.transform, worldPositionStays: false);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Ticket Visual";
        body.transform.SetParent(visualRoot.transform, worldPositionStays: false);
        body.transform.localScale = profile.bodyColliderSize;

        var view = root.AddComponent<TicketDepositView>();
        view.Prepare(deposit);

        float expectedNormalShift = profile.bodyColliderSize.y * 0.5f * (deposit.visualScale - 1f);
        float actualNormalShift = Vector3.Dot(visualRoot.transform.position - result.Position, expectedNormal);
        Assert.That(root.transform.position, Is.EqualTo(result.Position));
        Assert.That(actualNormalShift, Is.EqualTo(expectedNormalShift).Within(0.001f));

        Object.DestroyImmediate(deposit);
    }

    [Test]
    public void TicketDepositTargetScaleExpandsFromRampContact()
    {
        Vector3 expectedNormal = CreateSlopedSurface(30f, Vector3.zero, new Vector3(12f, 0.1f, 12f));
        var service = TestPlacementService();
        WorldPropPlacementProfile profile = WorldPropPlacementProfile.TicketDepositDefault(TicketDepositTier.Small);
        TicketDepositDefinition deposit = TheCircussyOneTestObjects.CreateTicketDepositDefinition(
            "small_ticket_stack",
            "TCO Test Small Ticket Stack");
        deposit.visualScale = 1f;

        bool placed = service.TryPlace(Vector3.zero, profile, out WorldPropPlacementResult result);

        Assert.That(placed, Is.True);

        GameObject root = new("TCO Test Targeted Ticket Deposit");
        root.transform.SetPositionAndRotation(result.Position, result.Rotation);

        GameObject visualRoot = new(VisualGroundAnchorUtility.VisualRootName);
        visualRoot.transform.SetParent(root.transform, worldPositionStays: false);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Ticket Visual";
        body.transform.SetParent(visualRoot.transform, worldPositionStays: false);
        body.transform.localScale = profile.bodyColliderSize;

        var view = root.AddComponent<TicketDepositView>();
        view.Prepare(deposit);
        view.SetInteractionTargeted(true);

        const float TargetedScaleMultiplier = 1.12f;
        float expectedNormalShift = profile.bodyColliderSize.y * 0.5f * (TargetedScaleMultiplier - 1f);
        float actualNormalShift = Vector3.Dot(visualRoot.transform.position - result.Position, expectedNormal);
        Assert.That(root.transform.position, Is.EqualTo(result.Position));
        Assert.That(actualNormalShift, Is.EqualTo(expectedNormalShift).Within(0.001f));

        Object.DestroyImmediate(deposit);
    }

    [Test]
    public void PlacementAvoidsEnvironmentBodyOverlap()
    {
        CreateSurface("TCO Test Ground", new Vector3(0f, -0.05f, 0f), new Vector3(8f, 0.1f, 8f));
        CreateSurface("TCO Test Wall", new Vector3(0.35f, 0.75f, 0f), new Vector3(0.45f, 1.5f, 2f));
        Physics.SyncTransforms();
        var service = TestPlacementService();
        WorldPropPlacementProfile profile = WorldPropPlacementProfile.ChestDefault();

        bool placed = service.TryPlace(Vector3.zero, profile, out WorldPropPlacementResult result);

        Assert.That(placed, Is.True);
        Assert.That(result.Attempt, Is.GreaterThan(0));
        Assert.That(Mathf.Abs(result.Position.x), Is.GreaterThan(0.2f));
    }

    [Test]
    public void PlacementAvoidsEnvironmentOverlapUsingFootprintForSmallVisualBodies()
    {
        CreateSurface("TCO Test Ground", new Vector3(0f, -0.05f, 0f), new Vector3(8f, 0.1f, 8f));
        CreateSurface("TCO Test Low Wall", new Vector3(0.65f, 0.2f, 0f), new Vector3(0.35f, 0.4f, 2f));
        Physics.SyncTransforms();
        var service = TestPlacementService();
        WorldPropPlacementProfile profile = WorldPropPlacementProfile.Create(
            footprintRadius: 1f,
            groundClearance: 0.005f,
            maxPlacementSlope: 65f,
            blocksPlayer: false,
            bodyColliderSize: new Vector3(0.05f, 0.05f, 0.05f));

        bool placed = service.TryPlace(Vector3.zero, profile, out WorldPropPlacementResult result);

        Assert.That(placed, Is.True);
        Assert.That(result.Attempt, Is.GreaterThan(0));
        Assert.That(Mathf.Abs(result.Position.x), Is.GreaterThan(0.25f));
    }

    [Test]
    public void PlacementAvoidsExistingPropFootprints()
    {
        CreateSurface("TCO Test Ground", new Vector3(0f, -0.05f, 0f), new Vector3(8f, 0.1f, 8f));
        Physics.SyncTransforms();
        var service = TestPlacementService();
        WorldPropPlacementProfile profile = WorldPropPlacementProfile.ChestDefault();

        service.TryPlace(Vector3.zero, profile, out WorldPropPlacementResult first);
        service.TryPlace(Vector3.zero, profile, out WorldPropPlacementResult second);

        float horizontalDistance = Vector2.Distance(
            new Vector2(first.Position.x, first.Position.z),
            new Vector2(second.Position.x, second.Position.z));
        Assert.That(second.Attempt, Is.GreaterThan(0));
        Assert.That(horizontalDistance, Is.GreaterThanOrEqualTo(profile.footprintRadius * 2f - 0.01f));
    }

    [Test]
    public void ColliderDriverEnablesBlockingBoxOnPickupLayer()
    {
        GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        root.name = "TCO Test Blocking Prop";
        WorldPropPlacementProfile profile = WorldPropPlacementProfile.ChestDefault();

        WorldPropColliderDriver.Apply(root, profile);

        BoxCollider collider = root.GetComponent<BoxCollider>();
        Assert.That(collider, Is.Not.Null);
        Assert.That(collider.enabled, Is.True);
        Assert.That(collider.isTrigger, Is.False);
        Assert.That(root.layer, Is.EqualTo(GameLayers.PickupIndex));
        Assert.That((GameLayers.EnvironmentMaskExcludingGameplay & (1 << root.layer)), Is.Zero);
        Assert.That(collider.size.x * root.transform.lossyScale.x, Is.EqualTo(profile.bodyColliderSize.x).Within(0.001f));
    }

    [Test]
    public void ColliderDriverDisablesNonBlockingColliders()
    {
        GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        root.name = "TCO Test Non Blocking Prop";
        WorldPropPlacementProfile profile = WorldPropPlacementProfile.HealingPropDefault();

        WorldPropColliderDriver.Apply(root, profile);

        Assert.That(root.GetComponentsInChildren<Collider>().All(collider => !collider.enabled), Is.True);
        Assert.That(root.layer, Is.EqualTo(GameLayers.PickupIndex));
    }

    [Test]
    public void RewardPropBuilderCreatesGroundedVisualBodyAndCollider()
    {
        CreateSurface("TCO Test Ground", new Vector3(0f, -0.05f, 0f), new Vector3(8f, 0.1f, 8f));
        Physics.SyncTransforms();
        WorldPropPlacementProfile profile = WorldPropPlacementProfile.ChestDefault();
        Material material = new(Shader.Find("Standard"));

        WorldRewardPropBuildResult result = WorldRewardPropBuilder.Create(
            "TCO Test Reward Prop",
            Vector3.zero,
            PrimitiveType.Cube,
            "Reward Body",
            profile,
            TestPlacementService(),
            material);

        Assert.That(result.Root.name, Is.EqualTo("TCO Test Reward Prop"));
        Assert.That(result.VisualRoot.name, Is.EqualTo(VisualGroundAnchorUtility.VisualRootName));
        Assert.That(result.Body.name, Is.EqualTo("Reward Body"));
        Assert.That(result.Body.transform.localScale, Is.EqualTo(profile.bodyColliderSize));
        Assert.That(result.BodyRenderer.sharedMaterial, Is.EqualTo(material));
        Assert.That(result.Root.transform.position, Is.EqualTo(result.Placement.Position));
        Assert.That(result.Root.transform.position.y, Is.EqualTo(profile.RootHeightOffset).Within(0.001f));
        Assert.That(result.Root.transform.rotation, Is.EqualTo(result.Placement.Rotation));
        Assert.That(result.Root.GetComponent<BoxCollider>(), Is.Not.Null);

        Object.DestroyImmediate(material);
    }

    [Test]
    public void RewardPropBuilderAppliesYawAroundSurfaceNormal()
    {
        CreateSurface("TCO Test Ground", new Vector3(0f, -0.05f, 0f), new Vector3(8f, 0.1f, 8f));
        Physics.SyncTransforms();
        WorldPropPlacementProfile profile = WorldPropPlacementProfile.ChestDefault();

        WorldRewardPropBuildResult result = WorldRewardPropBuilder.Create(
            "TCO Test Yawed Reward Prop",
            Vector3.zero,
            PrimitiveType.Cube,
            "Reward Body",
            profile,
            TestPlacementService(),
            null,
            yawDegrees: 90f);

        Assert.That(Vector3.Angle(result.Root.transform.up, Vector3.up), Is.LessThan(0.001f));
        Assert.That(Vector3.Angle(result.Root.transform.forward, Vector3.right), Is.LessThan(0.001f));
        Assert.That(Quaternion.Angle(result.Placement.Rotation, result.Root.transform.rotation), Is.LessThan(0.001f));
    }

    [Test]
    public void RewardPropBuilderCreatesMeshColliderForModelVisuals()
    {
        CreateSurface("TCO Test Ground", new Vector3(0f, -0.05f, 0f), new Vector3(8f, 0.1f, 8f));
        Physics.SyncTransforms();
        GameObject visualPrefab = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visualPrefab.name = "TCO Test Ticket Mesh";
        WorldPropPlacementProfile profile = WorldPropPlacementProfile.TicketDepositDefault(TicketDepositTier.Small);

        WorldRewardPropBuildResult result = WorldRewardPropBuilder.Create(
            "TCO Test Model Reward Prop",
            Vector3.zero,
            visualPrefab,
            0.25f,
            profile,
            TestPlacementService(),
            null,
            PrimitiveType.Cube,
            "Ticket Visual",
            WorldRewardPropColliderMode.Mesh);

        MeshCollider meshCollider = result.Root.GetComponentInChildren<MeshCollider>();
        Assert.That(result.Body.transform.localScale, Is.EqualTo(Vector3.one * 0.25f));
        Assert.That(meshCollider, Is.Not.Null);
        Assert.That(meshCollider.enabled, Is.True);
        Assert.That(meshCollider.isTrigger, Is.False);
        Assert.That(meshCollider.convex, Is.True);
        Assert.That(result.Root.GetComponent<BoxCollider>(), Is.Null);
        Assert.That(result.Root.layer, Is.EqualTo(GameLayers.PickupIndex));

        Object.DestroyImmediate(visualPrefab);
    }

    [Test]
    public void RewardPropBuilderPlacesAuthoredPrefabRootOnSurface()
    {
        CreateSurface("TCO Test Ground", new Vector3(0f, -0.05f, 0f), new Vector3(8f, 0.1f, 8f));
        Physics.SyncTransforms();
        var visualPrefab = new GameObject("TCO Test Grounded Prefab");
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.SetParent(visualPrefab.transform, worldPositionStays: false);
        body.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        body.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        WorldPropPlacementProfile profile = WorldPropPlacementProfile.TicketDepositDefault(TicketDepositTier.Small);

        WorldRewardPropBuildResult result = WorldRewardPropBuilder.Create(
            "TCO Test Grounded Authored Prop",
            Vector3.zero,
            visualPrefab,
            1f,
            profile,
            TestPlacementService(),
            null,
            PrimitiveType.Cube,
            "Authored Body",
            WorldRewardPropColliderMode.Authored);

        Assert.That(result.Root.transform.position.y, Is.EqualTo(0f).Within(0.001f));
        Assert.That(result.Placement.Position.y, Is.EqualTo(0f).Within(0.001f));
        Assert.That(result.Body.transform.localPosition, Is.EqualTo(Vector3.zero));

        Object.DestroyImmediate(visualPrefab);
    }

    [Test]
    public void RewardPropBuilderPreservesAuthoredPrefabColliders()
    {
        CreateSurface("TCO Test Ground", new Vector3(0f, -0.05f, 0f), new Vector3(8f, 0.1f, 8f));
        Physics.SyncTransforms();
        var visualPrefab = new GameObject("TCO Test Authored Chest Prefab");
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Authored Collider Body";
        body.transform.SetParent(visualPrefab.transform, worldPositionStays: false);
        BoxCollider authoredBox = body.GetComponent<BoxCollider>();
        authoredBox.size = new Vector3(0.9f, 0.6f, 0.7f);
        WorldPropPlacementProfile profile = WorldPropPlacementProfile.ChestDefault();

        WorldRewardPropBuildResult result = WorldRewardPropBuilder.Create(
            "TCO Test Authored Collider Reward Prop",
            Vector3.zero,
            visualPrefab,
            1f,
            profile,
            TestPlacementService(),
            null,
            PrimitiveType.Cube,
            "Chest Body",
            WorldRewardPropColliderMode.Authored);

        BoxCollider preservedBox = result.Body.GetComponentsInChildren<BoxCollider>().Single();
        Assert.That(result.Root.GetComponent<BoxCollider>(), Is.Null);
        Assert.That(preservedBox.enabled, Is.True);
        Assert.That(preservedBox.isTrigger, Is.False);
        Assert.That(preservedBox.size, Is.EqualTo(authoredBox.size));
        Assert.That(result.Root.layer, Is.EqualTo(GameLayers.PickupIndex));
        Assert.That(preservedBox.gameObject.layer, Is.EqualTo(GameLayers.PickupIndex));

        Object.DestroyImmediate(visualPrefab);
    }

    [Test]
    public void ChestSystemSpawnsPlayerBlockingChestOnRaisedTerrain()
    {
        CreateSurface("TCO Test Raised Ground", new Vector3(0f, 0.5f, 0f), new Vector3(90f, 1f, 90f));
        Physics.SyncTransforms();
        ChestDefinition chest = TheCircussyOneTestObjects.CreateChestDefinition("locked_chest", "TCO Test Locked Chest", ticketCost: 0);
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles");
        ChestCatalog chestCatalog = TheCircussyOneTestObjects.CreateChestCatalog(chest);
        ItemCatalog itemCatalog = TheCircussyOneTestObjects.CreateItemCatalog(item);
        WorldRewardPlacementCatalog placementCatalog = TheCircussyOneTestObjects.CreateWorldRewardPlacementCatalog(
            TheCircussyOneTestObjects.CreateWorldRewardPlacement(
                "locked_chest_ramp",
                "Locked Chest Ramp",
                WorldRewardPlacementKind.Chest,
                "locked_chest",
                Vector3.zero));
        new ChestSystem(
            chestCatalog,
            itemCatalog,
            new ItemInventory(),
            null,
            player: null,
            pauseState: null,
            placementService: TestPlacementService(),
            spawnPlanner: new WorldRewardSpawnPlanner(placementCatalog)).SpawnWorld(1);

        ChestView view = Object.FindObjectsByType<ChestView>(FindObjectsSortMode.None).Single();
        BoxCollider collider = view.GetComponent<BoxCollider>();

        Assert.That(view.transform.position.y, Is.EqualTo(1f + chest.placement.RootHeightOffset).Within(0.001f));
        Assert.That(collider, Is.Not.Null);
        Assert.That(collider.enabled, Is.True);
        Assert.That(view.gameObject.layer, Is.EqualTo(GameLayers.PickupIndex));

        Object.DestroyImmediate(chest);
        Object.DestroyImmediate(item);
        Object.DestroyImmediate(chestCatalog);
        Object.DestroyImmediate(itemCatalog);
        Object.DestroyImmediate(placementCatalog);
    }

    [Test]
    public void ChestSystemAlignsSpawnedChestToRampNormal()
    {
        Vector3 expectedNormal = CreateSlopedSurface(25f, Vector3.zero, new Vector3(90f, 0.1f, 90f));
        ChestDefinition chest = TheCircussyOneTestObjects.CreateChestDefinition("locked_chest", "TCO Test Locked Chest", ticketCost: 0);
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles");
        ChestCatalog chestCatalog = TheCircussyOneTestObjects.CreateChestCatalog(chest);
        ItemCatalog itemCatalog = TheCircussyOneTestObjects.CreateItemCatalog(item);
        WorldRewardPlacementCatalog placementCatalog = TheCircussyOneTestObjects.CreateWorldRewardPlacementCatalog(
            TheCircussyOneTestObjects.CreateWorldRewardPlacement(
                "locked_chest_ramp",
                "Locked Chest Ramp",
                WorldRewardPlacementKind.Chest,
                "locked_chest",
                Vector3.zero));
        new ChestSystem(
            chestCatalog,
            itemCatalog,
            new ItemInventory(),
            null,
            player: null,
            pauseState: null,
            placementService: TestPlacementService(),
            spawnPlanner: new WorldRewardSpawnPlanner(placementCatalog)).SpawnWorld(1);

        ChestView view = Object.FindObjectsByType<ChestView>(FindObjectsSortMode.None).Single();

        Assert.That(Vector3.Angle(view.transform.up, expectedNormal), Is.LessThan(1f));

        Object.DestroyImmediate(chest);
        Object.DestroyImmediate(item);
        Object.DestroyImmediate(chestCatalog);
        Object.DestroyImmediate(itemCatalog);
        Object.DestroyImmediate(placementCatalog);
    }

    [Test]
    public void ChestSystemKeepsGeneratedLidSeatedOnBody()
    {
        CreateSurface("TCO Test Ground", new Vector3(0f, -0.05f, 0f), new Vector3(90f, 0.1f, 90f));
        Physics.SyncTransforms();
        ChestDefinition chest = TheCircussyOneTestObjects.CreateChestDefinition("locked_chest", "TCO Test Locked Chest", ticketCost: 0);
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles");
        ChestCatalog chestCatalog = TheCircussyOneTestObjects.CreateChestCatalog(chest);
        ItemCatalog itemCatalog = TheCircussyOneTestObjects.CreateItemCatalog(item);
        new ChestSystem(
            chestCatalog,
            itemCatalog,
            new ItemInventory(),
            null,
            player: null,
            pauseState: null,
            placementService: TestPlacementService()).SpawnWorld(1);

        ChestView view = Object.FindObjectsByType<ChestView>(FindObjectsSortMode.None).Single();
        Renderer body = view.transform.Find($"{VisualGroundAnchorUtility.VisualRootName}/Chest Body").GetComponent<Renderer>();
        Renderer lid = view.transform.Find($"{VisualGroundAnchorUtility.VisualRootName}/Lid").GetComponent<Renderer>();

        Assert.That(lid.bounds.min.y, Is.LessThanOrEqualTo(body.bounds.max.y + 0.001f));

        Object.DestroyImmediate(chest);
        Object.DestroyImmediate(item);
        Object.DestroyImmediate(chestCatalog);
        Object.DestroyImmediate(itemCatalog);
    }

    [Test]
    public void ChestSystemUsesAssignedVisualPrefabWithoutGeneratedLid()
    {
        CreateSurface("TCO Test Ground", new Vector3(0f, -0.05f, 0f), new Vector3(90f, 0.1f, 90f));
        Physics.SyncTransforms();
        GameObject visualPrefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visualPrefab.name = "TCO Test Chest Mesh";
        ChestDefinition chest = TheCircussyOneTestObjects.CreateChestDefinition("locked_chest", "TCO Test Locked Chest", ticketCost: 0);
        chest.visualPrefab = visualPrefab;
        chest.visualScale = 2f;
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles");
        ChestCatalog chestCatalog = TheCircussyOneTestObjects.CreateChestCatalog(chest);
        ItemCatalog itemCatalog = TheCircussyOneTestObjects.CreateItemCatalog(item);
        new ChestSystem(
            chestCatalog,
            itemCatalog,
            new ItemInventory(),
            null,
            player: null,
            pauseState: null,
            placementService: TestPlacementService()).SpawnWorld(1);

        ChestView view = Object.FindObjectsByType<ChestView>(FindObjectsSortMode.None).Single();
        Transform visualRoot = view.transform.Find(VisualGroundAnchorUtility.VisualRootName);

        Assert.That(visualRoot.Find("Lid"), Is.Null);
        Assert.That(visualRoot.Find("Chest Body"), Is.Not.Null);
        Assert.That(visualRoot.Find("Chest Body").localScale, Is.EqualTo(Vector3.one * 2f));
        Collider authoredCollider = view.GetComponentsInChildren<Collider>().Single();
        Assert.That(authoredCollider, Is.TypeOf<CapsuleCollider>());
        Assert.That(authoredCollider.enabled, Is.True);
        Assert.That(authoredCollider.isTrigger, Is.False);
        Assert.That(view.GetComponent<BoxCollider>(), Is.Null);

        Object.DestroyImmediate(visualPrefab);
        Object.DestroyImmediate(chest);
        Object.DestroyImmediate(item);
        Object.DestroyImmediate(chestCatalog);
        Object.DestroyImmediate(itemCatalog);
    }

    [Test]
    public void TicketDepositSystemBlocksAllDepositsByDefault()
    {
        CreateSurface("TCO Test Ground", new Vector3(0f, -0.05f, 0f), new Vector3(14f, 0.1f, 14f));
        Physics.SyncTransforms();
        TicketDepositDefinition small = TheCircussyOneTestObjects.CreateTicketDepositDefinition("small_ticket_stack", "TCO Test Small Ticket Stack");
        TicketDepositDefinition large = TheCircussyOneTestObjects.CreateTicketDepositDefinition("jackpot_cache", "TCO Test Jackpot Cache");
        large.tier = TicketDepositTier.Large;
        large.placement = WorldPropPlacementProfile.TicketDepositDefault(TicketDepositTier.Large);
        TicketDepositCatalog catalog = TheCircussyOneTestObjects.CreateTicketDepositCatalog(small, large);
        new TicketDepositSystem(
            catalog,
            player: null,
            placementService: TestPlacementService()).SpawnWorld(1);

        TicketDepositView[] deposits = Object.FindObjectsByType<TicketDepositView>(FindObjectsSortMode.None);
        TicketDepositView smallView = deposits.Single(view => view.Definition == small);
        TicketDepositView largeView = deposits.Single(view => view.Definition == large);

        Assert.That(smallView.GetComponent<BoxCollider>().enabled, Is.True);
        Assert.That(largeView.GetComponent<BoxCollider>().enabled, Is.True);

        Object.DestroyImmediate(small);
        Object.DestroyImmediate(large);
        Object.DestroyImmediate(catalog);
    }

    [Test]
    public void WorldRewardSpawnSystemSpawnsAllRewardKinds()
    {
        CreateSurface("TCO Test Ground", new Vector3(0f, -0.05f, 0f), new Vector3(90f, 0.1f, 90f));
        Physics.SyncTransforms();
        ChestDefinition chest = TheCircussyOneTestObjects.CreateChestDefinition("locked_chest", "TCO Test Locked Chest", ticketCost: 0);
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles");
        TicketDepositDefinition deposit = TheCircussyOneTestObjects.CreateTicketDepositDefinition("small_ticket_stack", "TCO Test Small Ticket Stack");
        HealthPickupDefinition treat = TheCircussyOneTestObjects.CreateHealthPickupDefinition("treat", "Treat");
        HealingPropDefinition snackBox = TheCircussyOneTestObjects.CreateHealingPropDefinition("snack_box", "TCO Test Snack Box", treat);
        ChestCatalog chestCatalog = TheCircussyOneTestObjects.CreateChestCatalog(chest);
        ItemCatalog itemCatalog = TheCircussyOneTestObjects.CreateItemCatalog(item);
        TicketDepositCatalog depositCatalog = TheCircussyOneTestObjects.CreateTicketDepositCatalog(deposit);
        HealingPropCatalog propCatalog = TheCircussyOneTestObjects.CreateHealingPropCatalog(snackBox);
        HealthPickupCatalog healthPickupCatalog = TheCircussyOneTestObjects.CreateHealthPickupCatalog(treat);
        WorldRewardPlacementCatalog placementCatalog = TheCircussyOneTestObjects.CreateWorldRewardPlacementCatalog(
            TheCircussyOneTestObjects.CreateWorldRewardPlacement(
                "locked_chest_test",
                "Locked Chest Test",
                WorldRewardPlacementKind.Chest,
                "locked_chest",
                new Vector3(-6f, 0.5f, 0f)),
            TheCircussyOneTestObjects.CreateWorldRewardPlacement(
                "ticket_test",
                "Ticket Test",
                WorldRewardPlacementKind.TicketDeposit,
                "small_ticket_stack",
                Vector3.zero),
            TheCircussyOneTestObjects.CreateWorldRewardPlacement(
                "snack_test",
                "Snack Test",
                WorldRewardPlacementKind.HealingProp,
                "snack_box",
                new Vector3(6f, 0.3f, 0f)));
        var planner = new WorldRewardSpawnPlanner(placementCatalog);
        var rewardSystem = new WorldRewardSpawnSystem(
            new ChestSystem(chestCatalog, itemCatalog, new ItemInventory(), null, placementService: TestPlacementService(), spawnPlanner: planner),
            new TicketDepositSystem(depositCatalog, placementService: TestPlacementService(), spawnPlanner: planner),
            new HealingPropSystem(propCatalog, healthPickupCatalog, pickupFactory: null, placementService: TestPlacementService(), spawnPlanner: planner));

        rewardSystem.SpawnWorld(1);

        Assert.That(SceneInteractableRegistry.CountByKind(SceneInteractableKind.Chest), Is.EqualTo(1));
        Assert.That(SceneInteractableRegistry.CountByKind(SceneInteractableKind.TicketDeposit), Is.EqualTo(1));
        Assert.That(SceneInteractableRegistry.CountByKind(SceneInteractableKind.HealingProp), Is.EqualTo(1));

        Object.DestroyImmediate(chest);
        Object.DestroyImmediate(item);
        Object.DestroyImmediate(deposit);
        Object.DestroyImmediate(treat);
        Object.DestroyImmediate(snackBox);
        Object.DestroyImmediate(chestCatalog);
        Object.DestroyImmediate(itemCatalog);
        Object.DestroyImmediate(depositCatalog);
        Object.DestroyImmediate(propCatalog);
        Object.DestroyImmediate(healthPickupCatalog);
        Object.DestroyImmediate(placementCatalog);
    }

    [Test]
    public void ChestSystemSkipsSpecialEnemyChestWorldPlacements()
    {
        CreateSurface("TCO Test Ground", new Vector3(0f, -0.05f, 0f), new Vector3(30f, 0.1f, 30f));
        Physics.SyncTransforms();
        ChestDefinition special = TheCircussyOneTestObjects.CreateChestDefinition(
            "special_enemy_chest",
            "TCO Test Special Enemy Chest",
            ChestKind.SpecialEnemy,
            ticketCost: 0);
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles");
        ChestCatalog chestCatalog = TheCircussyOneTestObjects.CreateChestCatalog(special);
        ItemCatalog itemCatalog = TheCircussyOneTestObjects.CreateItemCatalog(item);
        WorldRewardPlacementCatalog placementCatalog = TheCircussyOneTestObjects.CreateWorldRewardPlacementCatalog(
            TheCircussyOneTestObjects.CreateWorldRewardPlacement(
                "special_enemy_chest_test",
                "Special Enemy Chest Test",
                WorldRewardPlacementKind.Chest,
                "special_enemy_chest",
                Vector3.zero));

        new ChestSystem(
            chestCatalog,
            itemCatalog,
            new ItemInventory(),
            null,
            placementService: TestPlacementService(),
            spawnPlanner: new WorldRewardSpawnPlanner(placementCatalog)).SpawnWorld(1);

        Assert.That(ChestWorldSpawnRules.CanSpawnInWorld(special), Is.False);
        Assert.That(SceneInteractableRegistry.CountByKind(SceneInteractableKind.Chest), Is.Zero);
        Assert.That(Object.FindObjectsByType<ChestView>(FindObjectsSortMode.None), Is.Empty);

        Object.DestroyImmediate(special);
        Object.DestroyImmediate(item);
        Object.DestroyImmediate(chestCatalog);
        Object.DestroyImmediate(itemCatalog);
        Object.DestroyImmediate(placementCatalog);
    }

    [Test]
    public void WorldRewardPlacementCatalogValidatesInvalidRows()
    {
        var valid = TheCircussyOneTestObjects.CreateWorldRewardPlacement(
            "locked_chest_west",
            "Locked Chest West",
            WorldRewardPlacementKind.Chest,
            "locked_chest",
            new Vector3(-4f, 0.5f, 2f));
        var duplicate = TheCircussyOneTestObjects.CreateWorldRewardPlacement(
            "locked_chest_west",
            "Duplicate Locked Chest West",
            WorldRewardPlacementKind.Chest,
            "locked_chest",
            new Vector3(-6f, 0.5f, 2f));
        var invalid = new WorldRewardPlacementDefinition
        {
            placementId = "",
            displayName = "",
            targetContentId = "Bad Id",
            count = 0,
            isActive = true
        };
        WorldRewardPlacementCatalog catalog = TheCircussyOneTestObjects.CreateWorldRewardPlacementCatalog(valid, duplicate, invalid, null);

        var issues = catalog.ValidateContent();

        Assert.That(issues.Any(issue => issue.Code == "catalog.null-entry"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.duplicate-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.invalid-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.missing-display-name"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "world-reward-placement.invalid-target-content-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "world-reward-placement.invalid-count"), Is.True);

        Object.DestroyImmediate(catalog);
    }

    [Test]
    public void WorldRewardSpawnPlannerUsesCatalogRowsByKind()
    {
        var chestPlacement = TheCircussyOneTestObjects.CreateWorldRewardPlacement(
            "locked_chest_west",
            "Locked Chest West",
            WorldRewardPlacementKind.Chest,
            "locked_chest",
            new Vector3(-4f, 0.5f, 2f));
        var depositPlacement = TheCircussyOneTestObjects.CreateWorldRewardPlacement(
            "ticket_roll_east",
            "Ticket Roll East",
            WorldRewardPlacementKind.TicketDeposit,
            "ticket_roll",
            new Vector3(6f, 0.35f, -2f));
        var invalidDepositPlacement = TheCircussyOneTestObjects.CreateWorldRewardPlacement(
            "invalid_ticket_roll",
            "Invalid Ticket Roll",
            WorldRewardPlacementKind.TicketDeposit,
            "Bad Id",
            new Vector3(99f, 0.35f, 99f));
        WorldRewardPlacementCatalog catalog = TheCircussyOneTestObjects.CreateWorldRewardPlacementCatalog(chestPlacement, depositPlacement, invalidDepositPlacement);
        var planner = new WorldRewardSpawnPlanner(catalog);

        WorldRewardSpawnRequest request = planner.RequestsFor(WorldRewardPlacementKind.TicketDeposit).Single();

        Assert.That(request.PlacementId, Is.EqualTo("ticket_roll_east"));
        Assert.That(request.Kind, Is.EqualTo(WorldRewardPlacementKind.TicketDeposit));
        Assert.That(request.TargetContentId, Is.EqualTo("ticket_roll"));
        Assert.That(request.Position, Is.EqualTo(new Vector3(6f, 0.35f, -2f)));

        Object.DestroyImmediate(catalog);
    }

    [Test]
    public void WorldRewardSpawnPlannerCanSkipIneligibleSequentialContent()
    {
        ChestDefinition special = TheCircussyOneTestObjects.CreateChestDefinition(
            "special_enemy_chest",
            "TCO Test Special Enemy Chest",
            ChestKind.SpecialEnemy,
            ticketCost: 0);
        ChestDefinition locked = TheCircussyOneTestObjects.CreateChestDefinition("locked_chest", "TCO Test Locked Chest", ticketCost: 0);
        ChestDefinition premium = TheCircussyOneTestObjects.CreateChestDefinition(
            "premium_chest",
            "TCO Test Premium Chest",
            ChestKind.Premium,
            ticketCost: 60);
        IReadOnlyList<ChestDefinition> definitions = new[] { special, locked, premium };
        var request = new WorldRewardSpawnRequest(
            "generated_chest",
            WorldRewardPlacementKind.Chest,
            targetContentId: null,
            Vector3.zero,
            required: false,
            repeatSequentialContent: true,
            generated: true);
        int sequentialIndex = 0;

        ChestDefinition first = WorldRewardSpawnPlanner.ResolveContent(
            definitions,
            request,
            ref sequentialIndex,
            ChestWorldSpawnRules.CanSpawnInWorld);
        ChestDefinition second = WorldRewardSpawnPlanner.ResolveContent(
            definitions,
            request,
            ref sequentialIndex,
            ChestWorldSpawnRules.CanSpawnInWorld);

        Assert.That(first, Is.SameAs(locked));
        Assert.That(second, Is.SameAs(premium));

        Object.DestroyImmediate(special);
        Object.DestroyImmediate(locked);
        Object.DestroyImmediate(premium);
    }

    [Test]
    public void ProceduralRunWorldGeneratorBuildsDeterministicHeightfieldForSameSeedAndAct()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.runtimeMeshTerrainEnabled = false;
        WorldGenerationResult first = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(2, 2, 12345));
        WorldGenerationResult second = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(2, 2, 12345));

        Assert.That(first.UsedPrototypePlacement, Is.True);
        Assert.That(first.HasGeneratedLayout, Is.False);
        Assert.That(first.HasGeneratedMap, Is.True);
        Assert.That(first.HasPlayerStart, Is.False);
        Assert.That(first.HasHeadlinerAnchor, Is.False);
        Assert.That(first.HasStageDoorAnchor, Is.False);
        Assert.That(first.GeneratedMap.Width, Is.EqualTo(second.GeneratedMap.Width));
        Assert.That(first.GeneratedMap.Depth, Is.EqualTo(second.GeneratedMap.Depth));
        Assert.That(first.GeneratedMap.PlayerStartIndex, Is.EqualTo(second.GeneratedMap.PlayerStartIndex));
        Assert.That(first.GeneratedMap.CountMask(GeneratedWorldMask.Reachable), Is.EqualTo(second.GeneratedMap.CountMask(GeneratedWorldMask.Reachable)));
        for (int i = 0; i < first.GeneratedMap.Samples.Count; i++)
        {
            Assert.That(first.GeneratedMap.Samples[i].Height, Is.EqualTo(second.GeneratedMap.Samples[i].Height).Within(0.0001f));
            Assert.That(first.GeneratedMap.Samples[i].Mask, Is.EqualTo(second.GeneratedMap.Samples[i].Mask));
        }

        Object.DestroyImmediate(config);
    }

    [Test]
    public void ProceduralRunWorldGeneratorVariesHeightfieldBySeed()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.terrainMode = RunWorldTerrainMode.SmoothStamps;
        config.runtimeMeshTerrainEnabled = false;
        WorldGenerationResult first = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(1, 1, 111));
        WorldGenerationResult second = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(1, 1, 222));

        bool anyDifferentHeight = first.GeneratedMap.Samples
            .Zip(second.GeneratedMap.Samples, (a, b) => Mathf.Abs(a.Height - b.Height) > 0.001f)
            .Any(different => different);

        Assert.That(anyDifferentHeight, Is.True);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void ProceduralRunWorldGeneratorCreatesReachableSafeMasks()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.runtimeMeshTerrainEnabled = false;
        WorldGenerationResult result = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(1, 1, 9001));

        GeneratedWorldMap map = result.GeneratedMap;

        Assert.That(map.CountMask(GeneratedWorldMask.Walkable), Is.GreaterThan(0));
        Assert.That(map.CountMask(GeneratedWorldMask.Reachable), Is.GreaterThan(0));
        Assert.That(map.CountMask(GeneratedWorldMask.RewardSafe), Is.GreaterThan(0));
        Assert.That(map.CountMask(GeneratedWorldMask.SpawnSafe), Is.GreaterThan(0));
        Assert.That(map.CountMask(GeneratedWorldMask.PlayerStartSafe), Is.GreaterThan(0));
        Assert.That(map.Samples[map.PlayerStartIndex].HasMask(GeneratedWorldMask.Reachable), Is.True);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void ProceduralRunWorldGeneratorKeepsBlockySpawnSafeInsideBoundaryMargin()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.terrainMode = RunWorldTerrainMode.StructuralGrammar;
        config.runtimeMeshTerrainEnabled = false;

        WorldGenerationResult result = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(1, 1, 9001));

        GeneratedWorldMap map = result.GeneratedMap;
        RunWorldGenerationProfile profile = config.ProfileForAct(1);
        for (int i = 0; i < map.Samples.Count; i++)
        {
            if (!map.Samples[i].HasMask(GeneratedWorldMask.SpawnSafe))
            {
                continue;
            }

            Vector3 position = map.PositionForIndex(i);
            float boundaryDistance = map.PlayableRadius - new Vector2(position.x, position.z).magnitude;
            Assert.That(
                boundaryDistance,
                Is.GreaterThanOrEqualTo(profile.BoundaryMargin - 0.001f),
                $"SpawnSafe sample {i} was too close to the arena boundary.");
        }

        Object.DestroyImmediate(config);
    }

    [Test]
    public void ProceduralRunWorldGeneratorCreatesGeneratedRewardRequestsWhenRuntimeMeshEnabled()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.runtimeMeshTerrainEnabled = true;
        config.hidePrototypeArenaWhenGenerated = false;
        config.generatedChestSpacing = 0f;
        config.generatedTicketDepositSpacing = 0f;
        config.generatedHealingPropSpacing = 0f;
        config.generatedLooseChestSpacing = 0f;
        config.generatedLooseTicketDepositSpacing = 0f;
        config.generatedLooseHealingPropSpacing = 0f;
        config.actProfiles[0].generatedChestCount = 2;
        config.actProfiles[0].generatedTicketDepositCount = 3;
        config.actProfiles[0].generatedHealingPropCount = 1;
        config.actProfiles[0].generatedLooseChestCount = 4;
        config.actProfiles[0].generatedLooseTicketDepositCount = 6;
        config.actProfiles[0].generatedLooseHealingPropCount = 2;

        WorldGenerationResult result = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(1, 1, 501));

        Assert.That(result.UsedPrototypePlacement, Is.False);
        Assert.That(result.HasGeneratedLayout, Is.True);
        Assert.That(result.RewardSpawnRequests.Count, Is.GreaterThan(0));
        Assert.That(result.RewardSpawnRequests.Count, Is.LessThanOrEqualTo(18));
        Assert.That(result.RewardSpawnRequests.Count(request => request.Kind == WorldRewardPlacementKind.Chest), Is.LessThanOrEqualTo(6));
        Assert.That(result.RewardSpawnRequests.Count(request => request.Kind == WorldRewardPlacementKind.TicketDeposit), Is.LessThanOrEqualTo(9));
        Assert.That(result.RewardSpawnRequests.Count(request => request.Kind == WorldRewardPlacementKind.HealingProp), Is.LessThanOrEqualTo(3));
        Assert.That(result.RewardSpawnRequests.All(request => request.Generated), Is.True);
        Assert.That(result.RewardSpawnRequests.All(request => request.RepeatSequentialContent), Is.True);
        Assert.That(result.RewardSpawnRequests.All(request => !request.HasTargetContent), Is.True);
        Assert.That(result.RewardSpawnRequests.Where(IsPocketRequest).All(request => MatchesRewardSafeSample(result.GeneratedMap, request.Position)), Is.True);
        Assert.That(result.RewardSpawnRequests.Where(IsLooseRequest).All(request => MatchesLooseReachableSample(result.GeneratedMap, request.Position)), Is.True);
        Assert.That(result.RewardSpawnRequests.Any(IsLooseRequest), Is.True);

        Object.DestroyImmediate(result.GeneratedRoot);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void ProceduralRunWorldGeneratorBuildsBlockingObstaclePropsAndRegistersFootprints()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.runtimeMeshTerrainEnabled = true;
        config.hidePrototypeArenaWhenGenerated = false;
        config.worldObstacleEnabled = true;
        config.worldObstacleCountPer100k = 240f;
        config.worldObstacleMaxCount = 6;
        config.worldObstacleMinSpacing = 4f;
        config.worldObstacleArenaEdgePadding = 4f;
        config.worldObstaclePlayerStartExclusionRadius = 0f;
        config.worldObstacleMaxSlope = 12f;
        config.chunkQuadsPerSide = 4;
        var placementService = new WorldPropPlacementService();

        WorldGenerationResult result = new ProceduralRunWorldGenerator(config: config, placementService: placementService)
            .Generate(new WorldGenerationRequest(1, 1, 551));

        Transform obstacleRoot = result.GeneratedRoot.transform.Find("Blocking Circus Obstacles");
        Assert.That(result.ObstacleSpawnRequests.Count, Is.GreaterThan(0));
        Assert.That(obstacleRoot, Is.Not.Null);
        Assert.That(obstacleRoot.childCount, Is.EqualTo(result.ObstacleSpawnRequests.Count));
        Assert.That(obstacleRoot.GetComponentsInChildren<Collider>().Length, Is.GreaterThanOrEqualTo(result.ObstacleSpawnRequests.Count));
        Assert.That(obstacleRoot.GetComponentsInChildren<CapsuleCollider>().Length, Is.Zero);
        Assert.That(obstacleRoot.GetComponentsInChildren<Renderer>().Length, Is.GreaterThan(result.ObstacleSpawnRequests.Count));

        Object.DestroyImmediate(result.GeneratedRoot);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void GeneratedWorldObstaclePlannerCreatesWideTallShortAndRootedTiltedBlockers()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.worldObstacleEnabled = true;
        config.worldObstacleCountPer100k = 640f;
        config.worldObstacleMaxCount = 24;
        config.worldObstacleMinSpacing = 1f;
        config.worldObstacleArenaEdgePadding = 4f;
        config.worldObstaclePlayerStartExclusionRadius = 0f;
        config.worldObstacleRewardExclusionRadius = 0.5f;
        config.worldObstacleMaxSlope = 12f;
        config.worldObstacleMaxTiltDegrees = 4f;
        config.worldObstacleWidthMax = 19.5f;
        config.worldObstacleDepthMax = 16.5f;
        config.worldObstacleJumpableHeightMax = 1.35f;
        config.worldObstacleTallHeightMin = 9f;
        config.worldObstacleTallHeightMax = 60f;
        config.worldObstacleJumpableChance = 0.32f;
        config.worldObstacleTallChance = 0.38f;
        WorldGenerationResult result = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(1, 1, 556));

        IReadOnlyList<WorldBlockingObstacleSpawnRequest> requests =
            GeneratedWorldObstaclePlanner.BuildRequests(result.GeneratedMap, config);

        Assert.That(requests.Count, Is.GreaterThan(8));
        Assert.That(requests.Max(request => Mathf.Max(request.Size.x, request.Size.z)), Is.GreaterThan(10f));
        Assert.That(requests.Max(request => request.Size.y), Is.GreaterThan(20f));
        Assert.That(requests.Any(request => request.Size.y <= config.WorldObstacleJumpableHeightMax + 0.001f), Is.True);

        IReadOnlyList<WorldBlockingObstacleSpawnRequest> tilted = requests
            .Where(request => Vector3.Angle(request.Rotation * Vector3.up, request.Normal) > 0.05f)
            .ToArray();
        Assert.That(tilted.Count, Is.GreaterThan(0));
        Assert.That(
            tilted.All(request => Vector3.Angle(request.Rotation * Vector3.up, request.Normal) <= config.WorldObstacleMaxTiltDegrees + 0.05f),
            Is.True);
        Assert.That(
            tilted.All(request => request.Position.y <= ClosestGeneratedSampleHeight(result.GeneratedMap, request.Position) + config.WorldObstacleSurfaceOffset + 0.001f),
            Is.True);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void GeneratedWorldRewardPlannerAvoidsBlockingObstacleFootprints()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.generatedChestSpacing = 0f;
        config.generatedTicketDepositSpacing = 0f;
        config.generatedHealingPropSpacing = 0f;
        config.generatedLooseChestSpacing = 0f;
        config.generatedLooseTicketDepositSpacing = 0f;
        config.generatedLooseHealingPropSpacing = 0f;
        config.actProfiles[0].generatedChestCount = 2;
        config.actProfiles[0].generatedTicketDepositCount = 2;
        config.actProfiles[0].generatedHealingPropCount = 2;
        config.actProfiles[0].generatedLooseChestCount = 2;
        config.actProfiles[0].generatedLooseTicketDepositCount = 2;
        config.actProfiles[0].generatedLooseHealingPropCount = 2;
        GeneratedWorldMap map = TinyGeneratedWorldMap();
        var obstacle = new WorldBlockingObstacleSpawnRequest(
            "test_obstacle",
            WorldBlockingObstacleShape.Box,
            map.PlayerStartPosition,
            Vector3.up,
            Quaternion.identity,
            new Vector3(2f, 2f, 2f),
            footprintRadius: 1.5f,
            avoidanceRadius: 3f,
            variantSeed: 1);

        IReadOnlyList<WorldRewardSpawnRequest> requests = GeneratedWorldRewardPlanner.BuildRequests(
            map,
            config.ProfileForAct(1),
            config,
            new[] { obstacle });

        Assert.That(requests.Count, Is.GreaterThan(0));
        Assert.That(
            requests.All(request => Vector2.Distance(
                new Vector2(request.Position.x, request.Position.z),
                new Vector2(obstacle.Position.x, obstacle.Position.z)) >= obstacle.AvoidanceRadius - 0.001f),
            Is.True);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void ProceduralRunWorldGeneratorRewardRequestsAreDeterministicForSameSeed()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.runtimeMeshTerrainEnabled = true;
        config.hidePrototypeArenaWhenGenerated = false;
        config.generatedChestSpacing = 0f;
        config.generatedTicketDepositSpacing = 0f;
        config.generatedHealingPropSpacing = 0f;
        config.generatedLooseChestSpacing = 0f;
        config.generatedLooseTicketDepositSpacing = 0f;
        config.generatedLooseHealingPropSpacing = 0f;
        config.actProfiles[0].generatedChestCount = 1;
        config.actProfiles[0].generatedTicketDepositCount = 1;
        config.actProfiles[0].generatedHealingPropCount = 1;
        config.actProfiles[0].generatedLooseChestCount = 1;
        config.actProfiles[0].generatedLooseTicketDepositCount = 1;
        config.actProfiles[0].generatedLooseHealingPropCount = 1;

        WorldGenerationResult first = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(1, 1, 777));
        WorldGenerationResult second = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(1, 1, 777));

        Assert.That(second.RewardSpawnRequests.Count, Is.EqualTo(first.RewardSpawnRequests.Count));
        for (int i = 0; i < first.RewardSpawnRequests.Count; i++)
        {
            Assert.That(second.RewardSpawnRequests[i].PlacementId, Is.EqualTo(first.RewardSpawnRequests[i].PlacementId));
            Assert.That(second.RewardSpawnRequests[i].Kind, Is.EqualTo(first.RewardSpawnRequests[i].Kind));
            Assert.That(second.RewardSpawnRequests[i].Position, Is.EqualTo(first.RewardSpawnRequests[i].Position));
        }

        Object.DestroyImmediate(first.GeneratedRoot);
        Object.DestroyImmediate(second.GeneratedRoot);
        Object.DestroyImmediate(config);
    }

    [UnityEngine.TestTools.UnityTest]
    public IEnumerator GeneratedWorldRewardPlannerAsyncMatchesSyncRequests()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.runtimeMeshTerrainEnabled = false;
        config.generatedChestSpacing = 18f;
        config.generatedTicketDepositSpacing = 10f;
        config.generatedHealingPropSpacing = 14f;
        config.generatedLooseChestSpacing = 12f;
        config.generatedLooseTicketDepositSpacing = 7f;
        config.generatedLooseHealingPropSpacing = 9f;

        WorldGenerationResult result = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(1, 1, 507));
        RunWorldGenerationProfile profile = config.ProfileForAct(1);
        IReadOnlyList<WorldRewardSpawnRequest> sync = GeneratedWorldRewardPlanner.BuildRequests(
            result.GeneratedMap,
            profile,
            config);
        IReadOnlyList<WorldRewardSpawnRequest> asyncResult = null;

        yield return GeneratedWorldRewardPlanner.BuildRequestsAsync(result.GeneratedMap, profile, config)
            .ToCoroutine(requests => asyncResult = requests);

        AssertRewardRequestsEqual(sync, asyncResult);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void GeneratedWorldRewardPlannerRespectsMinimumConfiguredSpacing()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.runtimeMeshTerrainEnabled = false;
        config.generatedChestSpacing = 18f;
        config.generatedTicketDepositSpacing = 10f;
        config.generatedHealingPropSpacing = 14f;
        config.generatedLooseChestSpacing = 12f;
        config.generatedLooseTicketDepositSpacing = 7f;
        config.generatedLooseHealingPropSpacing = 9f;

        WorldGenerationResult result = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(1, 1, 509));
        IReadOnlyList<WorldRewardSpawnRequest> requests = GeneratedWorldRewardPlanner.BuildRequests(
            result.GeneratedMap,
            config.ProfileForAct(1),
            config);

        Assert.That(requests.Count, Is.GreaterThan(20));
        AssertMinimumHorizontalSpacing(requests, 7f);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void ProceduralRunWorldGeneratorSpreadsGeneratedRewardsAcrossSectors()
    {
        RunWorldGenerationConfig config = RunWorldGenerationConfig.CreateRuntimeDefault();
        config.runtimeMeshTerrainEnabled = true;
        config.hidePrototypeArenaWhenGenerated = false;
        config.generatedChestSpacing = 0f;
        config.generatedTicketDepositSpacing = 0f;
        config.generatedHealingPropSpacing = 0f;
        config.generatedLooseChestSpacing = 0f;
        config.generatedLooseTicketDepositSpacing = 0f;
        config.generatedLooseHealingPropSpacing = 0f;
        config.rewardSpreadWeight = 1f;
        config.rewardHighGroundWeight = 0.25f;
        config.actProfiles[0].generatedChestCount = 3;
        config.actProfiles[0].generatedTicketDepositCount = 8;
        config.actProfiles[0].generatedHealingPropCount = 3;
        config.actProfiles[0].generatedLooseChestCount = 6;
        config.actProfiles[0].generatedLooseTicketDepositCount = 16;
        config.actProfiles[0].generatedLooseHealingPropCount = 6;

        WorldGenerationResult result = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(1, 1, 501));

        Dictionary<int, int> sectors = RewardSectorCounts(result.GeneratedMap, result.RewardSpawnRequests);

        Assert.That(result.RewardSpawnRequests.Count, Is.GreaterThanOrEqualTo(3));
        Assert.That(result.RewardSpawnRequests.Count, Is.LessThanOrEqualTo(42));
        Assert.That(sectors.Count, Is.GreaterThanOrEqualTo(Mathf.Min(3, result.RewardSpawnRequests.Count)));
        Assert.That(sectors.Values.Max(), Is.LessThanOrEqualTo(Mathf.CeilToInt(result.RewardSpawnRequests.Count * 0.75f)));

        Object.DestroyImmediate(result.GeneratedRoot);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void ProceduralRunWorldGeneratorDefaultActOneDoesNotCollapseRewardsOntoOneSector()
    {
        RunWorldGenerationConfig config = RunWorldGenerationConfig.CreateRuntimeDefault();
        config.runtimeMeshTerrainEnabled = true;
        config.hidePrototypeArenaWhenGenerated = false;

        WorldGenerationResult result = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(1, 1, 4242));

        Dictionary<int, int> sectors = RewardSectorCounts(result.GeneratedMap, result.RewardSpawnRequests);

        Assert.That(config.actProfiles[0].PlateauCount, Is.EqualTo(6));
        Assert.That(result.RewardSpawnRequests.Count, Is.GreaterThanOrEqualTo(12));
        Assert.That(result.RewardSpawnRequests.Count, Is.LessThanOrEqualTo(TotalRewardRequestCount(config.ProfileForAct(1))));
        Assert.That(result.RewardSpawnRequests.Count(IsPocketRequest), Is.GreaterThan(0));
        Assert.That(result.RewardSpawnRequests.Count(IsLooseRequest), Is.GreaterThan(0));
        Assert.That(sectors.Count, Is.GreaterThanOrEqualTo(4));
        Assert.That(sectors.Values.Max(), Is.LessThanOrEqualTo(Mathf.CeilToInt(result.RewardSpawnRequests.Count * 0.55f)));

        Object.DestroyImmediate(result.GeneratedRoot);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void GeneratedWorldSpawnPointResolverSelectsNearestSpawnSafeSample()
    {
        GeneratedWorldMap map = TinyGeneratedWorldMap();
        Vector3 desired = map.PositionFor(4, 2) + new Vector3(0.8f, 5f, 0.1f);

        bool resolved = GeneratedWorldSpawnPointResolver.TryResolve(map, desired, 3f, out Vector3 result);

        Assert.That(resolved, Is.True);
        Assert.That(result, Is.EqualTo(map.PositionFor(4, 2)));
    }

    [Test]
    public void GeneratedWorldSpawnPointResolverFallsBackWhenNoSampleIsInRange()
    {
        GeneratedWorldMap map = TinyGeneratedWorldMap();
        Vector3 desired = map.PositionFor(4, 2) + Vector3.right * 20f;

        bool resolved = GeneratedWorldSpawnPointResolver.TryResolve(map, desired, 3f, out Vector3 result);

        Assert.That(resolved, Is.False);
        Assert.That(result, Is.EqualTo(desired));
    }

    [Test]
    public void GeneratedWorldSpawnPointResolverSkipsCapsuleBlockedSamples()
    {
        GeneratedWorldMap map = TinyGeneratedWorldMap();
        Vector3 desired = map.PositionFor(2, 2);
        CreateSurface("TCO Test Spawn Ground", new Vector3(0f, -0.05f, 0f), new Vector3(12f, 0.1f, 12f));
        CreateSurface("TCO Test Spawn Wall", new Vector3(0.35f, 0.75f, 0f), new Vector3(0.45f, 1.5f, 2f));
        Physics.SyncTransforms();

        var query = new WorldPhysicsQuery();
        var clearance = new GeneratedWorldSpawnClearance(
            radius: 0.5f,
            height: 2f,
            groundClearance: 0.08f,
            probeHeight: 4f,
            probeDepth: 6f,
            environmentMask: TestSurfaceMask,
            query);

        bool resolved = GeneratedWorldSpawnPointResolver.TryResolve(
            map,
            desired,
            searchRadius: 5f,
            clearance,
            out Vector3 result);
        bool clear = query.IsCapsuleClear(
            result + Vector3.up * 0.5f,
            result + Vector3.up * 1.5f,
            0.5f,
            TestSurfaceMask);

        Assert.That(resolved, Is.True);
        Assert.That(Vector2.Distance(new Vector2(result.x, result.z), new Vector2(desired.x, desired.z)), Is.GreaterThan(1f));
        Assert.That(clear, Is.True);
    }

    [Test]
    public void ProceduralRunWorldGeneratorProfilesScaleActSizeWithStableSampleCounts()
    {
        RunWorldGenerationConfig config = RunWorldGenerationConfig.CreateRuntimeDefault();
        config.runtimeMeshTerrainEnabled = false;

        WorldGenerationResult actOne = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(1, 1, 42));
        WorldGenerationResult actThree = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(3, 3, 42));

        Assert.That(actOne.GeneratedMap.PlayableRadius, Is.EqualTo(256f));
        Assert.That(actThree.GeneratedMap.PlayableRadius, Is.EqualTo(512f));
        Assert.That(actOne.GeneratedMap.Width, Is.EqualTo(129));
        Assert.That(actThree.GeneratedMap.Width, Is.EqualTo(129));
        Assert.That(actThree.GeneratedMap.MaxHeight, Is.EqualTo(60f));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void ProceduralRunWorldGeneratorDefaultsToStructuralTerrainGrammar()
    {
        RunWorldGenerationConfig config = RunWorldGenerationConfig.CreateRuntimeDefault();

        Assert.That(config.terrainMode, Is.EqualTo(RunWorldTerrainMode.StructuralGrammar));
        Assert.That(config.defaultLayoutMode, Is.EqualTo(RunWorldLayoutMode.Generated));
        Assert.That(config.runtimeMeshTerrainEnabled, Is.True);
        Assert.That(config.hidePrototypeArenaWhenGenerated, Is.True);
        Assert.That(config.actProfiles[0].PlateauCount, Is.EqualTo(6));
        Assert.That(config.actProfiles[1].PlateauCount, Is.EqualTo(8));
        Assert.That(config.actProfiles[2].PlateauCount, Is.EqualTo(10));
        Assert.That(config.actProfiles[0].SurfaceVariationStrength, Is.Zero);
        Assert.That(config.actProfiles[0].Angularity, Is.GreaterThanOrEqualTo(0.8f));
        Assert.That(config.actProfiles[0].GeneratedChestCount, Is.EqualTo(14));
        Assert.That(config.actProfiles[0].GeneratedTicketDepositCount, Is.EqualTo(36));
        Assert.That(config.actProfiles[0].GeneratedHealingPropCount, Is.EqualTo(12));
        Assert.That(config.actProfiles[0].GeneratedLooseChestCount, Is.EqualTo(28));
        Assert.That(config.actProfiles[0].GeneratedLooseTicketDepositCount, Is.EqualTo(72));
        Assert.That(config.actProfiles[0].GeneratedLooseHealingPropCount, Is.EqualTo(24));
        Assert.That(config.actProfiles[1].GeneratedChestCount, Is.EqualTo(20));
        Assert.That(config.actProfiles[1].GeneratedTicketDepositCount, Is.EqualTo(54));
        Assert.That(config.actProfiles[1].GeneratedHealingPropCount, Is.EqualTo(16));
        Assert.That(config.actProfiles[1].GeneratedLooseChestCount, Is.EqualTo(40));
        Assert.That(config.actProfiles[1].GeneratedLooseTicketDepositCount, Is.EqualTo(108));
        Assert.That(config.actProfiles[1].GeneratedLooseHealingPropCount, Is.EqualTo(32));
        Assert.That(config.actProfiles[2].GeneratedChestCount, Is.EqualTo(28));
        Assert.That(config.actProfiles[2].GeneratedTicketDepositCount, Is.EqualTo(72));
        Assert.That(config.actProfiles[2].GeneratedHealingPropCount, Is.EqualTo(22));
        Assert.That(config.actProfiles[2].GeneratedLooseChestCount, Is.EqualTo(56));
        Assert.That(config.actProfiles[2].GeneratedLooseTicketDepositCount, Is.EqualTo(144));
        Assert.That(config.actProfiles[2].GeneratedLooseHealingPropCount, Is.EqualTo(44));
        Assert.That(config.GeneratedChestSpacing, Is.EqualTo(18f));
        Assert.That(config.GeneratedTicketDepositSpacing, Is.EqualTo(10f));
        Assert.That(config.GeneratedHealingPropSpacing, Is.EqualTo(14f));
        Assert.That(config.GeneratedLooseChestSpacing, Is.EqualTo(12f));
        Assert.That(config.GeneratedLooseTicketDepositSpacing, Is.EqualTo(7f));
        Assert.That(config.GeneratedLooseHealingPropSpacing, Is.EqualTo(9f));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void ProceduralRunWorldGeneratorReviewSeedsPassStructuralValidation()
    {
        RunWorldGenerationConfig config = RunWorldGenerationConfig.CreateRuntimeDefault();
        config.runtimeMeshTerrainEnabled = false;
        var generator = new ProceduralRunWorldGenerator(config: config);
        int[] reviewSeeds = { 1, 2, 3, 4, 5 };

        for (int actNumber = 1; actNumber <= 3; actNumber++)
        {
            foreach (int seed in reviewSeeds)
            {
                WorldGenerationResult result =
                    generator.Generate(new WorldGenerationRequest(actNumber, actNumber, seed));
                GeneratedWorldMap map = result.GeneratedMap;

                Assert.That(result.HasGeneratedMap, Is.True, $"act={actNumber} seed={seed}");
                Assert.That(map.HasStructuralValidation, Is.True, $"act={actNumber} seed={seed}");
                Assert.That(map.CountMask(GeneratedWorldMask.Reachable), Is.GreaterThan(0), $"act={actNumber} seed={seed}");
                Assert.That(map.CountMask(GeneratedWorldMask.RewardSafe), Is.GreaterThan(0), $"act={actNumber} seed={seed}");
                Assert.That(map.CountMask(GeneratedWorldMask.SpawnSafe), Is.GreaterThan(0), $"act={actNumber} seed={seed}");
                Assert.That(
                    map.Samples[map.PlayerStartIndex].HasMask(GeneratedWorldMask.Reachable),
                    Is.True,
                    $"act={actNumber} seed={seed}");

                BlockyCircusGroundsSandboxValidation validation = map.StructuralValidation;
                Assert.That(validation.AcceptedRampCount, Is.GreaterThan(0), $"act={actNumber} seed={seed}");
                Assert.That(
                    validation.PitTrapValid,
                    Is.True,
                    $"act={actNumber} seed={seed} trapSamples={validation.TrapSampleCount} unrepairedBasins={validation.UnrepairedBasinCount}");
                Assert.That(
                    validation.RewardPlacementValid,
                    Is.True,
                    $"act={actNumber} seed={seed} unreachableRewardSamples={validation.UnreachableRewardSampleCount}");
                Assert.That(
                    validation.OuterSectorCoverageValid,
                    Is.True,
                    $"act={actNumber} seed={seed} outerCoverage={validation.OuterSectorCoverage}/{validation.OuterSectorCount}");
            }
        }

        Object.DestroyImmediate(config);
    }

    [Test]
    public void ProceduralRunWorldGeneratorReviewSeedsKeepRewardRequestsOnValidGeneratedMasks()
    {
        RunWorldGenerationConfig config = RunWorldGenerationConfig.CreateRuntimeDefault();
        config.runtimeMeshTerrainEnabled = false;
        var generator = new ProceduralRunWorldGenerator(config: config);
        int[] reviewSeeds = { 1, 2, 3, 4, 5 };

        for (int actNumber = 1; actNumber <= 3; actNumber++)
        {
            RunWorldGenerationProfile profile = config.ProfileForAct(actNumber);
            foreach (int seed in reviewSeeds)
            {
                WorldGenerationResult result =
                    generator.Generate(new WorldGenerationRequest(actNumber, actNumber, seed));
                GeneratedWorldMap map = result.GeneratedMap;
                IReadOnlyList<WorldRewardSpawnRequest> requests =
                    GeneratedWorldRewardPlanner.BuildRequests(map, profile, config);

                Assert.That(requests.Count, Is.GreaterThan(0), $"act={actNumber} seed={seed}");
                Assert.That(requests.Count, Is.LessThanOrEqualTo(TotalRewardRequestCount(profile)), $"act={actNumber} seed={seed}");
                Assert.That(requests.Any(IsPocketRequest), Is.True, $"act={actNumber} seed={seed}");
                Assert.That(requests.Any(IsLooseRequest), Is.True, $"act={actNumber} seed={seed}");
                Assert.That(
                    requests.Where(IsPocketRequest).All(request => MatchesRewardSafeSample(map, request.Position)),
                    Is.True,
                    $"act={actNumber} seed={seed}");
                Assert.That(
                    requests.Where(IsLooseRequest).All(request => MatchesLooseReachableSample(map, request.Position)),
                    Is.True,
                    $"act={actNumber} seed={seed}");
            }
        }

        Object.DestroyImmediate(config);
    }

    [Test]
    public void ProceduralRunWorldGeneratorUsesPrototypeWhenLayoutModeSelectsPrototype()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.runtimeMeshTerrainEnabled = true;
        config.hidePrototypeArenaWhenGenerated = true;
        RunWorldLayoutState.ClearSessionOverrideForTests();
        var layoutState = new RunWorldLayoutState(RunWorldLayoutMode.Generated);
        layoutState.SetMode(RunWorldLayoutMode.Prototype);

        WorldGenerationResult result = new ProceduralRunWorldGenerator(null, config, layoutState: layoutState)
            .Generate(new WorldGenerationRequest(1, 1, 2029));

        Assert.That(result.UsedPrototypePlacement, Is.True);
        Assert.That(result.HasGeneratedMap, Is.True);
        Assert.That(result.HasGeneratedLayout, Is.False);
        Assert.That(result.GeneratedRoot, Is.Null);
        Assert.That(result.HasPlayerStart, Is.False);

        RunWorldLayoutState.ClearSessionOverrideForTests();
        Object.DestroyImmediate(config);
    }

    [Test]
    public void RunWorldLayoutStatePersistsSessionChoiceAcrossReloadStyleRecreation()
    {
        RunWorldLayoutState.ClearSessionOverrideForTests();
        var first = new RunWorldLayoutState(RunWorldLayoutMode.Generated);

        first.SetMode(RunWorldLayoutMode.Prototype);
        var recreated = new RunWorldLayoutState(RunWorldLayoutMode.Generated);

        Assert.That(recreated.Mode, Is.EqualTo(RunWorldLayoutMode.Prototype));

        RunWorldLayoutState.ClearSessionOverrideForTests();
    }

    [Test]
    public void ProceduralRunWorldGeneratorCreatesBroadStructuralSurfaces()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.terrainMode = RunWorldTerrainMode.StructuralGrammar;
        config.runtimeMeshTerrainEnabled = false;

        WorldGenerationResult result = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(2, 2, 4242));

        GeneratedWorldMap map = result.GeneratedMap;
        int raisedRewardSafeSamples = map.Samples.Count(sample =>
            sample.HasMask(GeneratedWorldMask.RewardSafe)
            && sample.Height >= map.MaxHeight * 0.25f
            && sample.SlopeDegrees <= 12f);
        int highReachableSamples = map.Samples.Count(sample =>
            sample.HasMask(GeneratedWorldMask.Reachable)
            && sample.Height >= map.MaxHeight * 0.45f);

        Assert.That(map.UsedFallback, Is.False);
        Assert.That(raisedRewardSafeSamples, Is.GreaterThan(0));
        Assert.That(highReachableSamples, Is.GreaterThan(0));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void ProceduralRunWorldGeneratorKeepsBlockyRampsAsConstructedMeshes()
    {
        RunWorldGenerationConfig config = RunWorldGenerationConfig.CreateRuntimeDefault();
        config.terrainMode = RunWorldTerrainMode.StructuralGrammar;
        config.runtimeMeshTerrainEnabled = true;
        config.hidePrototypeArenaWhenGenerated = false;

        WorldGenerationResult result = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(1, 1, 9191));

        GeneratedWorldSample[] rampSamples = result.GeneratedMap.Samples
            .Where(sample => sample.IsRamp)
            .ToArray();
        Transform rampRoot = result.GeneratedRoot.transform.Find("Constructed Ramp Meshes");
        MeshCollider[] rampColliders = rampRoot != null
            ? rampRoot.GetComponentsInChildren<MeshCollider>()
            : System.Array.Empty<MeshCollider>();
        GeneratedRampMarker[] rampMarkers = rampRoot != null
            ? rampRoot.GetComponentsInChildren<GeneratedRampMarker>()
            : System.Array.Empty<GeneratedRampMarker>();

        Assert.That(rampSamples, Is.Empty);
        Assert.That(rampRoot, Is.Not.Null);
        Assert.That(rampColliders.Length, Is.GreaterThanOrEqualTo(18));
        Assert.That(rampMarkers.Length, Is.EqualTo(rampColliders.Length));
        Assert.That(rampColliders.All(collider => collider.GetComponent<GeneratedRampMarker>() != null), Is.True);

        Object.DestroyImmediate(result.GeneratedRoot);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void ProceduralRunWorldGeneratorPreservesActMetadataWhenUsingBlockyGrammar()
    {
        RunWorldGenerationConfig config = RunWorldGenerationConfig.CreateRuntimeDefault();
        config.terrainMode = RunWorldTerrainMode.StructuralGrammar;
        config.runtimeMeshTerrainEnabled = false;

        WorldGenerationResult result = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(3, 3, 31313));

        Assert.That(result.GeneratedMap.ActNumber, Is.EqualTo(3));
        Assert.That(result.GeneratedMap.Seed, Is.EqualTo(31313));
        Assert.That(result.GeneratedMap.Width, Is.EqualTo(129));
        Assert.That(result.GeneratedMap.PlayableRadius, Is.EqualTo(512f));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void ProceduralRunWorldGeneratorKeepsSmoothStampModeAsDebugFallback()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.terrainMode = RunWorldTerrainMode.SmoothStamps;
        config.runtimeMeshTerrainEnabled = false;

        WorldGenerationResult result = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(1, 1, 31337));

        bool hasHeightVariation = result.GeneratedMap.Samples.Any(sample => sample.Height > 0.1f);

        Assert.That(result.HasGeneratedMap, Is.True);
        Assert.That(result.GeneratedMap.UsedFallback, Is.False);
        Assert.That(hasHeightVariation, Is.True);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void WorldRewardSpawnPlannerIgnoresGeneratedMapWhilePrototypePlacementIsActive()
    {
        var state = new RunWorldGenerationState();
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.runtimeMeshTerrainEnabled = false;
        WorldGenerationResult generatedData = new ProceduralRunWorldGenerator(state, config)
            .Generate(new WorldGenerationRequest(1, 1, 123));
        state.Set(new WorldGenerationResult(
            generatedData.WorldIndex,
            generatedData.ActNumber,
            generatedData.Seed,
            usedPrototypePlacement: true,
            generatedRoot: null,
            rewardSpawnRequests: null,
            enemySpawnBands: null,
            playerStart: Vector3.zero,
            hasPlayerStart: false,
            headlinerAnchor: Vector3.zero,
            hasHeadlinerAnchor: false,
            stageDoorAnchor: Vector3.zero,
            hasStageDoorAnchor: false,
            bounds: generatedData.Bounds,
            generatedMap: generatedData.GeneratedMap));
        WorldRewardPlacementCatalog catalog = TheCircussyOneTestObjects.CreateWorldRewardPlacementCatalog(
            TheCircussyOneTestObjects.CreateWorldRewardPlacement(
                "authored_chest",
                "Authored Chest",
                WorldRewardPlacementKind.Chest,
                "locked_chest",
                new Vector3(1f, 0f, 1f)));

        var planner = new WorldRewardSpawnPlanner(catalog, generationState: state);

        WorldRewardSpawnRequest request = planner.RequestsFor(WorldRewardPlacementKind.Chest).Single();

        Assert.That(request.PlacementId, Is.EqualTo("authored_chest"));
        Assert.That(request.Generated, Is.False);
        Assert.That(request.Position, Is.EqualTo(new Vector3(1f, 0f, 1f)));

        Object.DestroyImmediate(config);
        Object.DestroyImmediate(catalog);
    }

    [Test]
    public void GeneratedWorldTerrainBuilderCreatesChunkedMeshesAndColliders()
    {
        GeneratedWorldMap map = TinyGeneratedWorldMap();

        GeneratedWorldTerrainBuildResult result = GeneratedWorldTerrainBuilder.Build(
            map,
            new GeneratedWorldTerrainBuildSettings(material: null, chunkQuadsPerSide: 2));

        Assert.That(result.Success, Is.True);
        Assert.That(result.Root, Is.Not.Null);
        Assert.That(result.ChunkCount, Is.EqualTo(4));
        Assert.That(result.Root.transform.childCount, Is.EqualTo(4));

        for (int i = 0; i < result.Root.transform.childCount; i++)
        {
            Transform child = result.Root.transform.GetChild(i);
            Assert.That(child.GetComponent<MeshFilter>(), Is.Not.Null);
            Assert.That(child.GetComponent<MeshRenderer>(), Is.Not.Null);
            Assert.That(child.GetComponent<MeshCollider>(), Is.Not.Null);
            Assert.That(child.GetComponent<MeshCollider>().sharedMesh, Is.SameAs(child.GetComponent<MeshFilter>().sharedMesh));
            Mesh mesh = child.GetComponent<MeshFilter>().sharedMesh;
            Assert.That(mesh.vertexCount, Is.GreaterThanOrEqualTo(16));
            int topLikeTriangles = mesh.GetTriangles(0).Length
                + (mesh.subMeshCount > 2 ? mesh.GetTriangles(2).Length : 0);
            Assert.That(topLikeTriangles, Is.GreaterThan(0));
        }

        Object.DestroyImmediate(result.Root);
    }

    [Test]
    public void GeneratedWorldTerrainBuilderCreatesVerticalFacesForLargeHeightSteps()
    {
        GeneratedWorldMap map = SteppedGeneratedWorldMap();

        GeneratedWorldTerrainBuildResult result = GeneratedWorldTerrainBuilder.Build(
            map,
            new GeneratedWorldTerrainBuildSettings(material: null, chunkQuadsPerSide: 3));

        Mesh mesh = result.Root.GetComponentInChildren<MeshFilter>().sharedMesh;
        bool hasVerticalFaceNormal = mesh.normals.Any(normal => Mathf.Abs(Vector3.Dot(normal.normalized, Vector3.up)) < 0.1f);

        Assert.That(result.Success, Is.True);
        Assert.That(hasVerticalFaceNormal, Is.True);

        Object.DestroyImmediate(result.Root);
    }

    [Test]
    public void GeneratedWorldTerrainBuilderUsesSeparateWallMaterialForLargeHeightSteps()
    {
        GeneratedWorldMap map = SteppedGeneratedWorldMap();
        Material topMaterial = new(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"))
        {
            name = "TCO Test Top Terrain Material"
        };

        GeneratedWorldTerrainBuildResult result = GeneratedWorldTerrainBuilder.Build(
            map,
            new GeneratedWorldTerrainBuildSettings(topMaterial, chunkQuadsPerSide: 3));

        MeshRenderer renderer = result.Root.GetComponentInChildren<MeshRenderer>();
        Mesh mesh = result.Root.GetComponentInChildren<MeshFilter>().sharedMesh;

        Assert.That(result.Success, Is.True);
        Assert.That(mesh.subMeshCount, Is.EqualTo(3));
        Assert.That(mesh.GetTriangles(1).Length, Is.GreaterThan(0));
        Assert.That(renderer.sharedMaterials.Length, Is.EqualTo(3));
        Assert.That(renderer.sharedMaterials[0], Is.SameAs(topMaterial));
        Assert.That(renderer.sharedMaterials[1], Is.Not.SameAs(topMaterial));
        Assert.That(renderer.sharedMaterials[2], Is.Not.Null);

        Object.DestroyImmediate(result.Root);
        Object.DestroyImmediate(topMaterial);
    }

    [Test]
    public void GeneratedWorldTerrainBuilderAddsSmoothSurfaceZoneVertexColors()
    {
        GeneratedWorldMap map = MixedSurfaceGeneratedWorldMap();
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.generatedTerrainSurfaceTextureResolution = 128;
        config.generatedTerrainSurfaceZoneVarietyEnabled = true;
        config.generatedTerrainSurfaceZoneCount = 4;
        config.generatedTerrainSurfaceZonePaletteStrength = 1f;
        config.generatedTerrainSurfaceZoneBlendSharpness = 1.15f;

        GeneratedWorldSurfaceMaterialSet materialSet =
            GeneratedWorldSurfaceTextureGenerator.GenerateMaterialSet(
                fallbackMaterial: null,
                config,
                seed: 2034,
                actNumber: 1);

        GeneratedWorldTerrainBuildResult result = GeneratedWorldTerrainBuilder.Build(
            map,
            new GeneratedWorldTerrainBuildSettings(materialSet, chunkQuadsPerSide: 3));

        MeshFilter terrainFilter = result.Root.transform.Find("Generated Terrain Chunk 000").GetComponent<MeshFilter>();
        MeshRenderer terrainRenderer = result.Root.transform.Find("Generated Terrain Chunk 000").GetComponent<MeshRenderer>();
        Color[] colors = terrainFilter.sharedMesh.colors;

        Assert.That(result.Success, Is.True);
        Assert.That(result.Root.transform.Find("Surface Blend Overlays"), Is.Null);
        Assert.That(terrainFilter.sharedMesh.subMeshCount, Is.EqualTo(3));
        Assert.That(colors.Length, Is.EqualTo(terrainFilter.sharedMesh.vertexCount));
        Assert.That(colors.Any(color => ColorDistance(color, Color.white) > 0.004f), Is.True);
        Assert.That(terrainRenderer.sharedMaterials[0], Is.SameAs(materialSet.TopMaterial));
        Assert.That(terrainRenderer.sharedMaterials[2], Is.SameAs(materialSet.OrganicMaterial));

        Object.DestroyImmediate(result.Root);
        materialSet.Dispose();
        Object.DestroyImmediate(config);
    }

    [Test]
    public void GeneratedWorldTerrainBuilderKeepsWallNormalsValidForDoubleSidedLedges()
    {
        GeneratedWorldMap map = SteppedGeneratedWorldMap();

        GeneratedWorldTerrainBuildResult result = GeneratedWorldTerrainBuilder.Build(
            map,
            new GeneratedWorldTerrainBuildSettings(material: null, chunkQuadsPerSide: 3));

        Mesh mesh = result.Root.GetComponentInChildren<MeshFilter>().sharedMesh;
        int[] wallTriangles = mesh.GetTriangles(1);
        Vector3[] normals = mesh.normals;

        Assert.That(wallTriangles.Length, Is.GreaterThan(0));
        foreach (int index in wallTriangles)
        {
            Assert.That(normals[index].sqrMagnitude, Is.GreaterThan(0.95f));
        }

        Object.DestroyImmediate(result.Root);
    }

    [Test]
    public void GeneratedWorldTerrainBuilderAssignsFallbackMaterialWhenConfigHasNone()
    {
        GeneratedWorldMap map = TinyGeneratedWorldMap();

        GeneratedWorldTerrainBuildResult result = GeneratedWorldTerrainBuilder.Build(
            map,
            new GeneratedWorldTerrainBuildSettings(material: null, chunkQuadsPerSide: 2));

        Assert.That(result.Success, Is.True);
        foreach (MeshRenderer renderer in result.Root.GetComponentsInChildren<MeshRenderer>())
        {
            Assert.That(renderer.sharedMaterial, Is.Not.Null);
            Assert.That(renderer.sharedMaterial.shader, Is.Not.Null);
            Assert.That(renderer.sharedMaterial.shader.isSupported, Is.True);
        }

        Object.DestroyImmediate(result.Root);
    }

    [Test]
    public void GeneratedWorldTerrainBuilderCreatesSurfaceResolvableCollider()
    {
        GeneratedWorldMap map = TinyGeneratedWorldMap();

        GeneratedWorldTerrainBuildResult result = GeneratedWorldTerrainBuilder.Build(
            map,
            new GeneratedWorldTerrainBuildSettings(material: null, chunkQuadsPerSide: 2));
        Physics.SyncTransforms();

        Vector3 expected = map.PositionFor(2, 2);
        var resolver = new WorldSurfaceResolver();
        WorldSurfaceSample sample = resolver.Resolve(
            expected,
            probeHeight: 8f,
            probeDepth: 12f,
            GameLayers.EnvironmentMaskExcludingGameplay);

        Assert.That(sample.FoundSurface, Is.True);
        Assert.That(sample.Position.y, Is.EqualTo(expected.y).Within(0.01f));

        Object.DestroyImmediate(result.Root);
    }

    [Test]
    public void ProceduralRunWorldGeneratorBuildsRuntimeMeshWhenEnabled()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.runtimeMeshTerrainEnabled = true;
        config.hidePrototypeArenaWhenGenerated = false;
        config.chunkQuadsPerSide = 4;

        WorldGenerationResult result = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(1, 1, 2026));

        Assert.That(result.UsedPrototypePlacement, Is.False);
        Assert.That(result.HasGeneratedLayout, Is.True);
        Assert.That(result.GeneratedRoot, Is.Not.Null);
        Assert.That(result.HasPlayerStart, Is.True);
        Assert.That(result.PlayerStart.y, Is.GreaterThan(result.GeneratedMap.PlayerStartPosition.y));
        Assert.That(result.GeneratedRoot.GetComponentsInChildren<MeshCollider>().Length, Is.GreaterThan(0));
        Assert.That(result.GeneratedRoot.transform.Find("Constructed Ramp Meshes"), Is.Not.Null);
        var query = new WorldPhysicsQuery();
        Assert.That(
            query.IsCapsuleClear(
                result.PlayerStart + Vector3.up * 0.62f,
                result.PlayerStart + Vector3.up * 1.38f,
                0.62f,
                GameLayers.EnvironmentMaskExcludingGameplay),
            Is.True);

        Object.DestroyImmediate(result.GeneratedRoot);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void ProceduralRunWorldGeneratorUsesConfiguredTerrainMaterial()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.runtimeMeshTerrainEnabled = true;
        config.hidePrototypeArenaWhenGenerated = false;
        config.chunkQuadsPerSide = 4;
        config.generatedTerrainSurfaceTexturesEnabled = false;
        Material material = new(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"))
        {
            name = "TCO Test Generated Terrain Material"
        };
        config.terrainMaterial = material;

        WorldGenerationResult result = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(1, 1, 2028));

        Assert.That(result.UsedPrototypePlacement, Is.False);
        foreach (MeshRenderer renderer in result.GeneratedRoot.GetComponentsInChildren<MeshRenderer>())
        {
            Assert.That(renderer.sharedMaterial, Is.SameAs(material));
        }

        Object.DestroyImmediate(result.GeneratedRoot);
        Object.DestroyImmediate(material);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void ProceduralRunWorldGeneratorUsesGeneratedSurfaceMaterialsWhenEnabled()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.runtimeMeshTerrainEnabled = true;
        config.hidePrototypeArenaWhenGenerated = false;
        config.chunkQuadsPerSide = 4;
        config.generatedTerrainSurfaceTexturesEnabled = true;
        config.generatedTerrainSurfaceTextureResolution = 128;

        WorldGenerationResult result = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(1, 1, 2029));

        MeshRenderer[] renderers = result.GeneratedRoot.GetComponentsInChildren<MeshRenderer>();

        Assert.That(result.UsedPrototypePlacement, Is.False);
        Assert.That(renderers.Any(renderer =>
            renderer.sharedMaterials.Length > 2
            && renderer.sharedMaterials[0] != null
            && renderer.sharedMaterials[0].name.Contains("Top Surface")), Is.True);
        Assert.That(renderers.Any(renderer =>
            renderer.sharedMaterials.Length > 2
            && renderer.sharedMaterials[2] != null
            && renderer.sharedMaterials[2].name.Contains("Organic Ground Surface")), Is.True);

        Object.DestroyImmediate(result.GeneratedRoot);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void ProceduralRunWorldGeneratorUsesSurfaceZoneVertexColorsWithinSameAct()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.runtimeMeshTerrainEnabled = true;
        config.hidePrototypeArenaWhenGenerated = false;
        config.chunkQuadsPerSide = 3;
        config.generatedTerrainSurfaceTexturesEnabled = true;
        config.generatedTerrainSurfaceTextureResolution = 128;
        config.generatedTerrainSurfaceZoneVarietyEnabled = true;
        config.generatedTerrainSurfaceZoneCount = 4;
        config.generatedTerrainSurfaceZonePaletteStrength = 0.45f;

        WorldGenerationResult result = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(1, 1, 2034));

        MeshRenderer[] terrainRenderers = result.GeneratedRoot
            .GetComponentsInChildren<MeshRenderer>()
            .Where(renderer => renderer.gameObject.name.StartsWith("Generated Terrain Chunk"))
            .ToArray();
        Material[] topMaterials = terrainRenderers
            .Select(renderer => renderer.sharedMaterials.Length > 0 ? renderer.sharedMaterials[0] : null)
            .Where(material => material != null)
            .Distinct()
            .ToArray();
        bool hasTintedVertices = result.GeneratedRoot
            .GetComponentsInChildren<MeshFilter>()
            .Where(filter => filter.gameObject.name.StartsWith("Generated Terrain Chunk"))
            .SelectMany(filter => filter.sharedMesh.colors)
            .Any(color => ColorDistance(color, Color.white) > 0.004f);

        Assert.That(terrainRenderers.Length, Is.GreaterThan(1));
        Assert.That(topMaterials.Length, Is.EqualTo(1));
        Assert.That(hasTintedVertices, Is.True);

        Object.DestroyImmediate(result.GeneratedRoot);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void GeneratedWorldSurfaceTextureGeneratorBuildsDeterministicSeedVariantData()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.generatedTerrainSurfaceTextureResolution = 128;
        config.generatedTerrainOrganicTextureScale = 0.45f;
        GeneratedWorldSurfaceTextureSettings firstSettings =
            GeneratedWorldSurfaceTextureGenerator.Snapshot(config, seed: 17, actNumber: 1);
        GeneratedWorldSurfaceTextureSettings repeatSettings =
            GeneratedWorldSurfaceTextureGenerator.Snapshot(config, seed: 17, actNumber: 1);
        GeneratedWorldSurfaceTextureSettings variantSettings =
            GeneratedWorldSurfaceTextureGenerator.Snapshot(config, seed: 18, actNumber: 1);

        GeneratedWorldSurfaceTextureData first = GeneratedWorldSurfaceTextureGenerator.BuildTextureData(
            firstSettings,
            GeneratedWorldSurfaceMaterialRole.Top);
        GeneratedWorldSurfaceTextureData repeat = GeneratedWorldSurfaceTextureGenerator.BuildTextureData(
            repeatSettings,
            GeneratedWorldSurfaceMaterialRole.Top);
        GeneratedWorldSurfaceTextureData variant = GeneratedWorldSurfaceTextureGenerator.BuildTextureData(
            variantSettings,
            GeneratedWorldSurfaceMaterialRole.Top);

        Assert.That(first.Width, Is.EqualTo(128));
        Assert.That(first.Pixels, Is.EqualTo(repeat.Pixels));
        Assert.That(first.Pixels.Where((pixel, index) => !pixel.Equals(variant.Pixels[index])).Any(), Is.True);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void GeneratedWorldSurfaceTextureGeneratorHonorsForcedSurfaceStyleModes()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();

        config.generatedTerrainSurfaceStyleMode = GeneratedWorldSurfaceStyleMode.ClassicBigTop;
        GeneratedWorldSurfaceTextureSettings bright =
            GeneratedWorldSurfaceTextureGenerator.Snapshot(config, seed: 17, actNumber: 1);

        config.generatedTerrainSurfaceStyleMode = GeneratedWorldSurfaceStyleMode.CandyChecker;
        GeneratedWorldSurfaceTextureSettings candy =
            GeneratedWorldSurfaceTextureGenerator.Snapshot(config, seed: 17, actNumber: 1);

        config.generatedTerrainSurfaceStyleMode = GeneratedWorldSurfaceStyleMode.PaintedBoardwalk;
        GeneratedWorldSurfaceTextureSettings boardwalk =
            GeneratedWorldSurfaceTextureGenerator.Snapshot(config, seed: 17, actNumber: 1);

        Assert.That(bright.StyleMode, Is.EqualTo(GeneratedWorldSurfaceStyleMode.ClassicBigTop));
        Assert.That(candy.StyleMode, Is.EqualTo(GeneratedWorldSurfaceStyleMode.CandyChecker));
        Assert.That(boardwalk.StyleMode, Is.EqualTo(GeneratedWorldSurfaceStyleMode.PaintedBoardwalk));
        Assert.That(bright.SelectedStyleProfileIndex, Is.EqualTo(0));
        Assert.That(candy.SelectedStyleProfileIndex, Is.EqualTo(1));
        Assert.That(boardwalk.SelectedStyleProfileIndex, Is.EqualTo(2));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void GeneratedWorldSurfaceTextureGeneratorSelectsSeededStyleProfileDeterministically()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.generatedTerrainSurfaceStyleMode = GeneratedWorldSurfaceStyleMode.SeededCircus;

        GeneratedWorldSurfaceTextureSettings first =
            GeneratedWorldSurfaceTextureGenerator.Snapshot(config, seed: 17, actNumber: 1);
        GeneratedWorldSurfaceTextureSettings repeat =
            GeneratedWorldSurfaceTextureGenerator.Snapshot(config, seed: 17, actNumber: 1);
        GeneratedWorldSurfaceTextureSettings variant =
            GeneratedWorldSurfaceTextureGenerator.Snapshot(config, seed: 94, actNumber: 2);

        Assert.That(first.StyleMode, Is.EqualTo(GeneratedWorldSurfaceStyleMode.SeededCircus));
        Assert.That(first.SelectedStyleProfileIndex, Is.EqualTo(repeat.SelectedStyleProfileIndex));
        Assert.That(first.SelectedStyleProfileIndex, Is.InRange(0, 2));
        Assert.That(variant.SelectedStyleProfileIndex, Is.InRange(0, 2));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void GeneratedWorldSurfaceTextureGeneratorBuildsDistinctForcedStyleOutputs()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.generatedTerrainSurfaceTextureResolution = 128;

        config.generatedTerrainSurfaceStyleMode = GeneratedWorldSurfaceStyleMode.ClassicBigTop;
        GeneratedWorldSurfaceTextureSettings brightSettings =
            GeneratedWorldSurfaceTextureGenerator.Snapshot(config, seed: 23, actNumber: 1);
        GeneratedWorldSurfaceTextureData bright = GeneratedWorldSurfaceTextureGenerator.BuildTextureData(
            brightSettings,
            GeneratedWorldSurfaceMaterialRole.Top);

        config.generatedTerrainSurfaceStyleMode = GeneratedWorldSurfaceStyleMode.CandyChecker;
        GeneratedWorldSurfaceTextureSettings candySettings =
            GeneratedWorldSurfaceTextureGenerator.Snapshot(config, seed: 23, actNumber: 1);
        GeneratedWorldSurfaceTextureData candy = GeneratedWorldSurfaceTextureGenerator.BuildTextureData(
            candySettings,
            GeneratedWorldSurfaceMaterialRole.Top);

        Assert.That(AveragePixelDelta(bright, candy), Is.GreaterThan(10f));
        Assert.That(AverageLuminance(bright), Is.InRange(24f, 62f));
        Assert.That(AverageLuminance(candy), Is.InRange(24f, 66f));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void GeneratedWorldSurfaceTextureGeneratorScalesOrganicPatternWithoutBreakingTextureWrap()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.generatedTerrainSurfaceTextureResolution = 128;
        config.generatedTerrainOrganicPatternStrength = 0.34f;
        config.generatedTerrainOrganicPatternIterations = 64;

        config.generatedTerrainOrganicPatternScale = 1f;
        GeneratedWorldSurfaceTextureSettings largePatternSettings =
            GeneratedWorldSurfaceTextureGenerator.Snapshot(config, seed: 29, actNumber: 1);
        GeneratedWorldSurfaceTextureData largePattern = GeneratedWorldSurfaceTextureGenerator.BuildTextureData(
            largePatternSettings,
            GeneratedWorldSurfaceMaterialRole.Organic);

        config.generatedTerrainOrganicPatternScale = 3f;
        GeneratedWorldSurfaceTextureSettings smallPatternSettings =
            GeneratedWorldSurfaceTextureGenerator.Snapshot(config, seed: 29, actNumber: 1);
        GeneratedWorldSurfaceTextureData smallPattern = GeneratedWorldSurfaceTextureGenerator.BuildTextureData(
            smallPatternSettings,
            GeneratedWorldSurfaceMaterialRole.Organic);

        int maxEdgeDelta = 0;
        for (int i = 0; i < smallPattern.Width; i++)
        {
            maxEdgeDelta = Mathf.Max(
                maxEdgeDelta,
                PixelDelta(smallPattern.Pixels[i], smallPattern.Pixels[(smallPattern.Height - 1) * smallPattern.Width + i]),
                PixelDelta(smallPattern.Pixels[i * smallPattern.Width], smallPattern.Pixels[i * smallPattern.Width + smallPattern.Width - 1]));
        }

        Assert.That(AveragePixelDelta(largePattern, smallPattern), Is.GreaterThan(6f));
        Assert.That(maxEdgeDelta, Is.LessThanOrEqualTo(12));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void GeneratedWorldSurfaceTextureGeneratorBuildsTileSafeOrganicGroundData()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.generatedTerrainSurfaceTextureResolution = 128;
        config.generatedTerrainOrganicPatternStrength = 0.18f;
        config.generatedTerrainOrganicPatternIterations = 48;
        GeneratedWorldSurfaceTextureSettings settings =
            GeneratedWorldSurfaceTextureGenerator.Snapshot(config, seed: 20, actNumber: 1);

        GeneratedWorldSurfaceTextureData organic = GeneratedWorldSurfaceTextureGenerator.BuildTextureData(
            settings,
            GeneratedWorldSurfaceMaterialRole.Organic);

        int maxEdgeDelta = 0;
        int minLuma = 255;
        int maxLuma = 0;
        for (int i = 0; i < organic.Width; i++)
        {
            maxEdgeDelta = Mathf.Max(
                maxEdgeDelta,
                PixelDelta(organic.Pixels[i], organic.Pixels[(organic.Height - 1) * organic.Width + i]),
                PixelDelta(organic.Pixels[i * organic.Width], organic.Pixels[i * organic.Width + organic.Width - 1]));
        }

        for (int i = 0; i < organic.Pixels.Length; i++)
        {
            int luma = Luminance(organic.Pixels[i]);
            minLuma = Mathf.Min(minLuma, luma);
            maxLuma = Mathf.Max(maxLuma, luma);
        }

        Assert.That(maxEdgeDelta, Is.LessThanOrEqualTo(10));
        Assert.That(maxLuma - minLuma, Is.GreaterThan(12));
        Assert.That(maxLuma - minLuma, Is.LessThan(150));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void GeneratedWorldSurfaceTextureGeneratorBuildsPerTileTopReliefData()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.generatedTerrainSurfaceTextureResolution = 128;
        config.generatedTerrainTopTileHeightVariation = 0.12f;
        GeneratedWorldSurfaceTextureSettings settings =
            GeneratedWorldSurfaceTextureGenerator.Snapshot(config, seed: 31, actNumber: 1);

        GeneratedWorldSurfaceTextureData height = GeneratedWorldSurfaceTextureGenerator.BuildHeightTextureData(
            settings,
            GeneratedWorldSurfaceMaterialRole.Top);

        int[] centerHeights =
        {
            PixelGray(height, 16, 16),
            PixelGray(height, 48, 16),
            PixelGray(height, 80, 16),
            PixelGray(height, 112, 16),
            PixelGray(height, 16, 48),
            PixelGray(height, 48, 48),
            PixelGray(height, 80, 48),
            PixelGray(height, 112, 48)
        };

        int seamHeight = PixelGray(height, 32, 16);
        Assert.That(centerHeights.Max() - centerHeights.Min(), Is.GreaterThan(16));
        Assert.That(seamHeight, Is.LessThan((centerHeights[0] + centerHeights[1]) / 2 - 8));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void GeneratedWorldSurfaceTextureGeneratorBuildsRaisedCheckerTileReliefAndRoughEdges()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.generatedTerrainSurfaceTextureResolution = 128;
        config.generatedTerrainTopTileHeightVariation = 0.16f;
        config.generatedTerrainTopTileReliefStrength = 1.45f;
        config.generatedTerrainTopTileGroutDepthStrength = 1.5f;
        config.generatedTerrainTopTileEdgeWearStrength = 1f;
        config.generatedTerrainTopTileSmoothnessVariationStrength = 1.3f;
        GeneratedWorldSurfaceTextureSettings settings =
            GeneratedWorldSurfaceTextureGenerator.Snapshot(config, seed: 33, actNumber: 1);

        GeneratedWorldSurfaceTextureData height = GeneratedWorldSurfaceTextureGenerator.BuildHeightTextureData(
            settings,
            GeneratedWorldSurfaceMaterialRole.Top);
        GeneratedWorldSurfaceTextureData smoothness = GeneratedWorldSurfaceTextureGenerator.BuildSmoothnessTextureData(
            settings,
            GeneratedWorldSurfaceMaterialRole.Top);

        int center = PixelGray(height, 16, 16);
        int edge = PixelGray(height, 28, 16);
        int seam = PixelGray(height, 32, 16);
        int otherCenter = PixelGray(height, 48, 16);
        int centerSmoothness = PixelAlpha(smoothness, 16, 16);
        int edgeSmoothness = PixelAlpha(smoothness, 28, 16);
        int seamSmoothness = PixelAlpha(smoothness, 32, 16);

        Assert.That(Mathf.Abs(center - otherCenter), Is.GreaterThan(10));
        Assert.That(edge, Is.LessThan(center - 3));
        Assert.That(seam, Is.LessThan(((center + otherCenter) / 2) - 14));
        Assert.That(edgeSmoothness, Is.LessThan(centerSmoothness));
        Assert.That(seamSmoothness, Is.LessThan(centerSmoothness - 3));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void GeneratedWorldSurfaceTextureGeneratorBuildsPerPanelWallReliefData()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.generatedTerrainSurfaceTextureResolution = 128;
        config.generatedTerrainWallPanelHeightVariation = 0.12f;
        GeneratedWorldSurfaceTextureSettings settings =
            GeneratedWorldSurfaceTextureGenerator.Snapshot(config, seed: 37, actNumber: 1);

        GeneratedWorldSurfaceTextureData height = GeneratedWorldSurfaceTextureGenerator.BuildHeightTextureData(
            settings,
            GeneratedWorldSurfaceMaterialRole.Wall);

        int[] panelHeights =
        {
            PixelGray(height, 9, 64),
            PixelGray(height, 27, 64),
            PixelGray(height, 46, 64),
            PixelGray(height, 64, 64),
            PixelGray(height, 82, 64),
            PixelGray(height, 101, 64),
            PixelGray(height, 119, 64)
        };

        int seamHeight = PixelGray(height, 18, 64);
        Assert.That(panelHeights.Max() - panelHeights.Min(), Is.GreaterThan(14));
        Assert.That(seamHeight, Is.LessThan((panelHeights[0] + panelHeights[1]) / 2 - 6));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void GeneratedWorldSurfaceTextureGeneratorBuildsDarkWallPanelSeams()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.generatedTerrainSurfaceTextureResolution = 128;
        config.generatedTerrainWallPanelSeamStrength = 1.6f;
        config.generatedTerrainWallEdgeWearStrength = 0.55f;
        GeneratedWorldSurfaceTextureSettings settings =
            GeneratedWorldSurfaceTextureGenerator.Snapshot(config, seed: 39, actNumber: 1);

        GeneratedWorldSurfaceTextureData albedo = GeneratedWorldSurfaceTextureGenerator.BuildTextureData(
            settings,
            GeneratedWorldSurfaceMaterialRole.Wall);
        GeneratedWorldSurfaceTextureData height = GeneratedWorldSurfaceTextureGenerator.BuildHeightTextureData(
            settings,
            GeneratedWorldSurfaceMaterialRole.Wall);
        GeneratedWorldSurfaceTextureData smoothness = GeneratedWorldSurfaceTextureGenerator.BuildSmoothnessTextureData(
            settings,
            GeneratedWorldSurfaceMaterialRole.Wall);

        int panelLuminance = (Luminance(PixelAt(albedo, 9, 64)) + Luminance(PixelAt(albedo, 27, 64))) / 2;
        int seamLuminance = Luminance(PixelAt(albedo, 18, 64));
        int panelHeight = (PixelGray(height, 9, 64) + PixelGray(height, 27, 64)) / 2;
        int seamHeight = PixelGray(height, 18, 64);
        int panelSmoothness = (PixelAlpha(smoothness, 9, 64) + PixelAlpha(smoothness, 27, 64)) / 2;
        int seamSmoothness = PixelAlpha(smoothness, 18, 64);

        Assert.That(seamLuminance, Is.LessThanOrEqualTo(panelLuminance + 4));
        Assert.That(seamHeight, Is.LessThan(panelHeight - 8));
        Assert.That(seamSmoothness, Is.LessThan(panelSmoothness - 2));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void GeneratedWorldSurfaceTextureGeneratorKeepsWallPanelsVerticalBoardOnly()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.generatedTerrainSurfaceTextureResolution = 128;
        config.generatedTerrainWallPanelSeamStrength = 1.6f;
        GeneratedWorldSurfaceTextureSettings settings =
            GeneratedWorldSurfaceTextureGenerator.Snapshot(config, seed: 40, actNumber: 1);

        GeneratedWorldSurfaceTextureData albedo = GeneratedWorldSurfaceTextureGenerator.BuildTextureData(
            settings,
            GeneratedWorldSurfaceMaterialRole.Wall);
        GeneratedWorldSurfaceTextureData height = GeneratedWorldSurfaceTextureGenerator.BuildHeightTextureData(
            settings,
            GeneratedWorldSurfaceMaterialRole.Wall);

        Color32 horizontalLineCandidate = PixelAt(albedo, 9, 18);
        Color32 samePanelAbove = PixelAt(albedo, 9, 15);
        Color32 samePanelBelow = PixelAt(albedo, 9, 22);
        int candidateHeight = PixelGray(height, 9, 18);
        int aboveHeight = PixelGray(height, 9, 15);
        int belowHeight = PixelGray(height, 9, 22);

        Assert.That(PixelDelta(horizontalLineCandidate, samePanelAbove), Is.LessThan(18));
        Assert.That(PixelDelta(horizontalLineCandidate, samePanelBelow), Is.LessThan(18));
        Assert.That(Mathf.Abs(candidateHeight - aboveHeight), Is.LessThan(8));
        Assert.That(Mathf.Abs(candidateHeight - belowHeight), Is.LessThan(8));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void GeneratedWorldSurfaceTextureGeneratorBuildsVisibleRampPlankSeams()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.generatedTerrainSurfaceTextureResolution = 128;
        config.generatedTerrainRampPlankSeamStrength = 1.6f;
        config.generatedTerrainRampEdgeWearStrength = 0.65f;
        config.generatedTerrainRampWoodGrainStrength = 1.25f;
        GeneratedWorldSurfaceTextureSettings settings =
            GeneratedWorldSurfaceTextureGenerator.Snapshot(config, seed: 43, actNumber: 1);

        GeneratedWorldSurfaceTextureData albedo = GeneratedWorldSurfaceTextureGenerator.BuildTextureData(
            settings,
            GeneratedWorldSurfaceMaterialRole.Ramp);
        GeneratedWorldSurfaceTextureData height = GeneratedWorldSurfaceTextureGenerator.BuildHeightTextureData(
            settings,
            GeneratedWorldSurfaceMaterialRole.Ramp);
        GeneratedWorldSurfaceTextureData smoothness = GeneratedWorldSurfaceTextureGenerator.BuildSmoothnessTextureData(
            settings,
            GeneratedWorldSurfaceMaterialRole.Ramp);

        int plankLuminance = (Luminance(PixelAt(albedo, 10, 64)) + Luminance(PixelAt(albedo, 32, 64))) / 2;
        int seamLuminance = Luminance(PixelAt(albedo, 21, 64));
        int plankHeight = (PixelGray(height, 10, 64) + PixelGray(height, 32, 64)) / 2;
        int seamHeight = PixelGray(height, 21, 64);
        int plankSmoothness = (PixelAlpha(smoothness, 10, 64) + PixelAlpha(smoothness, 32, 64)) / 2;
        int seamSmoothness = PixelAlpha(smoothness, 21, 64);

        Assert.That(seamLuminance, Is.LessThan(plankLuminance - 6));
        Assert.That(seamHeight, Is.LessThan(plankHeight - 12));
        Assert.That(seamSmoothness, Is.LessThan(plankSmoothness - 2));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void GeneratedWorldSurfaceTextureGeneratorBuildsRampEdgeWearBands()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.generatedTerrainSurfaceTextureResolution = 128;
        config.generatedTerrainRampEdgeWearStrength = 1f;
        GeneratedWorldSurfaceTextureSettings settings =
            GeneratedWorldSurfaceTextureGenerator.Snapshot(config, seed: 47, actNumber: 1);

        GeneratedWorldSurfaceTextureData albedo = GeneratedWorldSurfaceTextureGenerator.BuildTextureData(
            settings,
            GeneratedWorldSurfaceMaterialRole.Ramp);
        GeneratedWorldSurfaceTextureData smoothness = GeneratedWorldSurfaceTextureGenerator.BuildSmoothnessTextureData(
            settings,
            GeneratedWorldSurfaceMaterialRole.Ramp);

        Color32 edge = PixelAt(albedo, 2, 64);
        Color32 center = PixelAt(albedo, 64, 64);

        Assert.That(PixelDelta(edge, center), Is.GreaterThan(10));
        Assert.That(PixelAlpha(smoothness, 2, 64), Is.LessThan(PixelAlpha(smoothness, 64, 64)));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void GeneratedWorldSurfaceTextureGeneratorBuildsSmoothnessMasksForTopAndWall()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.generatedTerrainSurfaceTextureResolution = 128;
        config.generatedTerrainTopSmoothnessMin = 0.08f;
        config.generatedTerrainTopSmoothnessMax = 0.24f;
        config.generatedTerrainWallSmoothnessMin = 0.05f;
        config.generatedTerrainWallSmoothnessMax = 0.18f;
        GeneratedWorldSurfaceTextureSettings settings =
            GeneratedWorldSurfaceTextureGenerator.Snapshot(config, seed: 41, actNumber: 1);

        GeneratedWorldSurfaceTextureData top = GeneratedWorldSurfaceTextureGenerator.BuildSmoothnessTextureData(
            settings,
            GeneratedWorldSurfaceMaterialRole.Top);
        GeneratedWorldSurfaceTextureData wall = GeneratedWorldSurfaceTextureGenerator.BuildSmoothnessTextureData(
            settings,
            GeneratedWorldSurfaceMaterialRole.Wall);

        (int topMin, int topMax) = AlphaRange(top);
        (int wallMin, int wallMax) = AlphaRange(wall);

        Assert.That(topMax - topMin, Is.GreaterThan(20));
        Assert.That(wallMax - wallMin, Is.GreaterThan(16));
        Assert.That(topMax, Is.LessThanOrEqualTo(Mathf.RoundToInt((config.generatedTerrainTopSmoothnessMax + 0.05f) * 255f)));
        Assert.That(wallMax, Is.LessThanOrEqualTo(Mathf.RoundToInt((config.generatedTerrainWallSmoothnessMax + 0.04f) * 255f)));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void GeneratedWorldSurfaceTextureGeneratorCreatesDistinctRuntimeMaterials()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.generatedTerrainSurfaceTextureResolution = 128;

        GeneratedWorldSurfaceMaterialSet materialSet =
            GeneratedWorldSurfaceTextureGenerator.GenerateMaterialSet(
                fallbackMaterial: null,
                config,
                seed: 19,
                actNumber: 1);

        try
        {
            Assert.That(materialSet.OwnsRuntimeObjects, Is.True);
            Assert.That(materialSet.TopMaterial, Is.Not.Null);
            Assert.That(materialSet.OrganicMaterial, Is.Not.Null);
            Assert.That(materialSet.WallMaterial, Is.Not.Null);
            Assert.That(materialSet.RampMaterial, Is.Not.Null);
            Assert.That(materialSet.RimMaterial, Is.Not.Null);
            Assert.That(materialSet.SurfaceZoneCount, Is.EqualTo(config.GeneratedTerrainSurfaceZoneCount));
            Assert.That(materialSet.TopMaterials.Length, Is.EqualTo(config.GeneratedTerrainSurfaceZoneCount));
            Assert.That(materialSet.OrganicMaterials.Length, Is.EqualTo(config.GeneratedTerrainSurfaceZoneCount));
            Assert.That(materialSet.WallMaterials.Length, Is.EqualTo(config.GeneratedTerrainSurfaceZoneCount));
            Assert.That(materialSet.RampMaterials.Length, Is.EqualTo(config.GeneratedTerrainSurfaceZoneCount));
            Assert.That(materialSet.TopZoneTints.Length, Is.EqualTo(config.GeneratedTerrainSurfaceZoneCount));
            Assert.That(materialSet.OrganicZoneTints.Length, Is.EqualTo(config.GeneratedTerrainSurfaceZoneCount));
            Assert.That(materialSet.WallZoneTints.Length, Is.EqualTo(config.GeneratedTerrainSurfaceZoneCount));
            Assert.That(materialSet.SurfaceZoneBlendSharpness, Is.EqualTo(config.GeneratedTerrainSurfaceZoneBlendSharpness));
            Assert.That(materialSet.TopMaterialAt(1), Is.Not.SameAs(materialSet.TopMaterial));
            Assert.That(materialSet.OrganicMaterialAt(1), Is.Not.SameAs(materialSet.OrganicMaterial));
            Assert.That(materialSet.WallMaterialAt(1), Is.Not.SameAs(materialSet.WallMaterial));
            Assert.That(materialSet.RampMaterialAt(1), Is.Not.SameAs(materialSet.RampMaterial));
            Assert.That(materialSet.TopMaterial, Is.Not.SameAs(materialSet.WallMaterial));
            Assert.That(materialSet.TopMaterial, Is.Not.SameAs(materialSet.OrganicMaterial));
            Assert.That(HasSurfaceTexture(materialSet.TopMaterial), Is.True);
            Assert.That(HasSurfaceTexture(materialSet.OrganicMaterial), Is.True);
            Assert.That(HasSurfaceTexture(materialSet.WallMaterial), Is.True);
            Assert.That(HasSurfaceTexture(materialSet.RampMaterial), Is.True);
            Assert.That(HasSurfaceTexture(materialSet.RimMaterial), Is.True);
            Assert.That(HasMaterialTexture(materialSet.TopMaterial, "_BumpMap"), Is.True);
            Assert.That(HasMaterialTexture(materialSet.TopMaterial, "_ParallaxMap"), Is.True);
            Assert.That(HasMaterialTexture(materialSet.TopMaterial, "_MetallicGlossMap"), Is.True);
            Assert.That(HasMaterialTexture(materialSet.OrganicMaterial, "_BumpMap"), Is.True);
            Assert.That(HasMaterialTexture(materialSet.OrganicMaterial, "_ParallaxMap"), Is.True);
            Assert.That(HasMaterialTexture(materialSet.OrganicMaterial, "_MetallicGlossMap"), Is.True);
            Assert.That(MaterialTextureScale(materialSet.OrganicMaterial, "_BaseMap"), Is.EqualTo(Vector2.one * config.generatedTerrainOrganicTextureScale));
            Assert.That(MaterialTextureScale(materialSet.OrganicMaterial, "_BumpMap"), Is.EqualTo(Vector2.one * config.generatedTerrainOrganicTextureScale));
            Assert.That(MaterialTextureScale(materialSet.OrganicMaterial, "_ParallaxMap"), Is.EqualTo(Vector2.one * config.generatedTerrainOrganicTextureScale));
            Assert.That(MaterialTextureScale(materialSet.OrganicMaterial, "_MetallicGlossMap"), Is.EqualTo(Vector2.one * config.generatedTerrainOrganicTextureScale));
            Assert.That(HasMaterialTexture(materialSet.WallMaterial, "_BumpMap"), Is.True);
            Assert.That(HasMaterialTexture(materialSet.WallMaterial, "_ParallaxMap"), Is.True);
            Assert.That(HasMaterialTexture(materialSet.WallMaterial, "_MetallicGlossMap"), Is.True);
            Assert.That(HasMaterialTexture(materialSet.RampMaterial, "_BumpMap"), Is.True);
            Assert.That(HasMaterialTexture(materialSet.RampMaterial, "_ParallaxMap"), Is.True);
            Assert.That(HasMaterialTexture(materialSet.RampMaterial, "_MetallicGlossMap"), Is.True);
            Assert.That(HasMaterialTexture(materialSet.RimMaterial, "_BumpMap"), Is.True);
            Assert.That(HasMaterialTexture(materialSet.RimMaterial, "_ParallaxMap"), Is.True);
            Assert.That(HasMaterialTexture(materialSet.RimMaterial, "_MetallicGlossMap"), Is.True);
        }
        finally
        {
            materialSet.Dispose();
            Object.DestroyImmediate(config);
        }
    }

    [Test]
    public void GeneratedWorldSurfaceTextureGeneratorProvidesOrganicZoneTints()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.generatedTerrainSurfaceTextureResolution = 128;
        config.generatedTerrainSurfaceZoneVarietyEnabled = true;
        config.generatedTerrainSurfaceZoneCount = 4;
        config.generatedTerrainSurfaceZonePaletteStrength = 1f;

        GeneratedWorldSurfaceMaterialSet materialSet =
            GeneratedWorldSurfaceTextureGenerator.GenerateMaterialSet(
                fallbackMaterial: null,
                config,
                seed: 19,
                actNumber: 1);

        try
        {
            Assert.That(ColorDistance(materialSet.OrganicZoneTints[1], Color.white), Is.GreaterThan(0.004f));
            Assert.That(ColorDistance(materialSet.OrganicZoneTints[2], Color.white), Is.GreaterThan(0.004f));
            Assert.That(ColorDistance(materialSet.WallZoneTints[1], Color.white), Is.GreaterThan(0.004f));
        }
        finally
        {
            materialSet.Dispose();
            Object.DestroyImmediate(config);
        }
    }

    [Test]
    public void ProceduralRunWorldGeneratorKeepsPrototypePlacementWhenRuntimeMeshDisabled()
    {
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.runtimeMeshTerrainEnabled = false;

        WorldGenerationResult result = new ProceduralRunWorldGenerator(config: config)
            .Generate(new WorldGenerationRequest(1, 1, 2027));

        Assert.That(result.UsedPrototypePlacement, Is.True);
        Assert.That(result.HasGeneratedMap, Is.True);
        Assert.That(result.HasGeneratedLayout, Is.False);
        Assert.That(result.GeneratedRoot, Is.Null);
        Assert.That(result.HasPlayerStart, Is.False);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void RunWorldGenerationStateClearRestoresPrototypeArena()
    {
        GameObject floor = new("Arena Floor");
        GameObject terrain = new("Arena Terrain");
        RunWorldGenerationConfig config = SmallWorldGenerationConfig();
        config.runtimeMeshTerrainEnabled = true;
        config.hidePrototypeArenaWhenGenerated = true;
        var state = new RunWorldGenerationState();

        WorldGenerationResult result = new ProceduralRunWorldGenerator(state, config)
            .Generate(new WorldGenerationRequest(1, 1, 2028));

        Assert.That(result.GeneratedRoot, Is.Not.Null);
        Assert.That(floor.activeSelf, Is.False);
        Assert.That(terrain.activeSelf, Is.False);

        state.Clear();

        Assert.That(floor.activeSelf, Is.True);
        Assert.That(terrain.activeSelf, Is.True);

        Object.DestroyImmediate(floor);
        Object.DestroyImmediate(terrain);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void GeneratedSpawnRequestsCanCycleSequentialContent()
    {
        TicketDepositDefinition small = TheCircussyOneTestObjects.CreateTicketDepositDefinition("small_ticket_stack", "Small Ticket Stack");
        TicketDepositDefinition roll = TheCircussyOneTestObjects.CreateTicketDepositDefinition("ticket_roll", "Ticket Roll");
        var definitions = new[] { small, roll };
        var request = new WorldRewardSpawnRequest(
            "generated_ticket",
            WorldRewardPlacementKind.TicketDeposit,
            targetContentId: null,
            Vector3.zero,
            required: false,
            repeatSequentialContent: true,
            generated: true);

        int sequentialIndex = 0;
        TicketDepositDefinition first = WorldRewardSpawnPlanner.ResolveContent(definitions, request, ref sequentialIndex);
        TicketDepositDefinition second = WorldRewardSpawnPlanner.ResolveContent(definitions, request, ref sequentialIndex);
        TicketDepositDefinition third = WorldRewardSpawnPlanner.ResolveContent(definitions, request, ref sequentialIndex);

        Assert.That(first, Is.SameAs(small));
        Assert.That(second, Is.SameAs(roll));
        Assert.That(third, Is.SameAs(small));

        Object.DestroyImmediate(small);
        Object.DestroyImmediate(roll);
    }

    [Test]
    public void TicketDepositSystemUsesPlacementCatalogTargetContentIds()
    {
        TicketDepositDefinition small = TheCircussyOneTestObjects.CreateTicketDepositDefinition("small_ticket_stack", "TCO Test Small Ticket Stack");
        TicketDepositDefinition large = TheCircussyOneTestObjects.CreateTicketDepositDefinition("jackpot_cache", "TCO Test Jackpot Cache");
        large.tier = TicketDepositTier.Large;
        large.placement = WorldPropPlacementProfile.TicketDepositDefault(TicketDepositTier.Large);
        TicketDepositCatalog depositCatalog = TheCircussyOneTestObjects.CreateTicketDepositCatalog(small, large);
        WorldRewardPlacementCatalog placementCatalog = TheCircussyOneTestObjects.CreateWorldRewardPlacementCatalog(
            TheCircussyOneTestObjects.CreateWorldRewardPlacement(
                "jackpot_cache_test",
                "Jackpot Cache Test",
                WorldRewardPlacementKind.TicketDeposit,
                "jackpot_cache",
                new Vector3(7f, 0.35f, -3f)));

        new TicketDepositSystem(
            depositCatalog,
            player: null,
            placementService: TestPlacementService(),
            runSeedState: null,
            spawnPlanner: new WorldRewardSpawnPlanner(placementCatalog)).SpawnWorld(1);

        TicketDepositView view = Object.FindObjectsByType<TicketDepositView>(FindObjectsSortMode.None).Single();

        Assert.That(view.Definition, Is.SameAs(large));
        Assert.That(view.transform.position.x, Is.EqualTo(7f).Within(0.001f));
        Assert.That(view.transform.position.z, Is.EqualTo(-3f).Within(0.001f));

        Object.DestroyImmediate(small);
        Object.DestroyImmediate(large);
        Object.DestroyImmediate(depositCatalog);
        Object.DestroyImmediate(placementCatalog);
    }

    [Test]
    public void HealingPropSystemUsesPlacementCatalogTargetContentIds()
    {
        HealthPickupDefinition treat = TheCircussyOneTestObjects.CreateHealthPickupDefinition("treat", "Treat");
        HealingPropDefinition snackBox = TheCircussyOneTestObjects.CreateHealingPropDefinition("snack_box", "TCO Test Snack Box", treat);
        HealingPropDefinition snackCart = TheCircussyOneTestObjects.CreateHealingPropDefinition("snack_cart", "TCO Test Snack Cart", treat);
        HealingPropCatalog propCatalog = TheCircussyOneTestObjects.CreateHealingPropCatalog(snackBox, snackCart);
        HealthPickupCatalog healthPickupCatalog = TheCircussyOneTestObjects.CreateHealthPickupCatalog(treat);
        WorldRewardPlacementCatalog placementCatalog = TheCircussyOneTestObjects.CreateWorldRewardPlacementCatalog(
            TheCircussyOneTestObjects.CreateWorldRewardPlacement(
                "snack_cart_test",
                "Snack Cart Test",
                WorldRewardPlacementKind.HealingProp,
                "snack_cart",
                new Vector3(-5f, 0.3f, 8f)));

        new HealingPropSystem(
            propCatalog,
            healthPickupCatalog,
            pickupFactory: null,
            player: null,
            placementService: TestPlacementService(),
            runSeedState: null,
            spawnPlanner: new WorldRewardSpawnPlanner(placementCatalog)).SpawnWorld(1);

        HealingPropView view = Object.FindObjectsByType<HealingPropView>(FindObjectsSortMode.None).Single();

        Assert.That(view.Definition, Is.SameAs(snackCart));
        Assert.That(view.transform.position.x, Is.EqualTo(-5f).Within(0.001f));
        Assert.That(view.transform.position.z, Is.EqualTo(8f).Within(0.001f));

        Object.DestroyImmediate(treat);
        Object.DestroyImmediate(snackBox);
        Object.DestroyImmediate(snackCart);
        Object.DestroyImmediate(propCatalog);
        Object.DestroyImmediate(healthPickupCatalog);
        Object.DestroyImmediate(placementCatalog);
    }

    [Test]
    public void DefaultWorldRewardPlacementsCoverVariedArenaRegions()
    {
        WorldRewardPlacementDefinition[] placements = WorldRewardPlacementDefaults.CreatePrototypePlacements();
        Vector3[] chestOffsets = placements
            .Where(placement => placement.kind == WorldRewardPlacementKind.Chest)
            .Select(placement => placement.offset)
            .ToArray();
        Vector3[] depositOffsets = placements
            .Where(placement => placement.kind == WorldRewardPlacementKind.TicketDeposit)
            .Select(placement => placement.offset)
            .ToArray();

        AssertCoversMultipleQuadrants(chestOffsets);
        AssertCoversMultipleQuadrants(depositOffsets);
        Assert.That(chestOffsets.Max(offset => offset.magnitude), Is.GreaterThan(25f));
        Assert.That(depositOffsets.Max(offset => offset.magnitude), Is.GreaterThan(25f));
    }

    private static RunWorldGenerationConfig SmallWorldGenerationConfig()
    {
        RunWorldGenerationConfig config = ScriptableObject.CreateInstance<RunWorldGenerationConfig>();
        config.actProfiles = new System.Collections.Generic.List<RunWorldGenerationProfile>
        {
            new()
            {
                actLabel = "Act I",
                actNumber = 1,
                playableRadius = 48f,
                maxHeight = 8f,
                gridSpacing = 8f,
                boundaryMargin = 8f,
                landformStampCount = 5,
                angularity = 0f,
                terraceSteps = 0,
                plateauCount = 2,
                plateauRadiusMinPercent = 0.13f,
                plateauRadiusMaxPercent = 0.26f,
                rampWidthPercent = 0.14f,
                rampBlendPercent = 0.08f,
                surfaceVariationStrength = 0.02f
            },
            new()
            {
                actLabel = "Act II",
                actNumber = 2,
                playableRadius = 64f,
                maxHeight = 12f,
                gridSpacing = 8f,
                boundaryMargin = 8f,
                landformStampCount = 6,
                angularity = 0f,
                terraceSteps = 0,
                plateauCount = 3,
                plateauRadiusMinPercent = 0.12f,
                plateauRadiusMaxPercent = 0.24f,
                rampWidthPercent = 0.14f,
                rampBlendPercent = 0.08f,
                surfaceVariationStrength = 0.02f
            },
            new()
            {
                actLabel = "Act III",
                actNumber = 3,
                playableRadius = 80f,
                maxHeight = 16f,
                gridSpacing = 8f,
                boundaryMargin = 8f,
                landformStampCount = 7,
                angularity = 0f,
                terraceSteps = 0,
                plateauCount = 4,
                plateauRadiusMinPercent = 0.11f,
                plateauRadiusMaxPercent = 0.22f,
                rampWidthPercent = 0.13f,
                rampBlendPercent = 0.07f,
                surfaceVariationStrength = 0.02f
            }
        };
        config.minimumReachablePercent = 35f;
        config.retryCount = 1;
        config.rewardSafeMaxSlope = 2f;
        config.spawnSafeMaxSlope = 40f;
        config.runtimeMeshTerrainEnabled = false;
        config.worldObstacleEnabled = false;
        return config;
    }

    private static int TotalRewardRequestCount(RunWorldGenerationProfile profile)
    {
        return profile.GeneratedChestCount
            + profile.GeneratedTicketDepositCount
            + profile.GeneratedHealingPropCount
            + profile.GeneratedLooseChestCount
            + profile.GeneratedLooseTicketDepositCount
            + profile.GeneratedLooseHealingPropCount;
    }

    private static bool IsPocketRequest(WorldRewardSpawnRequest request)
    {
        return request.PlacementId.Contains("_pocket_");
    }

    private static bool IsLooseRequest(WorldRewardSpawnRequest request)
    {
        return request.PlacementId.Contains("_loose_");
    }

    private static bool HasSurfaceTexture(Material material)
    {
        if (material == null)
        {
            return false;
        }

        return material.HasProperty("_BaseMap") && material.GetTexture("_BaseMap") != null
            || material.HasProperty("_MainTex") && material.GetTexture("_MainTex") != null;
    }

    private static bool HasMaterialTexture(Material material, string property)
    {
        return material != null
            && material.HasProperty(property)
            && material.GetTexture(property) != null;
    }

    private static Vector2 MaterialTextureScale(Material material, string property)
    {
        return material != null && material.HasProperty(property)
            ? material.GetTextureScale(property)
            : Vector2.zero;
    }

    private static float ColorDistance(Color a, Color b)
    {
        float r = a.r - b.r;
        float g = a.g - b.g;
        float bl = a.b - b.b;
        return Mathf.Sqrt(r * r + g * g + bl * bl);
    }

    private static Color MaterialBaseColor(Material material)
    {
        if (material == null)
        {
            return Color.clear;
        }

        if (material.HasProperty("_BaseColor"))
        {
            return material.GetColor("_BaseColor");
        }

        return material.HasProperty("_Color")
            ? material.GetColor("_Color")
            : Color.clear;
    }

    private static int PixelDelta(Color32 first, Color32 second)
    {
        return Mathf.Abs(first.r - second.r)
            + Mathf.Abs(first.g - second.g)
            + Mathf.Abs(first.b - second.b);
    }

    private static int Luminance(Color32 color)
    {
        return Mathf.RoundToInt(color.r * 0.2126f + color.g * 0.7152f + color.b * 0.0722f);
    }

    private static float AverageLuminance(GeneratedWorldSurfaceTextureData data)
    {
        if (data.Pixels.Length == 0)
        {
            return 0f;
        }

        long total = 0;
        for (int i = 0; i < data.Pixels.Length; i++)
        {
            total += Luminance(data.Pixels[i]);
        }

        return total / (float)data.Pixels.Length;
    }

    private static float AveragePixelDelta(
        GeneratedWorldSurfaceTextureData first,
        GeneratedWorldSurfaceTextureData second)
    {
        int count = Mathf.Min(first.Pixels.Length, second.Pixels.Length);
        if (count == 0)
        {
            return 0f;
        }

        long total = 0;
        for (int i = 0; i < count; i++)
        {
            total += PixelDelta(first.Pixels[i], second.Pixels[i]);
        }

        return total / (float)count;
    }

    private static Color32 PixelAt(GeneratedWorldSurfaceTextureData data, int x, int y)
    {
        x = Mathf.Clamp(x, 0, data.Width - 1);
        y = Mathf.Clamp(y, 0, data.Height - 1);
        return data.Pixels[y * data.Width + x];
    }

    private static int PixelGray(GeneratedWorldSurfaceTextureData data, int x, int y)
    {
        return PixelAt(data, x, y).r;
    }

    private static int PixelAlpha(GeneratedWorldSurfaceTextureData data, int x, int y)
    {
        return PixelAt(data, x, y).a;
    }

    private static (int Min, int Max) AlphaRange(GeneratedWorldSurfaceTextureData data)
    {
        int min = 255;
        int max = 0;
        for (int i = 0; i < data.Pixels.Length; i++)
        {
            int alpha = data.Pixels[i].a;
            min = Mathf.Min(min, alpha);
            max = Mathf.Max(max, alpha);
        }

        return (min, max);
    }

    private static bool MatchesRewardSafeSample(GeneratedWorldMap map, Vector3 position)
    {
        for (int i = 0; i < map.Samples.Count; i++)
        {
            if (!map.Samples[i].HasMask(GeneratedWorldMask.RewardSafe))
            {
                continue;
            }

            if ((map.PositionForIndex(i) - position).sqrMagnitude < 0.0001f)
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesLooseReachableSample(GeneratedWorldMap map, Vector3 position)
    {
        for (int i = 0; i < map.Samples.Count; i++)
        {
            GeneratedWorldSample sample = map.Samples[i];
            if (!sample.HasMask(GeneratedWorldMask.Reachable) || sample.HasMask(GeneratedWorldMask.RewardSafe))
            {
                continue;
            }

            if ((map.PositionForIndex(i) - position).sqrMagnitude < 0.0001f)
            {
                return true;
            }
        }

        return false;
    }

    private static void AssertRewardRequestsEqual(
        IReadOnlyList<WorldRewardSpawnRequest> expected,
        IReadOnlyList<WorldRewardSpawnRequest> actual)
    {
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Count, Is.EqualTo(expected.Count));
        for (int i = 0; i < expected.Count; i++)
        {
            Assert.That(actual[i].PlacementId, Is.EqualTo(expected[i].PlacementId));
            Assert.That(actual[i].Kind, Is.EqualTo(expected[i].Kind));
            Assert.That(actual[i].Position, Is.EqualTo(expected[i].Position));
            Assert.That(actual[i].Generated, Is.EqualTo(expected[i].Generated));
            Assert.That(actual[i].RepeatSequentialContent, Is.EqualTo(expected[i].RepeatSequentialContent));
        }
    }

    private static void AssertMinimumHorizontalSpacing(
        IReadOnlyList<WorldRewardSpawnRequest> requests,
        float minimumSpacing)
    {
        float minimumSpacingSqr = minimumSpacing * minimumSpacing;
        for (int i = 0; i < requests.Count; i++)
        {
            Vector2 a = new(requests[i].Position.x, requests[i].Position.z);
            for (int j = i + 1; j < requests.Count; j++)
            {
                Vector2 b = new(requests[j].Position.x, requests[j].Position.z);
                Assert.That(
                    (a - b).sqrMagnitude,
                    Is.GreaterThanOrEqualTo(minimumSpacingSqr - 0.001f),
                    $"Generated rewards {requests[i].PlacementId} and {requests[j].PlacementId} were too close.");
            }
        }
    }

    private static Dictionary<int, int> RewardSectorCounts(
        GeneratedWorldMap map,
        IReadOnlyList<WorldRewardSpawnRequest> requests)
    {
        var sectors = new Dictionary<int, int>();
        for (int i = 0; i < requests.Count; i++)
        {
            int sector = RewardSector(map, requests[i].Position);
            sectors.TryGetValue(sector, out int count);
            sectors[sector] = count + 1;
        }

        return sectors;
    }

    private static int RewardSector(GeneratedWorldMap map, Vector3 position)
    {
        Vector2 position2 = new(position.x, position.z);
        float normalizedAngle = (Mathf.Atan2(position2.y, position2.x) + Mathf.PI) / (Mathf.PI * 2f);
        int angularSector = Mathf.Clamp(Mathf.FloorToInt(normalizedAngle * 8f), 0, 7);
        int radialBand = Mathf.Clamp(
            Mathf.FloorToInt(Mathf.Clamp01(position2.magnitude / Mathf.Max(1f, map.PlayableRadius)) * 3f),
            0,
            2);
        int heightBand = Mathf.Clamp(
            Mathf.FloorToInt(Mathf.Clamp01(position.y / Mathf.Max(1f, map.MaxHeight)) * 3f),
            0,
            2);
        return angularSector + 8 * (radialBand + 3 * heightBand);
    }

    private static bool IsCardinal(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.99f)
        {
            return false;
        }

        Vector2 normalized = direction.normalized;
        return Mathf.Abs(Vector2.Dot(normalized, Vector2.right)) > 0.999f
            || Mathf.Abs(Vector2.Dot(normalized, Vector2.up)) > 0.999f;
    }

    private static GeneratedWorldMap TinyGeneratedWorldMap()
    {
        const int Width = 5;
        const int Depth = 5;
        const float Spacing = 2f;
        var samples = new GeneratedWorldSample[Width * Depth];
        for (int z = 0; z < Depth; z++)
        {
            for (int x = 0; x < Width; x++)
            {
                int index = z * Width + x;
                float height = 0f;
                samples[index] = new GeneratedWorldSample(
                    height,
                    Vector3.up,
                    0f,
                    GeneratedWorldMask.Walkable
                    | GeneratedWorldMask.Reachable
                    | GeneratedWorldMask.RewardSafe
                    | GeneratedWorldMask.SpawnSafe
                    | GeneratedWorldMask.PlayerStartSafe,
                    height / 2f,
                    1f,
                    1f);
            }
        }

        return new GeneratedWorldMap(
            actNumber: 1,
            seed: 7,
            attemptSeed: 7,
            width: Width,
            depth: Depth,
            playableRadius: 8f,
            gridSpacing: Spacing,
            maxHeight: 2f,
            wallHeightThreshold: 1f,
            samples: samples,
            playerStartIndex: (Depth / 2) * Width + Width / 2,
            usedFallback: false,
            warnings: null);
    }

    private static float ClosestGeneratedSampleHeight(GeneratedWorldMap map, Vector3 position)
    {
        float closestDistance = float.PositiveInfinity;
        float height = 0f;
        for (int i = 0; i < map.Samples.Count; i++)
        {
            Vector3 samplePosition = map.PositionForIndex(i);
            float distance = new Vector2(samplePosition.x - position.x, samplePosition.z - position.z).sqrMagnitude;
            if (distance >= closestDistance)
            {
                continue;
            }

            closestDistance = distance;
            height = samplePosition.y;
        }

        return height;
    }

    private static GeneratedWorldMap SteppedGeneratedWorldMap()
    {
        const int Width = 4;
        const int Depth = 3;
        const float Spacing = 2f;
        var samples = new GeneratedWorldSample[Width * Depth];
        for (int z = 0; z < Depth; z++)
        {
            for (int x = 0; x < Width; x++)
            {
                int index = z * Width + x;
                float height = x >= 2 ? 3f : 0f;
                samples[index] = new GeneratedWorldSample(
                    height,
                    Vector3.up,
                    0f,
                    GeneratedWorldMask.Walkable
                    | GeneratedWorldMask.Reachable
                    | GeneratedWorldMask.RewardSafe
                    | GeneratedWorldMask.SpawnSafe
                    | GeneratedWorldMask.PlayerStartSafe,
                    height / 3f,
                    1f,
                    1f);
            }
        }

        return new GeneratedWorldMap(
            actNumber: 1,
            seed: 11,
            attemptSeed: 11,
            width: Width,
            depth: Depth,
            playableRadius: 8f,
            gridSpacing: Spacing,
            maxHeight: 3f,
            wallHeightThreshold: 1f,
            samples: samples,
            playerStartIndex: 1,
            usedFallback: false,
            warnings: null);
    }

    private static GeneratedWorldMap MixedSurfaceGeneratedWorldMap()
    {
        const int Width = 4;
        const int Depth = 4;
        const float Spacing = 2f;
        var samples = new GeneratedWorldSample[Width * Depth];
        for (int z = 0; z < Depth; z++)
        {
            for (int x = 0; x < Width; x++)
            {
                bool organic = x < 2;
                int index = z * Width + x;
                float height = organic ? 0f : 0.5f;
                samples[index] = new GeneratedWorldSample(
                    height,
                    Vector3.up,
                    0f,
                    GeneratedWorldMask.Walkable
                    | GeneratedWorldMask.Reachable
                    | GeneratedWorldMask.RewardSafe
                    | GeneratedWorldMask.SpawnSafe
                    | GeneratedWorldMask.PlayerStartSafe,
                    height,
                    1f,
                    1f,
                    organic ? GeneratedWorldSurfaceKind.Continuous : GeneratedWorldSurfaceKind.Flat);
            }
        }

        return new GeneratedWorldMap(
            actNumber: 1,
            seed: 12,
            attemptSeed: 12,
            width: Width,
            depth: Depth,
            playableRadius: 8f,
            gridSpacing: Spacing,
            maxHeight: 1f,
            wallHeightThreshold: 0.25f,
            samples: samples,
            playerStartIndex: 1,
            usedFallback: false,
            warnings: null);
    }

    private static GameObject CreateSurface(string name, Vector3 position, Vector3 scale)
    {
        GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
        surface.name = name;
        surface.layer = TestSurfaceLayer;
        surface.transform.position = position;
        surface.transform.localScale = scale;
        return surface;
    }

    private static Vector3 CreateSlopedSurface(float angleDegrees, Vector3 center, Vector3 size)
    {
        GameObject slope = new("TCO Test Ramp Surface");
        var collider = slope.AddComponent<BoxCollider>();
        slope.layer = TestSurfaceLayer;
        collider.center = Vector3.zero;
        collider.size = size;
        slope.transform.rotation = Quaternion.AngleAxis(angleDegrees, Vector3.right);
        Vector3 normal = slope.transform.TransformDirection(Vector3.up).normalized;
        slope.transform.position = center - normal * 0.05f;
        Physics.SyncTransforms();
        return normal;
    }

    private static WorldPropPlacementService TestPlacementService()
    {
        return new WorldPropPlacementService(environmentMask: TestSurfaceMask);
    }

    private static void AssertCoversMultipleQuadrants(System.Collections.Generic.IReadOnlyList<Vector3> offsets)
    {
        Assert.That(offsets.Any(offset => offset.x < -0.01f && offset.z > 0.01f), Is.True);
        Assert.That(offsets.Any(offset => offset.x > 0.01f && offset.z > 0.01f), Is.True);
        Assert.That(offsets.Any(offset => offset.x < -0.01f && offset.z < -0.01f), Is.True);
        Assert.That(offsets.Any(offset => offset.x > 0.01f && offset.z < -0.01f), Is.True);
    }
}
