using ChessChallenge.Chess;
using System;
using System.Threading;
using System.Threading.Tasks;

public class BotRunner
{
    public event Action<Move> MoveChosen;
    public ChessChallenge.API.IChessBot bot;
    AutoResetEvent botTaskWaitHandle;
    int activeGameID;
    Board board;

    public BotRunner(Type botType)
    {
        bot = (ChessChallenge.API.IChessBot)Activator.CreateInstance(botType);
    }

    public void NotifyNewGameStarted(int gameID)
    {
        this.activeGameID = gameID;
        // Allow task to terminate
        botTaskWaitHandle?.Set();
        // Create new task
        botTaskWaitHandle = new AutoResetEvent(false);
        Task.Factory.StartNew(BotThinkerThread, TaskCreationOptions.LongRunning);

    }

    public void NotifyTurnToMove(Board board)
    {
        this.board = board;
        botTaskWaitHandle.Set();
    }


    void BotThinkerThread()
    {
        int threadID = activeGameID;
        //Console.WriteLine("Starting thread: " + threadID);

        while (true)
        {
            // Sleep thread until notified
            botTaskWaitHandle.WaitOne();
            // Get bot move
            if (threadID == activeGameID)
            {
                int time = 30 * 1000; // amount of time bot thinks is remaining in game
                var move = bot.Think(new ChessChallenge.API.Board(board), new ChessChallenge.API.Timer(time, time, time, 100));

                if (threadID == activeGameID)
                {

                    MoveChosen?.Invoke(new Move(move.RawValue));
                }
            }
            // Terminate if no longer playing this game
            if (threadID != activeGameID)
            {
                break;
            }
        }
        //Console.WriteLine("Exitting thread: " + threadID);
    }

}
