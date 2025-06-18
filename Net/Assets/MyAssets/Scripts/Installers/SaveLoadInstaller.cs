using UnityEngine;
using Zenject;

namespace MUSOAR
{
    public class SaveLoadInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SaveController>().FromNew().AsSingle().NonLazy();
        }
    }
}
