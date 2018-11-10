namespace Meadow.Foundation.Relays
{
    public interface IRelay
    {
        bool IsOn { get; set; }

        RelayType Type { get; }

        void Toggle();
    }
}