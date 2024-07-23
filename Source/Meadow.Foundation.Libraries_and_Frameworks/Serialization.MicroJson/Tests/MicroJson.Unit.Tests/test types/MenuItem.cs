namespace Unit.Tests;

internal class MenuContainer
{
    public MenuItem[] Menu { get; set; } = default!;
}

internal class MenuItem
{
    public string Text { get; set; } = default!;
    public string Id { get; set; } = default!;
    public string Type { get; set; } = default!;
    public int Value { get; set; }
    public MenuItem[] Sub { get; set; } = default!;
}
