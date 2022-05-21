using KAG.Unity.Common;
using KAG.Unity.Common.DataBindings;
using KAG.Unity.Common.Models;
using Zenject;

namespace KAG.Unity.UI.ViewModels
{
	public class JoinMatchViewModel : ViewModel<JoinMatchModel>, ITickable
	{
		public bool HasBeenStarted
		{
			get => _hasBeenStarted;
			set => ChangeProperty(ref _hasBeenStarted, value);
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
			set => ChangeProperty(ref _step, value);
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
		
		public JoinMatchViewModel(JoinMatchModel model)
			: base(model)
		{
			AddMethodBinding(nameof(JoinMatchModel.Status), nameof(OnStatusChanged));
			AddPropertyBinding(nameof(JoinMatchModel.Step), nameof(Step));
		}
		
		private void OnStatusChanged(JoinMatchStatus status)
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
		
		void ITickable.Tick()
		{
			if (_model.Status != JoinMatchStatus.GettingMatch)
				return;
			
			TimeSinceStart = MinutesAndSeconds.GetElapsedTimeSince(_model.StartTimestamp);
		}
	}
}