using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public string levelName;

    public void OnClick()
    {
        if (!string.IsNullOrEmpty(levelName))
        {
            Application.LoadLevel(levelName);
        }
    }

}
