using UnityEngine;
using Zenject;

namespace MUSOAR
{
    public class GameInstaller : MonoInstaller
    {

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InputManager>().FromNew().AsSingle().NonLazy(); 
            Container.BindInterfacesAndSelfTo<CursorController>().FromNew().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SaveController>().FromNew().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<LevelsManager>().FromNew().AsSingle().Lazy();
            Container.BindInterfacesAndSelfTo<TotalPlayTimeController>().FromNew().AsSingle().Lazy();
            Container.Bind<BlackScreen>().FromComponentInHierarchy().AsSingle().Lazy();
            Container.Bind<LevelTransitionWindow>().FromComponentInHierarchy().AsSingle().Lazy();

        }
    }
}

