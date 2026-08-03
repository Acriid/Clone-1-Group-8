using UnityEngine;

public class Fullscreen : MonoBehaviour
{
    void Start()
    {
        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
    }
}
