using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEditor.Events;

// Unity augšējā izvēlnē: Tools -> Board Game -> Sakārtot Settings ainu
// Sakārto un nostilo Settings ainas elementus. Visu var atsaukt ar Ctrl+Z.
public static class SettingsLayoutTool
{
    const string GuiSheet = "Assets/Sprites/2D Casual UI/Sprite/GUI.png";
    const string TmpFontPath = "Assets/Fonts/Cooper Black Regular SDF.asset";
    const string LegacyFontPath = "Assets/Fonts/Cooper Black Regular.ttf";

    // Izkārtojums (Canvas vienībās, 1920x1080)
    const float IconX = -505f, LabelX = -300f, LabelW = 330f;
    const float ControlX = 130f, ControlW = 420f, ValueX = 418f;
    static readonly float[] RowY = { 245f, 153f, 61f, -31f, -123f, -215f };

    static readonly Color TextDark = new Color(0.10f, 0.12f, 0.05f);
    static readonly Color DropText = new Color(0.12f, 0.25f, 0.05f);

    static Dictionary<string, Sprite> sprites;
    static TMP_FontAsset tmpFont;
    static Font legacyFont;

    [MenuItem("Tools/Board Game/Sakārtot Settings ainu")]
    public static void Run()
    {
        SettingsMenu menu = Object.FindFirstObjectByType<SettingsMenu>();
        if (menu == null)
        {
            EditorUtility.DisplayDialog("Settings", "Atver Settings ainu (tajā jābūt SettingsMenu skriptam).", "OK");
            return;
        }

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        Transform root = canvas.transform;

        LoadAssets();
        Undo.RegisterFullObjectHierarchyUndo(root.gameObject, "Sakārtot Settings");

        // ---- Virsraksti kreisajā kolonnā ----
        Label(Find(root, "MusicLabel", "Music"), "Music:", 0);
        Label(Find(root, "SfxLabel", "SoundEffects"), "Sound Effects:", 1);
        Label(Find(root, "SongLabel", "SongChanger"), "Song:", 2);
        Label(Find(root, "ResolutionLabel", "Resolution"), "Resolution:", 3);
        Label(Find(root, "QualityLabel", "Quality"), "Quality:", 4);

        // Fullscreen virsrakstu taisām no Music virsraksta kopijas
        Transform fsLabel = Find(root, "FullscreenLabel");
        Transform musicLabel = Find(root, "MusicLabel", "Music");
        if (fsLabel == null && musicLabel != null)
        {
            GameObject copy = Object.Instantiate(musicLabel.gameObject, musicLabel.parent);
            copy.name = "FullscreenLabel";
            Undo.RegisterCreatedObjectUndo(copy, "Sakārtot Settings");
            fsLabel = copy.transform;
        }
        Label(fsLabel, "Fullscreen:", 5);

        // ---- Ikonas ----
        Icon(root, "MusicIcon", "GUI_47", 0);
        Icon(root, "SfxIcon", "GUI_48", 1);
        Icon(root, "SongIcon", "GUI_33", 2);
        Icon(root, "ResolutionIcon", "GUI_36", 3);
        Icon(root, "QualityIcon", "GUI_45", 4);
        Icon(root, "FullscreenIcon", "GUI_51", 5);

        // ---- Slīdņi + procenti ----
        StyleSlider(menu.musicSlider, 0, 0.5f);
        StyleSlider(menu.sfxSlider, 1, 1f);
        ValueText(menu.musicValueText, "50%", 0);
        ValueText(menu.sfxValueText, "100%", 1);

        // ---- Dziesmas pārslēgšana ----
        if (menu.previousSongButton != null) Place(menu.previousSongButton.transform, ControlX - ControlW / 2f + 32f, RowY[2], 64f, 64f);
        if (menu.nextSongButton != null) Place(menu.nextSongButton.transform, ControlX + ControlW / 2f - 32f, RowY[2], 64f, 64f);
        // Kreisā bulta ir spoguļota labā bulta
        if (menu.previousSongButton != null) menu.previousSongButton.transform.localScale = new Vector3(-1f, 1f, 1f);
        if (menu.songNameText != null)
        {
            Place(menu.songNameText.transform, ControlX, RowY[2], ControlW - 150f, 60f);
            SetTmp(menu.songNameText, "1/5  Song name", 30f, TextAlignmentOptions.Center, TextDark);
            menu.songNameText.enableAutoSizing = true;
            menu.songNameText.fontSizeMin = 18f;
            menu.songNameText.fontSizeMax = 30f;
            menu.songNameText.textWrappingMode = TextWrappingModes.NoWrap;
            menu.songNameText.overflowMode = TextOverflowModes.Ellipsis;
        }

        // ---- Saraksti ----
        StyleDropdown(menu.resolutionDropdown, 3);
        StyleDropdown(menu.qualityDropdown, 4);

        // ---- Fullscreen ķeksis ----
        StyleToggle(menu.fullscreenToggle, 5);

        // Pārbīda fonu (mākoņus un Panel) aiz visa pārējā, Ribbon virs tiem
        Transform panel = Find(root, "Panel");
        if (panel != null) panel.SetSiblingIndex(0);
        Transform clouds = Find(root, "Clouds");
        if (clouds != null) clouds.SetSiblingIndex(0);

        EditorUtility.SetDirty(menu);
        EditorSceneManager.MarkSceneDirty(menu.gameObject.scene);
        Debug.Log("Settings aina sakārtota! Saglabā ar Ctrl+S. Atsaukt var ar Ctrl+Z.");
    }

