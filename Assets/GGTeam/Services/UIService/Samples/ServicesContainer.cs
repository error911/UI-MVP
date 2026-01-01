using GGTeam.Services.UIService.Settings;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GGTeam.Services.UIService.Samples
{
    public class ServicesContainer : MonoBehaviour
    {
        [SerializeField] private UIService uiService;
        [SerializeField, AssetSelector] private UIContractSettings _uiContractSettings;

        private void Awake()
        {
            RegisterUI();
        }

        private void RegisterUI()
        {
            uiService.Register(_uiContractSettings);
        }
    }
}