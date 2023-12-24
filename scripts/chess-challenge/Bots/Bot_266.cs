namespace auto_Bot_266;
// Game Tech Explained Bot
//
// Story:
// https://youtu.be/5vsLmM756LA
//
// This is a "readable" version of the code. You can find the original code, which I show in the video, and which I
// submitted to the competition, at https://github.com/GameTechExplained/Chess-Challenge. It is prettier for some
// definition of pretty.

using ChessChallenge.API;
using System;
using static System.Math;

public class Bot_266 : IChessBot
{
    static Entry[] _transpositions = new Entry[16777216];
    float _budgetCounter = 121, budget;
    bool _outOfTime;
    Timer timer;
    Board board;
    System.Collections.Generic.HashSet<Move> _killerMoves = new();

    float OneMinusEndgameT(bool white)
    {
        int endgameWeightSum = 0;
        foreach (var pl in board.GetAllPieceLists())
            if (pl.IsWhitePieceList == white)
                endgameWeightSum += (0x942200 >> 4 * (int)pl.TypeOfPieceInList & 0xf) * pl.Count;

        return Min(1, endgameWeightSum * 0.04f);
    }

    int EvaluateBoard()
    {
        float ownOneMinusEndgameT = OneMinusEndgameT(false), otherOneMinusEndgameT = OneMinusEndgameT(true),
              score = 0.0f;
        foreach (var pl in board.GetAllPieceLists())
            score += 0b1000010 >> (int)pl.TypeOfPieceInList != 0
                ? (pl.IsWhitePieceList ? ownOneMinusEndgameT : -otherOneMinusEndgameT) * EvaluatePieceSquareTable(Starts, pl)
                  + (pl.IsWhitePieceList ? 1.0f - ownOneMinusEndgameT : otherOneMinusEndgameT - 1.0f) * EvaluatePieceSquareTable(Ends, pl)
                : EvaluatePieceSquareTable(Starts, pl);
        return (int)(board.IsWhiteToMove ? score : -score);
    }


    ulong EvaluatePieceSquareTable(ulong[][] table, PieceList pl)
    {
        ulong value = 0;
        foreach (var p in pl)
        {
            var sq = p.Square;
            value += table[(int)p.PieceType][sq.File >= 4 ? 7 - sq.File : sq.File] << 8 * (pl.IsWhitePieceList ? 7 - sq.Rank : sq.Rank) >> 56;
        }
        return 25 * value;
    }

    (int, Move, bool) Search(int depthLeft, int checkExtensionsLeft, bool isCaptureOnly, int alpha = -32200, int beta = 32200)
    {
        if (board.IsInCheckmate())
            return (-32100, default, true);

        if (board.IsDraw())
            return (0, default, board.IsInStalemate() || board.IsInsufficientMaterial());

        if (depthLeft == 0)
        {
            ++depthLeft;
            if (board.IsInCheck() && checkExtensionsLeft > 0)
                --checkExtensionsLeft;
            else if (!isCaptureOnly && checkExtensionsLeft == 4)
                return Search(8, checkExtensionsLeft, true, alpha, beta);
            else
                return (EvaluateBoard(), default, true);
        }

        ulong key = board.ZobristKey;
        Entry trans = _transpositions[key % 16777216];
        int bestScore = -32150, score;
        Move best = default;
        if (trans.Key == key && Abs(trans.Depth) >= depthLeft)
        {
            board.MakeMove(trans.Move);
            bool toDraw = board.IsDraw();
            board.UndoMove(trans.Move);

            if (toDraw)
                trans = default;
            else
            {
                alpha = Max(alpha, bestScore = trans.Score);
                best = trans.Move;
                if (beta < alpha || trans.Depth >= 0)
                    return (trans.Score, trans.Move, true);
            }
        }

        if (isCaptureOnly && (score = EvaluateBoard()) > bestScore && beta < (alpha = Max(alpha, bestScore = score)))
            return (score, default, true);

        Span<Move> legal = stackalloc Move[256];
        board.GetLegalMovesNonAlloc(ref legal, isCaptureOnly);

        Span<(int, Move)> prioritizedMoves = stackalloc (int, Move)[legal.Length];
        int loopvar = 0;
        foreach (var lmove in legal)
            prioritizedMoves[loopvar++] = (
                  (trans.Key == key && lmove == trans.Move ? 5000 : _killerMoves.Contains(lmove) ? 500 : 0)
                + (lmove.PromotionPieceType == PieceType.Queen ? 5 : 0)
                + (0x0953310 >> 4 * (int)lmove.CapturePieceType & 0xf),
                lmove);

        prioritizedMoves.Sort((a, b) => -a.Item1.CompareTo(b.Item1));

        bool canUseTranspositions = true, approximate = false, canUse;
        loopvar = 0;
        foreach (var (_, move) in prioritizedMoves)
        {
            if (_outOfTime = timer.MillisecondsElapsedThisTurn >= budget)
                return (bestScore, best, canUseTranspositions);

            board.MakeMove(move);
            try
            {
                if (depthLeft >= 3 && ++loopvar >= 4 && !move.IsCapture)
                {
                    score = -Search(depthLeft - 2, checkExtensionsLeft, isCaptureOnly, -beta, -alpha).Item1;

                    if (_outOfTime)
                        break;

                    if (score < bestScore)
                        continue;
                }
                (score, _, canUse) = Search(depthLeft - 1, checkExtensionsLeft, isCaptureOnly, -beta, -alpha);

                if (_outOfTime)
                    break;

                score = -score + (Abs(score) >= 30000 ? Sign(score) : 0);

                if (score <= bestScore)
                    continue;

                bestScore = score;
                best = move;
                alpha = Max(alpha, score);
                canUseTranspositions = canUse;

                if (approximate = beta < alpha)
                {
                    _killerMoves.Add(move);
                    break;
                }
            }
            finally
            {
                board.UndoMove(move);
            }
        }

        if (!_outOfTime && !isCaptureOnly && canUseTranspositions && bestScore != 0)
            _transpositions[key % 16777216] = new Entry { Key = key, Depth = (short)(approximate ? -depthLeft : depthLeft), Score = (short)bestScore, Move = best };

        return (bestScore, best, canUseTranspositions);
    }

