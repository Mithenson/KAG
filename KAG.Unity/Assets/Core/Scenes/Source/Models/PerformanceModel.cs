using KAG.Unity.Common.Observables;
using UnityEngine;
using Zenject;

namespace KAG.Unity.Scenes.Models
{
	public sealed class PerformanceModel : Observable, ITickable
	{
		private const float FpsUpdateRate = 1.0f / 3.0f;
		
		public int Fps
		{
			get => _fps;
			set => ChangeProperty(ref _fps, value);
		}
		private int _fps;

		private ushort _frameCount;
		private float _elapsedTime;

		public PerformanceModel() => 
			Application.targetFrameRate = 60;

		void ITickable.Tick()
		{
			_frameCount++;
			_elapsedTime += Time.deltaTime;
			
			if (_elapsedTime > FpsUpdateRate)
			{
				Fps = Mathf.RoundToInt(_frameCount / _elapsedTime);
				
				_frameCount = 0;
				_elapsedTime -= FpsUpdateRate;
			}
		}
	}
}