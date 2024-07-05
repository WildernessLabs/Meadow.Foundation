namespace Unit.Tests;

public class PuzzlePiece
{
    public int X { get; set; }
    public int Y { get; set; }

    public int Width => PieceType switch
    {
        PieceType.Horizontal2 or PieceType.Solve => 2,
        PieceType.Horizontal3 => 3,
        _ => 1,
    };

    public int Height => PieceType switch
    {
        PieceType.Vertical2 => 2,
        PieceType.Vertical3 => 3,
        _ => 1,
    };

    public PieceType PieceType { get; set; }

    public bool IsSolved => PieceType == PieceType.Solve && X == 4 && Y == 2;

    public bool IsHorizontalPiece => PieceType == PieceType.Horizontal2 || PieceType == PieceType.Horizontal3 || PieceType == PieceType.Solve;

    public PuzzlePiece()
    { }

    public PuzzlePiece(int x, int y, PieceType type)
    {
        X = x;
        Y = y;
        PieceType = type;
    }

    public void MovePiece(int x, int y)
    {
        X = x;
        Y = y;
    }
}