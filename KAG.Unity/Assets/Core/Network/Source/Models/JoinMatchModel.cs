using System;
using System.Threading;
using System.Threading.Tasks;
using KAG.Unity.Common.Models;
using KAG.Unity.Common.Observables;
using Zenject;

namespace KAG.Unity.Network.Models
{
	public class JoinMatchModel : Observable
	{
		public JoinMatchStatus Status
		{
			get => _status;
			set => ChangeProperty(ref _status, value);
		}
		private JoinMatchStatus _status;
		
		public DateTime StartTimestamp
		{
			get => _startTimestamp;
			set
			{
				Status = JoinMatchStatus.GettingMatch;
				ChangeProperty(ref _startTimestamp, value);
			}
		}
		private DateTime _startTimestamp;
		
		public string Step
		{
			get => _step;
			set => ChangeProperty(ref _step, value);
		}
		private string _step;

		public Exception Exception
		{
			get => _exception;
			set => ChangeProperty(ref _exception, value);
		}
		private Exception _exception;

		private DiContainer _container;
		private ApplicationModel _applicationModel;
		private PlayerModel _playerModel;
		
		public JoinMatchModel(DiContainer container, ApplicationModel applicationModel, PlayerModel playerModel)
		{
			_container = container;
			_applicationModel = applicationModel;
			_playerModel = playerModel;
		}

		public async Task JoinMatch(CancellationToken cancellationToken)
		{
			var connectionHandler = _container.Resolve<JoinMatchHandler>();
			var connectionTask = connectionHandler.Execute(_playerModel.Id, cancellationToken);

			try
			{
				await connectionTask;
			}
			catch
			{
				if (connectionTask.IsCanceled)
				{
					await Task.FromCanceled(cancellationToken);
					return;
				}

				if (connectionTask.IsFaulted)
				{
					await Task.FromException(connectionTask.Exception);
					return;
				}

				await Task.FromException(new Exception("An unknown exception has occured."));
				return;
			}

			if (!connectionTask.IsCompleted)
				return;

			await _applicationModel.GoInGame();
		}
	}
}