using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreRecord
{
    private const string KeyScore = "Forge_BestScore";
    private const string KeyTurns = "Forge_BestTurns";
    private const string KeyKills = "Forge_BestKills";

    public int BestScore { get; private set; }
    public int BestTurns { get; private set; }
    public int BestKills { get; private set; }
    public bool IsNewBest { get; private set; }

    public ScoreRecord()
    {
        Load();
    }

    public void Load()
    {
        BestScore = PlayerPrefs.GetInt(KeyScore, 0);
        BestTurns = PlayerPrefs.GetInt(KeyTurns, 0);
        BestKills = PlayerPrefs.GetInt(KeyKills, 0);
        IsNewBest = false;
    }

    public bool TrySave(int score, int turns, int kills)
    {
        IsNewBest = score > BestScore;
        if (!IsNewBest)
        {
            return false;
        }

        BestScore = score;
        BestTurns = turns;
        BestKills = kills;

        PlayerPrefs.SetInt(KeyScore, BestScore);
        PlayerPrefs.SetInt(KeyTurns, BestTurns);
        PlayerPrefs.SetInt(KeyKills, BestKills);
        PlayerPrefs.Save();

        return true;
    }

    public void Clear()
    {
        PlayerPrefs.DeleteKey(KeyScore);
        PlayerPrefs.DeleteKey(KeyTurns);
        PlayerPrefs.DeleteKey(KeyKills);
        PlayerPrefs.Save();
        Load();
    }
}
