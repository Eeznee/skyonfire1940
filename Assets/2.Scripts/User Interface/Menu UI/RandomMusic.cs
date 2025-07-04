﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(AudioSource))]
public class RandomMusic : MonoBehaviour
{
    public AudioClip[] clips;
    public Text text;
    void Start()
    {
        bool playMusic = PlayerPrefs.GetInt("Music", 1) == 1;

        if (!playMusic)
        {
            text.text = "";
            return;
        }
        int id = Random.Range(0, clips.Length);
        GetComponent<AudioSource>().clip = clips[id];
        GetComponent<AudioSource>().Play();
        GetComponent<AudioSource>().volume = 1f;
        text.text = "Playing : " + clips[id].name;
    }
}
