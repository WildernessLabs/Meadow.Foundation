using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Telematics.OBD2;

public abstract class PcmBase : ControllerBase
{
    // PCMs should support 
    protected PcmBase(ICanBus[] canBuses, short moduleAddress, IControlModuleStore? store = null) : base(canBuses, moduleAddress, store)
    {
        RegisterPcmPids();

    }

    private void RegisterPcmPids()
    {
        RegisterPid(Pid.RunTimeSinceEngineStart, () =>
        {
            var secs = (ushort)Math.Min(GetTimeSinceEndineStarted().TotalSeconds, ushort.MaxValue);
            return [(byte)(secs >> 8), (byte)(secs & 0xFF)];
        });
        RegisterPid(Pid.TimeRunWithMilOn, () =>
        {
            var mins = (ushort)Math.Min(GetTimeWithMilOn().TotalMinutes, ushort.MaxValue);
            return [(byte)(mins >> 8), (byte)(mins & 0xFF)];
        });
        RegisterPid(Pid.TimeSinceTroubleCodesCleared, () =>
        {
            var mins = (ushort)Math.Min(GetTimeSinceDtcsCleared().TotalMinutes, ushort.MaxValue);
            return [(byte)(mins >> 8), (byte)(mins & 0xFF)];
        });
        RegisterPid(Pid.DistanceSinceCodesCleared, () =>
        {
            var km = (ushort)Math.Min(GetDistanceSinceDtcsCleared().Kilometers, ushort.MaxValue);
            return [(byte)(km >> 8), (byte)(km & 0xFF)];
        });
        RegisterPid(Pid.DistanceWithMilOn, () =>
        {
            var km = (ushort)Math.Min(GetDistanceWithMilOn().Kilometers, ushort.MaxValue);
            return [(byte)(km >> 8), (byte)(km & 0xFF)];
        });
    }

    protected abstract TimeSpan GetTimeSinceEndineStarted();
    protected abstract TimeSpan GetTimeWithMilOn();
    protected abstract TimeSpan GetTimeSinceDtcsCleared();
    protected abstract Length GetDistanceSinceDtcsCleared();
    protected abstract Length GetDistanceWithMilOn();
}
