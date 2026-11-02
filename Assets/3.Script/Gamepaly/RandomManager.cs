using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomManager : MonoBehaviour
{
    [SerializeField] private bool isFixedRandom = false;
    [SerializeField] private int randomSeed;

    public int CurrentSeed { get; private set; }

    public int GetSeed()
    {
        if (!isFixedRandom)
        {
            CurrentSeed = GenerateSeed();
        }
        else
        {
            CurrentSeed = randomSeed;
        }

        return CurrentSeed;
    }

    private int GenerateSeed()
    {
        return System.Guid.NewGuid().GetHashCode();
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"Seed: {CurrentSeed}");
    }
}
