using UnityEngine;
using UnityEngine.SceneManagement;

public class ToThemes : MonoBehaviour
{
    public void Restart()
    {
        SceneManager.LoadScene("ThemesMenu");
    }
}
