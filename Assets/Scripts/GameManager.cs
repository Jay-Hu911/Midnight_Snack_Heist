using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    [SerializeField] private Text scoreText;
    [SerializeField] private Text scaredTimeText;
    [SerializeField] private GhostStateController[] ghosts;

    public float FearRemaining { get; private set; }

    private int score;

    void Start()
    {
        RefreshScore();
    }

    public void AddScore(int amount)
    {
        score += amount;
        RefreshScore();
    }

    public void StartFear()
    {
        FearRemaining = 10f;
        scaredTimeText.gameObject.SetActive(true);
        scaredTimeText.text = "10";

        foreach (GhostStateController ghost in ghosts)
        {
            if (ghost != null)
                ghost.EnterScared();
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
        if (FearRemaining <= 0f) return;

        FearRemaining = Mathf.Max(0f, FearRemaining - Time.deltaTime);
        scaredTimeText.text = Mathf.CeilToInt(FearRemaining).ToString();
    }
}
