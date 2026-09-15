namespace TheCircussyOne.Config
{
    public enum ConfigValidationLevel
    {
        Valid,
        Warning,
        Error
    }

    public readonly struct ConfigValidationResult
    {
        public ConfigValidationResult(ConfigValidationLevel level, string message)
        {
            Level = level;
            Message = message;
        }

        public ConfigValidationLevel Level { get; }
        public string Message { get; }
        public bool IsValid => Level != ConfigValidationLevel.Error;

        public static ConfigValidationResult Valid()
        {
            return new(ConfigValidationLevel.Valid, string.Empty);
        }

        public static ConfigValidationResult Warning(string message)
        {
            return new(ConfigValidationLevel.Warning, message);
        }

        public static ConfigValidationResult Error(string message)
        {
            return new(ConfigValidationLevel.Error, message);
        }
    }

    public static class ConfigValidationRules
    {
        public static ConfigValidationResult MinLessOrEqual(float minimum, float maximum, string minimumLabel, string maximumLabel)
        {
            return minimum <= maximum
                ? ConfigValidationResult.Valid()
                : ConfigValidationResult.Error($"{minimumLabel} must be less than or equal to {maximumLabel}.");
        }

        public static ConfigValidationResult MajorSpacing(float minorSpacing, float majorSpacing)
        {
            if (minorSpacing <= 0f || majorSpacing <= 0f)
            {
                return ConfigValidationResult.Error("Grid spacing values must be greater than zero.");
            }

            return majorSpacing >= minorSpacing
                ? ConfigValidationResult.Valid()
                : ConfigValidationResult.Error("Major spacing must be greater than or equal to minor spacing.");
        }

        public static ConfigValidationResult WarningIfGreater(float value, float warningThreshold, string label)
        {
            return value > warningThreshold
                ? ConfigValidationResult.Warning($"{label} is unusually high; keep it only if this is intentional.")
                : ConfigValidationResult.Valid();
        }

        public static ConfigValidationResult ZeroOrPositive(float value, string label)
        {
            return value >= 0f
                ? ConfigValidationResult.Valid()
                : ConfigValidationResult.Error($"{label} can be zero to disable, but cannot be negative.");
        }
    }
}
