using BlockPuzzleCore;
using Meadow.Foundation.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MicroJson_ArrayList_Sample;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, MicroJson - Arrays and Lists");

        SerializeEnum();

        SerializePuzzle();

        LoadPuzzles();
    }

    static void SerializeEnum()
    {
        var value = PieceType.Horizontal2;

        var enumJson = MicroJson.Serialize(value);
        Console.WriteLine(enumJson);
    }

    static void SerializePuzzle()
    {
        var puzzle = new Puzzle();
        puzzle.AddPiece(0, 0, PieceType.Horizontal2);
        puzzle.AddPiece(0, 2, PieceType.Vertical2);
        puzzle.AddPiece(2, 0, PieceType.Horizontal3);
        puzzle.AddPiece(2, 3, PieceType.Vertical3);
        puzzle.AddPiece(4, 0, PieceType.Solve);

        var puzzleJson = MicroJson.Serialize(puzzle);

        Console.WriteLine(puzzleJson);
    }

    static IEnumerable<Puzzle> LoadPuzzles()
    {
        byte[] resourceData = LoadResource("puzzles.json");
        var puzzleJson = new string(System.Text.Encoding.UTF8.GetChars(resourceData));

        var puzzlesArray = MicroJson.Deserialize<Puzzle[]>(puzzleJson);
        Console.WriteLine($"Decoded {puzzlesArray.Length} puzzles into an array");

        var puzzlesList = MicroJson.Deserialize<List<Puzzle>>(puzzleJson);
        Console.WriteLine($"Decoded {puzzlesList.Count} puzzles into a list");

        return puzzlesList;
    }

    static byte[] LoadResource(string filename)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"MicroJson_ArrayList_Sample.{filename}";

        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using var ms = new MemoryStream();

        stream?.CopyTo(ms);
        return ms.ToArray();
    }
}