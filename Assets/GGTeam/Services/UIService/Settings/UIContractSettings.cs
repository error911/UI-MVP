using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;

namespace GGTeam.Services.UIService.Settings
{
    [CreateAssetMenu(fileName = nameof(UIContractSettings), menuName = "Game/UI/"+nameof(UIContractSettings), order = 100)]
    public class UIContractSettings : ScriptableObject
    {
        [SerializeField]
        [ListDrawerSettings(/*ListElementLabelName = "@Key",*/ DraggableItems = false, ShowPaging = false, DefaultExpandedState = true, ListElementLabelName = "@ToString()")]
        private UIContract[] uiContracts;

        public UIContract[] GetUIContracts() => uiContracts;


        #if UNITY_EDITOR
        [Button("Validate")]
        private void ValidateEditor()
        {
            Debug.ClearDeveloperConsole();
            
            if (uiContracts == null) Assert.IsNotNull(uiContracts);
            Assert.IsFalse(uiContracts.Length == 0, "No UI contracts defined.");

            foreach (var contract in uiContracts)
            {
                Assert.IsNotNull(contract.Presenter, "No presenter found.");
                Assert.IsNotNull(contract.ViewPrefab, "No view prefab defined.");
            }
            Debug.Log("Successful.");
        }
        #endif
    }
}