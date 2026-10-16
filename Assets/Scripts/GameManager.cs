using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    [SerializeField] private Text scoreText;
    [SerializeField] private Text scaredTimeText;
    [SerializeField] private GhostStateController[] ghosts;
    [SerializeField] private GameSoundEffects soundEffects;
    [SerializeField] private RecreatedLevelMusicController musicController;

    public float FearRemaining { get; private set; }

    private int score;

    void Start()
    {
        RefreshScore();
        if (scaredTimeText != null)
            scaredTimeText.gameObject.SetActive(false);
    }

    public void AddScore(int amount)
    {
        score += amount;
        RefreshScore();
    }

    public void StartFear()
    {
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
        foreach (GhostStateController ghost in ghosts)
        {
            if (ghost == null || !ghost.CanBeEaten) continue;
            if ((ghost.transform.position - playerPosition).sqrMagnitude > 0.25f) continue;

            ghost.EnterDead();
            AddScore(300);
            if (soundEffects != null)
                soundEffects.PlayDefeatCat();
            RefreshMusic();
        }
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
