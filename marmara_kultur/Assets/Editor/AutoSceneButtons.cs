#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

public static class AutoSceneButtons
{
    [MenuItem("Tools/Generate Scene Buttons (Assets/Scenes)")]
    public static void Generate()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) { Debug.LogError("Canvas bulunamadı."); return; }

        var menuler = canvas.transform.Find("Menuler");
        if (menuler == null) { Debug.LogError("Canvas altında 'Menuler' bulunamadı."); return; }

        // SceneLoader (kalıcı çağrı için şart)
        var loader = canvas.GetComponentInChildren<SceneLoader>(true);
        if (loader == null)
        {
            var loaderGO = new GameObject("SceneLoader");
            loaderGO.transform.SetParent(canvas.transform, false);
            loader = loaderGO.AddComponent<SceneLoader>();
        }

        // Eski butonları temizle
        for (int i = menuler.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(menuler.GetChild(i).gameObject);

        // Assets/Scenes içindeki sahneleri bul
        const string scenesPath = "Assets/Scenes";
        if (!Directory.Exists(scenesPath))
        {
            Debug.LogError("Assets/Scenes klasörü bulunamadı.");
            return;
        }

        string[] sceneFiles = Directory.GetFiles(scenesPath, "*.unity", SearchOption.AllDirectories);

        foreach (string sceneFile in sceneFiles)
        {
            string sceneName = Path.GetFileNameWithoutExtension(sceneFile);

            // Button root
            var btnGO = new GameObject(sceneName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(menuler, false);

            var rt = btnGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(320, 70);

            var img = btnGO.GetComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.15f, 1f); // koyu arka plan

            var btn = btnGO.GetComponent<Button>();
            btn.targetGraphic = img;

            // TMP Text
            var textGO = new GameObject("Text", typeof(RectTransform));
            textGO.transform.SetParent(btnGO.transform, false);

            var textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(16, 8);
            textRT.offsetMax = new Vector2(-16, -8);

            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = sceneName;
            tmp.fontSize = 28;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white; // beyaz yazı
            tmp.raycastTarget = false;

            // KALICI (Inspector’da görünen) onClick bağlama
            btn.onClick = new Button.ButtonClickedEvent();
            UnityEventTools.AddStringPersistentListener(btn.onClick, loader.LoadScene, sceneName);

            EditorUtility.SetDirty(btn);
        }

        EditorUtility.SetDirty(canvas.gameObject);
        Debug.Log($"Butonlar üretildi: {sceneFiles.Length} sahne.");
    }
}
#endif
