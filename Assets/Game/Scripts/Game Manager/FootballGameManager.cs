using UnityEngine;
using System.Collections.Generic;

public class FootballGameManager : MonoBehaviour
{
    // Game settings
    public float matchDuration = 420f; // 7 minutes in seconds
    private float currentTime;
    private bool isMatchActive = true;

    // Player references
    [SerializeField] private List<PlayerController> teamPlayers = new List<PlayerController>(3);
    [SerializeField] private List<PlayerController> opponentPlayers = new List<PlayerController>(3);
    [SerializeField] private List<PlayerController> allPlayers = new List<PlayerController>();
    private int currentPlayerIndex = 0;
    private PlayerController currentPlayer;

    // Ball reference
    [SerializeField] private BallController ball;
    [SerializeField] private Transform ballDefaultPosition;

    // UI references (assign in inspector)
    public UnityEngine.UI.Text timerText;
    public UnityEngine.UI.Text scoreText;
    private int teamScore = 0;
    private int opponentScore = 0;

    void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        currentTime = matchDuration;
        isMatchActive = true;

        // Validate players
        teamPlayers.RemoveAll(player => player == null);
        opponentPlayers.RemoveAll(player => player == null);

        if (teamPlayers.Count > 0)
        {
            currentPlayerIndex = 0;
            SetActivePlayer(teamPlayers[currentPlayerIndex]);
        }

        ResetBall();
    }

    void Update()
    {
        if (isMatchActive)
        {
            UpdateTimer();
            HandleInput();
        }
    }

    void UpdateTimer()
    {
        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        {
            currentTime = 0;
            isMatchActive = false;
            EndMatch();
        }

        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    void HandleInput()
    {
        if (!TeamHasBall() && Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchToNextPlayer();
        }

        if (TeamHasBall())
        {
            if (Input.GetButtonDown("Pass")) AttemptPass();
            else if (Input.GetButtonDown("Shoot")) AttemptShoot();
        }
    }

    void SwitchToNextPlayer()
    {
        if (teamPlayers.Count == 0) return;

        currentPlayerIndex = (currentPlayerIndex + 1) % teamPlayers.Count;
        SetActivePlayer(teamPlayers[currentPlayerIndex]);
    }

    

    public void SwitchPlayerControl(GameObject newPlayer)
    {
        foreach (PlayerController pc in allPlayers)
        {
            pc.SetControlled(pc.gameObject == newPlayer);
        }
    }

    bool TeamHasBall()
    {
        return ball.currentHolder != null && teamPlayers.Contains(ball.currentHolder);
    }

    void SwitchPlayer()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % teamPlayers.Count;
        currentPlayer = teamPlayers[currentPlayerIndex];
        SetActivePlayer(currentPlayer);
    }

    void SetActivePlayer(PlayerController player)
    {
        // Disable all players first
        foreach (var p in teamPlayers)
        {
            p.SetActive(false);
        }

        // Enable new player
        player.SetActive(true);
        currentPlayer = player;

        Debug.Log($"Now controlling: {player.name}");
    }

    void AttemptPass()
    {
        if (currentPlayer == null || !TeamHasBall()) return;

        // Find nearest teammate
        PlayerController nearestTeammate = null;
        float minDistance = float.MaxValue;

        foreach (var teammate in teamPlayers)
        {
            if (teammate == currentPlayer) continue;

            float distance = Vector3.Distance(currentPlayer.transform.position, teammate.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestTeammate = teammate;
            }
        }

        if (nearestTeammate != null)
        {
            // Pass the ball to the teammate
            currentPlayer.Pass(nearestTeammate.transform.position);

            // Switch control to the receiving player after a short delay
            StartCoroutine(SwitchAfterPass(nearestTeammate));
        }
    }

    System.Collections.IEnumerator SwitchAfterPass(PlayerController receiver)
    {
        // Wait for the pass to reach the receiver (adjust time as needed)
        yield return new WaitForSeconds(0.5f);

        if (teamPlayers.Contains(receiver))
        {
            currentPlayerIndex = teamPlayers.IndexOf(receiver);
            currentPlayer = receiver;
            SetActivePlayer(currentPlayer);
        }
    }

    void AttemptShoot()
    {
        if (currentPlayer == null || !TeamHasBall()) return;

        // Determine shoot direction (toward opponent goal)
        // You'll need to implement your own goal detection logic
        Vector3 shootDirection = DetermineShootDirection();

        currentPlayer.Shoot(shootDirection);
    }

    Vector3 DetermineShootDirection()
    {
        // This is a placeholder - implement your own logic based on your field setup
        // For example, you might raycast to find the goal or use a predefined direction
        return transform.forward;
    }

    public void ScoreGoal(bool teamScored)
    {
        if (teamScored)
        {
            teamScore++;
        }
        else
        {
            opponentScore++;
        }

        // Update UI
        if (scoreText != null)
        {
            scoreText.text = $"{teamScore} - {opponentScore}";
        }

        ResetBall();
    }

    void ResetBall()
    {
        ball.ResetPosition(ballDefaultPosition.position);
    }

    void EndMatch()
    {
        Debug.Log("Match ended! Final Score: " + teamScore + " - " + opponentScore);
        // Add match end logic (show results screen, etc.)
    }
}