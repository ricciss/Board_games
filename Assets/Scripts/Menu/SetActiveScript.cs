using UnityEngine;
using System.Collections;

public class SetActiveScript : MonoBehaviour
{
    public GameObject window2;

    [Tooltip("Objekti, kas jāpārslēdz kopā ar logiem (piem. Images)")]
    public GameObject[] alsoToggle;

    public void ToggleActiveAdterDelay(float delay)
    {
        StartCoroutine(ToggleActiveCoroutine(delay));
    }

    private IEnumerator ToggleActiveCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        window2.SetActive(!window2.activeSelf);

        foreach (GameObject obj in alsoToggle)
            if (obj != null) obj.SetActive(!obj.activeSelf);

        gameObject.SetActive(!gameObject.activeSelf);
    }
}