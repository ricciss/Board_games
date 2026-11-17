using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class FadeScript : MonoBehaviour
{
    Image img;
    Color tempColor;

    void Start()
    {
        img = GetComponent<Image>();
        tempColor = img.color;
        tempColor.a = 1f;
        img.color = tempColor;
        StartCoroutine(FadeIn(0.15f));
    }

    public IEnumerator FadeIn(float fadeTime)
    {
        for(float a=1f; a>= -0.05f; a -= 0.05f)
        {
            tempColor = img.color;
            tempColor.a = a;
            img.color = tempColor;
            yield return new WaitForSeconds(fadeTime);
        }
        img.raycastTarget = false;
    }

    public IEnumerator FadeOut(float fadeTime)
    {
        img.raycastTarget = true;
        for (float a = 0f; a <= 1.05f; a += 0.3f)
        {
            tempColor = img.color;
            tempColor.a = a;
            img.color = tempColor;
            yield return new WaitForSeconds(fadeTime);
        }
        img.raycastTarget = false;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
