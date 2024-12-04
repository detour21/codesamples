using UnityEngine;

public class Singleton : MonoBehaviour
{
	public static Singleton Instance { get; private set; }
	public GameStateManager GameStateManager { get; private set; }
	public AudioManager AudioManager { get; private set; }
	public UIManager UIManager { get; private set; }
	public GameplayManager GameplayManager { get; private set; }
	public DataManager DataManager { get; private set; }
	public DataConfig DataConfig { get; private set; }
	public VFXManager VFXManager { get; private set; }
	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
			return;
		}
		Instance = this;
        DataConfig = Resources.Load<DataConfig>("Data/DataConfig");
        AudioManager = GetComponentInChildren<AudioManager>();
		AudioManager.Initialize();
		GameplayManager = GetComponent<GameplayManager>();
		UIManager = GetComponent<UIManager>();
        UIManager.Initialize();
        DataManager = GetComponent<DataManager>();
        DataManager.Initialize();
		VFXManager = GetComponent<VFXManager>();
        GameStateManager = GetComponent<GameStateManager>();
        GameStateManager.Initialize();
    }
}
