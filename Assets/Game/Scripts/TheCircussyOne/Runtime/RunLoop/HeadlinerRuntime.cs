namespace TheCircussyOne.Runtime
{
    public sealed class HeadlinerRuntime
    {
        public HeadlinerRuntime(TheCircussyOne.Content.HeadlinerDefinition definition, EnemyRuntime enemy, int actNumber)
        {
            Definition = definition;
            Enemy = enemy;
            ActNumber = System.Math.Max(1, actNumber);
        }

        public TheCircussyOne.Content.HeadlinerDefinition Definition { get; }
        public EnemyRuntime Enemy { get; }
        public int ActNumber { get; }
        public bool IsActive => Enemy != null && !Enemy.IsDead && Enemy.View != null && Enemy.View.IsActive;
    }
}
