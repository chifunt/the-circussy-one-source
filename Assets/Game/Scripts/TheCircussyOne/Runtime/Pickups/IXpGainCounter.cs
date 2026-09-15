namespace TheCircussyOne.Runtime
{
    public interface IXpGainCounter
    {
        void Add(int amount);
    }

    public sealed class NullXpGainCounter : IXpGainCounter
    {
        public void Add(int amount)
        {
        }
    }
}
