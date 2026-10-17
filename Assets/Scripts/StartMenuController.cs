using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    [SerializeField] private Text level1RecordText;
    [SerializeField] private Text level2RecordText;

    private void Start()
    {
        ShowRecord(level1RecordText, "Level1");
        ShowRecord(level2RecordText, "Level2");
    }

    private void ShowRecord(Text label, string levelName)
    {
        if (label == null) return;

        string scoreKey = levelName + "_BestScore";
        if (!PlayerPrefs.HasKey(scoreKey))
        {
            label.text = "BEST 000000\nTIME 00:00:00";
            return;
        }

        int bestScore = PlayerPrefs.GetInt(scoreKey);
        int hundredths = PlayerPrefs.GetInt(levelName + "_BestTime", 0);
        int minutes = hundredths / 6000;
        int seconds = hundredths / 100 % 60;
        int remainder = hundredths % 100;

        label.text = $"BEST {bestScore:D6}\nTIME {minutes:00}:{seconds:00}:{remainder:00}";
    }

    public void OpenLevel1()
    {
        SceneManager.LoadScene("RecreatedLevel");
    }

    public void ReturnToMenu()
    {
    SceneManager.LoadScene("StartScene");
    }


}
