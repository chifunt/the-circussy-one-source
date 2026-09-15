namespace TheCircussyOne.Runtime
{
    public enum RunPhase
    {
        PerformerSelection = 0,
        WorldActive = 10,
        BossWarning = 20,
        BossActive = 30,
        BossDefeated = 40,
        Encore = 50,
        Intermission = 55,
        Finished = 60,
        Failed = 70,
        NextWorldTransition = 80
    }
}
