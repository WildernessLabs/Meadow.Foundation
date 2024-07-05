namespace Unit.Tests;

internal class MenuContainer
{
    public MenuItem[] Menu { get; set; }
}

internal class MenuItem
{
    public string Text { get; set; }
    public string Id { get; set; }
    public string Type { get; set; }
    public int Value { get; set; }
    public MenuItem[] Sub { get; set; }
}
