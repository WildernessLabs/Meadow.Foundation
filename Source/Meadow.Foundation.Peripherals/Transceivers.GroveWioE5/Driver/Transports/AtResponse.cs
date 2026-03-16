namespace Meadow.Foundation.Transceivers;

internal class AtResponse
{
    public string[] Lines { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}


