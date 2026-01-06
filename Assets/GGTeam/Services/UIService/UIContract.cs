using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GGTeam.Services.UIService
{
    [System.Serializable]
    public class UIContract
    {
        [SerializeReference, SuffixLabel(".presenter")] private IPresenter presenter;
        [SerializeField, ShowIf("@presenter != null")] private UIContractConfiguration configuration = new UIContractConfiguration();
        
        public IPresenter Presenter => presenter;
        public UIContractConfiguration Configuration => configuration;

        #if UNITY_EDITOR
        public override string ToString()
        {
            var name = "[ Asset is Empty ]";
            if (configuration.ViewAsset != null && configuration.ViewAsset.RuntimeKeyIsValid()) name = configuration.ViewAsset.editorAsset.name;
            if (presenter == null) name = "[ Presenter is Null ]";
            return name;
        }
        #endif
    }

    [System.Serializable]
    public class UIContractConfiguration
    {
        [SerializeField, AssetSelector, SuffixLabel(".asset")] private AssetReference viewAsset;
        [SerializeField, ShowIf(nameof(AssetIsValid))] private bool unloadOnHide = true;
        
        public bool UnloadOnHide => unloadOnHide;
        public AssetReference ViewAsset => viewAsset;

        private bool AssetIsValid() => viewAsset != null && @viewAsset.RuntimeKeyIsValid();

    }
}