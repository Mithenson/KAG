using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DarkRift.Client.Unity;
using KAG.Shared;
using KAG.Shared.Json;
using KAG.Shared.Prototype;
using KAG.Unity.Common;
using KAG.Unity.Common.Models;
using KAG.Unity.Network;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace KAG.Unity.SceneManagement
{
    public sealed class BootstrapInstaller : MonoInstaller
    {
        #region Nested types

        private enum Mode
        {
            LocalServer,
            LocalServerViaDocker,
            PlayFab
        }

        #endregion

        private const int AssetLoadPollingIntervalInMilliseconds = 500;
        private const int DelayBeforeAssetLoadInMilliseconds = 500;
        private const int DelayBeforeLobbyLoadInMilliseconds = 500;
        private const string PrototypeDefinitionsAddressableLabel = "prototype_definition";
        
        [SerializeField]
        private Mode _mode;
        
        [SerializeField]
        [ShowIf(nameof(_mode), Mode.LocalServer)]
        [LabelText("Socket provider")]
        private LocalMatchProvider _localServerNetworkProvider;

        [SerializeField]
        [ShowIf(nameof(_mode), Mode.LocalServer)]
        [LabelText("Process")]
        private LocalConsoleServerProcess _localServerProcess;
        
        [SerializeField]
        [ShowIf(nameof(_mode), Mode.LocalServerViaDocker)]
        [LabelText("Process")]
        private LocalMatchProvider _localServerViaDockerNetworkProvider;
        
        [SerializeField]
        [LabelText("Socket provider")]
        [ShowIf(nameof(_mode), Mode.LocalServerViaDocker)]
        private LocalConsoleServerViaDockerProcess _localServerViaDockerProcess;
        
        [SerializeField]
        [LabelText("Socket provider")]
        [ShowIf(nameof(_mode), Mode.PlayFab)]
        private PlayFabMatchProvider _playfabNetworkProvider;

        private ApplicationModel _applicationModel;
        private List<float> _assetLoadingProgresses;
        
        public override void InstallBindings()
        {
            PersistentMVVMInstaller.Install(Container);
            SimulationFoundationInstaller.Install(Container);
            
            InstallNetworkFoundation();
        }

        private void InstallNetworkFoundation()
        {
            switch (_mode)
            {
                case Mode.LocalServer:
                    Container.BindInterfacesAndSelfTo<LocalMatchProvider>().FromInstance(_localServerNetworkProvider).AsSingle();

                    #if UNITY_EDITOR
                    
                    Container.Bind<TrackedProcess>().WithId(InjectionKey.LocalServerProcess).FromInstance(_localServerProcess).AsSingle();
                    Container.QueueForInject(_localServerNetworkProvider);
                    _localServerProcess.Start();
                    
                    #endif
                    break;

                case Mode.LocalServerViaDocker:
                    Container.BindInterfacesAndSelfTo<LocalMatchProvider>().FromInstance(_localServerViaDockerNetworkProvider).AsSingle();

                    #if UNITY_EDITOR
                    
                    Container.Bind<TrackedProcess>().WithId(InjectionKey.LocalServerProcess).FromInstance(_localServerViaDockerProcess).AsSingle();
                    Container.QueueForInject(_localServerViaDockerNetworkProvider);
                    _localServerViaDockerProcess.Start();
                    
                    #endif
                    break;

                case Mode.PlayFab:
                    Container.BindInterfacesAndSelfTo<PlayFabMatchProvider>().FromInstance(_playfabNetworkProvider).AsSingle();
                    break;
            }
            
            Container.Bind<UnityClient>().FromComponentOn(gameObject).AsSingle();
        }
        
        private new async void Start()
        {
            _applicationModel = Container.Resolve<ApplicationModel>();
            _assetLoadingProgresses = new List<float>();

            _applicationModel.LoadingProgress = 0.0f;
            _applicationModel.LoadingDescription = "Loading assets";

            await Task.Delay(DelayBeforeAssetLoadInMilliseconds);
            await Task.WhenAll(InitializePrototypeRepository());
            
            await Task.Delay(DelayBeforeLobbyLoadInMilliseconds);
            await _applicationModel.GoInLobby();
        }

        private async Task InitializePrototypeRepository()
        {
            var loadProgressIndex = _assetLoadingProgresses.Count;
            _assetLoadingProgresses.Add(0.0f);
            
            var prototypes = new List<Prototype>();
            
            var handle = Addressables.LoadAssetsAsync<TextAsset>(
                PrototypeDefinitionsAddressableLabel,
                prototypeDefinition =>
                {
                    var prototype = JsonConvert.DeserializeObject<Prototype>(prototypeDefinition.text, JsonUtilities.StandardSerializerSettings);
                    prototypes.Add(prototype);
                });

            while (!handle.IsDone)
            {
                await Task.Delay(AssetLoadPollingIntervalInMilliseconds);
                SetAssetLoadingProgress(loadProgressIndex, handle.PercentComplete);
            }
            SetAssetLoadingProgress(loadProgressIndex, 1.0f);
            
            var componentTypeRepository = Container.Resolve<ComponentTypeRepository>();
            var prototypeRepository = Container.Resolve<PrototypeRepository>();
            
            prototypeRepository.Initialize(prototypes, componentTypeRepository);
        }

        private void SetAssetLoadingProgress(int index, float value)
        {
            _assetLoadingProgresses[index] = value;
            _applicationModel.LoadingProgress = _assetLoadingProgresses.Min();
        }
    }
}
