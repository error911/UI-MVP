using Sirenix.OdinInspector;
using UnityEngine;

namespace GGTeam.Services.UIService
{
    [System.Serializable]
    public class UIContract
    {
        [SerializeReference, SuffixLabel(".presenter")] private IPresenter presenter;
        [SerializeField, SuffixLabel(".prefab")] private UIView viewViewPrefab;
        
        public IPresenter Presenter => presenter;
        public UIView ViewPrefab => viewViewPrefab;
    }
}