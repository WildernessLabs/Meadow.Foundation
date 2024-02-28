using Meadow.Hardware;

namespace ICs.IOExpanders.PCanBasic;

public class PCanUsb : ICanController
{
    public PCanUsb()
    {
        // TODO: only supported on Windows
        // TODO: check for PCANBasic DLL
    }

    /// <inheritdoc/>
    public ICanBus CreateCanBus(ICanBusConfiguration configuration)
    {
        if (configuration is PCanConfiguration cfg)
        {
            if (cfg.IsFD)
            {
                return new PCanFdBus(cfg);
            }

            return new PCanBus(cfg);
        }

        else throw new ArgumentException($"Configuration is expected to be a {nameof(PCanConfiguration)}");
    }
}
