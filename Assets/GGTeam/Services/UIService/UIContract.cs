using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace GGTeam.Services.UIService
{
    [System.Serializable]
    public class UIContract
    {
        [SerializeReference, SuffixLabel(".presenter")] private IPresenter presenter;
        [SerializeField, SuffixLabel(".prefab")] private UIView viewPrefab;
        [SerializeField, AssetSelector, SuffixLabel(".prefab")] private AssetReference viewAsset;
        
        public IPresenter Presenter => presenter;
        public UIView ViewPrefab => viewPrefab;
        public AssetReference ViewAsset => viewAsset;

        public override string ToString()
        {
            var name = "[ NULL ]";
            if (viewPrefab != null) name = viewPrefab.name;
            if (presenter == null) name = "[ ERROR: Presenter is Null ]";
            return name;
        }
    }
}