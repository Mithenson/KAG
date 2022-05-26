using KAG.Unity.Common;
using KAG.Unity.Common.Models;
using UnityEngine;

namespace KAG.Unity.UI.ViewModels
{
	public class ApplicationViewModel : ViewModel<ApplicationModel>
	{
		public bool IsInLobby
		{
			get => _isInLobby;
			set => ChangeProperty(ref _isInLobby, value);
		}
		private bool _isInLobby;

		public bool IsInGame
		{
			get => _isInGame;
			set => ChangeProperty(ref _isInGame, value);
		}
		private bool _isInGame;
		
		public bool IsLoading
		{
			get => _isLoading;
			set => ChangeProperty(ref _isLoading, value);
		}
		private bool _isLoading;
		
		public float LoadingProgress
		{
			get => _loadingProgress;
			set => ChangeProperty(ref _loadingProgress, Mathf.Clamp01(value));
		}
		private float _loadingProgress;
		
		public string LoadingText
		{
			get => _loadingText;
			set => ChangeProperty(ref _loadingText, value);
		}
		private string _loadingText;
		
		public ApplicationViewModel(ApplicationModel model) : base(model)
		{
			AddMethodBinding(nameof(ApplicationModel.GameStatus), nameof(OnGameStatusChanged));
			AddPropertyBinding(nameof(ApplicationModel.IsLoading), nameof(IsLoading));
			AddPropertyBinding(nameof(ApplicationModel.LoadingProgress), nameof(LoadingProgress));
			AddMethodBinding(nameof(ApplicationModel.LoadingProgress), nameof(OnLoadingProgressChanged));
		}

		public void OnGameStatusChanged(GameStatus status)
		{
			switch (status)
			{
				case GameStatus.InLobby:
					IsInLobby = true;
					break;
				
				case GameStatus.Transitioning:
					IsInLobby = false;
					IsInGame = false;
					break;

				case GameStatus.InGame:
					IsInGame = true;
					break;
			}
		}

		public void OnLoadingProgressChanged(float progress)
		{
			var percentage = Mathf.RoundToInt(progress * 100.0f);
			LoadingText = $"Loading...{percentage}%";
		}
		
		public void Quit() => 
			_model.Quit();
	}
}