namespace TheCircussyOne.Runtime
{
    public enum GameHapticsCue
    {
        UiHover,
        UiClick,
        LevelUp,
        ItemObtained,
        XpCollect,
        TicketCollect,
        SnackEat,
        InteractionCanceled,
        ChestOpen,
        SnackOpen,
        StageDoorOpen,
        PlayerDamaged,
        PlayerJump,
        PlayerLand,
        GameOver,
        HeadlinerDefeated,
        EncoreStarted
    }

    public interface IGameHaptics
    {
        void Play(GameHapticsCue cue, float intensityMultiplier = 1f);
        void StopAll();
    }

    public interface IGamepadHapticsBackend
    {
        bool IsAvailable { get; }
        void SetMotorSpeeds(float lowFrequency, float highFrequency);
        void StopMotorSpeeds();
    }

    public sealed class NullGameHaptics : IGameHaptics
    {
        public static readonly NullGameHaptics Instance = new();

        private NullGameHaptics()
        {
        }

        public void Play(GameHapticsCue cue, float intensityMultiplier = 1f)
        {
        }

        public void StopAll()
        {
        }
    }
}
