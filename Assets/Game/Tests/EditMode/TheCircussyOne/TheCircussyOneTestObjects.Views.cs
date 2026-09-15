using TMPro;
using UnityEditor;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

internal static partial class TheCircussyOneTestObjects
{
    public static PlayerView CreatePlayer(GameConfig config)
    {
        var player = new GameObject(TestPrefix + "Player");
        SetLayerIfConfigured(player, GameLayers.Player);

        var bodyScaleRoot = new GameObject("Body Scale Root");
        bodyScaleRoot.transform.SetParent(player.transform, false);

        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(bodyScaleRoot.transform, false);
        body.transform.localPosition = Vector3.up;
        Object.DestroyImmediate(body.GetComponent<Collider>());

        var healthBar = new GameObject("Health Bar");
        healthBar.transform.SetParent(player.transform, false);
        healthBar.AddComponent<HealthBarView>();

        var controller = player.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.55f;
        controller.center = Vector3.up;

        player.AddComponent<ActorBodyView>();
        var view = player.AddComponent<PlayerView>();
        view.ApplyConfig(config);
        return view;
    }

    public static EnemyView CreateEnemyPrefab()
    {
        var enemy = new GameObject(TestPrefix + "Enemy Prefab");
        SetLayerIfConfigured(enemy, GameLayers.Enemy);

        var bodyScaleRoot = new GameObject("Body Scale Root");
        bodyScaleRoot.transform.SetParent(enemy.transform, false);
        SetLayerIfConfigured(bodyScaleRoot, GameLayers.Enemy);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(bodyScaleRoot.transform, false);
        body.transform.localPosition = Vector3.up;
        SetLayerIfConfigured(body, GameLayers.Enemy);

        var collider = body.GetComponent<CapsuleCollider>();
        Object.DestroyImmediate(collider);
        CreateEnemySphereHitbox(enemy.transform, "Enemy Hurtbox", EnemyHitboxRole.Hurtbox, 0.65f, new Vector3(0f, 0.95f, 0f));
        CreateEnemyBoxHitbox(enemy.transform, "Enemy Contact Hitbox", EnemyHitboxRole.ContactDamage, new Vector3(0.75f, 0.9f, 0.65f), new Vector3(0f, 0.55f, 0.35f));
        CreateEnemyCapsuleHitbox(enemy.transform, "Enemy Movement Body", EnemyHitboxRole.MovementBody, 0.52f, 1.45f, new Vector3(0f, 0.75f, 0f));

        var healthBar = new GameObject("Health Bar");
        healthBar.transform.SetParent(enemy.transform, false);
        healthBar.AddComponent<HealthBarView>();

        enemy.AddComponent<ActorBodyView>();
        var view = enemy.AddComponent<EnemyView>();
        enemy.SetActive(false);
        return view;
    }

    public static EnemyRuntime CreateEnemy(GameConfig config, Vector3 position, string name = "Enemy", int spawnId = 0)
    {
        EnemyDefinition definition = CreateEnemyDefinition(config);
        return CreateEnemy(definition, position, name, spawnId);
    }

    public static EnemyRuntime CreateEnemy(EnemyDefinition definition, Vector3 position, string name = "Enemy", int spawnId = 0)
    {
        EnemyView view = CreateEnemyPrefab();
        view.gameObject.name = TestPrefix + name;
        view.Prepare(definition, null, position);
        Physics.SyncTransforms();
        return new EnemyRuntime(
            definition,
            view,
            definition != null ? definition.baseHealth : 4,
            definition != null ? definition.baseMoveSpeed : 2f,
            definition != null ? definition.turnDegreesPerSecond : 540f,
            definition != null ? definition.contactDamage : 2,
            definition != null ? definition.behaviorType : EnemyBehaviorType.Chaser,
            definition != null ? definition.spawnWeight : 100f,
            definition != null ? definition.body : null,
            definition != null ? definition.climb : null,
            definition != null ? definition.stack : null,
            definition != null ? definition.locomotion : null,
            definition != null ? definition.xpBudget : 1,
            definition != null ? definition.dropStyle : TheCircussyOne.Rules.XpDropStyle.Compact,
            spawnId);
    }

