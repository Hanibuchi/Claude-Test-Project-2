using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Screens")]
    [SerializeField] private UIDocument mainMenuDocument;
    [SerializeField] private UIDocument hudDocument;
    [SerializeField] private UIDocument gameOverDocument;

    private Button startButton;
    private Button retryButton;
    private Button reviveButton;
    private Label hudScoreLabel;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnEnable()
    {
        BindMainMenu();
        BindHud();
        BindGameOver();
    }

    void OnDisable()
    {
        UnbindMainMenu();
        UnbindGameOver();
    }

    void Start()
    {
        ShowMainMenu();
    }

    private void BindMainMenu()
    {
        if (mainMenuDocument == null)
        {
            return;
        }

        startButton = mainMenuDocument.rootVisualElement.Q<Button>("start-button");
        if (startButton != null)
        {
            startButton.clicked += OnStartClicked;
        }
    }

    private void UnbindMainMenu()
    {
        if (startButton != null)
        {
            startButton.clicked -= OnStartClicked;
        }
    }

    private void BindHud()
    {
        if (hudDocument == null)
        {
            return;
        }

        hudScoreLabel = hudDocument.rootVisualElement.Q<Label>("score-label");
    }

    private void BindGameOver()
    {
        if (gameOverDocument == null)
        {
            return;
        }

        VisualElement root = gameOverDocument.rootVisualElement;
        retryButton = root.Q<Button>("retry-button");
        reviveButton = root.Q<Button>("revive-button");

        if (retryButton != null)
        {
            retryButton.clicked += OnRetryClicked;
        }

        if (reviveButton != null)
        {
            reviveButton.clicked += OnReviveClicked;
        }
    }

    private void UnbindGameOver()
    {
        if (retryButton != null)
        {
            retryButton.clicked -= OnRetryClicked;
        }

        if (reviveButton != null)
        {
            reviveButton.clicked -= OnReviveClicked;
        }
    }

    private void OnStartClicked()
    {
        GameManager.Instance?.StartGame();
    }

    private void OnRetryClicked()
    {
        GameManager.Instance?.StartGame();
    }

    private void OnReviveClicked()
    {
        GameManager.Instance?.RequestRevive();
    }

    public void ShowMainMenu()
    {
        SetDocumentActive(mainMenuDocument, true);
        SetDocumentActive(hudDocument, false);
        SetDocumentActive(gameOverDocument, false);
    }

    public void ShowHud()
    {
        SetDocumentActive(mainMenuDocument, false);
        SetDocumentActive(hudDocument, true);
        SetDocumentActive(gameOverDocument, false);
    }

    public void ShowGameOver()
    {
        SetDocumentActive(hudDocument, false);
        SetDocumentActive(gameOverDocument, true);
    }

    public void UpdateHud(float survivalTime)
    {
        if (hudScoreLabel == null)
        {
            return;
        }

        hudScoreLabel.text = $"TIME: {survivalTime:0.0}";
    }

    private void SetDocumentActive(UIDocument document, bool active)
    {
        if (document == null)
        {
            return;
        }

        document.rootVisualElement.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
