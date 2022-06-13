using System.Collections.Generic;
using System.Threading.Tasks;
using Cinemachine;
using DarkRift.Client.Unity;
using KAG.Shared;
using KAG.Shared.Events;
using KAG.Shared.Json;
using KAG.Shared.Prototype;
using KAG.Unity.Common;
using KAG.Unity.Common.Models;
using KAG.Unity.Network;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KAG.Unity.Scenes
{
    public sealed class BootstrapInstaller : SceneInstaller
    {
        #region Nested types

        private enum Mode
        {
            LocalServer,
            LocalServerViaDocker,
            PlayFab
        }

        #endregion

        private const int DelayBeforeLobbyLoadInMilliseconds = 250;

        [SerializeField]
        [FoldoutGroup("General dependencies")]
        private CinemachineVirtualCamera _virtualCamera;

        [SerializeField]
        [FoldoutGroup("General dependencies")]
        private InputActionAsset _inputs;
        
        [SerializeField]
        [FoldoutGroup("Network settings")]
        private Mode _mode;
        
        [SerializeField]
        [ShowIf(nameof(_mode), Mode.LocalServer)]
        [LabelText("Socket provider")]
        [FoldoutGroup("Network settings")]
        private LocalMatchProvider _localServerNetworkProvider;

        [SerializeField]
        [ShowIf(nameof(_mode), Mode.LocalServer)]
        [LabelText("Process")]
        [FoldoutGroup("Network settings")]
        private LocalConsoleServerProcess _localServerProcess;
        
        [SerializeField]
        [ShowIf(nameof(_mode), Mode.LocalServerViaDocker)]
        [LabelText("Process")]
        [FoldoutGroup("Network settings")]
        private LocalMatchProvider _localServerViaDockerNetworkProvider;
        
        [SerializeField]
        [LabelText("Socket provider")]
        [ShowIf(nameof(_mode), Mode.LocalServerViaDocker)]
        [FoldoutGroup("Network settings")]
        private LocalConsoleServerViaDockerProcess _localServerViaDockerProcess;
        
        [SerializeField]
        [LabelText("Socket provider")]
        [ShowIf(nameof(_mode), Mode.PlayFab)]
        [FoldoutGroup("Network settings")]
        private PlayFabMatchProvider _playfabNetworkProvider;

        public override void InstallBindings()
        {
            Container.BindInstance(_virtualCamera).AsSingle();
            Container.Bind<EventHub>().ToSelf().AsSingle();
            
            InputInstaller.Install(Container, _inputs);
            PersistentMVVMInstaller.Install(Container);
            SimulationFoundationInstaller.Install(Container);
            
            InstallNetworkFoundation();

            Time.fixedDeltaTime = Constants.Time.FixedTimeStep;
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
            var prototypeDefinitionsLoadOperation = new AssetLoadOperation<TextAsset>(UnityConstants.Addressables.PrototypeDefinitionLabel);
            
            await LoadAssets(prototypeDefinitionsLoadOperation);
            
            InitializePresentationRepository(prototypeDefinitionsLoadOperation);
            
            await Task.Delay(DelayBeforeLobbyLoadInMilliseconds);
            
            var applicationModel = Container.Resolve<ApplicationModel>();
            await applicationModel.GoInLobby();
        }

        private void InitializePresentationRepository(AssetLoadOperation<TextAsset> loadOperation)
        {
            var prototypes = new List<Prototype>();
            foreach (var prototypeDefinition in loadOperation.Results)
            {
                var prototype = JsonConvert.DeserializeObject<Prototype>(prototypeDefinition.text, JsonUtilities.StandardSerializerSettings);
                prototypes.Add(prototype);
            }

            var componentTypeRepository = Container.Resolve<ComponentTypeRepository>();
            var prototypeRepository = Container.Resolve<PrototypeRepository>();

            prototypeRepository.Initialize(prototypes, componentTypeRepository);
        }
    }
}
