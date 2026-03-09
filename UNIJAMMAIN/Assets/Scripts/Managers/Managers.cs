using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_instance;
    static Managers Instance { get { Init(); return s_instance; } }


    UI_Manager _ui = new UI_Manager();
    ResourceManager _resource = new ResourceManager();
    GameManager _game = new GameManager();
    InputManager _input = new InputManager();
    PoolManager _pool = new PoolManager();
    SceneManagerEx _scene = new SceneManagerEx();
    SoundManager _sound = new SoundManager();
    SteamAchievementManager _steam = new SteamAchievementManager();

    public static GameManager Game { get { return Instance._game; } }
    public static UI_Manager UI { get { return Instance._ui; } }
    public static InputManager Input { get { return Instance._input; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static SoundManager Sound { get { return Instance._sound; } }
    public static SteamAchievementManager Steam { get { return Instance._steam; } }
    void Awake()
    {
        Init();
    }
    // Update is called once per frame
    void Update()
    {
        _input.OnUpdate();
    }

    public static void Init()
    {
        if (!Application.isPlaying) return;

        // 혹시 모를 이중 초기화 방지용 방어코드
        if (s_instance == null && Application.isPlaying)
        {
            GameObject go = GameObject.Find("@Manager");
            if (go == null)
            {
                go = new GameObject { name = "@Manager" };
                go.AddComponent<Managers>();
            }
            DontDestroyOnLoad(go);

            // if (go.GetComponent<SteamManager>() == null)
            // {
            //     go.AddComponent<SteamManager>();
            // }
            // --------------------------------------------------------

            //수정된 코드: 스팀 매니저 전용 오브젝트 따로 생성
            GameObject steamGo = GameObject.Find("SteamManager");
            if (steamGo == null)
            {
                steamGo = new GameObject { name = "SteamManager" };
                steamGo.AddComponent<SteamManager>();
                DontDestroyOnLoad(steamGo);
            }
            // --------------------------------------------------------

            s_instance = go.GetComponent<Managers>();
            s_instance._pool.Init();
            s_instance._sound.Init();
            s_instance._game.Init();
            LocalizationManager.Init_LocalizationManager();
            UI.ShowAnyUI<SceneLoadingManager>("Loading");
            Debug.Log("매니저 인잇");
        }
    }

    public static void Clear()
    {
        Sound.Clear();
        Input.Clear();
        Pool.Clear();
        Scene.Clear();
        Game.Clear();
        UI.Clear();
    }
}
