using System;
using System.Collections.Generic;
using System.Threading;
using KAG.Unity.Common;
using KAG.Unity.Common.DataBindings;
using KAG.Unity.Common.Models;
using KAG.Unity.Network;
using UnityEngine;
using Zenject;

namespace KAG.Unity.UI.ViewModels
{
	public class JoinMatchViewModel : ViewModel, ITickable
	{
		private const int _timeOutDelayInMilliseconds = 9000;
		
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

		public bool IsCompleted
		{
			get => _isCompleted;
			set => ChangeProperty(ref _isCompleted, value);
		}
		private bool _isCompleted;

		private readonly PlayerModel _playerModel;
		private readonly JoinMatchModel _model;
		private readonly NetworkManager _networkManager;

		private CancellationTokenSource _joinMatchCancellationToken;
		
		public JoinMatchViewModel(PlayerModel playerModel, JoinMatchModel model, NetworkManager networkManager)
		{
			_playerModel = playerModel;
			_model = model;
			_networkManager = networkManager;

			AddMethodBinding(playerModel, nameof(PlayerModel.Name), nameof(OnPlayerNameChanged));
			AddMethodBinding(model, nameof(JoinMatchModel.Status), nameof(OnStatusChanged));
			AddPropertyBinding(model, nameof(JoinMatchModel.Step), nameof(Step));

			_joinMatchCancellationToken = new CancellationTokenSource(_timeOutDelayInMilliseconds);
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
					break;

				case JoinMatchStatus.GettingMatch:
					HasBeenStarted = true;
					TimeSinceStart = MinutesAndSeconds.Zero;
					break;

				case JoinMatchStatus.JoiningMatch:
					IsJoiningMatch = true;
					break;

				case JoinMatchStatus.Faulted:
					IsFaulted = true;
					break;

				case JoinMatchStatus.Completed:
					IsCompleted = true;
					break;
			}
		}

		public void JoinMatchOrCancel()
		{
			if (!HasBeenStarted)
				_model.StartTimestamp = DateTime.UtcNow;
			else
				_model.Status = JoinMatchStatus.Idle;
		}

		void ITickable.Tick()
		{
			if (!HasBeenStarted || IsJoiningMatch)
				return;
			
			TimeSinceStart = MinutesAndSeconds.GetElapsedTimeSince(_model.StartTimestamp);
		}
	}
}