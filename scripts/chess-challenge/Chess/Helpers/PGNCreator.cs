using System.Text;

namespace ChessChallenge.Chess
{

    public static class PGNCreator
    {

        public static string CreatePGN(Move[] moves)
        {
            return CreatePGN(moves, GameResult.InProgress, FenUtility.StartPositionFEN);
        }

        public static string CreatePGN(Board board, GameResult result, string whiteName = "", string blackName = "")
        {
            return CreatePGN(board.AllGameMoves.ToArray(), result, board.GameStartFen, whiteName, blackName);
        }

        public static string CreatePGN(Move[] moves, GameResult result, string startFen, string whiteName = "", string blackName = "")
        {
            startFen = startFen.Replace("\n", "").Replace("\r", "");

            StringBuilder pgn = new();
            Board board = new Board();
            board.LoadPosition(startFen);
            // Headers
            if (!string.IsNullOrEmpty(whiteName))
            {
                pgn.AppendLine($"[White \"{whiteName}\"]");
            }
            if (!string.IsNullOrEmpty(blackName))
            {
                pgn.AppendLine($"[Black \"{blackName}\"]");
            }

            if (startFen != FenUtility.StartPositionFEN)
            {
                pgn.AppendLine($"[FEN \"{startFen}\"]");
            }
            if (result is not GameResult.NotStarted or GameResult.InProgress)
            {
                pgn.AppendLine($"[Result \"{result}\"]");
            }

            for (int plyCount = 0; plyCount < moves.Length; plyCount++)
            {
                string moveString = MoveUtility.GetMoveNameSAN(moves[plyCount], board);
                board.MakeMove(moves[plyCount]);

                if (plyCount % 2 == 0)
                {
                    pgn.Append((plyCount / 2 + 1) + ". ");
                }
                pgn.Append(moveString + " ");
            }

            if (Arbiter.IsWinResult(result))
            {
                pgn.Append(Arbiter.IsWhiteWinsResult(result) ? "1-0" : "0-1");
            }
            else if (Arbiter.IsDrawResult(result))
            {
                pgn.Append("1/2-1/2");
            }

            return pgn.ToString();
        }

        public static string CreatePGN_InGameFormat(Board b, Move[] moves)
        {


            StringBuilder pgn = new();
            Board board = new Board();
            board.LoadPosition(b.GameStartFen);

            for (int plyCount = 0; plyCount < moves.Length; plyCount++)
            {
                string moveString = MoveUtility.GetMoveNameSAN(moves[plyCount], board);
                board.MakeMove(moves[plyCount]);

                if (plyCount % 2 == 0)
                {
                    pgn.Append((plyCount / 2 + 1) + ". ");
                }
                pgn.Append(moveString + "  ");
                if (plyCount % 2 != 0)
                {
                    pgn.AppendLine();
                }
            }

            return pgn.ToString();
        }

        public static string CreatePGN_LichessFormat(Board b, Move[] moves)
        {

            StringBuilder pgn = new();
            Board board = new Board();
            board.LoadPosition(b.GameStartFen);

            for (int plyCount = 0; plyCount < moves.Length; plyCount++)
            {
                string moveString = MoveUtility.GetMoveNameSAN(moves[plyCount], board);
                board.MakeMove(moves[plyCount]);
                if (plyCount != moves.Length - 1)
                {
                    pgn.Append(moveString + "_");
                }
            }

            return pgn.ToString();
        }

    }
}