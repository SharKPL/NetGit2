using Zenject;

namespace MUSOAR
{
    public class MenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // Other
            Container.Bind<PauseManager>().FromComponentInHierarchy().AsSingle().Lazy();
            Container.Bind<MenuManager>().FromComponentInHierarchy().AsSingle().Lazy();
            Container.Bind<CreditsWindow>().FromComponentInHierarchy().AsSingle().Lazy();
            Container.Bind<MultiplayerWindow>().FromComponentInHierarchy().AsSingle().Lazy();
            Container.Bind<SingleplayerWindow>().FromComponentInHierarchy().AsSingle().Lazy();
            // Settings
            Container.Bind<SettingsWindow>().FromComponentInHierarchy().AsSingle().Lazy();
            Container.Bind<SettingsScreenWindow>().FromComponentInHierarchy().AsSingle().Lazy();
            Container.Bind<SettingsAudioWindow>().FromComponentInHierarchy().AsSingle().Lazy();
            Container.Bind<SettingsGraphicsWindow>().FromComponentInHierarchy().AsSingle().Lazy();
            Container.Bind<SettingsControlWindow>().FromComponentInHierarchy().AsSingle().Lazy();
            // SaveLoad
            Container.Bind<SaveLoadWindow>().FromComponentInHierarchy().AsSingle().Lazy();
        }
    }
}

