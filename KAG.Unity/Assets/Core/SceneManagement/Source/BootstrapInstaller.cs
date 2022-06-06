using DarkRift.Client.Unity;
using KAG.Unity.Common;
using KAG.Unity.Common.Models;
using KAG.Unity.Network;
using Sirenix.OdinInspector;
using UnityEngine;
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

        private new void Start()
        {
            var applicationModel = Container.Resolve<ApplicationModel>();
            applicationModel.GoInLobby();
        }
    }
}
