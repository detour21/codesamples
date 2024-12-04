using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

	[SerializeField] private ScreenControllerBase MainMenuScreen;
	[SerializeField] private HUDSCreenController HUDScreen;
	[SerializeField] private ScreenControllerBase PauseScreen;
	[SerializeField] private ScreenControllerBase GameOverScreen;
	[SerializeField] private ScreenControllerBase LevelCompleteScreen;
	[SerializeField] private ScreenControllerBase HelpScreen;
	[SerializeField] private SettingsScreenController SettingsScreen;
	[SerializeField] private ScreenControllerBase DebugScreen;
	[SerializeField] private TopBarController TopBarController;

	private List<ScreenControllerBase> AllScreens = new List<ScreenControllerBase>();

	public void Initialize()
	{
		//gather screen controllers
		AllScreens.Clear();
		AllScreens.Add(MainMenuScreen);
		AllScreens.Add(HUDScreen);
		AllScreens.Add(PauseScreen);
		AllScreens.Add(GameOverScreen);
		AllScreens.Add(LevelCompleteScreen);
		AllScreens.Add(HelpScreen);
		AllScreens.Add(DebugScreen);
		AllScreens.Add(SettingsScreen);
		TopBarController.Initialize();
	}

	public void SetUIState (GameStates state)
	{
		switch (state)
		{
			case GameStates.STATE_INIT:

				break;
			case GameStates.STATE_MENU:
			case GameStates.STATE_GAME_ABORT:
				ActivateScreen(MainMenuScreen, true);
				break;
			case GameStates.STATE_GAME_INIT:
				ActivateScreen(HUDScreen, true);
				break;
			case GameStates.STATE_GAME_RUNNING:
				DeactivateScreen(PauseScreen);
				break;
			case GameStates.STATE_GAME_PAUSED:
				ActivateScreen(PauseScreen);
				break;
			case GameStates.STATE_GAME_OVER:
				ActivateScreen(GameOverScreen, true);
				break;
			case GameStates.STATE_GAME_HELP:
				ActivateScreen(HelpScreen);
				break;
			case GameStates.STATE_GAME_LEVEL_COMPLETE:
				ActivateScreen(LevelCompleteScreen, true);
				break;
			case GameStates.STATE_DEBUG:
				ActivateScreen(DebugScreen);
				break;
			default:
				break;
		}
	}

	private void ActivateScreen(ScreenControllerBase activeScreen, bool HideOthers = false)
	{
		if (HideOthers)
		{
			foreach (ScreenControllerBase screen in AllScreens)
			{
				screen.Deactivate();
			}
		}
		activeScreen.Activate();
	}

	private void DeactivateScreen(ScreenControllerBase screen)
	{
		screen.Deactivate();
	}

	public void StartOrResumePlay (bool wasPaused)
	{
		if (wasPaused)
		{
			Singleton.Instance.GameplayManager.ResumeTimer();
		}
		else
		{
			//game start
			Singleton.Instance.GameplayManager.StartTimer();
			TopBarController.EnableDockButton(true);
			Singleton.Instance.AudioManager.StopSound("MainMenuLoop");
			Singleton.Instance.AudioManager.musicManager.StartGenMusic();
		}
	}

	public void EnableDockButton(bool state)
	{
		TopBarController.EnableDockButton(state);
	}

	public void SetLetters (LetterInfo current, LetterInfo next, LetterInfo dock)
	{
		TopBarController.SetLetters(current, next, dock);
	}

	public void EnableTimer (bool state)
	{
		TopBarController.EnableTimer(state);
	}

	public void UpdateTimer (float value, float timeRemaining)
	{
		TopBarController.UpdateTimer(value, timeRemaining);
	}

	public void LoadLetterImage(string letter, Image target)
	{
		Sprite sprite = Resources.Load<Sprite>("Textures/Letters/" + letter.ToUpper());
		if (sprite != null)
		{
			target.sprite = sprite;
		}
	}

	public void LoadLetterHighlightImage(string letter, Image target)
	{
		Sprite sprite = Resources.Load<Sprite>("Textures/Letters_HL/" + letter.ToUpper());
		if (sprite != null)
		{
			target.sprite = sprite;
		}
	}

	public void ShowScorePopup (WordSearchResult result, int totalScore, int wordCount)
	{
		HUDScreen.ShowScore(result.totalScore.ToString(), result.scorePopupPosition);
		HUDScreen.SetScore(totalScore, wordCount);
		//VFX
		Singleton.Instance.VFXManager.PlayVFX(VFXItem.WORD_SCORE, result.scorePopupPosition);
	}
	
	public void ShowLastScoredWord (string word, int score) 
	{
		HUDScreen.SetLastWord($"{word} ({score.ToString()})");
	}

	public void ShowPauseButton(bool state)
	{
		TopBarController.ShowPauseButton(state);
	}

	public void ShowSettings (bool state) 
	{
		SettingsScreen.gameObject.SetActive(state);
		if (state) 
		{
			SettingsScreen.Activate();
		}
		else 
		{
			SettingsScreen.Deactivate();
		}
		
	}
}
