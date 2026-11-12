using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// Uzliec šo uz tukša UI objekta Canvas iekšā (piem. "Clouds"),
// kas izstiepts pa visu ekrānu un atrodas Hierarchy AUGŠĀ (virs Panel),
// lai mākoņi būtu fonā aiz izvēlnes.
[RequireComponent(typeof(RectTransform))]
public class CloudSpawner : MonoBehaviour
{
    [Header("Mākoņu bildes (var ielikt cik grib)")]
    public Sprite[] cloudSprites;

    [Header("Daudzums")]
    [Range(1, 50)] public int cloudCount = 8;

    [Header("Kustība")]
    public float minSpeed = 15f;
    public float maxSpeed = 45f;
    public bool moveRight = true;          // virziens
    public bool randomDirection = false;   // ja ieslēgts, katrs mākonis lido uz savu pusi

    [Header("Šūpošanās augšā/lejā")]
    public float bobAmount = 8f;           // cik pikseļus kustas augšā/lejā
    public float bobSpeed = 0.6f;

    [Header("Izmērs un caurspīdīgums")]
    [Tooltip("Mākoņa augstums kā daļa no ekrāna augstuma (0.15 = 15%)")]
    [Range(0.02f, 1f)] public float cloudSize = 0.15f;
    public float minScale = 0.6f;
    public float maxScale = 1.3f;
    [Range(0f, 1f)] public float minAlpha = 0.7f;
    [Range(0f, 1f)] public float maxAlpha = 1f;

    [Header("Kur var parādīties (0 = apakša, 1 = augša)")]
    [Range(0f, 1f)] public float minHeight = 0.1f;
    [Range(0f, 1f)] public float maxHeight = 0.95f;

    private class Cloud
    {
        public RectTransform rect;
        public float speed;
        public float baseY;
        public float bobOffset;
        public int direction;
    }

    private readonly List<Cloud> clouds = new List<Cloud>();
    private RectTransform area;

    void Start()
    {
        area = GetComponent<RectTransform>();

        if (cloudSprites == null || cloudSprites.Length == 0)
        {
            Debug.LogWarning("CloudSpawner: nav pievienota neviena mākoņa bilde!");
            return;
        }

        for (int i = 0; i < cloudCount; i++)
        {
            Cloud c = CreateCloud();
            // Sākumā izmētā pa visu ekrānu, nevis visus pie malas
            float halfW = area.rect.width / 2f;
            Vector2 pos = c.rect.anchoredPosition;
            pos.x = Random.Range(-halfW, halfW);
            c.rect.anchoredPosition = pos;
            clouds.Add(c);
        }
    }

    Cloud CreateCloud()
    {
        GameObject go = new GameObject("Cloud", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(transform, false);

        Image img = go.GetComponent<Image>();
        img.raycastTarget = false; // lai mākoņi netraucē spiest pogas
        img.preserveAspect = true;

        Cloud c = new Cloud { rect = go.GetComponent<RectTransform>() };
        c.rect.anchorMin = c.rect.anchorMax = new Vector2(0.5f, 0.5f);
        ResetCloud(c, img);
        return c;
    }

    // Uzliek mākonim jaunu bildi, izmēru, ātrumu un augstumu
    void ResetCloud(Cloud c, Image img)
    {
        img.sprite = cloudSprites[Random.Range(0, cloudSprites.Length)];

        // Izmērs atkarīgs no ekrāna augstuma, nevis no bildes oriģinālā izmēra
        float height = area.rect.height * cloudSize * Random.Range(minScale, maxScale);
        float aspect = img.sprite.rect.width / img.sprite.rect.height;
        c.rect.localScale = Vector3.one;
        c.rect.sizeDelta = new Vector2(height * aspect, height);

        Color col = img.color;
        col.a = Random.Range(minAlpha, maxAlpha);
        img.color = col;

        c.speed = Random.Range(minSpeed, maxSpeed);
        c.direction = randomDirection ? (Random.value < 0.5f ? -1 : 1) : (moveRight ? 1 : -1);
        c.bobOffset = Random.Range(0f, Mathf.PI * 2f);

        float h = area.rect.height;
        c.baseY = Mathf.Lerp(-h / 2f, h / 2f, Random.Range(minHeight, maxHeight));

        // Novieto aiz ekrāna malas, no kuras tas ielidos
        float startX = (area.rect.width / 2f + CloudHalfWidth(c)) * -c.direction;
        c.rect.anchoredPosition = new Vector2(startX, c.baseY);
    }

    float CloudHalfWidth(Cloud c)
    {
        return c.rect.rect.width * c.rect.localScale.x / 2f;
    }

    void Update()
    {
        float halfW = area.rect.width / 2f;

        foreach (Cloud c in clouds)
        {
            Vector2 pos = c.rect.anchoredPosition;
            pos.x += c.speed * c.direction * Time.deltaTime;
            pos.y = c.baseY + Mathf.Sin(Time.time * bobSpeed + c.bobOffset) * bobAmount;
            c.rect.anchoredPosition = pos;

            // Kad mākonis pilnībā aizlidojis aiz ekrāna -> sāk no jauna ar citu izskatu
            float edge = halfW + CloudHalfWidth(c);
            if ((c.direction > 0 && pos.x > edge) || (c.direction < 0 && pos.x < -edge))
            {
                ResetCloud(c, c.rect.GetComponent<Image>());
            }
        }
    }
}