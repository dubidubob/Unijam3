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

    public static GameManager Game { get { return Instance._game; } }
    public static UI_Manager UI { get { return Instance._ui; } }
    public static InputManager Input { get { return Instance._input; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static SoundManager Sound { get { return Instance._sound; } }
    void Start()
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
        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@Manager");
            if (go == null)
            {

                go = new GameObject { name = "@Manager" };
                go.AddComponent<Managers>();
            }
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(go);
            }

            s_instance = go.GetComponent<Managers>();
            s_instance._pool.Init();
            s_instance._sound.Init();
        }
    }

    private void OnApplicationQuit() => Clear(true);

    public static void Clear(bool quitting = false)
    {
        Input.Clear();
        Pool.Clear(quitting);
        Sound.Clear();
        Scene.Clear();
        UI.Clear();
        Game.Clear();
    }
}
