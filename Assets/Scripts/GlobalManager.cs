using Sacristan.Utils;
using UnityEngine;

public class GlobalManager : Singleton<GlobalManager>
{
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
