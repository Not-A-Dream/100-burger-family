using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class AssetPackUISetup
{
    const string SourceAssetPath = "Assets/ExternalAssets/AssetPackUI/pack_001.png";
    const string OutputFolder = "Assets/Generated/AssetPackUI";
    const string SidebarSpriteName = "sidebar_panel.png";

    [MenuItem("Tools/100 Burger Family/Apply Asset Pack UI")]
    public static void Apply()
    {
        if (!File.Exists(SourceAssetPath))
        {
            Debug.LogError($"[AssetPackUISetup] Source image not found: {SourceAssetPath}");
            return;
        }

        Directory.CreateDirectory(OutputFolder);
        EnsureSpriteAsset(SourceAssetPath, Path.Combine(OutputFolder, SidebarSpriteName),
            RectFromTopLeft(8, 40, 222, 150));

        AssetDatabase.Refresh();

        var importer = AssetImporter.GetAtPath($"{OutputFolder}/{SidebarSpriteName}") as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }

        var sidebarSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{OutputFolder}/{SidebarSpriteName}");
        if (sidebarSprite == null)
        {
            Debug.LogError("[AssetPackUISetup] Sidebar sprite import failed.");
            return;
        }

        var canvas = FindMainCanvas();
        if (canvas == null)
        {
            Debug.LogError("[AssetPackUISetup] Canvas not found.");
            return;
        }

        var uiController = Object.FindFirstObjectByType<UIController>();
        var hud = Object.FindFirstObjectByType<InGameHUD>();
        var screen = Object.FindFirstObjectByType<UIScreenController>();

        var hudRoot = RebuildHudRoot(canvas.transform);
        var sidebar = CreateSidebar(hudRoot.transform, sidebarSprite);
        var speechBubble = CreateSpeechBubble(hudRoot.transform, sidebarSprite);
        var buttons = EnsureButtons(canvas.transform);

        if (screen != null)
        {
            screen.GetType().GetField("hudTopPanel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(screen, sidebar);
            screen.GetType().GetField("hudButtons", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(screen, buttons);
            EditorUtility.SetDirty(screen);
        }

        var orderText = CreateText(sidebar.transform, "Text_Order", "오늘의 주문\n패밀리 클래식 버거", new Vector2(22f, -18f), new Vector2(274f, 56f), 19f);
        orderText.color = new Color(1f, 0.86f, 0.38f);
        var statusText = CreateText(sidebar.transform, "Text_Status", "상태  재료 준비", new Vector2(22f, -82f), new Vector2(274f, 24f), 15f);
        var timerText = CreateText(sidebar.transform, "Text_Timer", "시간  00:00", new Vector2(22f, -112f), new Vector2(180f, 24f), 16f);
        var heldItemText = CreateText(sidebar.transform, "Text_HeldItem", "손  비어 있음", new Vector2(22f, -143f), new Vector2(274f, 24f), 15f);
        var burgerText = CreateText(sidebar.transform, "Text_BurgerCount", "완성  0", new Vector2(22f, -174f), new Vector2(130f, 22f), 14f);
        var inventoryText = CreateText(sidebar.transform, "Text_Inventory", "성공 0  ·  실패 0", new Vector2(22f, -202f), new Vector2(274f, 22f), 13f);

        var feedbackText = CreateText(speechBubble.transform, "Text_Feedback", "", new Vector2(24f, -14f), new Vector2(632f, 28f), 17f);
        feedbackText.alignment = TextAlignmentOptions.Center;
        var promptText = CreateText(speechBubble.transform, "Text_Prompt", "WASD 이동 · E 상호작용 · Q 내려놓기", new Vector2(24f, -44f), new Vector2(632f, 58f), 16f);
        promptText.alignment = TextAlignmentOptions.Center;
        var lastMessageText = feedbackText;

        if (uiController != null)
        {
            uiController.statusText = statusText;
            uiController.timerText = timerText;
            uiController.burgerCountText = burgerText;
            uiController.lastMessageText = lastMessageText;
            EditorUtility.SetDirty(uiController);
        }

        if (hud != null)
        {
            hud.burgerCountText = burgerText;
            hud.orderText = orderText;
            hud.statusText = statusText;
            hud.timeText = timerText;
            hud.heldItemText = heldItemText;
            hud.promptText = promptText;
            hud.feedbackText = feedbackText;
            hud.inventoryText = inventoryText;
            EditorUtility.SetDirty(hud);
        }

        HideLegacyPanel(canvas.transform, "TopPanel");

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[AssetPackUISetup] AssetPackHUD rebuilt. Sidebar sprite generated from UI Sprites area.");
    }

    static RectInt RectFromTopLeft(int x, int y, int width, int height)
    {
        var bytes = File.ReadAllBytes(SourceAssetPath);
        var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!source.LoadImage(bytes))
            throw new System.Exception($"Failed to load source image size: {SourceAssetPath}");

        var rect = new RectInt(x, source.height - y - height, width, height);
        Object.DestroyImmediate(source);
        return rect;
    }

    static void EnsureSpriteAsset(string sourcePath, string outputPath, RectInt crop)
    {
        var srcBytes = File.ReadAllBytes(sourcePath);
        var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!source.LoadImage(srcBytes))
            throw new System.Exception($"Failed to load source image: {sourcePath}");

        var cleaned = CropAndClean(source, crop);
        File.WriteAllBytes(outputPath, cleaned.EncodeToPNG());
        Object.DestroyImmediate(source);
        Object.DestroyImmediate(cleaned);
    }

    static Texture2D CropAndClean(Texture2D source, RectInt crop)
    {
        var sourcePixels = source.GetPixels32();
        var pixels = new Color32[crop.width * crop.height];
        for (int y = 0; y < crop.height; y++)
        {
            for (int x = 0; x < crop.width; x++)
            {
                int srcIndex = (crop.y + y) * source.width + (crop.x + x);
                pixels[y * crop.width + x] = sourcePixels[srcIndex];
            }
        }
        var bgColors = new HashSet<Color32>();
        for (int x = 0; x < crop.width; x++)
        {
            bgColors.Add(pixels[x]);
            bgColors.Add(pixels[(crop.height - 1) * crop.width + x]);
        }
        for (int y = 0; y < crop.height; y++)
        {
            bgColors.Add(pixels[y * crop.width]);
            bgColors.Add(pixels[y * crop.width + crop.width - 1]);
        }

        var clear = new bool[pixels.Length];
        var queue = new Queue<Vector2Int>();

        void PushIfBg(int x, int y)
        {
            if (x < 0 || y < 0 || x >= crop.width || y >= crop.height) return;
            int idx = y * crop.width + x;
            if (clear[idx]) return;
            if (!bgColors.Contains(pixels[idx])) return;
            clear[idx] = true;
            queue.Enqueue(new Vector2Int(x, y));
        }

        for (int x = 0; x < crop.width; x++)
        {
            PushIfBg(x, 0);
            PushIfBg(x, crop.height - 1);
        }
        for (int y = 0; y < crop.height; y++)
        {
            PushIfBg(0, y);
            PushIfBg(crop.width - 1, y);
        }

        while (queue.Count > 0)
        {
            var p = queue.Dequeue();
            PushIfBg(p.x + 1, p.y);
            PushIfBg(p.x - 1, p.y);
            PushIfBg(p.x, p.y + 1);
            PushIfBg(p.x, p.y - 1);
        }

        for (int i = 0; i < pixels.Length; i++)
        {
            if (clear[i])
                pixels[i].a = 0;
        }

        int minX = crop.width, minY = crop.height, maxX = -1, maxY = -1;
        for (int y = 0; y < crop.height; y++)
        {
            for (int x = 0; x < crop.width; x++)
            {
                if (pixels[y * crop.width + x].a == 0) continue;
                minX = Mathf.Min(minX, x);
                minY = Mathf.Min(minY, y);
                maxX = Mathf.Max(maxX, x);
                maxY = Mathf.Max(maxY, y);
            }
        }

        if (maxX < minX || maxY < minY)
            return new Texture2D(1, 1, TextureFormat.RGBA32, false);

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;
        var trimmed = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var outPixels = new Color32[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
                outPixels[y * width + x] = pixels[(minY + y) * crop.width + (minX + x)];
        }
        trimmed.SetPixels32(outPixels);
        trimmed.Apply();
        return trimmed;
    }

    static Canvas FindMainCanvas()
    {
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var canvas in canvases)
        {
            if (canvas.name == "Canvas" && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return canvas;
        }

        foreach (var canvas in canvases)
        {
            if (canvas.name == "Canvas")
                return canvas;
        }

        foreach (var canvas in canvases)
        {
            if (canvas.transform.parent == null)
                return canvas;
        }

        return canvases.Length > 0 ? canvases[0] : null;
    }

    static GameObject RebuildHudRoot(Transform canvas)
    {
        var staleHudRoots = new List<GameObject>();
        foreach (var existing in Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (existing == null)
                continue;

            if (existing.name == "AssetPackHUD")
                staleHudRoots.Add(existing.gameObject);
        }

        foreach (var stale in staleHudRoots)
        {
            if (stale != null)
                Object.DestroyImmediate(stale);
        }

        var hudRoot = new GameObject("AssetPackHUD", typeof(RectTransform));
        hudRoot.transform.SetParent(canvas, false);
        hudRoot.transform.SetAsLastSibling();

        var rt = hudRoot.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
        return hudRoot;
    }

    static GameObject CreateSidebar(Transform hudRoot, Sprite sprite)
    {
        var sidebar = new GameObject("SidebarPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        sidebar.transform.SetParent(hudRoot, false);

        var rt = sidebar.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(24f, -24f);
        rt.sizeDelta = new Vector2(330f, 250f);
        rt.localScale = Vector3.one;

        var img = sidebar.GetComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.color = Color.white;
        img.raycastTarget = false;
        return sidebar;
    }

    static GameObject CreateSpeechBubble(Transform hudRoot, Sprite sprite)
    {
        var bubble = new GameObject("PromptBubble", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bubble.transform.SetParent(hudRoot, false);

        var rt = bubble.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 26f);
        rt.sizeDelta = new Vector2(680f, 110f);

        var image = bubble.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = new Color(1f, 1f, 1f, 0.97f);
        image.raycastTarget = false;

        var tail = new GameObject("Tail", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        tail.transform.SetParent(bubble.transform, false);
        var tailRt = tail.GetComponent<RectTransform>();
        tailRt.anchorMin = tailRt.anchorMax = new Vector2(0.5f, 0f);
        tailRt.pivot = new Vector2(0.5f, 0.5f);
        tailRt.anchoredPosition = new Vector2(-180f, -8f);
        tailRt.sizeDelta = new Vector2(24f, 24f);
        tailRt.localRotation = Quaternion.Euler(0f, 0f, 45f);
        var tailImage = tail.GetComponent<Image>();
        tailImage.color = new Color(0.20f, 0.16f, 0.16f, 0.97f);
        tailImage.raycastTarget = false;
        return bubble;
    }

    static TMP_Text CreateText(Transform parent, string name, string defaultText, Vector2 anchoredPos, Vector2 size, float fontSize)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        rt.localScale = Vector3.one;

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = defaultText;
        tmp.font = ReadableTMPFont.Resolve();
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.fontStyle = FontStyles.Bold;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        return tmp;
    }

    static GameObject EnsureSidebar(Transform canvas, Sprite sprite)
    {
        var hudRoot = canvas.Find("AssetPackHUD")?.gameObject;
        if (hudRoot == null)
        {
            hudRoot = new GameObject("AssetPackHUD", typeof(RectTransform));
            hudRoot.transform.SetParent(canvas, false);
        }

        var existing = hudRoot.transform.Find("SidebarPanel");
        if (existing != null)
        {
            var existingRt = existing.GetComponent<RectTransform>();
            if (existingRt == null)
                Object.DestroyImmediate(existing.gameObject);
        }

        var sidebar = hudRoot.transform.Find("SidebarPanel")?.gameObject;
        if (sidebar == null)
        {
            sidebar = new GameObject("SidebarPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            sidebar.transform.SetParent(hudRoot.transform, false);
        }

        var rt = sidebar.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(24f, -24f);
        rt.sizeDelta = new Vector2(260f, 212f);
        var img = sidebar.GetComponent<Image>() ?? sidebar.AddComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.color = Color.white;
        img.raycastTarget = false;
        return sidebar;
    }

    static GameObject EnsureButtons(Transform canvas)
    {
        var buttons = canvas.Find("Buttons");
        if (buttons == null)
        {
            var go = new GameObject("Buttons", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(canvas, false);
            buttons = go.transform;
        }
        else if (buttons.GetComponent<RectTransform>() == null)
        {
            Object.DestroyImmediate(buttons.gameObject);
            var go = new GameObject("Buttons", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(canvas, false);
            buttons = go.transform;
        }

        var rt = buttons.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-24f, 24f);
        rt.sizeDelta = new Vector2(250f, 64f);

        var img = buttons.GetComponent<Image>() ?? buttons.gameObject.AddComponent<Image>();
        img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        img.type = Image.Type.Sliced;
        img.color = new Color(0.26f, 0.18f, 0.16f, 0.94f);
        img.raycastTarget = false;
        return buttons.gameObject;
    }

    static TMP_Text EnsureText(Transform parent, string name, Vector2 anchoredPos, Vector2 size, float fontSize)
    {
        var t = parent.Find(name);
        if (t != null && t.GetComponent<RectTransform>() == null)
        {
            Object.DestroyImmediate(t.gameObject);
            t = null;
        }

        var go = t != null ? t.gameObject : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        if (t == null)
            go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        var tmp = go.GetComponent<TextMeshProUGUI>() ?? go.AddComponent<TextMeshProUGUI>();
        tmp.text = "";
        tmp.font = ReadableTMPFont.Resolve();
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.fontStyle = FontStyles.Bold;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        return tmp;
    }

    static void HideLegacyPanel(Transform canvas, string name)
    {
        var t = canvas.Find(name);
        if (t != null)
            t.gameObject.SetActive(false);
    }
}
