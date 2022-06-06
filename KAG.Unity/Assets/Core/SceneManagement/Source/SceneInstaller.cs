using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KAG.Unity.Common.Models;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;
using Object = UnityEngine.Object;

namespace KAG.Unity.SceneManagement
{
	public abstract class SceneInstaller : MonoInstaller
	{
		#region Nested types

		protected readonly struct LoadProgressHandle
		{
			private readonly ApplicationModel _applicationModel;
			private readonly List<float> _progresses;
			private readonly int _index;

			public LoadProgressHandle(ApplicationModel applicationModel, List<float> progresses)
			{
				_applicationModel = applicationModel;
				_progresses = progresses;
                
				_index = progresses.Count;
				progresses.Add(0.0f);
			}

			public void Set(float value)
			{
				_progresses[_index] = value;
				_applicationModel.LoadingProgress = _progresses.Min();
			}
		}
        
		protected abstract class AssetLoadOperation
		{
			public abstract Task Load(LoadProgressHandle progressHandle);
		}
		protected sealed class AssetLoadOperation<TObject> : AssetLoadOperation
			where TObject : Object
		{
			public IList<TObject> Results => 
				_handle.Result;
            
			private string _label;
			private Action<TObject> _onResolve;
			private Action<IList<TObject>> _onComplete;
			private AsyncOperationHandle<IList<TObject>> _handle;
            
			public AssetLoadOperation(string label)
			{
				_label = label;
				_onResolve = _ => { };
				_onComplete = _ => { };
			}
			public AssetLoadOperation(string label, Action<TObject> onResolve)
			{
				_label = label;
				_onResolve = onResolve;
				_onComplete = _ => { };
			}
			public AssetLoadOperation(string label, Action<IList<TObject>> onComplete)
			{
				_label = label;
				_onResolve = _ => { };
				_onComplete = onComplete;
			}
			public AssetLoadOperation(string label, Action<TObject> onResolve, Action<IList<TObject>> onComplete)
			{
				_label = label;
				_onResolve = onResolve;
				_onComplete = onComplete;
			}

			public override async Task Load(LoadProgressHandle progressHandle)
			{
				_handle = Addressables.LoadAssetsAsync<TObject>(_label, _onResolve);
				while (!_handle.IsDone)
				{
					await Task.Delay(AssetLoadPollingIntervalInMilliseconds);
					progressHandle.Set(_handle.PercentComplete);
				}
                
				progressHandle.Set(1.0f);
				_onComplete(_handle.Result);
			}
		}

		#endregion
        
		private const int AssetLoadPollingIntervalInMilliseconds = 500;
		private const int DelayBeforeAssetLoadInMilliseconds = 500;

		protected async Task LoadAssets(params AssetLoadOperation[] operations)
		{
			await Task.Delay(DelayBeforeAssetLoadInMilliseconds);
            
			var loadProgresses = new List<float>();
			var applicationModel = Container.Resolve<ApplicationModel>();
            
			applicationModel.LoadingProgress = 0.0f;
			applicationModel.LoadingDescription = "Loading assets";

			var tasks = new Task[operations.Length];
			for (var i = 0; i < operations.Length; i++)
			{
				var progressHandle = new LoadProgressHandle(applicationModel, loadProgresses);
				tasks[i] = operations[i].Load(progressHandle);
			}
            
			await Task.WhenAll(tasks);
		}
	}
}