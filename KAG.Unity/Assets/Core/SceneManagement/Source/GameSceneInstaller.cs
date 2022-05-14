using KAG.Unity.Network;
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
        private LocalNetworkSocketProvider _localServerNetworkProvider;

        [SerializeField, LabelText("Process"), ShowIf(nameof(_mode), Mode.LocalServer)]
        private LocalConsoleServerProcess _localServerProcess;
        
        [SerializeField, LabelText("Process"), ShowIf(nameof(_mode), Mode.LocalServerViaDocker)]
        private LocalNetworkSocketProvider _localServerViaDockerNetworkProvider;
        
        [SerializeField, LabelText("Socket provider"), ShowIf(nameof(_mode), Mode.LocalServerViaDocker)]
        private LocalConsoleServerViaDockerProcess _localServerViaDockerProcess;
        
        [SerializeField, LabelText("Socket provider"), ShowIf(nameof(_mode), Mode.PlayFab)]
        private PlayFabNetworkSocketProvider _playfabNetworkProvider;
        
        public override void InstallBindings()
        {
            
        }
    }
}
