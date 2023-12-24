using ChessChallenge.Chess;
using Godot;

public partial class GameManager : Node
{

    [Export] float minMoveDelay;
    [Export] NodePath boardUIPath;
    [Export] NodePath humanPlayerPath;
    [Export] Label botNameUI;
    [Export] Label pgnUI;
    [Export] Label gameResultUI;
    [Export] ScrollContainer pgnScroll;

    [Export] Button exitButton;
    [Export] Button resignButton;
    [Export] Button rematchButton;
    [Export] Button lichessButton;
    [Export] Button botCodeButton;

    [ExportCategory("Audio")]
    [Export] float volumeSfx;
    [Export] AudioStreamPlayer audioPlayer;
    [Export] AudioStream gameOverSfx;

    GameResult activeGameResult = GameResult.InProgress;
    BoardUI boardUI;
    HumanPlayer humanPlayer;

    public Board board;
    bool humanPlaysWhite = true;

    BotRunner bot;
    Move botMove;
    int gameID;
    float timeSinceLastMove;
    string lichessPGN;


    public override void _Ready()
    {
        // Set default bot if null (for testing)
        if (BotSettings.ActiveBotType == null)
        {
            BotSettings.ActiveBotName = "[Autosetting : Bot_614]";
            BotSettings.ActiveBotType = typeof(auto_Bot_614.Bot_614);
        }

        humanPlayer = GetNode<HumanPlayer>(humanPlayerPath);
        humanPlayer.MoveChosen += OnMoveChosen;

        bot = new(BotSettings.ActiveBotType);
        bot.MoveChosen += OnBotMoveChosen;

        InitUI();
        StartGame();
    }

    void InitUI()
    {
        boardUI = GetNode<BoardUI>(boardUIPath);
        boardUI.Init();

        botNameUI.Text = BotSettings.ActiveBotName;

        exitButton.Pressed += ExitToMenu;
        resignButton.Pressed += Resign;
        rematchButton.Pressed += Rematch;
        lichessButton.Pressed += OpenInLichess;
        botCodeButton.Pressed += OpenBotCode;
    }

    void StartGame()
    {
        rematchButton.Disabled = true;
        lichessButton.Disabled = true;
        resignButton.Disabled = false;
        pgnUI.Text = "";
        gameResultUI.GetParent<Label>().Visible = false;

        if (BotSettings.Stats != null)
        {
            bool whiteFirstGame = (BotSettings.ActiveBotIndex - 1) % 2 == 0;
            var wdl = BotSettings.Stats.GetWDL(BotSettings.ActiveBotSaveID);
            humanPlaysWhite = ((wdl.w + wdl.d + wdl.l) % 2 == 0) == whiteFirstGame || BotSettings.ActiveBotSaveID == "CopyCat";
        }
        else
        {
            humanPlaysWhite = !humanPlaysWhite;
        }
        // humanPlaysWhite = true;

        // Start game
        gameID++;
        board = new();
        board.LoadStartPosition();
        boardUI.UpdatePosition(board);
        boardUI.ResetSquareColours();
        boardUI.SetPerspective(!humanPlaysWhite);

        humanPlayer.Init(boardUI, board, humanPlaysWhite);
        botMove = Move.NullMove;

        bot.NotifyNewGameStarted(gameID);
        NotifyTurnToMove();
    }

