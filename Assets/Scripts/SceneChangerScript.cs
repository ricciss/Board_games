using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneChangerScript : MonoBehaviour
{
    [Tooltip("Cik ilgi gaidīt pirms ainas maiņas (lai paspēj nospēlēt klikšķa skaņa)")]
    public float delay = 0.3f;

    // Atver jebkuru ainu pēc nosaukuma (pogas OnClick laukā ieraksti ainas nosaukumu)
    public void LoadScene(string sceneName)
    {
        StartCoroutine(Load(sceneName));
    }

    public void OpenSettings()
    {
        LoadScene("Settings");
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

    public void ExitGame()
    {
        StartCoroutine(Exit());
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