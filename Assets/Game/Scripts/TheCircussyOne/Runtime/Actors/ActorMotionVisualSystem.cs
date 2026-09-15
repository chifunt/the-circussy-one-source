using VContainer.Unity;
using TheCircussyOne.Config;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class ActorMotionVisualSystem : ITickable
    {
        private readonly ActorMotionVisualConfig config;
        private readonly PlayerView player;
        private readonly ActorRegistry registry;
        private readonly IGameTime time;

        public ActorMotionVisualSystem(ActorMotionVisualConfig config, PlayerView player, ActorRegistry registry, IGameTime time)
        {
            this.config = config;
            this.player = player;
            this.registry = registry;
            this.time = time;
        }

        public void Tick()
        {
            player?.TickMotionVisuals(config, time.DeltaTime);

            var enemies = registry.Enemies;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                EnemyRuntime enemy = enemies[i];
                if (enemy == null || enemy.IsDead || enemy.View == null || !enemy.View.IsActive)
                {
                    continue;
                }

                enemy.View.TickMotionVisuals(config, time.DeltaTime);
            }
        }
    }
}
