using System.Runtime.CompilerServices;
using UnityEngine;
using System.Collections;

public class SetActiveScript : MonoBehaviour
{
    public GameObject window2;

    public void ToggleActiveAdterDelay(float delay)
    {
        StartCoroutine(ToggleActiveCoroutine(delay));
    }
    private IEnumerator ToggleActiveCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        window2.SetActive(!window2.activeSelf);
        gameObject.SetActive(!gameObject.activeSelf);
    }
    
}
