using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KAG.Unity.Common.Observables;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KAG.Unity.Common.Models
{
	public class ApplicationModel : Observable
	{
		private const int LobbySceneIndex = 1;
		private const int GameSceneIndex = 2;
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

		public GameStatus GameStatus
		{
			get => _gameStatus;
			set => ChangeProperty(ref _gameStatus, value);
		}
		private GameStatus _gameStatus;

		private List<float> _loadingProgresses;

		public ApplicationModel()
		{
			_isLoading = true;

			_loadingProgresses = new List<float>(2);
		}

		public async Task GoInLobby()
		{
			await PrepareLoadingOperation();
			
			var operation = SceneManager.LoadSceneAsync(LobbySceneIndex, LoadSceneMode.Additive);
			await WaitForLoadOperation(operation);
			await CompleteLoad(GameStatus.InLobby);
		}
		public async Task GoInGame()
		{
			await PrepareLoadingOperation();
			
			var loadOperation = SceneManager.LoadSceneAsync(GameSceneIndex, LoadSceneMode.Additive);
			var unloadOperation =  SceneManager.UnloadSceneAsync(LobbySceneIndex);

			await Task.WhenAll(WaitForLoadOperation(loadOperation), WaitForLoadOperation(unloadOperation));
			await CompleteLoad(GameStatus.InGame);
		}
		public async Task GoBackToLobby()
		{
			await PrepareLoadingOperation();
			
			var loadOperation = SceneManager.LoadSceneAsync(LobbySceneIndex, LoadSceneMode.Additive);
			var unloadOperation =  SceneManager.UnloadSceneAsync(GameSceneIndex);
			
			await Task.WhenAll(WaitForLoadOperation(loadOperation), WaitForLoadOperation(unloadOperation));
			await CompleteLoad(GameStatus.InLobby);
		}

		private async Task PrepareLoadingOperation()
		{
			GameStatus = GameStatus.Transitioning;

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