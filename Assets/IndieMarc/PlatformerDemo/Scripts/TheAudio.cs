using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main script for playing audio
/// Author: Indie Marc (Marc-Antoine Desbiens)
/// </summary>

namespace IndieMarc.Platformer
{

    public class TheAudio : MonoBehaviour
    {
        public string[] preload_channels;

        private static TheAudio _instance;

        private Dictionary<string, AudioSource> channels = new Dictionary<string, AudioSource>();

        void Awake()
        {
            _instance = this;

            foreach (string channel in preload_channels)
            {
                CreateChannel(channel);
            }
        }

        //channel: Two sounds on the same channel will never play at the same time
        //priority: if false, will not play if a sound is already playing on the channel, if true, will replace current sound playing on channel
        public void PlaySound(string channel, AudioClip sound, float vol = 0.8f, bool priority = false)
        {
            if (string.IsNullOrEmpty(channel) || sound == null)
                return;

            AudioSource source = GetChannel(channel);

            if (source == null)
                source = CreateChannel(channel); //Create channel if doesnt exist, for optimisation put the channel in preload_channels so its created at start instead of here

            if (source)
            {
                if (priority || !source.isPlaying)
                {
                    source.clip = sound;
                    source.volume = vol;
                    source.Play();
                }
            }
        }

        public AudioSource GetChannel(string channel)
        {
            if (channels.ContainsKey(channel))
                return channels[channel];
            return null;
        }

        public bool DoesChannelExist(string channel)
        {
            return channels.ContainsKey(channel);
        }

        public AudioSource CreateChannel(string channel, int priority = 128)
        {
            if (string.IsNullOrEmpty(channel))
                return null;

            GameObject cobj = new GameObject("AudioChannel-" + channel);
            cobj.transform.parent = transform;
            AudioSource caudio = cobj.AddComponent<AudioSource>();
            caudio.playOnAwake = false;
            caudio.loop = false;
            caudio.priority = priority;
            channels[channel] = caudio;
            return caudio;
        }

        public static TheAudio Get()
        {
            return _instance;
        }
    }

}