using System.Threading.Tasks;
using KAG.Unity.Common.Observables;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KAG.Unity.Common.Models
{
	public class ApplicationModel : Observable
	{
		private const int LobbySceneIndex = 0;
		private const int GameSceneIndex = 1;
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
		
		public async Task GoInGame()
		{
			await LoadScene(GameSceneIndex, LoadSceneMode.Additive);
			GameStatus = GameStatus.InGame;
		}
		public async Task GoBackToLobby()
		{
			await UnloadedScene(GameSceneIndex);
			GameStatus = GameStatus.InLobby;
		}

		private async Task LoadScene(int sceneIndex, LoadSceneMode loadSceneMode)
		{
			await PrepareLoadingOperation();
			
			var operation = SceneManager.LoadSceneAsync(sceneIndex, loadSceneMode);
			await WaitForLoadOperation(operation);
		}
		private async Task UnloadedScene(int sceneIndex)
		{
			await PrepareLoadingOperation();
			
			var operation = SceneManager.UnloadSceneAsync(sceneIndex);
			await WaitForLoadOperation(operation);
		}

		private async Task PrepareLoadingOperation()
		{
			GameStatus = GameStatus.Transitioning;

			LoadingProgress = 0.0f;
			IsLoading = true;
			
			await Task.Delay(DelayBeforeLoadingStartInMilliseconds);
		}
		private async Task WaitForLoadOperation(AsyncOperation operation)
		{
			while (!operation.isDone)
			{
				await Task.Delay(PollingIntervalInMilliseconds);
				LoadingProgress = operation.progress;
			}

			LoadingProgress = 1.0f;
			await Task.Delay(DelayAfterLoadingEndInMilliseconds);

			IsLoading = false;
			await Task.CompletedTask;
		}

		public void Quit() => 
			Application.Quit();
	}
}