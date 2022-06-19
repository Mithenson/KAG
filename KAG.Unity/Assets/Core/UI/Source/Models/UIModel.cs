using KAG.Unity.Common.Observables;

namespace KAG.Unity.UI.Models
{
	public class UIModel : Observable
	{
		public bool IsHoveringAnyElement
		{
			get => _isHoveringAnyElement;
			set => ChangeProperty(ref _isHoveringAnyElement, value);
		}
		private bool _isHoveringAnyElement;
		
		public ushort ActivePanelsCount
		{
			get => _activePanelsCount;
			set => ChangeProperty(ref _activePanelsCount, value);
		}
		private ushort _activePanelsCount;
	}
}