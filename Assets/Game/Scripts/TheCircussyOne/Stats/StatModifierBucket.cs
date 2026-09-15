namespace TheCircussyOne.Stats
{
    public enum StatModifierBucket
    {
        Flat,
        AdditivePercent,
        MultiplicativePercent,
        WeaponLocalPercent,
        GlobalPercent,
        TagSpecializationPercent,
        ConditionalPercent,
        CritMultiplier,
        RareUniqueMultiplier,
        // Compatibility value retained for serialized enum stability. New authored content uses AdditivePercent.
        Haste,
        // Compatibility value retained for serialized enum stability. New authored content does not use cooldown seconds.
        FlatSeconds
    }
}
