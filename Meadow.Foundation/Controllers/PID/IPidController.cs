namespace Netduino.Foundation.Controllers.PID
{
    public interface IPidController
    {
        float ProportionalComponent { get; set; }
        float IntegralComponent { get; set; }
        float DerivativeComponent { get; set; }
        float OutputMin { get; set; }
        float OutputMax { get; set; }
        bool OutputTuningInformation { get; set; }
        float ActualInput { get; set; }
        float TargetInput { get; set; }
        void ResetIntegrator();
        float CalculateControlOutput();
    }
}
