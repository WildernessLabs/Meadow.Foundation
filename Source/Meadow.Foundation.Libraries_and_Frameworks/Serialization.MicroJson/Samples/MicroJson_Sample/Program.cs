using Meadow.Foundation.Serialization;
using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace MicroJson_Sample;

internal class Program
{
    //<!=SNIP=>

    public static void Main(string[] args)
    {
        var resourceData = LoadResource("menu.json");

        var menuJson = new string(System.Text.Encoding.UTF8.GetChars(resourceData));

        DeserializeTypeSafe(menuJson);
        DeserializeAsHashtable(menuJson);
    }

    private static void DeserializeTypeSafe(string menuJson)
    {
        string testJsonItem = "{\"ScreenX\":290,\"ScreenY\":210,\"RawX\":3341,\"RawY\":3353}";
        var point = MicroJson.Deserialize<CalibrationPoint>(testJsonItem);

        string testJsonArray = "[{\"ScreenX\":30,\"ScreenY\":30,\"RawX\":522,\"RawY\":514},{\"ScreenX\":290,\"ScreenY\":210,\"RawX\":3341,\"RawY\":3353}]";

        var points = MicroJson.Deserialize<CalibrationPoint[]>(testJsonArray);

        var menu = MicroJson.Deserialize<MenuContainer>(menuJson);
    }

    private static void DeserializeAsHashtable(string menuJson)
    {
        var menuData = MicroJson.DeserializeString(menuJson) as Hashtable;

        if (menuData["menu"] == null)
        {
            throw new ArgumentException("JSON root must contain a 'menu' item");
        }

        Console.WriteLine($"Root element is {menuData["menu"]}");

        var items = (ArrayList)menuData["menu"];

        foreach (Hashtable item in items)
        {
            Console.WriteLine($"Found {item["text"]}");
        }
    }

    private static byte[] LoadResource(string filename)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"MicroJson_Sample.{filename}";

        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    //<!=SNOP=>
}