    public override void _Process(double delta)
    {
        timeSinceLastMove += (float)delta;

        if (!botMove.IsNull && timeSinceLastMove >= minMoveDelay)
        {
            OnMoveChosen(botMove);
            botMove = Move.NullMove;
        }
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            ExitToMenu();
        }
    }

    void OnBotMoveChosen(Move move)
    {
        // get back to main thread
        botMove = move;
    }

    void OnMoveChosen(Move move)
    {
        bool legal = IsLegal(move);

        timeSinceLastMove = 0;
        bool isHumanMove = board.IsWhiteToMove == humanPlaysWhite;
        bool animate = !isHumanMove;
        bool isCapture = board.Square[move.TargetSquareIndex] != PieceHelper.None || move.IsEnPassant;

        board.MakeMove(move, inSearch: false);
        boardUI.UpdatePosition(board, move, isCapture, animate, sfx: true);

        if (legal)
        {
            activeGameResult = Arbiter.GetGameState(board);
        }
        else
        {
            activeGameResult = board.IsWhiteToMove ? GameResult.BlackIllegalMove : GameResult.WhiteIllegalMove;
        }


        // Update UI
        pgnUI.Text = PGNCreator.CreatePGN_InGameFormat(board, board.AllGameMoves.ToArray()).Replace("\r", "");
        if (!legal)
        {
            pgnUI.Text += "{Illegal Move}";
        }
        pgnScroll.ScrollVertical = (int)pgnScroll.GetVScrollBar().MaxValue;

        // Notify next move
        if (activeGameResult is GameResult.InProgress)
        {
            NotifyTurnToMove();
        }
        else
        {
            OnGameOver(activeGameResult);
        }
    }

    bool IsLegal(Move move)
    {
        Move[] legals = new MoveGenerator().GenerateMoves(board).ToArray();
        foreach (var l in legals)
        {
            if (Move.SameMove(move, l)) return true;
        }
        return false;
    }

    void NotifyTurnToMove()
    {

        bool isHumanTurn = board.IsWhiteToMove == humanPlaysWhite;

        if (!isHumanTurn)
        {
            bot.NotifyTurnToMove(board);
        }
    }

    void Rematch()
    {
        StartGame();
    }

    void Resign()
    {
        GameResult forfeitResult = humanPlaysWhite ? GameResult.WhiteResignation : GameResult.BlackResignation;
        OnGameOver(forfeitResult);
    }

    void ExitToMenu()
    {
        if (activeGameResult is GameResult.InProgress && board.AllGameMoves.Count >= 2)
        {
            Resign();
        }
        SceneHelper.SwitchScene(GetTree(), SceneHelper.botSelectionScene);
    }

    void OnGameOver(GameResult result)
    {
        humanPlayer.Deactivate();
        lichessPGN = PGNCreator.CreatePGN_LichessFormat(board, board.AllGameMoves.ToArray());
        bool isDraw = Arbiter.IsDrawResult(result);
        bool isHumanWins = Arbiter.IsWinResult(result) && Arbiter.IsWhiteWinsResult(result) == humanPlaysWhite;
        bool isHumanLoses = !isHumanWins && !isDraw;

        if (BotSettings.Stats != null)
        {
            BotSettings.Stats.AddResults(BotSettings.ActiveBotSaveID, isHumanWins ? 1 : 0, isDraw ? 1 : 0, isHumanLoses ? 1 : 0);
            PlayerStats.SaveBotStats(BotSettings.Stats);
        }

        resignButton.Disabled = true;
        rematchButton.Disabled = false;
        lichessButton.Disabled = false;
        activeGameResult = GameResult.NotStarted;

        gameResultUI.GetParent<Label>().Visible = true;
        gameResultUI.Text = GetGameResultString(result);
        PlayGameOverSound();
    }

    void PlayGameOverSound()
    {
        audioPlayer.Stop();
        audioPlayer.Stream = gameOverSfx;
        audioPlayer.VolumeDb = volumeSfx;
        audioPlayer.Play();
    }

    string GetGameResultString(GameResult result)
    {
        if (Arbiter.IsWhiteWinsResult(result)) return "White wins";
        if (Arbiter.IsBlackWinsResult(result)) return "Black wins";

        return result switch
        {
            GameResult.FiftyMoveRule => "50 move draw",
            GameResult.Repetition => "Repetition draw",
            GameResult.InsufficientMaterial => "Material draw",
            GameResult.Stalemate => "Stalemate",
            _ => ""
        };
    }

    string GetName(bool white)
    {
        return humanPlaysWhite == white ? "Human" : BotSettings.ActiveBotName;
    }

    void OpenInLichess()
    {
        Helpers.OpenUrl("https://lichess.org/analysis/pgn/" + lichessPGN);
    }

    void OpenBotCode()
    {
        Helpers.OpenUrl("https://github.com/SebLague/Tiny-Chess-Bot-Challenge-Results/blob/main/Bots/Bot_" + BotSettings.ActiveBotID + ".cs");
    }
}
