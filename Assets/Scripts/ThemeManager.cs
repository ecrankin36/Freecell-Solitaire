using UnityEngine;
using UnityEngine.SceneManagement;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance;

    public Sprite defaultTheme;
    public Sprite royalBlueTheme;
    public Sprite ochreTheme;

    private string currentThemeKey = "CurrentTheme";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyTheme();
    }

    public void SetTheme(int index)
    {
        PlayerPrefs.SetInt(currentThemeKey, index);
        PlayerPrefs.Save();
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        int index = PlayerPrefs.GetInt(currentThemeKey, 0);
        print(index);

        Sprite chosen = defaultTheme;
        if (index == 1) chosen = royalBlueTheme;
        if (index == 2) chosen = ochreTheme;

        GameObject[] backgrounds = GameObject.FindGameObjectsWithTag("Background");

        foreach (GameObject bg in backgrounds)
        {
            SpriteRenderer sr = bg.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sprite = chosen;
        }
    }
	
    public Sprite GetCurrentThemeSprite()
    {
        int index = PlayerPrefs.GetInt("CurrentTheme", 0);

        if (index == 1) return royalBlueTheme;
        if (index == 2) return ochreTheme;
        return defaultTheme;
    }
}
