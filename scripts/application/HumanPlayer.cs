using ChessChallenge.Chess;
using Godot;
using System;

public partial class HumanPlayer : Node
{
    public event Action<Move> MoveChosen;

    public enum InputState
    {
        None,
        PieceSelected,
        DraggingPiece
    }

    InputState currentState;
    Coord selectedPieceSquare;

    BoardUI boardUI;
    Board board;

    bool leftMouseHeld;
    bool leftMouseDownThisFrame;
    bool leftMouseUpThisFrame;
    bool rightMouseDownThisFrame;
    bool rightMouseHeld;

    bool isPlayingWhite = true;
    bool isActive = true;


    public void Init(BoardUI boardUI, Board board, bool isPlayingWhite)
    {
        this.boardUI = boardUI;
        this.board = board;
        this.isPlayingWhite = isPlayingWhite;
        isActive = true;
    }

    public void Deactivate()
    {
        if (currentState == InputState.DraggingPiece)
        {
            CancelPieceSelection();
        }
        isActive = false;
    }

    public override void _Process(double delta)
    {
        if (!isActive) return;

        Vector2 mousePos = GetViewport().GetMousePosition();
        UpdateMouseButtonState();

        if (currentState == InputState.None)
        {
            HandlePieceSelection(mousePos);
        }
        else if (currentState == InputState.DraggingPiece)
        {
            HandleDragMovement(mousePos);
        }
        else if (currentState == InputState.PieceSelected)
        {
            HandlePointAndClickMovement(mousePos);
        }

    }

    void HandlePointAndClickMovement(Vector2 mousePos)
    {
        if (leftMouseHeld)
        {
            HandlePiecePlacement(mousePos);
        }
    }

    void HandleDragMovement(Vector2 mousePos)
    {
        boardUI.DragPiece(selectedPieceSquare, mousePos);
        // If mouse is released, then try place the piece
        if (leftMouseUpThisFrame)
        {
            HandlePiecePlacement(mousePos);
        }
        if (rightMouseDownThisFrame)
        {
            CancelPieceSelection();
        }
    }

    void HandlePiecePlacement(Vector2 mousePos)
    {
        Coord targetSquare;
        if (boardUI.TryGetCoordFromPosition(mousePos, out targetSquare))
        {
            if (targetSquare.Equals(selectedPieceSquare))
            {
                CancelPieceSelection();
            }
            else
            {
                int targetIndex = BoardHelper.IndexFromCoord(targetSquare.fileIndex, targetSquare.rankIndex);
                if (PieceHelper.IsColour(board.Square[targetIndex], board.MoveColour) && board.Square[targetIndex] != 0)
                {
                    CancelPieceSelection();
                    HandlePieceSelection(mousePos);
                }
                else
                {
                    CancelPieceSelection();
                    TryMakeMove(selectedPieceSquare, targetSquare);
                }
            }
        }
        else
        {
            CancelPieceSelection();
        }
    }

    void CancelPieceSelection()
    {
        if (currentState != InputState.None)
        {
            currentState = InputState.None;
            boardUI.ResetSquareColours();
            boardUI.HighlightLastMadeMoveSquares(board);
            boardUI.ResetPiecePosition(selectedPieceSquare);
        }
    }

    void HandlePieceSelection(Vector2 mousePos)
    {
        if (leftMouseDownThisFrame)
        {
            if (boardUI.TryGetCoordFromPosition(mousePos, out selectedPieceSquare))
            {
                int index = BoardHelper.IndexFromCoord(selectedPieceSquare);
                // If square contains a piece, select that piece for dragging
                if (PieceHelper.IsColour(board.Square[index], board.MoveColour) && board.IsWhiteToMove == isPlayingWhite)
                {
                    boardUI.HighlightLegalMoves(board, selectedPieceSquare);
                    boardUI.HighlightSquare(selectedPieceSquare);
                    currentState = InputState.DraggingPiece;
                }
            }

        }
    }

    void UpdateMouseButtonState()
    {
        leftMouseDownThisFrame = Input.IsMouseButtonPressed(MouseButton.Left) && !leftMouseHeld;
        leftMouseUpThisFrame = !Input.IsMouseButtonPressed(MouseButton.Left) && leftMouseHeld;
        leftMouseHeld = Input.IsMouseButtonPressed(MouseButton.Left);

        rightMouseDownThisFrame = Input.IsMouseButtonPressed(MouseButton.Right) && !rightMouseHeld;
        rightMouseHeld = Input.IsMouseButtonPressed(MouseButton.Right);
    }

    void TryMakeMove(Coord startSquare, Coord targetSquare)
    {
        int startIndex = BoardHelper.IndexFromCoord(startSquare);
        int targetIndex = BoardHelper.IndexFromCoord(targetSquare);
        bool moveIsLegal = false;
        Move chosenMove = new Move();

        MoveGenerator moveGenerator = new MoveGenerator();
        //bool wantsKnightPromotion = Keyboard.current[Key.LeftAlt].isPressed;
        bool wantsKnightPromotion = false;

        var legalMoves = moveGenerator.GenerateMoves(board);
        for (int i = 0; i < legalMoves.Length; i++)
        {
            var legalMove = legalMoves[i];

            if (legalMove.StartSquareIndex == startIndex && legalMove.TargetSquareIndex == targetIndex)
            {
                if (legalMove.IsPromotion)
                {
                    if (legalMove.MoveFlag == Move.PromoteToQueenFlag && wantsKnightPromotion)
                    {
                        continue;
                    }
                    if (legalMove.MoveFlag != Move.PromoteToQueenFlag && !wantsKnightPromotion)
                    {
                        continue;
                    }
                }
                moveIsLegal = true;
                chosenMove = legalMove;
                //	Debug.Log (legalMove.PromotionPieceType);
                break;
            }
        }

        if (moveIsLegal)
        {
            ChoseMove(chosenMove);
            currentState = InputState.None;
        }
        else
        {
            CancelPieceSelection();
        }
    }

    void ChoseMove(Move move)
    {
        MoveChosen.Invoke(move);
    }
}
