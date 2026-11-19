using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// =====================================================================
//  LEADERBOARD
//  Rezultāti glabājas failā leaderboard.json (nevis PlayerPrefs),
//  lai tie nepazustu, kad spēle izsauc PlayerPrefs.DeleteAll().
//
//  Pagaidām rāda paraugdatus (showExampleData), kamēr spēle nav gatava.
//  Vēlāk, kad spēle būs gatava, var izsaukt:
//      LeaderboardScript.RecordGame(vārds, tēlaIndekss, spēlētājuSkaits);  // kad sākas spēle
//      LeaderboardScript.AddWin(vārds);                                     // kad kāds uzvar
// =====================================================================

[Serializable]
public class LeaderboardEntry
{
    public string name;
    public int characterIndex;   // pēdējais izvēlētais tēls
    public int playerCount;      // spēlētāju skaits pēdējā spēlē
    public int games;            // cik reizes spēlēts
    public int wins;             // uzvaras (kad Circus ainā tās tiks pieskaitītas)
}

[Serializable]
public class LeaderboardData
{
    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
}

public class LeaderboardScript : MonoBehaviour
{
    // ------------------------------------------------------------------
    //  DATI (static – var izsaukt no jebkuras ainas)
    // ------------------------------------------------------------------
    const string FileName = "leaderboard.json";
    static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    public static LeaderboardData Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(File.ReadAllText(FilePath));
                if (data != null && data.entries != null) return data;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Leaderboard: neizdevās nolasīt failu – " + e.Message);
        }
        return new LeaderboardData();
    }

    public static void Save(LeaderboardData data)
    {
        try
        {
            File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
        }
        catch (Exception e)
        {
            Debug.LogWarning("Leaderboard: neizdevās saglabāt failu – " + e.Message);
        }
    }

    static LeaderboardEntry FindOrCreate(LeaderboardData data, string playerName)
    {
        LeaderboardEntry entry = data.entries.Find(
            e => string.Equals(e.name, playerName, StringComparison.OrdinalIgnoreCase));
        if (entry == null)
        {
            entry = new LeaderboardEntry { name = playerName };
            data.entries.Add(entry);
        }
        return entry;
    }

    // Izsauc, kad sākas spēle (vēlāk)
    public static void RecordGame(string playerName, int characterIndex, int playerCount)
    {
        if (string.IsNullOrWhiteSpace(playerName)) return;

        LeaderboardData data = Load();
        LeaderboardEntry entry = FindOrCreate(data, playerName.Trim());
        entry.characterIndex = characterIndex;
        entry.playerCount = playerCount;
        entry.games++;
        Save(data);
    }

    // Izsauc Circus ainā, kad spēlētājs uzvar
    public static void AddWin(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName)) return;

        LeaderboardData data = Load();
        FindOrCreate(data, playerName.Trim()).wins++;
        Save(data);
    }

    public static void ClearAll()
    {
        Save(new LeaderboardData());
    }

    // Kārtība: vairāk uzvaru -> vairāk spēļu -> alfabēts
    public static List<LeaderboardEntry> GetSorted()
    {
        List<LeaderboardEntry> list = Load().entries;
        list.Sort((a, b) =>
        {
            int c = b.wins.CompareTo(a.wins);
            if (c != 0) return c;
            c = b.games.CompareTo(a.games);
            if (c != 0) return c;
            return string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase);
        });
        return list;
    }

    // ------------------------------------------------------------------
    //  UI (uzliec uz ScriptHolder Leaderboard ainā)
    // ------------------------------------------------------------------
    [Header("Kur zīmēt tabulu (tukšs objekts uz Panel)")]
    public RectTransform table;

    [Header("Izskats")]
    public TMP_FontAsset font;               // Cooper Black Regular SDF
    public Sprite rowSprite;                 // nav obligāts, piem. GUI_19 (zaļā josla)
    public Sprite headerSprite;              // nav obligāts, piem. GUI_23 (tumšā josla)
    public Sprite[] medals = new Sprite[3];  // nav obligāti: zelts, sudrabs, bronza (GUI_4, GUI_5, GUI_6)

    [Header("Tēli (tādā pašā secībā kā ChooseCharacterScript)")]
    public Sprite[] characterSprites;        // ja ievilkti – rāda tēla bildi
    public string[] characterNames;          // ja nav bilžu – rāda nosaukumu
    public bool showWins = false;            // ieslēdz, kad Circus ainā būs uzvaras

    [Header("Paraugdati (kamēr spēle vēl nav gatava)")]
    [Tooltip("Ja īstu rezultātu vēl nav, rāda izdomātus rezultātus. Tie netiek saglabāti.")]
    public bool showExampleData = true;
    public int maxRows = 8;
    public float rowHeight = 62f;
    public float rowSpacing = 10f;
    public float fontSize = 34f;
    public Color textColor = new Color(0.10f, 0.12f, 0.05f);
    public Color headerTextColor = Color.white;
    public Color rowColor = Color.white;                       // tonis rindas bildei
    public Color rowColorNoSprite = new Color(1f, 1f, 1f, 0.35f);

    [Header("Notīrīšanas poga (nav obligāta)")]
    public Button clearButton;
    public Text clearButtonText;             // pogas teksts (Text (Legacy))
    public string clearText = "CLEAR";
    public string confirmText = "SURE?";

    // Kolonnu robežas (0 = tabulas kreisā mala, 1 = labā mala)
    static readonly Vector2 ColRank = new Vector2(0.00f, 0.10f);
    static readonly Vector2 ColChar = new Vector2(0.10f, 0.30f);
    static readonly Vector2 ColName = new Vector2(0.31f, 0.62f);
    static readonly Vector2 ColPlayers = new Vector2(0.62f, 0.81f);
    static readonly Vector2 ColGames = new Vector2(0.81f, 1.00f);
    // Ja showWins ieslēgts, "Players" vietā rāda "Wins"

    bool waitingForConfirm;
    Coroutine confirmRoutine;

    void Start()
    {
        if (clearButton != null) clearButton.onClick.AddListener(OnClearPressed);
        if (clearButtonText != null) clearButtonText.text = clearText;
        Rebuild();
    }

    // Uzzīmē tabulu no jauna
    public void Rebuild()
    {
        if (table == null)
        {
            Debug.LogWarning("Leaderboard: nav ievilkts 'Table' objekts.");
            return;
        }

        for (int i = table.childCount - 1; i >= 0; i--)
            Destroy(table.GetChild(i).gameObject);

        List<LeaderboardEntry> list = GetSorted();
        if (list.Count == 0 && showExampleData) list = ExampleData();

        // Virsraksta rinda
        RectTransform header = CreateRow("Header", 0, headerSprite, headerSprite != null ? Color.white : new Color(0f, 0f, 0f, 0.25f));
        CreateText(header, "#", ColRank, TextAlignmentOptions.Center, headerTextColor);
        CreateText(header, "Character", ColChar, TextAlignmentOptions.Center, headerTextColor);
        CreateText(header, "Name", ColName, TextAlignmentOptions.MidlineLeft, headerTextColor);
        CreateText(header, showWins ? "Wins" : "Players", ColPlayers, TextAlignmentOptions.Center, headerTextColor);
        CreateText(header, "Games", ColGames, TextAlignmentOptions.Center, headerTextColor);

        if (list.Count == 0)
        {
            RectTransform empty = CreateRow("Empty", 1, null, Color.clear);
            CreateText(empty, "No games played yet!", new Vector2(0f, 1f), TextAlignmentOptions.Center, textColor);
            return;
        }

        int count = Mathf.Min(maxRows, list.Count);
        for (int i = 0; i < count; i++)
        {
            LeaderboardEntry e = list[i];
            RectTransform row = CreateRow("Row " + (i + 1), i + 1, rowSprite,
                                          rowSprite != null ? rowColor : rowColorNoSprite);

            // Vieta: medaļa top 3, citiem numurs
            if (i < 3 && medals != null && i < medals.Length && medals[i] != null)
                CreateIcon(row, medals[i], ColRank);
            else
                CreateText(row, (i + 1).ToString(), ColRank, TextAlignmentOptions.Center, textColor);

            // Tēls: bilde, ja ir; citādi nosaukums; citādi numurs
            int ci = e.characterIndex;
            if (characterSprites != null && ci >= 0 && ci < characterSprites.Length && characterSprites[ci] != null)
                CreateIcon(row, characterSprites[ci], ColChar);
            else if (characterNames != null && ci >= 0 && ci < characterNames.Length && !string.IsNullOrEmpty(characterNames[ci]))
                CreateText(row, characterNames[ci], ColChar, TextAlignmentOptions.Center, textColor);
            else
                CreateText(row, "#" + (ci + 1), ColChar, TextAlignmentOptions.Center, textColor);

            CreateText(row, e.name, ColName, TextAlignmentOptions.MidlineLeft, textColor);
            CreateText(row, showWins ? e.wins.ToString() : e.playerCount.ToString(), ColPlayers, TextAlignmentOptions.Center, textColor);
            CreateText(row, e.games.ToString(), ColGames, TextAlignmentOptions.Center, textColor);
        }
    }

    // Izdomāti rezultāti tikai izskatam (netiek saglabāti failā)
    static List<LeaderboardEntry> ExampleData()
    {
        return new List<LeaderboardEntry>
        {
            new LeaderboardEntry { name = "Rihards", characterIndex = 0, playerCount = 4, games = 12, wins = 7 },
            new LeaderboardEntry { name = "Anna",    characterIndex = 1, playerCount = 3, games = 10, wins = 5 },
            new LeaderboardEntry { name = "Janis",   characterIndex = 2, playerCount = 2, games = 8,  wins = 4 },
            new LeaderboardEntry { name = "Liga",    characterIndex = 3, playerCount = 4, games = 6,  wins = 2 },
            new LeaderboardEntry { name = "Peteris", characterIndex = 0, playerCount = 5, games = 4,  wins = 1 },
            new LeaderboardEntry { name = "Marta",   characterIndex = 1, playerCount = 2, games = 2,  wins = 0 },
        };
    }

    // ---------- Notīrīšana ar apstiprinājumu ----------
    public void OnClearPressed()
    {
        if (!waitingForConfirm)
        {
            // Pirmais klikšķis – prasa apstiprinājumu
            waitingForConfirm = true;
            if (clearButtonText != null) clearButtonText.text = confirmText;
            confirmRoutine = StartCoroutine(CancelConfirmAfter(3f));
            return;
        }

        // Otrais klikšķis – notīra
        if (confirmRoutine != null) StopCoroutine(confirmRoutine);
        waitingForConfirm = false;
        if (clearButtonText != null) clearButtonText.text = clearText;
        ClearAll();
        Rebuild();
    }

    IEnumerator CancelConfirmAfter(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        waitingForConfirm = false;
        if (clearButtonText != null) clearButtonText.text = clearText;
    }

    // ---------- Testēšanai ----------
    // Inspector: labais klikšķis uz LeaderboardScript virsraksta -> "Pievienot testa rezultātus"
    [ContextMenu("Pievienot testa rezultātus")]
    void AddTestResults()
    {
        string[] names = { "Rihards", "Anna", "Janis", "Liga", "Peteris", "Marta" };
        for (int p = 0; p < names.Length; p++)
        {
            int times = UnityEngine.Random.Range(1, 6);
            for (int i = 0; i < times; i++)
                RecordGame(names[p], p % 4, UnityEngine.Random.Range(1, 8));
        }
        Debug.Log("Leaderboard: pievienoti testa rezultāti (" + FilePath + ")");
        if (Application.isPlaying) Rebuild();
    }

    [ContextMenu("Notīrīt visus rezultātus")]
    void ClearFromMenu()
    {
        ClearAll();
        Debug.Log("Leaderboard: notīrīts.");
        if (Application.isPlaying) Rebuild();
    }

    // ---------- UI palīgfunkcijas ----------
    RectTransform CreateRow(string name, int index, Sprite sprite, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(table, false);

        RectTransform r = (RectTransform)go.transform;
        r.anchorMin = new Vector2(0f, 1f);
        r.anchorMax = new Vector2(1f, 1f);
        r.pivot = new Vector2(0.5f, 1f);
        r.sizeDelta = new Vector2(0f, rowHeight);
        r.anchoredPosition = new Vector2(0f, -index * (rowHeight + rowSpacing));

        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        return r;
    }

    void CreateText(RectTransform row, string text, Vector2 column, TextAlignmentOptions align, Color color)
    {
        GameObject go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(row, false);

        RectTransform r = (RectTransform)go.transform;
        r.anchorMin = new Vector2(column.x, 0f);
        r.anchorMax = new Vector2(column.y, 1f);
        r.offsetMin = new Vector2(8f, 4f);
        r.offsetMax = new Vector2(-8f, -4f);

        TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = text;
        t.color = color;
        t.alignment = align;
        t.enableAutoSizing = true;
        t.fontSizeMin = 18f;
        t.fontSizeMax = fontSize;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Ellipsis;
        t.raycastTarget = false;
    }

    void CreateIcon(RectTransform row, Sprite sprite, Vector2 column)
    {
        GameObject go = new GameObject("Medal", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(row, false);

        RectTransform r = (RectTransform)go.transform;
        r.anchorMin = new Vector2(column.x, 0f);
        r.anchorMax = new Vector2(column.y, 1f);
        r.offsetMin = new Vector2(4f, -6f);
        r.offsetMax = new Vector2(-4f, 6f);

        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
    }
}