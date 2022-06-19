using KAG.Unity.Common;
using KAG.Unity.Scenes.Models;

namespace KAG.Unity.Scenes.ViewModels
{
	public sealed class PerformanceViewModel : ViewModel<PerformanceModel>
	{
		public string FpsText
		{
			get => _fpsText;
			set => ChangeProperty(ref _fpsText, value);
		}
		private string _fpsText;

		public PerformanceViewModel(PerformanceModel model) : base(model) => 
			AddMethodBinding(nameof(PerformanceModel.Fps), nameof(OnFpsChanged));

		public void OnFpsChanged(int fps) =>
			FpsText = fps > 99 ? $"{fps:000} FPS" : $"{fps:00} FPS";
	}
}