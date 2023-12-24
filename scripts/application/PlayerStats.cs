using System.IO;
using System.Linq;
using System.Text.Json;

public static class PlayerStats
{

    public static AllBotStats TryLoadBotStats(string[] allIDs)
    {
        AllBotStats botStats = new AllBotStats() { botStats = allIDs.Select(i => new BotStat() { botID = i }).ToArray() };

        if (File.Exists(SavePath))
        {
            string text = File.ReadAllText(SavePath);
            try
            {
                AllBotStats loadedStats = JsonSerializer.Deserialize<AllBotStats>(text);
                if (loadedStats.botStats == null)
                {
                    return botStats;
                }

                for (int i = 0; i < botStats.botStats.Length; i++)
                {
                    foreach (var loadedStat in loadedStats.botStats)
                    {
                        if (botStats.botStats[i].botID == loadedStat.botID)
                        {
                            botStats.botStats[i] = loadedStat;
                            break;
                        }
                    }
                }

                return botStats;
            }
            catch { }
        }

        return botStats;


    }

    public static void SaveBotStats(AllBotStats stats)
    {
        if (stats != null && stats.botStats != null) // can be null if starting game from wrong scene (for testing)
        {
            Directory.CreateDirectory(SaveDirectory);
            JsonSerializerOptions options = new JsonSerializerOptions() { WriteIndented = true };
            File.WriteAllText(SavePath, JsonSerializer.Serialize(stats, options));
        }
    }

    static string SavePath => Path.Combine(SaveDirectory, "Stats.json");

    static string SaveDirectory
    {
        get
        {
            string p = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(p, "SebastianLague", "Tiny-Chess-Bots");
        }
    }


    public class AllBotStats
    {
        public BotStat[] botStats { get; set; }

        public (int w, int d, int l) GetWDL(string id)
        {
            for (int i = 0; i < botStats.Length; i++)
            {
                if (botStats[i].botID == id)
                {
                    return (botStats[i].numWins, botStats[i].numDraws, botStats[i].numLosses);
                }
            }
            return (0, 0, 0);
        }

        public void AddResults(string id, int w, int d, int l)
        {
            if (botStats == null) return;
            for (int i = 0; i < botStats.Length; i++)
            {
                if (botStats[i].botID == id)
                {
                    botStats[i].numWins += w;
                    botStats[i].numDraws += d;
                    botStats[i].numLosses += l;
                    break;
                }
            }
        }
    }

    public struct BotStat
    {
        public string botID { get; set; }
        public int numWins { get; set; }
        public int numDraws { get; set; }
        public int numLosses { get; set; }
    }
}
