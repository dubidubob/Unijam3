using System;
using UnityEngine;
using Steamworks;

// 이 스크립트는 Steam API를 초기화하고 유지관리하는 필수 스크립트입니다.
[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour
{
	protected static bool s_EverInitialized = false;
	protected static SteamManager s_Instance;
	protected static bool s_Initialized = false;

	public static bool Initialized
	{
		get
		{
			return s_Initialized;
		}
	}

	protected virtual void Awake()
	{
		if (s_Instance != null)
		{
			Destroy(gameObject);
			return;
		}
		s_Instance = this;

		if (s_EverInitialized)
		{
			throw new System.Exception("Tried to Initialize the SteamAPI twice in one session!");
		}

		// We want our SteamManager Instance to persist across scenes.
		DontDestroyOnLoad(gameObject);

		if (!Packsize.Test())
		{
			Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
		}

		if (!DllCheck.Test())
		{
			Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
		}

		try
		{
			// If Steam is not running or the game wasn't started through Steam, SteamAPI_RestartAppIfNecessary starts the
			// Steam client and also launches this game again if the User owns it. This can act as a rudimentary form of DRM.
			if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
			{
				Application.Quit();
				return;
			}
		}
		catch (System.DllNotFoundException e)
		{ // We catch this exception here, as it will be the first occurrence of it.
			Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e, this);
			Application.Quit();
			return;
		}

		// Initializes the Steamworks API.
		// If this returns false, this indicates one of the following conditions:
		// [*] The Steam client isn't running. A running Steam client is required to provide implementations of the various Steamworks interfaces.
		// [*] The Steam client couldn't determine the App ID of game. If you're running your application from the executable or debugger directly rather than from Steam,
		//     then you must have a [steam_appid.txt] in your game directory, with the appID in it and nothing else. Steam will look for this file in the current working directory.
		//     If you are running your executable from a different directory you may need to relocate the [steam_appid.txt] file.
		// [*] Your application is not running under the same OS user context as the Steam client, such as a different user or administration access level.
		// [*] Ensure that you own a license for the App ID on the currently active Steam account. Your game must show up in your Steam library.
		// [*] Your App ID is not completely setup, i.e. in Release State: Unavailable, or it's missing default packages.
		// Valve's documentation for this function: http://partner.steamgames.com/documentation/api
		s_Initialized = SteamAPI.Init();
		if (!s_Initialized)
		{
			Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
			return;
		}

		s_EverInitialized = true;
	}

	// This should only ever get called on client builds.
	// If you are instantiating SteamManager yourself, ensure that you call this to connect your SteamManager to the Steamworks.NET callback dispatcher.
	protected virtual void OnEnable()
	{
		if (s_Instance == null)
		{
			s_Instance = this;
		}

		if (!s_Initialized)
		{
			return;
		}

		if (m_SteamAPIWarningMessageHook == null)
		{
			// Set up our callback to receive warning messages from Steam.
			// You must launch with "-debug_steamapi" in the launch args to receive warnings.
			m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
			SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
		}
	}

	// OnApplicationQuit gets called too early to shutdown the SteamAPI.
	// Because the SteamManager should be persistent, we only want to shutdown on destroy.
	protected virtual void OnDestroy()
	{
		if (s_Instance != this)
		{
			return;
		}

		s_Instance = null;

		if (!s_Initialized)
		{
			return;
		}

		SteamAPI.Shutdown();
	}

	protected virtual void Update()
	{
		if (!s_Initialized)
		{
			return;
		}

		// Run Steam client callbacks
		SteamAPI.RunCallbacks();
	}

	protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
	protected static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}
}