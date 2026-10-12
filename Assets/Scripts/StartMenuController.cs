using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    public void OpenLevel1()
    {
        SceneManager.LoadScene("RecreatedLevel");
    }

    public void ReturnToMenu()
    {
    SceneManager.LoadScene("StartScene");
    }


}
