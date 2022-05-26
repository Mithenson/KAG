using KAG.Unity.Common.Models;
using KAG.Unity.Network;
using KAG.Unity.UI.ViewModels;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace KAG.Unity.SceneManagement
{
    public class GameSceneInstaller : MonoInstaller
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

        [SerializeField, LabelText("Socket provider"), ShowIf(nameof(_mode), Mode.LocalServer)]
        private LocalMatchProvider _localServerNetworkProvider;

        [SerializeField, LabelText("Process"), ShowIf(nameof(_mode), Mode.LocalServer)]
        private LocalConsoleServerProcess _localServerProcess;
        
        [SerializeField, LabelText("Process"), ShowIf(nameof(_mode), Mode.LocalServerViaDocker)]
        private LocalMatchProvider _localServerViaDockerNetworkProvider;
        
        [SerializeField, LabelText("Socket provider"), ShowIf(nameof(_mode), Mode.LocalServerViaDocker)]
        private LocalConsoleServerViaDockerProcess _localServerViaDockerProcess;
        
        [SerializeField, LabelText("Socket provider"), ShowIf(nameof(_mode), Mode.PlayFab)]
        private PlayFabMatchProvider _playfabNetworkProvider;
        
        public override void InstallBindings()
        {
            // Global
            Container.BindInterfacesAndSelfTo<ApplicationModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerModel>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<ApplicationViewModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<SettingsViewModel>().AsSingle();
            
            // Local
            Container.BindInterfacesAndSelfTo<JoinMatchModel>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<JoinMatchViewModel>().AsSingle();
            
            // Game
            Container.BindInterfacesAndSelfTo<NetworkManager>().AsSingle();
        }
    }
}
