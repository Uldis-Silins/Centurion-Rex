using Sacristan.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalManager : Singleton<GlobalManager>
{
    public static bool IsGame => SceneName != "Menu";
    private static string SceneName => SceneManager.GetActiveScene().name;

    protected override void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        base.Awake();

        DontDestroyOnLoad(gameObject);

        #if ENABLE_CONSOLE
            gameObject.AddComponent<Console>();
        #endif
    }


}