    public static GameObject CreateRuntimeModelPrefab(string name = "Runtime Model Prefab")
    {
        var model = new GameObject(TestPrefix + name);
        model.AddComponent<Animator>();

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(model.transform, false);
        visual.transform.localPosition = Vector3.up;
        visual.transform.localScale = new Vector3(0.75f, 1.5f, 0.75f);
        return model;
    }

    private static EnemyHitbox CreateEnemySphereHitbox(Transform parent, string name, EnemyHitboxRole role, float radius, Vector3 localPosition)
    {
        var hitboxObject = new GameObject(name);
        hitboxObject.transform.SetParent(parent, false);
        SetLayerIfConfigured(hitboxObject, GameLayers.Enemy);
        var hitbox = hitboxObject.AddComponent<EnemyHitbox>();
        hitbox.ConfigureSphere(role, radius, localPosition);
        return hitbox;
    }

    private static EnemyHitbox CreateEnemyBoxHitbox(Transform parent, string name, EnemyHitboxRole role, Vector3 size, Vector3 localPosition)
    {
        var hitboxObject = new GameObject(name);
        hitboxObject.transform.SetParent(parent, false);
        SetLayerIfConfigured(hitboxObject, GameLayers.Enemy);
        var hitbox = hitboxObject.AddComponent<EnemyHitbox>();
        hitbox.ConfigureBox(role, size, localPosition);
        return hitbox;
    }

    private static EnemyHitbox CreateEnemyCapsuleHitbox(Transform parent, string name, EnemyHitboxRole role, float radius, float height, Vector3 localPosition)
    {
        var hitboxObject = new GameObject(name);
        hitboxObject.transform.SetParent(parent, false);
        SetLayerIfConfigured(hitboxObject, GameLayers.Enemy);
        var hitbox = hitboxObject.AddComponent<EnemyHitbox>();
        hitbox.ConfigureCapsule(role, radius, height, localPosition);
        return hitbox;
    }

    public static ProjectileView CreateProjectilePrefab(WeaponDefinition weapon = null)
    {
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = TestPrefix + "Projectile Prefab";
        projectile.transform.localScale = Vector3.one * 0.22f;
        SetLayerIfConfigured(projectile, GameLayers.Projectile);

        var collider = projectile.GetComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.center = Vector3.zero;
        collider.radius = 0.5f;

        var view = projectile.AddComponent<ProjectileView>();
        view.ApplyVisuals(weapon);
        projectile.SetActive(false);
        return view;
    }

    public static PickupView CreatePickupPrefab()
    {
        var pickup = new GameObject(TestPrefix + "Pickup Prefab");
        pickup.name = TestPrefix + "Pickup Prefab";
        SetLayerIfConfigured(pickup, GameLayers.Pickup);

        GameObject visualRoot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visualRoot.name = "Visual Root";
        visualRoot.transform.SetParent(pickup.transform, false);
        visualRoot.transform.localScale = Vector3.one * 0.28f;
        SetLayerIfConfigured(visualRoot, GameLayers.Pickup);
        Object.DestroyImmediate(visualRoot.GetComponent<Collider>());

        var view = pickup.AddComponent<PickupView>();
        pickup.SetActive(false);
        return view;
    }

    public static DamageNumberView CreateDamageNumberPrefab()
    {
        var damageNumber = new GameObject(TestPrefix + "Damage Number Prefab");
        var text = damageNumber.AddComponent<TextMeshPro>();
        text.text = "0";
        text.font = FindTmpFontAsset();
        text.fontSize = 4f;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        var view = damageNumber.AddComponent<DamageNumberView>();
        damageNumber.SetActive(false);
        return view;
    }

    public static XpGainCounterView CreateXpGainCounterPrefab()
    {
        var counter = new GameObject(TestPrefix + "XP Gain Counter Prefab");
        var text = counter.AddComponent<TextMeshPro>();
        text.text = "+0";
        text.font = FindTmpFontAsset();
        text.fontSize = 3.6f;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        var view = counter.AddComponent<XpGainCounterView>();
        counter.SetActive(false);
        return view;
    }

