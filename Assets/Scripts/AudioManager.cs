using UnityEngine;

namespace OrbitLauncher
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _sfxSource;
        private AudioSource _chimeSource;
        private AudioSource _ambientSource;

        private AudioClip[] _pentatonicChimes;
        private AudioClip _releaseClip;
        private AudioClip _shardClip;
        private AudioClip _warpClip;
        private AudioClip _deathClip;
        private AudioClip _clickClip;
        private AudioClip _ambientClip;

        public bool IsMuted { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;

            _chimeSource = gameObject.AddComponent<AudioSource>();
            _chimeSource.playOnAwake = false;

            _ambientSource = gameObject.AddComponent<AudioSource>();
            _ambientSource.loop = true;
            _ambientSource.volume = 0.28f;
            _ambientSource.playOnAwake = false;

            IsMuted = PlayerPrefs.GetInt("OL_Muted", 0) == 1;
            LoadOrGenerateClips();
            ApplyMuteState();
        }

        private void Start()
        {
            if (_ambientClip != null && !_ambientSource.isPlaying)
            {
                _ambientSource.clip = _ambientClip;
                _ambientSource.Play();
            }
        }

        private void LoadOrGenerateClips()
        {
            float[] pentatonicFreqs = { 523.25f, 587.33f, 659.25f, 783.99f, 880.00f, 1046.50f };
            _pentatonicChimes = new AudioClip[pentatonicFreqs.Length];
            for (int i = 0; i < pentatonicFreqs.Length; i++)
            {
                _pentatonicChimes[i] = Resources.Load<AudioClip>("Audio/chime_" + i);
                if (_pentatonicChimes[i] == null)
                {
                    _pentatonicChimes[i] = CreateChimeClip("chime_" + i, pentatonicFreqs[i], 0.42f);
                }
            }

            _releaseClip = Resources.Load<AudioClip>("Audio/release") ?? CreateSweepClip("release", 240f, 1280f, 0.24f);
            _shardClip = Resources.Load<AudioClip>("Audio/shard_pickup") ?? CreateChimeClip("shard", 1318.5f, 0.30f);
            _warpClip = Resources.Load<AudioClip>("Audio/warp_shield") ?? CreateSweepClip("warp", 330f, 990f, 0.55f);
            _deathClip = Resources.Load<AudioClip>("Audio/death_shatter") ?? CreateNoiseBurstClip("death", 0.50f);
            _clickClip = Resources.Load<AudioClip>("Audio/ui_click") ?? CreateChimeClip("click", 880f, 0.07f);
            _ambientClip = Resources.Load<AudioClip>("Audio/ambient_pad") ?? CreateAmbientClip();
        }

        public void ToggleMute()
        {
            IsMuted = !IsMuted;
            PlayerPrefs.SetInt("OL_Muted", IsMuted ? 1 : 0);
            PlayerPrefs.Save();
            ApplyMuteState();
        }

        private void ApplyMuteState()
        {
            if (_sfxSource != null) _sfxSource.mute = IsMuted;
            if (_chimeSource != null) _chimeSource.mute = IsMuted;
            if (_ambientSource != null) _ambientSource.mute = IsMuted;
        }

        public void PlayCaptureChime(int consecutivePerfects)
        {
            if (_pentatonicChimes == null || _pentatonicChimes.Length == 0) return;
            int idx = Mathf.Clamp(consecutivePerfects, 0, _pentatonicChimes.Length - 1);
            _chimeSource.pitch = 1f + Mathf.Max(0, consecutivePerfects - 5) * 0.03f;
            _chimeSource.PlayOneShot(_pentatonicChimes[idx], 0.85f);
        }

        public void PlayRelease(int comboMultiplier)
        {
            if (_releaseClip == null) return;
            _sfxSource.pitch = 0.96f + Mathf.Clamp(comboMultiplier, 1, 4) * 0.05f;
            _sfxSource.PlayOneShot(_releaseClip, 0.7f);
        }

        public void PlayShardPickup()
        {
            if (_shardClip == null) return;
            _sfxSource.pitch = Random.Range(0.98f, 1.06f);
            _sfxSource.PlayOneShot(_shardClip, 0.8f);
        }

        public void PlayWarpShield()
        {
            if (_warpClip == null) return;
            _sfxSource.pitch = 1.0f;
            _sfxSource.PlayOneShot(_warpClip, 0.9f);
        }

        public void PlayDeath()
        {
            if (_deathClip == null) return;
            _sfxSource.pitch = 1.0f;
            _sfxSource.PlayOneShot(_deathClip, 0.95f);
        }

        public void PlayClick()
        {
            if (_clickClip == null) return;
            _sfxSource.pitch = 1.0f;
            _sfxSource.PlayOneShot(_clickClip, 0.6f);
        }

        private AudioClip CreateChimeClip(string name, float freq, float duration)
        {
            int sr = 44100;
            int count = Mathf.CeilToInt(sr * duration);
            float[] data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)sr;
                float env = Mathf.Exp(-t * 8f) * Mathf.Clamp01(t * 100f);
                data[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f +
                           Mathf.Sin(4f * Mathf.PI * freq * t) * 0.25f) * env;
            }
            AudioClip clip = AudioClip.Create(name, count, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateSweepClip(string name, float fStart, float fEnd, float duration)
        {
            int sr = 44100;
            int count = Mathf.CeilToInt(sr * duration);
            float[] data = new float[count];
            float phase = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)sr;
                float f = Mathf.Lerp(fStart, fEnd, t / duration);
                phase += 2f * Mathf.PI * f / sr;
                float env = Mathf.Exp(-t * 9f) * Mathf.Clamp01(t * 80f);
                data[i] = Mathf.Sin(phase) * env * 0.6f;
            }
            AudioClip clip = AudioClip.Create(name, count, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateNoiseBurstClip(string name, float duration)
        {
            int sr = 44100;
            int count = Mathf.CeilToInt(sr * duration);
            float[] data = new float[count];
            float low = 0f;
            System.Random rng = new System.Random(101);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)sr;
                float env = Mathf.Exp(-t * 6.5f);
                float n = (float)(rng.NextDouble() * 2.0 - 1.0);
                low = low * 0.88f + n * 0.12f;
                float sub = Mathf.Sin(2f * Mathf.PI * (130f * Mathf.Exp(-t * 4f)) * t);
                data[i] = (low * 0.6f + sub * 0.45f) * env;
            }
            AudioClip clip = AudioClip.Create(name, count, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateAmbientClip()
        {
            int sr = 22050;
            float duration = 6.0f;
            int count = Mathf.CeilToInt(sr * duration);
            float[] data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)sr;
                float lfo = 0.85f + 0.15f * Mathf.Sin(2f * Mathf.PI * (t / duration) * 2f);
                float sig = Mathf.Sin(2f * Mathf.PI * 65.41f * t) * 0.30f +
                            Mathf.Sin(2f * Mathf.PI * 98.00f * t) * 0.22f +
                            Mathf.Sin(2f * Mathf.PI * 146.83f * t) * 0.16f;
                data[i] = sig * lfo * 0.45f;
            }
            AudioClip clip = AudioClip.Create("ambient_pad", count, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}

