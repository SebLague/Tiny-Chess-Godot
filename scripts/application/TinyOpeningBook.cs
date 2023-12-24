using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public static class TinyOpeningBook
{
    static Random rng = new();
    static Dictionary<string, string[]> book;

    static TinyOpeningBook()
    {
        book = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string[]>>(TinyOpeningBookData.data);
    }

    public static Move TryGetMove(Board board, double randomlyDontUseBookProb = 0)
    {
        string fen = board.GetFenString();
        string[] fenSections = fen.Split(' ');
        string bookFen = fenSections[0] + " " + fenSections[1] + " " + fenSections[2] + " " + fenSections[3];
        if (rng.NextDouble() >= randomlyDontUseBookProb && book.TryGetValue(bookFen, out var entries))
        {
            double[] moveWeights = entries.Select(entry => double.Parse(entry.Split(' ')[1], System.Globalization.CultureInfo.InvariantCulture)).ToArray();

            double[] probCumul = new double[moveWeights.Length];
            for (int i = 0; i < moveWeights.Length; i++)
            {
                probCumul[i] = probCumul[Math.Max(0, i - 1)] + moveWeights[i];
            }


            double random = rng.NextDouble();
            for (int i = 0; i < probCumul.Length; i++)
            {
                if (random <= probCumul[i])
                {
                    string moveString = entries[i].Split(' ')[0];
                    return new Move(moveString, board);
                }
            }
        }
        return Move.NullMove;
    }

}
