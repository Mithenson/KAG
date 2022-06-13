using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KAG.Shared.Events;
using KAG.Unity.Common.Observables;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Zenject;

namespace KAG.Unity.Common.Models
{
	public sealed class ApplicationModel : Observable
	{
		private const int PollingIntervalInMilliseconds = 500;
		private const int DelayBeforeLoadingStartInMilliseconds = 1_000;
		private const int DelayAfterLoadingEndInMilliseconds = 1_000;

		public bool IsLoading
		{
			get => _isLoading;
			set => ChangeProperty(ref _isLoading, value);
		}
		private bool _isLoading;

		public float LoadingProgress
		{
			get => _loadingProgress;
			set => ChangeProperty(ref _loadingProgress, value);
		}
		private float _loadingProgress;

		public string LoadingDescription
		{
			get => _loadingDescription;
			set => ChangeProperty(ref _loadingDescription, value);
		}
		private string _loadingDescription;

		public GameStatus GameStatus
		{
			get => _gameStatus;
			set => ChangeProperty(ref _gameStatus, value);
		}
		private GameStatus _gameStatus;

		private readonly EventHub _eventHub;
		
		private List<float> _loadingProgresses;

		public ApplicationModel(EventHub eventHub)
		{
			_eventHub = eventHub;
			
			_isLoading = true;
			_loadingProgresses = new List<float>(2);
			
			_eventHub.Define<SceneTransitionEventArgs>(EventKey.SceneTransition);
		}

		public async Task GoInLobby()
		{
			LoadingDescription = "Going into lobby";
			await PrepareLoadingOperation(GameStatus.InLobby);
			
			var operation = SceneManager.LoadSceneAsync(UnityConstants.Scenes.LobbySceneIndex, LoadSceneMode.Additive);
			await WaitForLoadOperation(operation);
			await CompleteLoad(GameStatus.InLobby);
		}
		public async Task GoInGame()
		{
			LoadingDescription = "Going into game";
			await PrepareLoadingOperation(GameStatus.InGame);
			
			var loadOperation = SceneManager.LoadSceneAsync(UnityConstants.Scenes.GameSceneIndex, LoadSceneMode.Additive);
			var unloadOperation =  SceneManager.UnloadSceneAsync(UnityConstants.Scenes.LobbySceneIndex);
			
			await Task.WhenAll(WaitForLoadOperation(loadOperation), WaitForLoadOperation(unloadOperation));
		}
		public async Task GoBackToLobby()
		{
			LoadingDescription = "Going back to lobby";
			await PrepareLoadingOperation(GameStatus.InLobby);
			
			var loadOperation = SceneManager.LoadSceneAsync(UnityConstants.Scenes.LobbySceneIndex, LoadSceneMode.Additive);
			var unloadOperation =  SceneManager.UnloadSceneAsync(UnityConstants.Scenes.GameSceneIndex);
			
			await Task.WhenAll(WaitForLoadOperation(loadOperation), WaitForLoadOperation(unloadOperation));
			await CompleteLoad(GameStatus.InLobby);
		}

		private async Task PrepareLoadingOperation(GameStatus destination)
		{
			GameStatus = GameStatus.Transitioning;
			_eventHub.Invoke(EventKey.SceneTransition, this, new SceneTransitionEventArgs(destination));

			LoadingProgress = 0.0f;
			IsLoading = true;
			
			_loadingProgresses.Clear();
			
			await Task.Delay(DelayBeforeLoadingStartInMilliseconds);
		}
		private async Task WaitForLoadOperation(AsyncOperation operation)
		{
			var loadProgressIndex = _loadingProgresses.Count;
			_loadingProgresses.Add(0.0f);
			
			while (!operation.isDone)
			{
				await Task.Delay(PollingIntervalInMilliseconds);
				SetLoadingProgress(loadProgressIndex, operation.progress);
			}

			SetLoadingProgress(loadProgressIndex, 1.0f);
			await Task.CompletedTask;
		}

		private void SetLoadingProgress(int index, float value)
		{
			_loadingProgresses[index] = value;
			
			LoadingProgress = _loadingProgresses.Min();
		}

		private async Task CompleteLoad(GameStatus status)
		{
			await Task.Delay(DelayAfterLoadingEndInMilliseconds);
			
			IsLoading = false;
			GameStatus = status;
			
			await Task.CompletedTask;
		}

		public void Quit() => 
			Application.Quit();
	}
}