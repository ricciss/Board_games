using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    [Header("Dziesmas (var ielikt cik grib)")]
    public AudioClip[] songs;

    [Header("Iestatījumi")]
    [Range(0f, 1f)] public float volume = 0.5f;
    public bool playOnStart = true;
    public bool keepBetweenScenes = true; // mūzika neapstājas, mainot ainu

    private AudioSource audioSource;
    private int currentIndex = 0;
    private bool isPaused = false;
    private bool started = false;

    private static MusicPlayer instance;
    public static MusicPlayer Instance => instance; // lai Settings aina var piekļūt mūzikai

    const string VolumeKey = "MusicVolume";

    // Maina skaļumu un saglabā to (paliek arī pēc spēles aizvēršanas)
    public void SetVolume(float value)
    {
        volume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(VolumeKey, volume);
    }

    void Awake()
    {
        // Ielādē saglabāto skaļumu (ja tāda nav, paliek Inspector vērtība)
        volume = PlayerPrefs.GetFloat(VolumeKey, volume);

        if (keepBetweenScenes)
        {
            // Lai, atgriežoties ainā, neveidojas otrs mūzikas atskaņotājs
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;      // svarīgi: viena dziesma neatkārtojas
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        if (playOnStart) PlaySong(0);
    }

    void Update()
    {
        audioSource.volume = volume;

        // Kamēr spēle nav fokusā vai tikko atgriezās, neko nemainām
        if (!hasFocus || Time.unscaledTime < ignoreUntil) return;

        // Kad dziesma TIEŠĀM beidzas -> nākamā
        if (started && !isPaused && !audioSource.isPlaying && SongFinished())
        {
            NextSong();
        }
    }

    private bool hasFocus = true;
    private float ignoreUntil = 0f;

    // Pārbauda, vai dziesma nospēlēta līdz galam (nevis tikai apturēta)
    bool SongFinished()
    {
        if (audioSource.clip == null) return true;
        float t = audioSource.time;
        return t <= 0.01f || t >= audioSource.clip.length - 0.1f;
    }

    void OnApplicationFocus(bool focus)
    {
        hasFocus = focus;
        if (focus) ignoreUntil = Time.unscaledTime + 0.5f; // īsa pauze pēc atgriešanās
    }

    void OnApplicationPause(bool pause)
    {
        if (!pause) ignoreUntil = Time.unscaledTime + 0.5f;
    }

    public void PlaySong(int index)
    {
        if (songs == null || songs.Length == 0)
        {
            Debug.LogWarning("MusicPlayer: nav pievienota neviena dziesma!");
            return;
        }

        // Ja index ir aiz saraksta beigām, sāk no sākuma
        currentIndex = index % songs.Length;
        if (currentIndex < 0) currentIndex += songs.Length;

        audioSource.clip = songs[currentIndex];
        audioSource.Play();
        started = true;
        isPaused = false;

        if (OnSongChanged != null) OnSongChanged();
    }

    // Paziņo citiem skriptiem (piem. Settings), ka dziesma nomainījās
    public event System.Action OnSongChanged;

    public string CurrentSongName
    {
        get
        {
            if (audioSource == null || audioSource.clip == null) return "-";
            return audioSource.clip.name;
        }
    }

    public int CurrentIndex => currentIndex;
    public int SongCount => songs == null ? 0 : songs.Length;

    public void NextSong()
    {
        PlaySong(currentIndex + 1);
    }

    public void PreviousSong()
    {
        PlaySong(currentIndex - 1);
    }

    public void Pause()
    {
        isPaused = true;
        audioSource.Pause();
    }

    public void Resume()
    {
        isPaused = false;
        audioSource.UnPause();
    }

    public void Stop()
    {
        started = false;
        audioSource.Stop();
    }
}