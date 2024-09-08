using UnityEngine;

public class ChromeManager : MonoBehaviour
{
    void Awake()
    {
        ApplicationChrome.statusBarState = ApplicationChrome.States.VisibleOverContent;
    }
}
