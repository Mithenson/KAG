using System;
using KAG.Unity.Common.DataBindings;
using KAG.Unity.Common.Observables;

namespace KAG.Unity.Common.Models
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
	}
}