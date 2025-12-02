using UnityEngine;

public class ThemeButton : MonoBehaviour
{
    public int themeIndex;
	// 0 = default, 1 = blue, 2 = ochre

    public void OnClickTheme()
    {
        ThemeManager.Instance.SetTheme(themeIndex);
    }
}
