using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    public void OpenLevel1()
    {
        SceneManager.LoadScene("RecreatedLevel");
    }


}
