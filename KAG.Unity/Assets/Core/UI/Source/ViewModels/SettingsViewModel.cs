using KAG.Unity.Common.Models;
using KAG.Unity.Common.Observables;

namespace KAG.Unity.UI.ViewModels
{
	public class SettingsViewModel : Observable
	{
		public bool IsActive
		{
			get => _isActive;
			set => ChangeProperty(ref _isActive, value);
		}
		private bool _isActive;
	}
}