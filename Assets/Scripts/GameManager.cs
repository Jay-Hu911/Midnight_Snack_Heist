using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    [SerializeField] private Text scoreText;

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

    private void RefreshScore()
    {
        if(scoreText != null)
        {
            scoreText.text = score.ToString("D6");
        }
    }
    void Update()
    {

    }
}
