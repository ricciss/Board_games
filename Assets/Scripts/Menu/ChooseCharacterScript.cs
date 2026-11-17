using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChooseCharacterScript : MonoBehaviour
{
    public GameObject[] characters;
    int characterIndex;

    public GameObject inputField;
    string characterName;

    // Spēlētāju skaita dropdown (TMP_Dropdown)
    public TMP_Dropdown playerCountDropdown;
    const int MinPlayers = 1;
    const int MaxPlayers = 7;
    public int playerCount = 1;

    public SceneChangerScript sceneChangerScript;

    void Awake()
    {
        characterIndex = 0;
        foreach (GameObject character in characters)
        {
            character.SetActive(false);
        }
        characters[characterIndex].SetActive(true);

        SetupPlayerCountDropdown();
    }

    void SetupPlayerCountDropdown()
    {
        if (playerCountDropdown == null) return;

        // Aizpilda dropdown ar opcijām 1..7
        playerCountDropdown.ClearOptions();
        List<string> options = new List<string>();
        for (int i = MinPlayers; i <= MaxPlayers; i++)
        {
            options.Add(i == 1 ? "1 player" : i + " players");
        }
        playerCountDropdown.AddOptions(options);

        // Ielādē iepriekš saglabāto vērtību (ja ir)
        playerCount = Mathf.Clamp(PlayerPrefs.GetInt("PlayerCount", MinPlayers), MinPlayers, MaxPlayers);
        playerCountDropdown.SetValueWithoutNotify(playerCount - MinPlayers);
        playerCountDropdown.RefreshShownValue();

        playerCountDropdown.onValueChanged.AddListener(OnPlayerCountChanged);
    }

    public void OnPlayerCountChanged(int index)
    {
        // Dropdown indekss 0 = 1 spēlētājs, 6 = 7 spēlētāji
        playerCount = Mathf.Clamp(index + MinPlayers, MinPlayers, MaxPlayers);
    }

    public void NextCharacter()
    {
        characters[characterIndex].SetActive(false);
        characterIndex++;
        if (characterIndex == characters.Length)
            characterIndex = 0;

        characters[characterIndex].SetActive(true);
    }

    public void previousCharacter()
    {
        characters[characterIndex].SetActive(false);
        characterIndex--;
        if (characterIndex == -1)
            characterIndex = characters.Length - 1;

        characters[characterIndex].SetActive(true);
    }

    public void Play()
    {
        characterName = inputField.GetComponent<TMP_InputField>().text;

        // Drošībai vēlreiz nolasa dropdown vērtību
        if (playerCountDropdown != null)
            OnPlayerCountChanged(playerCountDropdown.value);

        if (characterName.Length > 2)
        {
            PlayerPrefs.SetInt("SelectedCharacter", characterIndex);
            PlayerPrefs.SetString("PlayerName", characterName);
            PlayerPrefs.SetInt("PlayerCount", playerCount);
            PlayerPrefs.Save();
            StartCoroutine(sceneChangerScript.Delay("play", characterIndex, characterName, playerCount));
        }
        else
            inputField.GetComponent<TMP_InputField>().Select();
    }
}