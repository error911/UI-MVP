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
        private UIContract[] uiWindowsContracts;
        
        [SerializeField]
        [ListDrawerSettings(/*ListElementLabelName = "@Key",*/ DraggableItems = false, ShowPaging = false, DefaultExpandedState = true, ListElementLabelName = "@ToString()")]
        private UIContract[] uiWidgetsContracts;

        public UIContract[] WindowsContracts() => uiWindowsContracts;
        public UIContract[] WidgetsContracts() => uiWidgetsContracts;


        #if UNITY_EDITOR
        [Button("Validate")]
        private void ValidateEditor()
        {
            Debug.ClearDeveloperConsole();
            
          //  if (uiWindowsContracts == null) Assert.IsNotNull(uiWindowsContracts);
          //  if (uiWidgetsContracts == null) Assert.IsNotNull(uiWidgetsContracts);
          //  Assert.IsFalse(uiWindowsContracts.Length == 0, "No UI contracts defined.");
          //  Assert.IsFalse(uiWidgetsContracts.Length == 0, "No UI contracts defined.");

            if (uiWindowsContracts != null)
            foreach (var contract in uiWindowsContracts)
            {
                Assert.IsNotNull(contract.Presenter, "No presenter found.");
                Assert.IsNotNull(contract.ViewPrefab, "No view prefab defined.");
            }
            
            if (uiWidgetsContracts != null)
            foreach (var contract in uiWidgetsContracts)
            {
                Assert.IsNotNull(contract.Presenter, "No presenter found.");
                Assert.IsNotNull(contract.ViewPrefab, "No view prefab defined.");
            }
            
            
            Debug.Log("Successful.");
        }
        #endif
    }
}