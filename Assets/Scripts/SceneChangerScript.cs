using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

public class SceneChangerScript : MonoBehaviour
{
    public SaveLoadScript saveLoadScript;
    public FadeScript fadeScript;

    public void CloseGame()
    {
        StartCoroutine(Delay("quit", -1, "", -1));
    }

    public IEnumerator Delay(string action, int charIndex, string playerName, int playerCount)
    {
        if (string.Equals(action, "quit", StringComparison.OrdinalIgnoreCase))
        {
            yield return new WaitForSeconds(0.1f);
            PlayerPrefs.DeleteAll();

            if (UnityEditor.EditorApplication.isPlaying)
                UnityEditor.EditorApplication.isPlaying = false;
            else
            {
                yield return fadeScript.FadeOut(0.1f);
                Application.Quit();
            }
        
    }   else if (string.Equals(action, "play", StringComparison.OrdinalIgnoreCase))
        {
            yield return fadeScript.FadeOut(0.1f);
            saveLoadScript.SavePlayerData(charIndex, playerName, playerCount);
            SceneManager.LoadScene(2, LoadSceneMode.Single);
        }
        else if (string.Equals(action, "settings", StringComparison.OrdinalIgnoreCase))
        {
            yield return fadeScript.FadeOut(0.1f);
            //saveLoadScript.SavePlayerData(charIndex, playerName, playerCount);
            SceneManager.LoadScene(1, LoadSceneMode.Single);

        }   else if (string.Equals(action, "menu", StringComparison.OrdinalIgnoreCase))
        {
            yield return fadeScript.FadeOut(0.1f);
            SceneManager.LoadScene(0, LoadSceneMode.Single);
        }

    }

    public void ToMainMenu()
    {
        StartCoroutine(Delay("menu", -1, "", -1));
    }

    public void ExitGame()
    {
        StartCoroutine(Exit());
    }
    [Tooltip("Cik ilgi gaidīt pirms ainas maiņas (lai paspēj nospēlēt klikšķa skaņa)")]
    public float delay = 0.3f;

    // Atver jebkuru ainu pēc nosaukuma (pogas OnClick laukā ieraksti ainas nosaukumu)
    public void LoadScene(string sceneName)
    {
        StartCoroutine(Load(sceneName));
    }

    public void OpenSettings()
    {
        StartCoroutine(Delay("settings", -1, "", -1));
       
    }

    public void BackToMenu()
    {
        LoadScene("MainMenu");
    }

    IEnumerator Load(string sceneName)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    

    IEnumerator Exit()
    {
        yield return new WaitForSeconds(0.8f);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}


//Fona japievieno animēti mākoņi, kas lidinās apkārt
/*Jāizstrādā jauna aina priekš Settings ar:
1) Iespēju mainīt mūzikas skaļumu
2) Iespēju mainīt skaņas efektu skaļumu
3) Pārslēgt manuāli dziesmu
4) Mainīt spēles rezulūciju
5) Mainīt spēles kvalitāti
6) Viena paša izdomāta funkcionalitāte
7) Poga, kas atgriež uz galveno izvēlni*/