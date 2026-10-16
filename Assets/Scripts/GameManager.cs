using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    [SerializeField] private Text scoreText;
    [SerializeField] private Text scaredTimeText;
    [SerializeField] private GhostStateController[] ghosts;
    [SerializeField] private GameSoundEffects soundEffects;
    [SerializeField] private RecreatedLevelMusicController musicController;
    [SerializeField] private PacStudentController playerController;
    [SerializeField] private GameObject[] lifeIcons;

    public float FearRemaining { get; private set; }
    public int Lives { get; private set; } = 3;
    public bool IsDeathSequenceActive { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool CanInteract => !IsDeathSequenceActive && !IsGameOver;

    private int score;
    private Text gameOverText;

    void Start()
    {
        RefreshScore();
        RefreshLives();
        if (scaredTimeText != null)
            scaredTimeText.gameObject.SetActive(false);
        CreateGameOverText();
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
            IsGameOver = true;
            if (gameOverText != null) gameOverText.gameObject.SetActive(true);
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

    private void CreateGameOverText()
    {
        if (scoreText == null || scoreText.canvas == null) return;

        GameObject textObject = new GameObject("Game Over", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(scoreText.canvas.transform, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(500f, 100f);

        gameOverText = textObject.GetComponent<Text>();
        gameOverText.font = scoreText.font;
        gameOverText.fontSize = 48;
        gameOverText.alignment = TextAnchor.MiddleCenter;
        gameOverText.color = Color.white;
        gameOverText.text = "GAME OVER";
        textObject.SetActive(false);
    }

    private void RefreshScore()
    {
        if(scoreText != null)
        {
            scoreText.text = score.ToString("D6");
        }
    }
    void Update()
    {
        if (!CanInteract) return;

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
