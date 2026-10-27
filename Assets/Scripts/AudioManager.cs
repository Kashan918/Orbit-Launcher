using System.Collections;
using UnityEngine;

namespace OrbitLauncher
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Music Clips (Auto-Loaded from Resources/Audio)")]
        [SerializeField] private AudioClip _menuMusicClip;
        [SerializeField] private AudioClip _gameplayMusicClip;
        [Range(0f, 1f)]
        [SerializeField] private float _musicVolume = 0.52f;

        private AudioSource _sfxSource;
        private AudioSource _chimeSource;
        private AudioSource _menuMusicSource;
        private AudioSource _gameplayMusicSource;

        private AudioClip[] _pentatonicChimes;
        private AudioClip _releaseClip;
        private AudioClip _shardClip;
        private AudioClip _warpClip;
        private AudioClip _deathClip;
        private AudioClip _clickClip;

        private bool _playingGameplayTrack;
        private Coroutine _crossfadeRoutine;

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

            _menuMusicSource = gameObject.AddComponent<AudioSource>();
            _menuMusicSource.loop = true;
            _menuMusicSource.volume = _musicVolume;
            _menuMusicSource.playOnAwake = false;

            _gameplayMusicSource = gameObject.AddComponent<AudioSource>();
            _gameplayMusicSource.loop = true;
            _gameplayMusicSource.volume = 0f;
            _gameplayMusicSource.playOnAwake = false;

            IsMuted = PlayerPrefs.GetInt("OL_Muted", 0) == 1;
            LoadOrGenerateClips();
            ApplyMuteState();
        }

        private void Start()
        {
            PlayMenuMusic(true);
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

            if (_menuMusicClip == null)
                _menuMusicClip = Resources.Load<AudioClip>("Audio/bgm_main_menu") ?? CreateProceduralMenuMusic();

            if (_gameplayMusicClip == null)
                _gameplayMusicClip = Resources.Load<AudioClip>("Audio/bgm_gameplay") ?? CreateProceduralGameplayMusic();

            _menuMusicSource.clip = _menuMusicClip;
            _gameplayMusicSource.clip = _gameplayMusicClip;
        }

        public void PlayMenuMusic(bool immediate = false)
        {
            _playingGameplayTrack = false;
            if (_menuMusicSource != null && _menuMusicClip != null && !_menuMusicSource.isPlaying)
            {
                _menuMusicSource.clip = _menuMusicClip;
                _menuMusicSource.Play();
            }

            if (immediate)
            {
                if (_menuMusicSource != null) _menuMusicSource.volume = _musicVolume;
                if (_gameplayMusicSource != null)
                {
                    _gameplayMusicSource.volume = 0f;
                    _gameplayMusicSource.Stop();
                }
                return;
            }

            if (_crossfadeRoutine != null) StopCoroutine(_crossfadeRoutine);
            _crossfadeRoutine = StartCoroutine(CrossfadeMusic(_menuMusicSource, _gameplayMusicSource, 0.65f));
        }

        public void PlayGameplayMusic()
        {
            if (_playingGameplayTrack && _gameplayMusicSource != null && _gameplayMusicSource.isPlaying)
                return;

            _playingGameplayTrack = true;
            if (_gameplayMusicSource != null && _gameplayMusicClip != null && !_gameplayMusicSource.isPlaying)
            {
                _gameplayMusicSource.clip = _gameplayMusicClip;
                _gameplayMusicSource.Play();
            }

            if (_crossfadeRoutine != null) StopCoroutine(_crossfadeRoutine);
            _crossfadeRoutine = StartCoroutine(CrossfadeMusic(_gameplayMusicSource, _menuMusicSource, 0.65f));
        }

        private IEnumerator CrossfadeMusic(AudioSource fadeIn, AudioSource fadeOut, float duration)
        {
            float startIn = fadeIn != null ? fadeIn.volume : 0f;
            float startOut = fadeOut != null ? fadeOut.volume : 0f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (fadeIn != null) fadeIn.volume = Mathf.Lerp(startIn, _musicVolume, t);
                if (fadeOut != null) fadeOut.volume = Mathf.Lerp(startOut, 0f, t);
                yield return null;
            }

            if (fadeIn != null) fadeIn.volume = _musicVolume;
            if (fadeOut != null)
            {
                fadeOut.volume = 0f;
                fadeOut.Stop();
            }
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
            if (_menuMusicSource != null) _menuMusicSource.mute = IsMuted;
            if (_gameplayMusicSource != null) _gameplayMusicSource.mute = IsMuted;
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

        private static float MidiFreq(int midi) => 440f * Mathf.Pow(2f, (midi - 69) / 12f);

        private AudioClip CreateProceduralMenuMusic()
        {
            int sr = 22050;
            float beatSec = 0.6f;
            float stepSec = beatSec * 0.25f;
            float duration = 16f * beatSec;
            int count = Mathf.CeilToInt(sr * duration);
            float[] data = new float[count];
            int[][] chords =
            {
                new[] { 48, 55, 58, 63, 67 },
                new[] { 44, 51, 56, 60, 63 },
                new[] { 51, 58, 62, 63, 67 },
                new[] { 46, 53, 58, 60, 65 }
            };
            int[] arp = { 0, 2, 3, 4, 3, 2, 4, 3 };

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)sr;
                float beatPos = t / beatSec;
                int bar = ((int)(beatPos / 4f)) % 4;
                int[] ch = chords[bar];

                float pad = (Mathf.Sin(2f * Mathf.PI * MidiFreq(ch[1]) * t) +
                             Mathf.Sin(2f * Mathf.PI * MidiFreq(ch[2]) * t) +
                             Mathf.Sin(2f * Mathf.PI * MidiFreq(ch[3]) * t)) * 0.12f;

                int step = (int)(t / stepSec);
                float st = t - step * stepSec;
                float af = MidiFreq(ch[arp[step % arp.Length]] + 12);
                float arpSig = Mathf.Sin(2f * Mathf.PI * af * t) * Mathf.Exp(-st * 16f) * 0.24f;

                float bf = MidiFreq(ch[0] - 12);
                float bass = Mathf.Sin(2f * Mathf.PI * bf * t) * 0.26f;

                data[i] = Mathf.Clamp(pad + arpSig + bass, -0.9f, 0.9f);
            }
            AudioClip clip = AudioClip.Create("bgm_main_menu_proc", count, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateProceduralGameplayMusic()
        {
            int sr = 22050;
            float beatSec = 60f / 128f;
            float stepSec = beatSec * 0.25f;
            float duration = 16f * beatSec;
            int count = Mathf.CeilToInt(sr * duration);
            float[] data = new float[count];
            int[][] chords =
            {
                new[] { 45, 52, 57, 60, 64 },
                new[] { 41, 48, 53, 57, 60 },
                new[] { 48, 55, 60, 64, 67 },
                new[] { 43, 50, 55, 59, 62 }
            };
            int[] arp = { 0, 2, 3, 4, 2, 3, 4, 3 };

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)sr;
                float beatPos = t / beatSec;
                int bar = ((int)(beatPos / 4f)) % 4;
                int[] ch = chords[bar];

                int step16 = (int)(t / stepSec);
                float st = t - step16 * stepSec;
                float af = MidiFreq(ch[arp[step16 % arp.Length]] + 12);
                float arpSig = Mathf.Sin(2f * Mathf.PI * af * t) * Mathf.Exp(-st * 20f) * 0.24f;

                int step8 = (int)(t / (stepSec * 2f));
                float bt = t - step8 * (stepSec * 2f);
                float bf = MidiFreq(step8 % 2 == 0 ? ch[0] - 12 : ch[0]);
                float bass = Mathf.Sin(2f * Mathf.PI * bf * t) * Mathf.Exp(-bt * 10f) * 0.32f;

                float bFrac = (beatPos - Mathf.Floor(beatPos)) * beatSec;
                float kick = bFrac < 0.14f
                    ? Mathf.Sin(2f * Mathf.PI * (46f + 110f * Mathf.Exp(-bFrac * 40f)) * bFrac) * Mathf.Exp(-bFrac * 18f) * 0.35f
                    : 0f;

                data[i] = Mathf.Clamp(arpSig + bass + kick, -0.9f, 0.9f);
            }
            AudioClip clip = AudioClip.Create("bgm_gameplay_proc", count, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
