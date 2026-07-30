namespace ConnectFour;

/// <summary>
/// Holds all state and rules for a single Connect Four game.
/// Registered as a singleton so the board survives re-renders and page navigation.
/// </summary>
public class GameState
{
    // The board is a flat array of 42 cells: 7 columns x 6 rows.
    // Index math: column = index % 7 (0-6), row = index / 7 (0 = top, 5 = bottom).
    public PieceColor[] TheBoard { get; private set; } = new PieceColor[42];

    public PieceColor WinnerColor { get; private set; } = PieceColor.Blank;

    // Player 1 always plays on even move counts, Player 2 on odd.
    public int PlayerTurn => PiecesPlayed % 2 == 0 ? 1 : 2;

    public int PiecesPlayed { get; private set; }

    // Every game is a draw once all 42 slots are filled and nobody has won.
    public bool IsDraw => PiecesPlayed == 42 && WinnerColor == PieceColor.Blank;

    // ---- ADDITIONAL FEATURE: move history ----------------------------------
    // A running log of every move made in the current game, exposed read-only.
    private readonly List<GameMove> _moves = new();
    public IReadOnlyList<GameMove> Moves => _moves;
    // ------------------------------------------------------------------------

    /// <summary>Clears the board and starts a fresh game.</summary>
    public void ResetBoard()
    {
        TheBoard = new PieceColor[42];
        PiecesPlayed = 0;
        WinnerColor = PieceColor.Blank;
        _moves.Clear();
    }

    /// <summary>
    /// Drops a piece into the given column (0-6) for the current player.
    /// Returns the board index where the piece landed.
    /// </summary>
    public int PlayPiece(int column)
    {
        if (WinnerColor != PieceColor.Blank)
        {
            throw new ArgumentException($"Game is over. {WinnerColor} has already won.");
        }

        if (column < 0 || column > 6)
        {
            throw new ArgumentException("Column must be between 0 and 6.");
        }

        // The top cell of a column being occupied means the column is full.
        if (TheBoard[column] != PieceColor.Blank)
        {
            throw new ArgumentException("That column is full.");
        }

        // Find the lowest empty slot in the column (pieces stack from the bottom).
        var landingSpot = column;
        for (var i = column; i < 42; i += 7)
        {
            if (TheBoard[i] == PieceColor.Blank)
            {
                landingSpot = i;
            }
        }

        var color = PlayerTurn == 1 ? PieceColor.Red : PieceColor.Yellow;
        TheBoard[landingSpot] = color;

        // Record the move for the history panel (columns/rows shown as 1-based).
        _moves.Add(new GameMove(
            MoveNumber: PiecesPlayed + 1,
            Player: PlayerTurn,
            Color: color,
            Column: (landingSpot % 7) + 1,
            Row: (landingSpot / 7) + 1));

        PiecesPlayed++;

        WinnerColor = CheckForWin();

        return landingSpot;
    }

    /// <summary>Scans the whole board and returns the winning color, or Blank if none.</summary>
    public PieceColor CheckForWin()
    {
        for (var i = 0; i < 42; i++)
        {
            if (TheBoard[i] == PieceColor.Blank)
            {
                continue;
            }

            var col = i % 7;
            var row = i / 7;

            // Horizontal (—): needs 3 columns of room to the right.
            if (col <= 3 &&
                TheBoard[i] == TheBoard[i + 1] &&
                TheBoard[i] == TheBoard[i + 2] &&
                TheBoard[i] == TheBoard[i + 3])
            {
                return TheBoard[i];
            }

            // Vertical (|): needs 3 rows of room below.
            if (row <= 2 &&
                TheBoard[i] == TheBoard[i + 7] &&
                TheBoard[i] == TheBoard[i + 14] &&
                TheBoard[i] == TheBoard[i + 21])
            {
                return TheBoard[i];
            }

            // Diagonal down-right (\): room to the right and below.
            if (col <= 3 && row <= 2 &&
                TheBoard[i] == TheBoard[i + 8] &&
                TheBoard[i] == TheBoard[i + 16] &&
                TheBoard[i] == TheBoard[i + 24])
            {
                return TheBoard[i];
            }

            // Diagonal down-left (/): room to the left and below.
            if (col >= 3 && row <= 2 &&
                TheBoard[i] == TheBoard[i + 6] &&
                TheBoard[i] == TheBoard[i + 12] &&
                TheBoard[i] == TheBoard[i + 18])
            {
                return TheBoard[i];
            }
        }

        return PieceColor.Blank;
    }
}

/// <summary>A single move in the game's history.</summary>
public record GameMove(int MoveNumber, int Player, PieceColor Color, int Column, int Row);

public enum PieceColor
{
    Blank,
    Red,
    Yellow
}