    // ================= PALĪGFUNKCIJAS =================

    static void LoadAssets()
    {
        sprites = new Dictionary<string, Sprite>();
        foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(GuiSheet))
            if (o is Sprite s) sprites[s.name] = s;
        tmpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath);
        legacyFont = AssetDatabase.LoadAssetAtPath<Font>(LegacyFontPath);
        if (sprites.Count == 0) Debug.LogWarning("Neatradu GUI.png sprites: " + GuiSheet);
    }

    static Sprite Spr(string name)
    {
        return sprites != null && sprites.TryGetValue(name, out Sprite s) ? s : null;
    }

    // Atrod objektu pēc pirmā nosaukuma, kas eksistē
    static Transform Find(Transform root, params string[] names)
    {
        Transform[] all = root.GetComponentsInChildren<Transform>(true);
        foreach (string name in names)
            foreach (Transform t in all)
                if (t.name == name) return t;
        return null;
    }

    static void Place(Transform t, float x, float y, float w, float h)
    {
        RectTransform r = (RectTransform)t;
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f);
        r.localScale = Vector3.one;
        r.anchoredPosition = new Vector2(x, y);
        r.sizeDelta = new Vector2(w, h);
    }

    static void Stretch(RectTransform r, float left, float bottom, float right, float top)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = new Vector2(left, bottom);
        r.offsetMax = new Vector2(-right, -top);
    }

    static void SetTmp(TMP_Text t, string text, float size, TextAlignmentOptions align, Color color)
    {
        if (tmpFont != null) t.font = tmpFont;
        t.text = text;
        t.fontSize = size;
        t.enableAutoSizing = false;
        t.alignment = align;
        t.color = color;
    }

    static void Label(Transform t, string text, int row)
    {
        if (t == null) return;
        Place(t, LabelX, RowY[row], LabelW, 70f);
        TMP_Text tmp = t.GetComponent<TMP_Text>();
        if (tmp != null)
        {
            SetTmp(tmp, text, 40f, TextAlignmentOptions.MidlineLeft, TextDark);
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 26f;
            tmp.fontSizeMax = 40f;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
        }
    }

    static void Icon(Transform root, string name, string sprite, int row)
    {
        Transform t = Find(root, name);
        if (t == null)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(root, false);
            Undo.RegisterCreatedObjectUndo(go, "Sakārtot Settings");
            t = go.transform;
        }
        Place(t, IconX, RowY[row], 54f, 54f);
        Image img = t.GetComponent<Image>();
        img.sprite = Spr(sprite);
        img.preserveAspect = true;
        img.raycastTarget = false;
        img.color = Color.white;
    }

    static void ValueText(TMP_Text t, string sample, int row)
    {
        if (t == null) return;
        Place(t.transform, ValueX, RowY[row], 120f, 60f);
        SetTmp(t, sample, 32f, TextAlignmentOptions.Center, TextDark);
    }

    static void StyleSlider(Slider s, int row, float preview)
    {
        if (s == null) return;
        s.minValue = 0f;
        s.maxValue = 1f;
        s.SetValueWithoutNotify(preview); // tikai priekšskatījumam, spēlē vērtību ieliek skripts
        Place(s.transform, ControlX, RowY[row], ControlW, 46f);
        s.direction = Slider.Direction.LeftToRight;

        Transform bg = s.transform.Find("Background");
        if (bg != null)
        {
            Stretch((RectTransform)bg, 0, 0, 0, 0);
            Image img = bg.GetComponent<Image>();
            img.sprite = Spr("GUI_23");
            img.type = Image.Type.Simple;
            img.color = Color.white;
        }

        // Fill: "Filled" tips, lai joslas noapaļotie gali nesaspiežas
        if (s.fillRect != null)
        {
            RectTransform area = (RectTransform)s.fillRect.parent;
            Stretch(area, 5, 5, 5, 5);
            Stretch(s.fillRect, 0, 0, 0, 0);
            Image fill = s.fillRect.GetComponent<Image>();
            fill.sprite = Spr("GUI_19");
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.color = Color.white;
            fill.fillAmount = s.normalizedValue;
        }

        if (s.handleRect != null)
        {
            RectTransform area = (RectTransform)s.handleRect.parent;
            Stretch(area, 22, 0, 22, 0);
            RectTransform h = s.handleRect;
            h.anchorMin = new Vector2(h.anchorMin.x, 0.5f);
            h.anchorMax = new Vector2(h.anchorMax.x, 0.5f);
            h.pivot = new Vector2(0.5f, 0.5f);
            h.sizeDelta = new Vector2(58f, 58f);
            h.anchoredPosition = Vector2.zero;
            Image img = h.GetComponent<Image>();
            img.sprite = Spr("GUI_7");
            img.preserveAspect = true;
            img.color = Color.white;
        }
    }

    static void StyleDropdown(TMP_Dropdown d, int row)
    {
        if (d == null) return;
        Place(d.transform, ControlX, RowY[row], ControlW, 66f);

        Image bg = d.GetComponent<Image>();
        bg.sprite = Spr("GUI_19");
        bg.type = Image.Type.Simple;
        bg.color = Color.white;

        if (d.captionText != null)
        {
            Stretch(d.captionText.rectTransform, 28, 6, 60, 6);
            SetTmp(d.captionText, d.captionText.text, 30f, TextAlignmentOptions.MidlineLeft, DropText);
        }

        Transform arrow = d.transform.Find("Arrow");
        if (arrow != null)
        {
            RectTransform a = (RectTransform)arrow;
            a.anchorMin = a.anchorMax = new Vector2(1f, 0.5f);
            a.pivot = new Vector2(0.5f, 0.5f);
            a.anchoredPosition = new Vector2(-34f, 0f);
            a.sizeDelta = new Vector2(30f, 30f);
            a.localEulerAngles = new Vector3(0, 0, -90f);
            Image img = arrow.GetComponent<Image>();
            img.sprite = Spr("GUI_55");
            img.preserveAspect = true;
            img.color = Color.white;
        }

        // Atvērtais saraksts
        RectTransform template = d.template;
        if (template != null)
        {
            template.sizeDelta = new Vector2(template.sizeDelta.x, 320f);
            Image tImg = template.GetComponent<Image>();
            if (tImg != null) tImg.color = new Color(1f, 1f, 0.92f);

            Transform content = Find(template, "Content");
            Transform item = Find(template, "Item");
            const float itemH = 56f;
            if (content != null) ((RectTransform)content).sizeDelta = new Vector2(((RectTransform)content).sizeDelta.x, itemH);
            if (item != null)
            {
                RectTransform ir = (RectTransform)item;
                ir.sizeDelta = new Vector2(ir.sizeDelta.x, itemH);

                Toggle tg = item.GetComponent<Toggle>();
                if (tg != null)
                {
                    ColorBlock cb = tg.colors;
                    cb.normalColor = new Color(1f, 1f, 1f, 0f);
                    cb.highlightedColor = new Color(0.55f, 0.85f, 0.25f, 0.6f);
                    cb.selectedColor = new Color(0.55f, 0.85f, 0.25f, 0.35f);
                    cb.pressedColor = new Color(0.45f, 0.75f, 0.2f, 0.8f);
                    tg.colors = cb;
                }
                Transform ib = item.Find("Item Background");
                if (ib != null) ib.GetComponent<Image>().color = Color.white;
            }
            if (d.itemText != null)
            {
                SetTmp(d.itemText, d.itemText.text, 28f, TextAlignmentOptions.MidlineLeft, DropText);
                d.itemText.rectTransform.offsetMin = new Vector2(48f, d.itemText.rectTransform.offsetMin.y);
            }
        }
    }

    static void StyleToggle(Toggle t, int row)
    {
        if (t == null) return;
        Place(t.transform, ControlX - ControlW / 2f + 30f, RowY[row], 60f, 60f);

        if (t.targetGraphic != null)
        {
            RectTransform bg = t.targetGraphic.rectTransform;
            Stretch(bg, 0, 0, 0, 0);
            Image img = (Image)t.targetGraphic;
            img.sprite = Spr("GUI_8");
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            img.color = Color.white;
        }
        if (t.graphic != null)
        {
            RectTransform ck = t.graphic.rectTransform;
            Stretch(ck, 12, 12, 12, 12);
            Image img = t.graphic as Image;
            if (img != null)
            {
                img.sprite = Spr("GUI_66");
                img.preserveAspect = true;
                img.color = Color.white;
            }
        }
        // Toggle iekšējais teksts vairs nav vajadzīgs (ir FullscreenLabel)
        Transform lbl = t.transform.Find("Label");
        if (lbl != null) lbl.gameObject.SetActive(false);
    }

    // =====================================================================
    //  HIERARCHY + SKAŅAS
    // =====================================================================

    [MenuItem("Tools/Board Game/Sakārtot Settings Hierarchy + skaņas")]
    public static void OrganizeAndSounds()
    {
        SettingsMenu menu = Object.FindFirstObjectByType<SettingsMenu>();
        if (menu == null)
        {
            EditorUtility.DisplayDialog("Settings", "Atver Settings ainu (tajā jābūt SettingsMenu skriptam).", "OK");
            return;
        }
        Transform root = Object.FindFirstObjectByType<Canvas>().transform;
        Undo.RegisterFullObjectHierarchyUndo(root.gameObject, "Sakārtot Hierarchy");

        // --- Pārdēvē neskaidros nosaukumus ---
        Rename(root, "Music", "MusicLabel");
        Rename(root, "SoundEffects", "SfxLabel");
        Rename(root, "SongChanger", "SongLabel");
        Rename(root, "Resolution", "ResolutionLabel");
        Rename(root, "Quality", "QualityLabel");
        Rename(root, "SoungName", "SongName");
        Rename(root, "Music Slider", "MusicSlider");

        // --- Grupas ---
        Transform options = Group(root, "Options");
        Move(Group(options, "MusicRow"), root, "MusicIcon", "MusicLabel", "MusicSlider", "MusicValue");
        Move(Group(options, "SfxRow"), root, "SfxIcon", "SfxLabel", "SfxSlider", "SfxValue");
        Move(Group(options, "SongRow"), root, "SongIcon", "SongLabel", "PrevSongButton", "SongName", "NextSongButton");
        Move(Group(options, "ResolutionRow"), root, "ResolutionIcon", "ResolutionLabel", "ResolutionDropdown");
        Move(Group(options, "QualityRow"), root, "QualityIcon", "QualityLabel", "QualityDropdown");
        Move(Group(options, "FullscreenRow"), root, "FullscreenIcon", "FullscreenLabel", "FullscreenToggle");
        Move(Group(root, "Buttons"), root, "ResetButton", "BackButton");

        // Secība: fons -> panelis -> lente -> iestatījumi -> pogas
        string[] order = { "Clouds", "Panel", "Ribbon", "Options", "Buttons" };
        for (int i = 0; i < order.Length; i++)
        {
            Transform t = root.Find(order[i]);
            if (t != null) t.SetSiblingIndex(i);
        }

        // --- Skaņas un kursors visiem interaktīvajiem elementiem ---
        SoundEffectScript sfx = Object.FindFirstObjectByType<SoundEffectScript>();
        CursorScript cursor = Object.FindFirstObjectByType<CursorScript>();
        int count = 0;
        if (sfx == null || cursor == null)
        {
            Debug.LogWarning("Neatradu SoundEffectScript vai CursorScript (ScriptHolder) - skaņas netika pievienotas.");
        }
        else
        {
            foreach (Selectable sel in root.GetComponentsInChildren<Selectable>(true))
            {
                // Scrollbar sarakstā neaiztiekam
                if (sel is Scrollbar) continue;
                // Slīdņiem klikšķa skaņu neliekam (efektu slīdnis jau atskaņo paraugskaņu)
                AddSounds(sel.gameObject, sfx, cursor, !(sel is Slider));
                count++;
            }
        }

        EditorSceneManager.MarkSceneDirty(menu.gameObject.scene);
        Debug.Log("Hierarchy sakārtota, skaņas pieliktas " + count + " elementiem. Saglabā ar Ctrl+S.");
    }

    static void Rename(Transform root, string from, string to)
    {
        if (Find(root, to) != null) return;
        Transform t = Find(root, from);
        if (t != null && t.parent != null && (t.parent == root || t.parent.name.EndsWith("Row")))
            t.name = to;
    }

    // Tukšs konteiners, izstiepts pa visu vecāku, lai bērnu pozīcijas nemainās
    static Transform Group(Transform parent, string name)
    {
        Transform t = parent.Find(name);
        if (t == null)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Sakārtot Hierarchy");
            go.transform.SetParent(parent, false);
            t = go.transform;
        }
        RectTransform r = (RectTransform)t;
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
        r.pivot = new Vector2(0.5f, 0.5f);
        r.localScale = Vector3.one;
        return t;
    }

    static void Move(Transform group, Transform root, params string[] names)
    {
        foreach (string n in names)
        {
            Transform t = Find(root, n);
            if (t == null) { Debug.LogWarning("Neatradu: " + n); continue; }
            Undo.SetTransformParent(t, group, "Sakārtot Hierarchy");
            t.SetAsLastSibling();
        }
    }

    // Tāpat kā izvēlnes pogām: hover -> skaņa + kursors, iziet -> parastais kursors,
    // nospiež -> klikšķa skaņa + kursors, atlaiž -> pogas kursors
    static void AddSounds(GameObject go, SoundEffectScript sfx, CursorScript cursor, bool clickSound)
    {
        EventTrigger trigger = go.GetComponent<EventTrigger>();
        if (trigger == null) trigger = Undo.AddComponent<EventTrigger>(go);
        Undo.RecordObject(trigger, "Skaņas");
        trigger.triggers.Clear();

        Add(trigger, EventTriggerType.PointerEnter, cursor.OnButtonCursor, sfx.Hover);
        Add(trigger, EventTriggerType.PointerExit, cursor.DefaultCursor);
        if (clickSound) Add(trigger, EventTriggerType.PointerDown, cursor.ClickedButtonCursor, sfx.Click);
        else Add(trigger, EventTriggerType.PointerDown, cursor.ClickedButtonCursor);
        Add(trigger, EventTriggerType.PointerUp, cursor.OnButtonCursor);
        EditorUtility.SetDirty(trigger);
    }

    static void Add(EventTrigger trigger, EventTriggerType type, params UnityAction[] actions)
    {
        EventTrigger.Entry e = new EventTrigger.Entry { eventID = type };
        foreach (UnityAction a in actions)
            UnityEventTools.AddVoidPersistentListener(e.callback, a);
        trigger.triggers.Add(e);
    }
}
