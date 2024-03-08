using System.Collections.Generic;

namespace BlockPuzzleCore;

public class Puzzle
{
    public List<PuzzlePiece> Pieces { get; set; }

    public int MinMoves { get; set; } = -1;
    public int NumBlocks => Pieces.Count;

    public Puzzle()
    {
        Pieces = [];
    }

    public bool AddPiece(int x, int y, PieceType type)
    {
        PuzzlePiece piece = new PuzzlePiece(x, y, type);

        Pieces.Add(piece);

        return true;
    }
}