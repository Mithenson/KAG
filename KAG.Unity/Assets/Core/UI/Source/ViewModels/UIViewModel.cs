using KAG.Unity.Common;
using KAG.Unity.UI.Models;

namespace KAG.Unity.UI.ViewModels
{
	public sealed class UIViewModel : ViewModel<UIModel>
	{
		public bool IsHoveringAnyElement
		{
			get => _isHoveringAnyElement;
			set => ChangeProperty(ref _isHoveringAnyElement, value);
		}
		private bool _isHoveringAnyElement;

		public bool IsInPanel
		{
			get => _isInPanel;
			set => ChangeProperty(ref _isInPanel, value);
		}
		private bool _isInPanel;

		public UIViewModel(UIModel model) : base(model)
		{
			AddPropertyBinding(nameof(UIModel.IsHoveringAnyElement), nameof(IsHoveringAnyElement));
			AddMethodBinding(nameof(UIModel.ActivePanelsCount), nameof(OnActivePanelsCountChanged));
		}

		public void OnActivePanelsCountChanged(ushort activePanelsCount) =>
			IsInPanel = activePanelsCount > 0;
	}
}