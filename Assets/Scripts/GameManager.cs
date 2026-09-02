using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Title,
        Playing,
        GameOver
    }

    public static GameManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private PlayerController player;

    [Header("Scene Start Point")]
    [SerializeField] private Transform startPoint;

    public GameState State { get; private set; } = GameState.Title;
    public float SurvivalTime { get; private set; }

    private Vector3 lastSafePosition;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }

        lastSafePosition = startPoint != null ? startPoint.position : (player != null ? player.transform.position : Vector3.zero);

        SetState(GameState.Title);
    }

    void Update()
    {
        if (State != GameState.Playing)
        {
            return;
        }

        SurvivalTime += Time.deltaTime;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHud(SurvivalTime);
        }
    }

    public void StartGame()
    {
        SurvivalTime = 0f;

        if (player != null)
        {
            player.Respawn(lastSafePosition);
        }

        SetState(GameState.Playing);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowHud();
        }
    }

    public void UpdateSafePosition(Vector3 position)
    {
        lastSafePosition = position;
    }

    public void OnPlayerDeath()
    {
        if (State != GameState.Playing)
        {
            return;
        }

        SetState(GameState.GameOver);

        if (player != null)
        {
            player.SetControlEnabled(false);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver();
        }
    }

    public void RequestRevive()
    {
        if (State != GameState.GameOver)
        {
            return;
        }

        if (AdManager.Instance != null)
        {
            AdManager.Instance.ShowRewardedAd();
        }
    }

    public void OnRevivalAdCompleted()
    {
        if (State != GameState.GameOver)
        {
            return;
        }

        if (player != null)
        {
            player.Respawn(lastSafePosition);
        }

        SetState(GameState.Playing);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowHud();
        }
    }

    private void SetState(GameState newState)
    {
        State = newState;
    }
}