    public Move Think(Board b, Timer t)
    {
        board = b;

        // [Seb tweak start] (adding tiny opening book for extra variety when playing against humans)
        if (board.PlyCount < 6)
        {
            Move bookMove = TinyOpeningBook.TryGetMove(board, randomlyDontUseBookProb: 0.3);
            if (!bookMove.IsNull)
            {
                return bookMove;
            }
        }
        // [Seb tweak end]


        timer = t;
        budget = Min(0.033333333333333333333333f, 2.0f / --_budgetCounter) * t.MillisecondsRemaining;
        _outOfTime = false;

        _killerMoves.Clear();
        Move bestMove = default, move;

        int depth = 0;
        while (++depth <= 15 && !_outOfTime)
            if ((move = Search(depth, 4, false).Item2) != default)
                bestMove = move;

        return bestMove == default ? b.GetLegalMoves()[0] : bestMove;
    }

    static readonly ulong[] Knights = { 0x3234363636363432ul, 0x34383c3d3c3d3834ul, 0x363c3e3f3f3e3c36ul, 0x363c3f40403f3d36ul },
                                   Bishops = { 0x3c3e3e3e3e3e3e3cul, 0x3e4040414042413eul, 0x3e4041414242403eul, 0x3e4042424242403eul },
                                   Rooks = { 0x6465636363636364ul, 0x6466646464646464ul, 0x6466646464646464ul, 0x6466646464646465ul },
                                   Queens = { 0xb0b2b2b3b4b2b2b0ul, 0xb2b4b4b4b4b5b4b2ul, 0xb2b4b5b5b5b5b5b2ul, 0xb3b4b5b5b5b5b4b3ul };
    ulong[][] Starts = { null, new[] { 0x141e161514151514ul, 0x141e161514131614ul, 0x141e181614121614ul, 0x141e1a1918141014ul }, Knights, Bishops, Rooks,
                               Queens, new[] { 0x0004080a0c0e1414ul, 0x020406080a0c1416ul, 0x020406080a0c0f12ul, 0x02040406080c0f10ul } },
                     Ends = { null, new[] { 0x14241e1a18161614ul, 0x14241e1a18161614ul, 0x14241e1a18161614ul, 0x14241e1a18161614ul }, Knights, Bishops, Rooks,
                               Queens, new[] { 0x0c0f0e0d0c0b0a06ul, 0x0e100f0e0d0c0b0aul, 0x0e1114171614100aul, 0x0e1116191815100aul } };

    struct Entry
    {
        public ulong Key;
        public short Score, Depth;
        public Move Move;
    }

}
