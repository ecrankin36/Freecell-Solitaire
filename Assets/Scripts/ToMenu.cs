using UnityEngine;
using UnityEngine.SceneManagement;

public class ToMenu : MonoBehaviour
{
    public void Restart()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
