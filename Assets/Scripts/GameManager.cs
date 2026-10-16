using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    [SerializeField] private Text scoreText;
    [SerializeField] private Text timeText;
    [SerializeField] private Text scaredTimeText;
    [SerializeField] private GhostStateController[] ghosts;
    [SerializeField] private GameSoundEffects soundEffects;
    [SerializeField] private RecreatedLevelMusicController musicController;
    [SerializeField] private PacStudentController playerController;
    [SerializeField] private GameObject[] lifeIcons;
    [SerializeField] private PlayerInteractionController playerInteraction;
    [SerializeField] private Button exitButton;

    public float FearRemaining { get; private set; }
    public int Lives { get; private set; } = 3;
    public bool IsDeathSequenceActive { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool HasGameplayStarted { get; private set; }
    public bool CanInteract => HasGameplayStarted && !IsDeathSequenceActive && !IsGameOver;
    public float ElapsedSeconds { get; private set; }

    private int score;
    private GameObject overlay;
    private Text overlayText;

    void Start()
    {
        RefreshScore();
        RefreshLives();
        RefreshTime();
        if (scaredTimeText != null)
            scaredTimeText.gameObject.SetActive(false);
        CreateOverlay();
        if (playerController != null) playerController.SetGameplayEnabled(false);
        foreach (GhostStateController ghost in ghosts)
            if (ghost != null) ghost.SetPaused(true);
        StartCoroutine(OpeningCountdown());
    }

    public void AddScore(int amount)
    {
        score += amount;
        RefreshScore();
    }

    public void StartFear()
    {
        if (!CanInteract) return;

        FearRemaining = 10f;
        if (scaredTimeText != null)
        {
            scaredTimeText.gameObject.SetActive(true);
            scaredTimeText.text = "10";
        }

        foreach (GhostStateController ghost in ghosts)
        {
            if (ghost != null)
                ghost.EnterScared();
        }
        RefreshMusic();
    }

    public void TryEatGhostsAt(Vector3 playerPosition)
    {
        if (!CanInteract) return;

        foreach (GhostStateController ghost in ghosts)
        {
            if (ghost == null || ghost.State == GhostStateController.GhostState.Dead) continue;
            if ((ghost.transform.position - playerPosition).sqrMagnitude > 0.25f) continue;

            if (ghost.State == GhostStateController.GhostState.Normal)
            {
                StartCoroutine(DeathSequence());
                return;
            }

            ghost.EnterDead();
            AddScore(300);
            if (soundEffects != null)
                soundEffects.PlayDefeatCat();
            RefreshMusic();
        }
    }

    private IEnumerator DeathSequence()
    {
        IsDeathSequenceActive = true;
        Lives = Mathf.Max(0, Lives - 1);
        RefreshLives();

        if (playerController != null) playerController.BeginDeath();
        foreach (GhostStateController ghost in ghosts)
            if (ghost != null) ghost.SetPaused(true);
        if (musicController != null) musicController.StopMusic();
        if (soundEffects != null) soundEffects.PlayMouseDeath();

        yield return new WaitForSeconds(1f);

        if (Lives == 0)
        {
            BeginGameOver();
            yield break;
        }

        FearRemaining = 0f;
        if (scaredTimeText != null)
            scaredTimeText.gameObject.SetActive(false);
        foreach (GhostStateController ghost in ghosts)
            if (ghost != null) ghost.ResetForRound();
        if (playerController != null) playerController.ResetAfterDeath();

        IsDeathSequenceActive = false;
        RefreshMusic();
    }

    private void RefreshLives()
    {
        for (int i = 0; i < lifeIcons.Length; i++)
            if (lifeIcons[i] != null) lifeIcons[i].SetActive(i < Lives);
    }

    private void CreateOverlay()
    {
        if (scoreText == null || scoreText.canvas == null) return;

        overlay = new GameObject("Game Overlay", typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(scoreText.canvas.transform, false);
        RectTransform background = overlay.GetComponent<RectTransform>();
        background.anchorMin = Vector2.zero;
        background.anchorMax = Vector2.one;
        background.offsetMin = Vector2.zero;
        background.offsetMax = Vector2.zero;
        overlay.GetComponent<Image>().color = new Color(0.05f, 0.08f, 0.18f, 0.75f);

        GameObject textObject = new GameObject("Overlay Text", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(overlay.transform, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(500f, 100f);

        overlayText = textObject.GetComponent<Text>();
        overlayText.font = scoreText.font;
        overlayText.fontSize = 64;
        overlayText.alignment = TextAnchor.MiddleCenter;
        overlayText.color = Color.white;
        overlayText.raycastTarget = false;
        overlay.SetActive(false);
    }

    private IEnumerator OpeningCountdown()
    {
        if (overlay != null) overlay.SetActive(true);
        string[] messages = { "3", "2", "1", "GO!" };
        foreach (string message in messages)
        {
            if (overlayText != null) overlayText.text = message;
            yield return new WaitForSeconds(1f);
        }

        if (overlay != null) overlay.SetActive(false);
        if (musicController != null)
        {
            musicController.StopMusic();
            musicController.PlayNormalMusic();
        }
        foreach (GhostStateController ghost in ghosts)
            if (ghost != null) ghost.SetPaused(false);
        if (playerController != null) playerController.SetGameplayEnabled(true);
        HasGameplayStarted = true;
    }

    private void BeginGameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        if (playerController != null) playerController.SetGameplayEnabled(false);
        foreach (GhostStateController ghost in ghosts)
            if (ghost != null) ghost.SetPaused(true);
        if (musicController != null) musicController.StopMusic();
        if (exitButton != null) exitButton.interactable = false;
        if (overlayText != null) overlayText.text = "GAME OVER";
        if (overlay != null) overlay.SetActive(true);
        StartCoroutine(ReturnToMenuAfterDelay());
    }

    private IEnumerator ReturnToMenuAfterDelay()
    {
        yield return new WaitForSecondsRealtime(3f);
        SceneManager.LoadScene("StartScene");
    }

    private void RefreshScore()
    {
        if(scoreText != null)
        {
            scoreText.text = score.ToString("D6");
        }
    }

    private void RefreshTime()
    {
        if (timeText == null) return;

        int centiseconds = Mathf.FloorToInt(ElapsedSeconds * 100f);
        int minutes = centiseconds / 6000;
        int seconds = centiseconds / 100 % 60;
        int hundredths = centiseconds % 100;
        timeText.text = $"{minutes:00}:{seconds:00}:{hundredths:00}";
    }

    void Update()
    {
        if (!CanInteract) return;

        ElapsedSeconds += Time.deltaTime;
        RefreshTime();

        if (playerInteraction != null && playerInteraction.RemainingFoodCount == 0)
        {
            BeginGameOver();
            return;
        }

        if (FearRemaining > 0f)
        {
            float previous = FearRemaining;
            FearRemaining = Mathf.Max(0f, FearRemaining - Time.deltaTime);

            if (FearRemaining <= 0f)
            {
                if (scaredTimeText != null)
                    scaredTimeText.gameObject.SetActive(false);
                foreach (GhostStateController ghost in ghosts)
                    if (ghost != null) ghost.EnterNormal();
                RefreshMusic();
            }
            else
            {
                if (scaredTimeText != null)
                    scaredTimeText.text = Mathf.CeilToInt(FearRemaining).ToString();

                if (previous > 3f && FearRemaining <= 3f)
                    foreach (GhostStateController ghost in ghosts)
                        if (ghost != null) ghost.EnterRecovering();
            }
        }

        bool ghostRevived = false;
        foreach (GhostStateController ghost in ghosts)
            if (ghost != null && ghost.TickDeath(Time.deltaTime, FearRemaining))
                ghostRevived = true;
        if (ghostRevived) RefreshMusic();
    }

    private void RefreshMusic()
    {
        if (musicController == null) return;

        foreach (GhostStateController ghost in ghosts)
        {
            if (ghost != null && ghost.State == GhostStateController.GhostState.Dead)
            {
                musicController.PlayDefeatedMusic();
                return;
            }
        }

        if (FearRemaining > 0f)
            musicController.PlayScaredMusic();
        else
            musicController.PlayNormalMusic();
    }
}
