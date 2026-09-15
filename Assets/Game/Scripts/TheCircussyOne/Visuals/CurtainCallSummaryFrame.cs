namespace TheCircussyOne.Visuals
{
    public readonly struct CurtainCallSummaryFrame
    {
        public CurtainCallSummaryFrame(
            string time,
            string act,
            string cause,
            string kills,
            string level,
            string tickets,
            string weapons,
            string items,
            string damageDealt)
        {
            Time = time ?? string.Empty;
            Act = act ?? string.Empty;
            Cause = cause ?? string.Empty;
            Kills = kills ?? string.Empty;
            Level = level ?? string.Empty;
            Tickets = tickets ?? string.Empty;
            Weapons = weapons ?? string.Empty;
            Items = items ?? string.Empty;
            DamageDealt = damageDealt ?? string.Empty;
        }

        public string Time { get; }
        public string Act { get; }
        public string Cause { get; }
        public string Kills { get; }
        public string Level { get; }
        public string Tickets { get; }
        public string Weapons { get; }
        public string Items { get; }
        public string DamageDealt { get; }
    }
}
