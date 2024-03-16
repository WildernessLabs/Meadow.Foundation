using Meadow.Foundation.Serialization;
using System.Collections.Generic;
using Xunit;

namespace Unit.Tests;

public class PuzzleJsonTests
{
    [Fact]
    public void DeserializePuzzlesAsArrayTest()
    {
        var json = Inputs.GetInputResource("puzzles.json");
        var result = MicroJson.Deserialize<Puzzle[]>(json);

        Assert.NotNull(result);
        Assert.Equal(10, result.Length);

        foreach (var puzzle in result)
        {
            Assert.NotNull(puzzle);
            Assert.NotNull(puzzle.Pieces);

            foreach (var piece in puzzle.Pieces)
            {
                Assert.NotNull(piece);
            }
        }
    }

    [Fact]
    public void DeserializePuzzlesAsListTest()
    {
        var json = Inputs.GetInputResource("puzzles.json");
        var result = MicroJson.Deserialize<List<Puzzle>>(json);

        Assert.NotNull(result);
        Assert.Equal(10, result.Count);

        foreach (var puzzle in result)
        {
            Assert.NotNull(puzzle);
            Assert.NotNull(puzzle.Pieces);

            foreach (var piece in puzzle.Pieces)
            {
                Assert.NotNull(piece);
            }
        }
    }
}
