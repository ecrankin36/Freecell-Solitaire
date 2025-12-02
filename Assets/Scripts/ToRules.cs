using UnityEngine;
using UnityEngine.SceneManagement;

public class ToRules : MonoBehaviour
{
    public void Restart()
    {
        SceneManager.LoadScene("RulesMenu");
    }
}
