using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// High-level UI screens for the prototype flow.
/// </summary>
public enum UIPanelType
{
    MainMenu,
    Lobby,
    InGame,
    Result
}

/// <summary>
/// Controls which UI panel (screen) is currently visible.
/// Attach this to Canvas (recommended).
/// </summary>
public class UIScreenController : MonoBehaviour
{
    [System.Serializable]
    public class PanelEntry
    {
        public UIPanelType type;
        public GameObject panel;
    }

    [Header("Assign the 4 panels here (MainMenu/Lobby/InGame/Result).")]
    [SerializeField] private List<PanelEntry> panels = new List<PanelEntry>();

    private readonly Dictionary<UIPanelType, GameObject> _map = new Dictionary<UIPanelType, GameObject>();

    private void Awake()
    {
        _map.Clear();

        if (panels == null)
        {
            Debug.LogWarning($"{nameof(UIScreenController)}: panels list is null.");
            return;
        }

        foreach (var p in panels)
        {
            if (p == null || p.panel == null)
            {
                Debug.LogWarning($"{nameof(UIScreenController)}: Missing panel reference for {p?.type}.");
                continue;
            }

            _map[p.type] = p.panel;
        }
    }

    private void Start()
    {
        // Default screen
        Show(UIPanelType.MainMenu);
    }

    /// <summary>
    /// Show exactly one panel and hide the rest.
    /// </summary>
    public void Show(UIPanelType type)
    {
        if (_map.Count == 0)
        {
            Debug.LogWarning($"{nameof(UIScreenController)}: No panels mapped. Did you assign Panels in the Inspector?");
            return;
        }

        foreach (var kv in _map)
        {
            if (kv.Value == null) continue;
            kv.Value.SetActive(kv.Key == type);
        }
    }

    // --- Unity Button-friendly wrappers (so OnClick menus always show them) ---
    public void ShowMainMenu() => Show(UIPanelType.MainMenu);
    public void ShowLobby()    => Show(UIPanelType.Lobby);
    public void ShowInGame()   => Show(UIPanelType.InGame);
    public void ShowResult()   => Show(UIPanelType.Result);
}
