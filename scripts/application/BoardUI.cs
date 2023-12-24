using ChessChallenge.Chess;
using Godot;

public partial class BoardUI : Node
{
    [Export] float scale;
    [Export] bool blackPerspective;
    [Export] float boardOffset;

    [ExportCategory("Colours")]
    [Export] Color colLightNormal;
    [Export] Color colLightHighlight;
    [Export] Color colLightMoveFrom;
    [Export] Color colLightMoveTo;
    [Export] Color colLightLegal;
    [Export] Color colLightCheck;
    [Export] Color colDarkNormal;
    [Export] Color colDarkHighlight;
    [Export] Color colDarkMoveFrom;
    [Export] Color colDarkMoveTo;
    [Export] Color colDarkLegal;
    [Export] Color colDarkCheck;

    [ExportCategory("Pieces")]
    [Export] float moveAnimationDuration;
    [Export] float pieceScale;
    [Export] Texture2D pieceTex;

    [ExportCategory("Audio")]
    [Export] float volumeSfx;
    [Export] AudioStreamPlayer audioPlayer;
    [Export] AudioStream moveSfx;
    [Export] AudioStream captureSfx;

    // Consts
    const int ReferenceWidth = 1280;
    const int ReferenceHeight = 720;
    const int SquareSize = 100;
    static readonly int[] pieceImageOrder = { 5, 3, 2, 4, 1, 0 };

    // State
    bool whiteIsBottom = true;

    // References
    ColorRect[,] squares;
    Sprite2D[,] pieceSquares;
    MoveGenerator moveGenerator;

    // Animation state
    Move animMove;
    bool isAnimatingMove;
    float moveAnimT;
    Board moveAnimBoardTarget;
    bool playSoundInAnim;
    AudioStream soundToPlay;

    float scaledSquareSize;
    float scaledPieceSize;
    Vector2 offset;


    public void Init()
    {
        moveGenerator = new();

        scaledSquareSize = SquareSize * scale;
        scaledPieceSize = pieceScale * scale;
        offset = new Vector2(ReferenceWidth, ReferenceHeight) / 2;

        CreateBoardUI();
    }

    public override void _Process(double delta)
    {
        if (isAnimatingMove)
        {
            UpdateMoveAnimation(delta);
        }
    }

    void UpdateMoveAnimation(double delta)
    {
        moveAnimT += (float)delta / moveAnimationDuration;
        if (playSoundInAnim && moveAnimT > 0.5f)
        {
            PlaySound(soundToPlay);
            playSoundInAnim = false;
        }
        Coord startCoord = BoardHelper.CoordFromIndex(animMove.StartSquareIndex);
        Coord targetCoord = BoardHelper.CoordFromIndex(animMove.TargetSquareIndex);
        Sprite2D piece = pieceSquares[startCoord.fileIndex, startCoord.rankIndex];
        piece.ZIndex = 1;

        SetSquareColour(startCoord, colLightMoveFrom, colDarkMoveFrom);
        if (moveAnimBoardTarget.IsInCheck())
        {
            Color checkAnimColLight = colLightNormal + (colLightCheck - colLightNormal) * InOut(moveAnimT);
            Color checkAnimColDark = colDarkNormal + (colDarkCheck - colDarkNormal) * InOut(moveAnimT);
            int kingIndex = moveAnimBoardTarget.KingSquare[moveAnimBoardTarget.MoveColourIndex];

            SetSquareColour(new Coord(kingIndex), checkAnimColLight, checkAnimColDark);
        }
        Vector2 startPos = PiecePositionFromCoord(startCoord);
        Vector2 endPos = PiecePositionFromCoord(targetCoord);
        piece.Position = startPos + (endPos - startPos) * InOut(moveAnimT);

        if (moveAnimT >= 1)
        {
            isAnimatingMove = false;
            ResetSquareColours();
            UpdatePosition(moveAnimBoardTarget);
            HighlightMoveSquares(animMove);
        }
    }

