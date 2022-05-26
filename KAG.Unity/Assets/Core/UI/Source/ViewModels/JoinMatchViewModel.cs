using System;
using System.Threading;
using KAG.Unity.Common;
using KAG.Unity.Common.Models;
using KAG.Unity.Network;
using UnityEngine;
using Zenject;

namespace KAG.Unity.UI.ViewModels
{
	public class JoinMatchViewModel : ViewModel, ITickable
	{
		private const int _timeOutDelayInMilliseconds = 30_0000;
		
		public bool PlayerNameIsNotEmpty
		{
			get => _playerNameIsNotEmpty;
			set => ChangeProperty(ref _playerNameIsNotEmpty, value);
		}
		private bool _playerNameIsNotEmpty;
		
		public bool CanJoin
		{
			get => _canJoin;
			set => ChangeProperty(ref _canJoin, value);
		}
		private bool _canJoin;
		
		public bool HasBeenStarted
		{
			get => _hasBeenStarted;
			set
			{
				if (!value)
					IsJoiningMatch = false;
				
				ChangeProperty(ref _hasBeenStarted, value);
			}
		}
		private bool _hasBeenStarted;
		
		public MinutesAndSeconds TimeSinceStart
		{
			get => _timeSinceStart;
			set => ChangeProperty(ref _timeSinceStart, value);
		}
		private MinutesAndSeconds _timeSinceStart;
		
		public string Step
		{
			get => _step;
			set => ChangeProperty(ref _step, $"{value}...");
		}
		private string _step;

		public bool IsJoiningMatch
		{
			get => _isJoiningMatch;
			set => ChangeProperty(ref _isJoiningMatch, value);
		}
		private bool _isJoiningMatch;

		public bool IsFaulted
		{
			get => _isFaulted;
			set => ChangeProperty(ref _isFaulted, value);
		}
		private bool _isFaulted;

		public string Error
		{
			get => _error;
			set => ChangeProperty(ref _error, value);
		}
		private string _error;

		public bool IsCompleted
		{
			get => _isCompleted;
			set => ChangeProperty(ref _isCompleted, value);
		}
		private bool _isCompleted;

		private readonly PlayerModel _playerModel;
		private readonly JoinMatchModel _model;
		private readonly NetworkManager _networkManager;

		private CancellationTokenSource _joinMatchCancellation;
		
		public JoinMatchViewModel(PlayerModel playerModel, JoinMatchModel model, NetworkManager networkManager)
		{
			_playerModel = playerModel;
			_model = model;
			_networkManager = networkManager;
			
			AddMethodBinding(playerModel, nameof(PlayerModel.Name), nameof(OnPlayerNameChanged));
			AddMethodBinding(model, nameof(JoinMatchModel.Status), nameof(OnStatusChanged));
			AddPropertyBinding(model, nameof(JoinMatchModel.Step), nameof(Step));
			AddMethodBinding(model, nameof(JoinMatchModel.Exception), nameof(OnExceptionCaught));
		}

		public void OnPlayerNameChanged(string playerName)
		{
			PlayerNameIsNotEmpty = !string.IsNullOrEmpty(playerName);

			if (playerName == null)
				return;
			
			CanJoin = playerName.Length >= 3;
		}

		public void OnStatusChanged(JoinMatchStatus status)
		{
			switch (status)
			{
				case JoinMatchStatus.Idle:
					HasBeenStarted = false;
					IsFaulted = false;
					IsCompleted = false;
					break;

				case JoinMatchStatus.GettingMatch:
					HasBeenStarted = true;
					IsFaulted = false;
					IsCompleted = false;
					TimeSinceStart = MinutesAndSeconds.Zero;
					break;

				case JoinMatchStatus.JoiningMatch:
					IsJoiningMatch = true;
					break;

				case JoinMatchStatus.Faulted:
					HasBeenStarted = false;
					IsFaulted = true;
					break;

				case JoinMatchStatus.Completed:
					IsCompleted = true;
					break;
			}
		}

		public void OnExceptionCaught(Exception exception)
		{
			if (exception == null)
				return;
			
			string error;
			if (exception is NetworkException networkException)
				error = networkException.Error.Message;
			else if (exception.InnerException is NetworkException innerNetworkException)
				error = innerNetworkException.Error.Message;
			else
				error = "An unknown issue occured.";

			if (error.Length > 26)
				error = error.Remove(26).Insert(26, "...");

			Error = error;
		}

		public async void JoinMatchOrCancel()
		{
			if (!HasBeenStarted)
			{
				_joinMatchCancellation = new CancellationTokenSource(_timeOutDelayInMilliseconds);
				var task = _networkManager.JoinMatch(_joinMatchCancellation.Token);
				
				try
				{
					await task;
				}
				catch
				{
					if (task.IsCanceled)
						return;

					throw;
				}
			}
			else
			{
				_joinMatchCancellation.Cancel();
				_model.Status = JoinMatchStatus.Idle;
			}
		}

		void ITickable.Tick()
		{
			if (!HasBeenStarted || IsJoiningMatch)
				return;
			
			TimeSinceStart = MinutesAndSeconds.GetElapsedTimeSince(_model.StartTimestamp);
		}
	}
}