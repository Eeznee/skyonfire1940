using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Gun Audio Samples", menuName = "SOF/Weapons/Gun Audio Sample")]
public class GunAudioSample : ScriptableObject
{
    public AudioClip autoOut;
    public AudioClip autoIn;
    public AudioClip endOut;
    public AudioClip endIn;
    public int amount = 1;

    private static float Matching(int firingAmount, int sampleAmount)
    {
        float matching = (float)firingAmount / sampleAmount;
        if (matching > 1f) matching = 1f / matching;
        return matching;
    }
    public static GunAudioSample GetBestSample(GunAudioSample[] samples,int firingAmount)
    {
        float bestMatching = 0f;
        GunAudioSample bestSample = samples[0];

        for(int i = 0; i < samples.Length; i++)
        {
            float matching = Matching(firingAmount, samples[i].amount);
            if (matching > bestMatching) 
            { 
                bestMatching = matching; 
                bestSample = samples[i]; 
            }
        }
        return bestSample;
    }
}
