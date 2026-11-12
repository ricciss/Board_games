using UnityEngine;

public class SoundEffectScript : MonoBehaviour
{
    public AudioClip[] audioClips;
    public AudioSource effectsSource;

    const string VolumeKey = "SfxVolume";

    // Kopīgs efektu skaļums visām ainām (saglabājas arī pēc spēles aizvēršanas)
    public static float Volume
    {
        get { return PlayerPrefs.GetFloat(VolumeKey, 1f); }
        set { PlayerPrefs.SetFloat(VolumeKey, Mathf.Clamp01(value)); }
    }

    void Play(int index)
    {
        if (effectsSource == null || audioClips == null || index >= audioClips.Length) return;
        if (audioClips[index] == null) return;

        effectsSource.PlayOneShot(audioClips[index], Volume);
    }

    public void Hover() { Play(0); }
    public void Click() { Play(1); }
    public void OnDice() { Play(2); }
}