    public void UpdatePosition(Board board, Move move, bool isCapture, bool animate = false, bool sfx = false)
    {
        //GD.Print("Update pos " + MoveUtility.GetMoveNameUCI(move)+ "  " +  FenUtility.CurrentFen(board));
        // animate = false;
        if (animate && moveAnimationDuration > 0)
        {
            // Interrupt previous animation
            if (isAnimatingMove)
            {
                ResetSquareColours();
                UpdatePosition(moveAnimBoardTarget);
                HighlightMoveSquares(animMove);
            }
            // Set up move animation
            animMove = move;
            moveAnimBoardTarget = new Board(board);
            isAnimatingMove = true;
            moveAnimT = 0;

        }
        else
        {
            ResetSquareColours();
            UpdatePosition(board);
            HighlightMoveSquares(move);

        }

        if (sfx)
        {
            soundToPlay = isCapture ? captureSfx : moveSfx;

            if (animate)
            {
                playSoundInAnim = true;
            }
            else
            {
                PlaySound(soundToPlay);
            }
        }

    }


    public void UpdatePosition(Board board)
    {
        //GD.Print("Update pos  " + FenUtility.CurrentFen(board));
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                Coord coord = new Coord(file, rank);
                int piece = board.Square[BoardHelper.IndexFromCoord(coord.fileIndex, coord.rankIndex)];
                pieceSquares[file, rank].RegionRect = PieceRect(piece);
                pieceSquares[file, rank].Position = PiecePositionFromCoord(file, rank);
                pieceSquares[file, rank].ZIndex = 0;
            }
        }

        if (board.IsInCheck())
        {
            SetSquareColour(new Coord(board.KingSquare[board.MoveColourIndex]), colLightCheck, colDarkCheck);
        }
    }

    void PlaySound(AudioStream sound)
    {
        audioPlayer.Stop();
        audioPlayer.Stream = sound;
        audioPlayer.VolumeDb = volumeSfx;
        audioPlayer.Play();
    }

    Rect2 PieceRect(int piece)
    {
        if (piece == PieceHelper.None)
        {
            return new Rect2(0, 0, 0, 0);
        }
        const int spriteSize = 333;
        //return new Rect2(0, size, size, size);]
        int type = PieceHelper.PieceType(piece);
        bool white = PieceHelper.IsWhite(piece);
        return new Rect2(spriteSize * pieceImageOrder[PieceHelper.PieceType(type) - 1], white ? 0 : spriteSize, spriteSize, spriteSize);

    }

    void CreateBoardUI()
    {
        squares = new ColorRect[8, 8];
        pieceSquares = new Sprite2D[8, 8];

        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                // Create square
                ColorRect square = new();

                squares[file, rank] = square;
                square.Position = PositionFromCoord(file, rank);
                square.Size = new Vector2(scaledSquareSize, scaledSquareSize);
                AddChild(square);
            }
        }

        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                Sprite2D pieceSquare = new();
                pieceSquare.TextureFilter = CanvasItem.TextureFilterEnum.LinearWithMipmaps;
                //pieceSquare.Modulate = new Color(1, 1, 1, 0.4f);
                pieceSquares[file, rank] = pieceSquare;
                AddChild(pieceSquare);
                pieceSquare.Scale = Vector2.One * scaledPieceSize;
                pieceSquare.Texture = pieceTex;
                pieceSquare.RegionEnabled = true;
                ResetPiecePosition(new Coord(file, rank));
            }
        }

        ResetSquareColours();
    }

    public void SetPerspective(bool blackPerspective)
    {
        if (blackPerspective != this.blackPerspective)
        {
            this.blackPerspective = blackPerspective;
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    squares[file, rank].Position = PositionFromCoord(file, rank);
                }
            }

            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    ResetPiecePosition(new Coord(file, rank));
                }
            }

        }
    }

    public void HighlightMoveSquares(Move move)
    {
        SetSquareColour(BoardHelper.CoordFromIndex(move.StartSquareIndex), colLightMoveFrom, colDarkMoveFrom);
        SetSquareColour(BoardHelper.CoordFromIndex(move.TargetSquareIndex), colLightMoveTo, colDarkMoveTo);
    }

    public void HighlightLastMadeMoveSquares(Board board)
    {
        if (board.AllGameMoves.Count > 0)
        {
            HighlightMoveSquares(board.AllGameMoves[^1]);
        }
    }

    public void HighlightSquare(Coord coord)
    {
        SetSquareColour(coord, colLightHighlight, colDarkHighlight);
    }

    public void HighlightLegalMoves(Board board, Coord fromSquare)
    {

        Move[] moves = moveGenerator.GenerateMoves(board).ToArray();

        for (int i = 0; i < moves.Length; i++)
        {
            Move move = moves[i];

            if (move.StartSquareIndex == BoardHelper.IndexFromCoord(fromSquare))
            {
                Coord coord = BoardHelper.CoordFromIndex(move.TargetSquareIndex);
                SetSquareColour(coord, colLightLegal, colDarkLegal);
            }
        }
    }


    public void DragPiece(Coord coord, Vector2 mousePos)
    {
        pieceSquares[coord.fileIndex, coord.rankIndex].GlobalPosition = mousePos;
        pieceSquares[coord.fileIndex, coord.rankIndex].ZIndex = 1;
    }

    public void ResetPiecePosition(Coord coord)
    {
        pieceSquares[coord.fileIndex, coord.rankIndex].Position = PiecePositionFromCoord(coord.fileIndex, coord.rankIndex);
        pieceSquares[coord.fileIndex, coord.rankIndex].ZIndex = 0;
    }


    public void ResetSquareColours()
    {
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                SetSquareColour(new Coord(file, rank), colLightNormal, colDarkNormal);
            }
        }
    }

    void SetSquareColour(Coord coord, Color colLight, Color colDark)
    {
        squares[coord.fileIndex, coord.rankIndex].Color = coord.IsLightSquare() ? colLight : colDark;
    }


    public Vector2 PositionFromCoord(int file, int rank)
    {
        Vector2 squareOffset = -new Vector2(scaledSquareSize, scaledSquareSize) / 2;
        if (blackPerspective)
        {
            rank = 7 - rank;
            file = 7 - file;
        }
        return new Vector2(-3.5f + file, 7 - rank - 3.5f) * scaledSquareSize + offset + squareOffset + new Vector2(boardOffset, 0);
    }

    Vector2 PiecePositionFromCoord(Coord coord) => PiecePositionFromCoord(coord.fileIndex, coord.rankIndex);


    Vector2 PiecePositionFromCoord(int file, int rank)
    {
        return PositionFromCoord(file, rank) + new Vector2(scaledSquareSize, scaledSquareSize) / 2;
    }

    public bool TryGetCoordFromPosition(Vector2 pos, out Coord selectedCoord)
    {
        Vector2 squareOffset = -new Vector2(scaledSquareSize, scaledSquareSize) / 2;
        double file = ((pos.X - offset.X - squareOffset.X - boardOffset) / scaledSquareSize) + 3.5;
        double rank = ((pos.Y - offset.Y - squareOffset.X) / scaledSquareSize) + 3.5;
        if (blackPerspective)
        {
            file = 8 - file;
            rank = 8 - rank;
        }

        selectedCoord = new Coord((int)file, 7 - (int)rank);
        return file >= 0 && file < 8 && rank >= 0 && rank < 8;
    }

    public bool IsBlackPerspective => blackPerspective;
    public static float InOut(float t) => 3 * Square(Clamp01(t)) - 2 * Cube(Clamp01(t));
    static float Clamp01(float t) => System.Math.Clamp(t, 0, 1);
    static float Square(float x) => x * x;
    static float Cube(float x) => x * x * x;

}
