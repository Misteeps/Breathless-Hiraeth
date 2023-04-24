using System;

using UnityEngine;
using UnityEngine.Audio;

using Simplex;


namespace Game
{
    public static class Audio
    {
        #region Group
        public class Group
        {
            public readonly string name;
            public readonly AudioMixerGroup output;
            public readonly AudioSource global;

            public float Decibels
            {
                get => (Mixer.GetFloat($"{name} Volume", out float volume)) ? volume : throw new Exception().Overwrite($"Failed getting {name:info} volume");
                set => Mixer.SetFloat($"{name} Volume", value);
            }


            public Group(string name)
            {
                this.name = name;
                this.output = Mixer.FindMatchingGroups(name)[0];
                this.global = CreateSource2D(GameObject.FindGameObjectWithTag("GameController").transform);

                global.bypassEffects = true;
                global.bypassReverbZones = true;
                global.bypassListenerEffects = true;
            }

            public AudioSource CreateSource2D(Transform parent = null, float x = 0, float y = 0, float z = 0)
            {
                GameObject gameObject = new GameObject($"{name} Audio");
                gameObject.transform.parent = parent;
                gameObject.transform.localPosition = new Vector3(x, y, z);

                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.outputAudioMixerGroup = output;
                source.playOnAwake = false;

                source.volume = 0.5f;
                source.spatialBlend = 0;
                source.dopplerLevel = 0;
                source.spread = 0;
                source.minDistance = 0;
                source.maxDistance = 0;

                return source;
            }
            public AudioSource CreateSource3D(Transform parent = null, float x = 0, float y = 0, float z = 0)
            {
                GameObject gameObject = new GameObject($"{name} Audio");
                gameObject.transform.parent = parent;
                gameObject.transform.localPosition = new Vector3(x, y, z);

                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.outputAudioMixerGroup = output;
                source.playOnAwake = false;

                source.volume = 0.5f;
                source.spatialBlend = 1;

                return source;
            }

            public void SetVolume(int value) => Decibels = (value == 0) ? -200 : 20 * Mathf.Log10(Mathf.InverseLerp(0, 100, value));
        }
        #endregion Group


        public static AudioMixer Mixer { get; } = Monolith.Refs.audioMixer;

        public static Group Master { get; } = new Group("Master");
        public static Group UI { get; } = new Group("UI");
        public static Group SFX { get; } = new Group("SFX");
        public static Group Voice { get; } = new Group("Voice");
        public static Group Ambiance { get; } = new Group("Ambiance");
        public static Group Music { get; } = new Group("Music");


        public static AudioSource Volume(this AudioSource source, float volume) { source.volume = volume; return source; }
        public static AudioSource Pitch(this AudioSource source, float pitch) { source.pitch = pitch; return source; }
        public static AudioSource Pan(this AudioSource source, float pan) { source.panStereo = pan; return source; }
        public static AudioSource Blend(this AudioSource source, float blend) { source.spatialBlend = blend; return source; }
        public static AudioSource Reverb(this AudioSource source, float reverb) { source.reverbZoneMix = reverb; return source; }
    }
}