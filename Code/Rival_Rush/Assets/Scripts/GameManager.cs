using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Agar Legacy Text hai toh: using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Match Settings")]
    public float matchTimeInSeconds = 120f; // 2 Minutes
    private float currentTimer;

    [Header("UI References")]
    public TextMeshProUGUI timerText;   // Legacy ke liye: public Text timerText;
    public TextMeshProUGUI resultText;  // Legacy ke liye: public Text resultText;
    public GameObject gameOverPanel;

    [Header("Score Tracker")]
    public int player1Score = 0;
    public int player2Score = 0;

    public bool IsGameOver { get; private set; } = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Time.timeScale = 1f;
    }

    void Start()
    {
        currentTimer = matchTimeInSeconds;

        // Game start hote hi timer dikhega aur game over panel chupa rahega
        if (timerText != null) timerText.gameObject.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (IsGameOver)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            return;
        }

        currentTimer -= Time.deltaTime;
        if (currentTimer <= 0f)
        {
            currentTimer = 0f;
            DeclareTie();
        }

        DisplayTime(currentTimer);
    }

    private void DisplayTime(float timeToDisplay)
    {
        int minutes = Mathf.FloorToInt(timeToDisplay / 60f);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60f);
        if (timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void DeclareWinner(PlayerController.PlayerID winner)
    {
        if (IsGameOver) return;
        IsGameOver = true;

        if (winner == PlayerController.PlayerID.Player1)
        {
            player1Score++;
            ShowEndScreen("PLAYER 1 WINS!\n\nPress 'R' to Restart");
        }
        else
        {
            player2Score++;
            ShowEndScreen("PLAYER 2 WINS!\n\nPress 'R' to Restart");
        }
    }

    public void DeclareTie()
    {
        if (IsGameOver) return;
        IsGameOver = true;

        ShowEndScreen("TIME'S UP - IT'S A TIE!\n\nPress 'R' to Restart");
    }

    private void ShowEndScreen(string message)
    {
        Time.timeScale = 0f; // Piche ka game freeze karein

        // Result aate hi Timer ko screen se band (hide) kar dega
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (resultText != null) resultText.text = message;
    }
}