    public static WorldInteractionPromptView CreateWorldInteractionPromptPrefab(WorldInteractionPromptVisualConfig config = null)
    {
        config ??= CreateWorldInteractionPromptConfig();
        var prompt = new GameObject(TestPrefix + "World Interaction Prompt Prefab");
        var view = prompt.AddComponent<WorldInteractionPromptView>();
        view.ConfigureDefaults(config);
        prompt.SetActive(false);
        return view;
    }

    public static EnemySpawnIndicatorView CreateEnemySpawnIndicatorPrefab()
    {
        var indicator = new GameObject(TestPrefix + "Enemy Spawn Indicator Prefab");
        var view = indicator.AddComponent<EnemySpawnIndicatorView>();
        EnemySpawnVisualConfig config = CreateEnemySpawnVisualConfig();
        view.ConfigureShapeDefaults(config);
        Object.DestroyImmediate(config);
        indicator.SetActive(false);
        return view;
    }

    public static ParticleEffectView CreateParticleEffectPrefab(VfxEffectId effectId)
    {
        var effect = new GameObject(TestPrefix + effectId + " Prefab");
        var particles = effect.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.loop = false;
        main.duration = 0.05f;
        main.startLifetime = 0.05f;
        main.playOnAwake = false;
        var emission = particles.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });

        var view = effect.AddComponent<ParticleEffectView>();
        view.Configure(effectId);
        effect.SetActive(false);
        return view;
    }

    public static ProjectileExplosionView CreateProjectileExplosionPrefab(VfxVisualConfig config = null)
    {
        var effect = new GameObject(TestPrefix + "Projectile Explosion Prefab");
        var particles = effect.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.loop = false;
        main.duration = 0.05f;
        main.startLifetime = 0.05f;
        main.playOnAwake = false;
        var emission = particles.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });

        var particleView = effect.AddComponent<ParticleEffectView>();
        particleView.Configure(VfxEffectId.ProjectileExplosion);
        var view = effect.AddComponent<ProjectileExplosionView>();
        view.ConfigureDefaults(config ?? CreateVfxVisualConfig());
        effect.SetActive(false);
        return view;
    }

    public static OrbitWeaponView CreateOrbitWeaponPrefab(WeaponDefinition weapon = null)
    {
        var orbit = new GameObject(TestPrefix + "Orbit Weapon Prefab");
        var view = orbit.AddComponent<OrbitWeaponView>();
        view.ConfigureDefaults(weapon);
        orbit.SetActive(false);
        return view;
    }

    public static FireHoopAreaTelegraphView CreateFireHoopAreaTelegraphPrefab(VfxVisualConfig config = null)
    {
        var telegraph = new GameObject(TestPrefix + "Fire Hoop Area Telegraph Prefab");
        var view = telegraph.AddComponent<FireHoopAreaTelegraphView>();
        view.ConfigureDefaults(config ?? CreateVfxVisualConfig());
        telegraph.SetActive(false);
        return view;
    }

    public static WorldAmbientDustView CreateWorldAmbientDustPrefab(VfxVisualConfig config = null)
    {
        var dust = new GameObject(TestPrefix + "World Ambient Dust Prefab");
        var view = dust.AddComponent<WorldAmbientDustView>();
        view.ConfigureDefaults(config ?? CreateVfxVisualConfig());
        dust.SetActive(false);
        return view;
    }

    public static GameObject CreateRoot(string name)
    {
        return new GameObject(TestPrefix + name);
    }

    private static void SetLayerIfConfigured(GameObject gameObject, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer >= 0)
        {
            gameObject.layer = layer;
        }
    }

    private static TMP_FontAsset FindTmpFontAsset()
    {
        try
        {
            if (TMP_Settings.defaultFontAsset != null)
            {
                return TMP_Settings.defaultFontAsset;
            }
        }
        catch (System.NullReferenceException)
        {
        }

        string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
        for (int i = 0; i < guids.Length; i++)
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[i]));
            if (font != null)
            {
                return font;
            }
        }

        return null;
    }
}
