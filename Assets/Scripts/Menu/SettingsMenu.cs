using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// Settings ainas galvenais skripts. Uzliec uz ScriptHolder Settings ainā.
public class SettingsMenu : MonoBehaviour
{
    [Header("1) Mūzikas skaļums")]
    public Slider musicSlider;
    public TMP_Text musicValueText; // nav obligāts – parāda, piem., "50%"

    [Header("2) Skaņas efektu skaļums")]
    public Slider sfxSlider;
    public TMP_Text sfxValueText;      // nav obligāts
    public SoundEffectScript soundEffects; // lai, velkot slīdni, atskan paraugskaņa

    [Header("3) Dziesmas pārslēgšana")]
    public Button previousSongButton;
    public Button nextSongButton;
    public TMP_Text songNameText;      // rāda, piem., "2/5  Forest"

    [Header("4) Izšķirtspēja")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;    // nav obligāts

    [Header("5) Kvalitāte")]
    public TMP_Dropdown qualityDropdown;

    [Header("6) Atjaunot noklusējuma iestatījumus")]
    public Button resetButton;
    [Range(0f, 1f)] public float defaultMusicVolume = 0.5f;
    [Range(0f, 1f)] public float defaultSfxVolume = 1f;
    [Tooltip("Kvalitātes līmeņa numurs sarakstā: 0 = Low, 1 = Medium, 2 = High")]
    public int defaultQualityLevel = 1;
    public bool defaultFullscreen = true;

    private float lastPreviewTime;
    private readonly List<Vector2Int> resolutions = new List<Vector2Int>();

    const string QualityKey = "QualityLevel";

    void Start()
    {
        SetupMusicSlider();
        SetupSfxSlider();
        SetupSongButtons();
        SetupResolution();
        SetupQuality();
        if (resetButton != null) resetButton.onClick.AddListener(ResetToDefaults);
    }

    // ---------- 6) ATJAUNOT NOKLUSĒJUMU ----------
    // Mainot slīdņu/sarakstu vērtības, automātiski izsaucas tās pašas funkcijas,
    // kas tos iestatījumus pielieto un saglabā.
    public void ResetToDefaults()
    {
        if (musicSlider != null) musicSlider.value = defaultMusicVolume;
        else OnMusicVolumeChanged(defaultMusicVolume);

        if (sfxSlider != null) sfxSlider.value = defaultSfxVolume;
        else SoundEffectScript.Volume = defaultSfxVolume;

        // Fullscreen jāieliek pirms izšķirtspējas
        if (fullscreenToggle != null) fullscreenToggle.isOn = defaultFullscreen;
        else OnFullscreenChanged(defaultFullscreen);

        // Izšķirtspēja = monitora īstā izšķirtspēja
        if (resolutionDropdown != null && resolutions.Count > 0)
        {
            Resolution native = Screen.currentResolution;
            int i = resolutions.IndexOf(new Vector2Int(native.width, native.height));
            if (i < 0) i = resolutions.Count - 1; // lielākā pieejamā
            resolutionDropdown.value = i;
        }

        int q = Mathf.Clamp(defaultQualityLevel, 0, QualitySettings.names.Length - 1);
        if (qualityDropdown != null) qualityDropdown.value = q;
        else OnQualityChanged(q);

        PlayerPrefs.Save();
    }

    // ---------- 5) KVALITĀTE ----------

    // Palaižas automātiski, kad spēle startē (jebkurā ainā),
    // lai saglabātā kvalitāte būtu ieslēgta jau no sākuma
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplySavedQuality()
    {
        if (PlayerPrefs.HasKey(QualityKey))
        {
            int level = PlayerPrefs.GetInt(QualityKey);
            if (level >= 0 && level < QualitySettings.names.Length)
                QualitySettings.SetQualityLevel(level, true);
        }
    }

    void SetupQuality()
    {
        if (qualityDropdown == null) return;

        // Nosaukumi ņemti no Edit -> Project Settings -> Quality
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
        qualityDropdown.SetValueWithoutNotify(QualitySettings.GetQualityLevel());
        qualityDropdown.RefreshShownValue();
        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
    }

    public void OnQualityChanged(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt(QualityKey, index);
    }

    // ---------- 4) IZŠĶIRTSPĒJA ----------
    void SetupResolution()
    {
        if (resolutionDropdown != null)
        {
            // Savāc izšķirtspējas bez dublikātiem (viena izšķirtspēja var būt ar vairākiem Hz)
            resolutions.Clear();
            foreach (Resolution r in Screen.resolutions)
            {
                Vector2Int size = new Vector2Int(r.width, r.height);
                if (!resolutions.Contains(size)) resolutions.Add(size);
            }

            // Ja pašreizējā izšķirtspēja nav sarakstā (piem. logā), pievieno to
            Vector2Int current = new Vector2Int(Screen.width, Screen.height);
            if (!resolutions.Contains(current)) resolutions.Add(current);

            resolutions.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));

            List<string> options = new List<string>();
            foreach (Vector2Int s in resolutions) options.Add(s.x + " x " + s.y);

            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.SetValueWithoutNotify(resolutions.IndexOf(current));
            resolutionDropdown.RefreshShownValue();
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }
    }

    public void OnResolutionChanged(int index)
    {
        if (index < 0 || index >= resolutions.Count) return;
        Vector2Int s = resolutions[index];
        Screen.SetResolution(s.x, s.y, Screen.fullScreenMode);
    }

    public void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    }

    // ---------- 3) DZIESMAS PĀRSLĒGŠANA ----------
    void SetupSongButtons()
    {
        if (previousSongButton != null) previousSongButton.onClick.AddListener(PreviousSong);
        if (nextSongButton != null) nextSongButton.onClick.AddListener(NextSong);

        // Atjauno nosaukumu arī tad, kad dziesma nomainās pati
        if (MusicPlayer.Instance != null) MusicPlayer.Instance.OnSongChanged += UpdateSongName;
        UpdateSongName();
    }

    public void PreviousSong()
    {
        if (MusicPlayer.Instance != null) MusicPlayer.Instance.PreviousSong();
    }

    public void NextSong()
    {
        if (MusicPlayer.Instance != null) MusicPlayer.Instance.NextSong();
    }

    void UpdateSongName()
    {
        if (songNameText == null) return;

        MusicPlayer mp = MusicPlayer.Instance;
        if (mp == null || mp.SongCount == 0)
        {
            songNameText.text = "No music";
            return;
        }
        songNameText.text = (mp.CurrentIndex + 1) + "/" + mp.SongCount + "  " + mp.CurrentSongName;
    }

    void OnDestroy()
    {
        // Atsakāmies no paziņojumiem, kad Settings aina tiek aizvērta
        if (MusicPlayer.Instance != null) MusicPlayer.Instance.OnSongChanged -= UpdateSongName;
    }

    // ---------- 2) SKAŅAS EFEKTU SKAĻUMS ----------
    void SetupSfxSlider()
    {
        if (sfxSlider == null) return;

        sfxSlider.minValue = 0f;
        sfxSlider.maxValue = 1f;
        sfxSlider.value = SoundEffectScript.Volume;

        UpdateText(sfxValueText, sfxSlider.value);
        sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
    }

    public void OnSfxVolumeChanged(float value)
    {
        SoundEffectScript.Volume = value;
        UpdateText(sfxValueText, value);

        // Paraugskaņa, bet ne biežāk kā reizi 0.15 sekundēs
        if (soundEffects != null && Time.unscaledTime - lastPreviewTime > 0.15f)
        {
            soundEffects.Click();
            lastPreviewTime = Time.unscaledTime;
        }
    }

    // ---------- 1) MŪZIKAS SKAĻUMS ----------
    void SetupMusicSlider()
    {
        if (musicSlider == null) return;

        musicSlider.minValue = 0f;
        musicSlider.maxValue = 1f;

        // Slīdnis sākumā rāda pašreizējo skaļumu
        if (MusicPlayer.Instance != null)
            musicSlider.value = MusicPlayer.Instance.volume;
        else
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);

        UpdateText(musicValueText, musicSlider.value);
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (MusicPlayer.Instance != null)
            MusicPlayer.Instance.SetVolume(value);
        else
            PlayerPrefs.SetFloat("MusicVolume", value); // ja Settings palaists bez izvēlnes

        UpdateText(musicValueText, value);
    }

    // ---------- PALĪGFUNKCIJAS ----------
    void UpdateText(TMP_Text label, float value)
    {
        if (label != null) label.text = Mathf.RoundToInt(value * 100f) + "%";
    }

    void OnDisable()
    {
        PlayerPrefs.Save();
    }
}