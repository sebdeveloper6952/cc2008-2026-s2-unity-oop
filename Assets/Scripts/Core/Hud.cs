using System.Text;
using UnityEngine;
using UnityEngine.UI;

// The on-screen interface: oxygen, fuel cells, the label over the object under the cursor,
// short messages and the end screen. It builds itself at startup.
// The game font has no accented letters: keep every string plain ASCII.
public class Hud : MonoBehaviour
{
    public static Hud Instance { get; private set; }

    [SerializeField] Font font;

    Text oxygenText, inventoryText, gunText, gunKeys, promptText, toastText, endTitle, endBody;
    Image oxygenBar;
    GameObject endPanel;
    Transform promptTarget;
    float promptHeight;
    float toastUntil;
    Player player;
    Camera cam;
    Canvas canvas;

    static readonly Color Amber = new Color(0.94f, 0.63f, 0.38f);
    static readonly Color Danger = new Color(0.9f, 0.3f, 0.3f);

    void Awake()
    {
        Instance = this;
        Build();
    }

    void Start()
    {
        player = FindFirstObjectByType<Player>();
        cam = Camera.main;
    }

    void LateUpdate()
    {
        // oxygen
        float s = player.Oxygen.Remaining;
        oxygenText.text = "O2  " + Mathf.FloorToInt(s / 60f) + ":" + Mathf.FloorToInt(s % 60f).ToString("00");
        bool low = s < 30f;
        oxygenText.color = low ? Danger : Color.white;
        oxygenBar.color = low ? Danger : Amber;
        oxygenBar.rectTransform.sizeDelta = new Vector2(320f * s / player.Oxygen.Max, 8f);

        // inventory
        var sb = new StringBuilder();
        foreach (Item item in player.Inventory.Items)
        {
            sb.AppendLine("- " + item);
        }
        if (player.Inventory.Count == 0) sb.AppendLine("(empty)");
        sb.Append("Total charge: " + player.Inventory.TotalCharge().ToString("0") + "%");
        inventoryText.text = sb.ToString();

        // gun in hand, and the keys for the ones carried
        Gun current = player.Loadout.Current;
        gunText.text = current != null ? current.Name.ToUpper() : "NONE";
        var keys = new StringBuilder();
        for (int i = 0; i < player.Loadout.Guns.Count; i++)
        {
            keys.Append("[" + (i + 1) + "] " + player.Loadout.Guns[i].Name + "   ");
        }
        gunKeys.text = keys.ToString().TrimEnd();

        // the label follows its object
        if (promptTarget != null)
        {
            Vector2 screen = cam.WorldToScreenPoint(promptTarget.position + Vector3.up * promptHeight);
            Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, screen, uiCam, out Vector2 local);
            promptText.rectTransform.anchoredPosition = local;
        }
        else
        {
            promptText.enabled = false;
        }

        // messages
        float left = toastUntil - Time.unscaledTime;
        toastText.enabled = left > 0f;
        var c = toastText.color;
        c.a = Mathf.Clamp01(left / 0.4f);
        toastText.color = c;
    }

    // ------------------------------------------------------------ API

    public void ShowPrompt(string text, Transform target)
    {
        if (promptTarget != target)
        {
            promptTarget = target;
            promptHeight = TopOf(target) + 0.35f;
        }
        promptText.text = text;
        promptText.enabled = true;
    }

    public void HidePrompt()
    {
        promptTarget = null;
        promptText.enabled = false;
    }

    public void Toast(string message, float seconds = 2.5f)
    {
        toastText.text = message;
        toastUntil = Time.unscaledTime + seconds;
    }

    public void ShowEnd(bool won, string message)
    {
        HidePrompt();
        endPanel.SetActive(true);
        endTitle.text = won ? "RESCUE INCOMING" : "MISSION FAILED";
        endTitle.color = won ? Amber : Danger;
        endBody.text = message + "\n\n[R] Restart";
    }

    // ------------------------------------------------------------ building

    static float TopOf(Transform t)
    {
        var renderers = t.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return 1f;
        Bounds b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        return b.max.y - t.position.y;
    }

    void Build()
    {
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var title = Label("Title", 30, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(48, -36));
        title.text = "OUTPOST 13";
        title.color = Amber;

        oxygenText = Label("Oxygen", 60, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(48, -76));

        oxygenBar = new GameObject("OxygenBar", typeof(Image)).GetComponent<Image>();
        oxygenBar.transform.SetParent(transform, false);
        var br = oxygenBar.rectTransform;
        br.anchorMin = br.anchorMax = br.pivot = new Vector2(0, 1);
        br.anchoredPosition = new Vector2(50, -156);

        var cells = Label("CellsTitle", 28, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(48, -186));
        cells.text = "FUEL CELLS";
        cells.color = Amber;
        inventoryText = Label("Inventory", 26, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(48, -224));

        var gunTitle = Label("GunTitle", 28, TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(-48, -36));
        gunTitle.text = "GUN";
        gunTitle.color = Amber;
        gunText = Label("Gun", 44, TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(-48, -70));
        gunKeys = Label("GunKeys", 24, TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(-48, -126));

        promptText = Label("Prompt", 28, TextAnchor.LowerCenter, new Vector2(0.5f, 0.5f), Vector2.zero);
        promptText.rectTransform.pivot = new Vector2(0.5f, 0f);
        promptText.enabled = false;

        toastText = Label("Toast", 30, TextAnchor.LowerCenter, new Vector2(0.5f, 0), new Vector2(0, 70));
        toastText.rectTransform.pivot = new Vector2(0.5f, 0f);
        toastText.rectTransform.sizeDelta = new Vector2(1600, 120);
        toastText.enabled = false;

        endPanel = new GameObject("EndPanel", typeof(Image));
        endPanel.transform.SetParent(transform, false);
        var pr = (RectTransform)endPanel.transform;
        pr.anchorMin = Vector2.zero; pr.anchorMax = Vector2.one; pr.offsetMin = pr.offsetMax = Vector2.zero;
        endPanel.GetComponent<Image>().color = new Color(0.03f, 0.03f, 0.05f, 0.82f);
        endTitle = Label("EndTitle", 88, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 80), endPanel.transform);
        endTitle.rectTransform.sizeDelta = new Vector2(1600, 140);
        endBody = Label("EndBody", 32, TextAnchor.UpperCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -10), endPanel.transform);
        endBody.rectTransform.pivot = new Vector2(0.5f, 1f);
        endBody.rectTransform.sizeDelta = new Vector2(1400, 200);
        endPanel.SetActive(false);
    }

    Text Label(string name, int size, TextAnchor align, Vector2 anchor, Vector2 pos, Transform parent = null)
    {
        var go = new GameObject(name, typeof(Text), typeof(Outline));
        go.transform.SetParent(parent != null ? parent : transform, false);
        var t = go.GetComponent<Text>();
        t.font = font;
        t.fontSize = size;
        t.alignment = align;
        t.color = Color.white;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        var o = go.GetComponent<Outline>();
        o.effectColor = new Color(0, 0, 0, 0.85f);
        o.effectDistance = new Vector2(2, -2);
        var r = t.rectTransform;
        r.anchorMin = r.anchorMax = anchor;
        r.pivot = anchor;
        r.anchoredPosition = pos;
        r.sizeDelta = new Vector2(700, 60);
        return t;
    }
}
