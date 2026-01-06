using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;

namespace GGTeam.Services.UIService.Settings
{
    [CreateAssetMenu(fileName = nameof(UIContractSettings), menuName = "Game/UI/"+nameof(UIContractSettings), order = 100)]
    public class UIContractSettings : ScriptableObject
    {
        [SerializeField]
        [ListDrawerSettings(DraggableItems = false, ShowPaging = false, DefaultExpandedState = true, ListElementLabelName = "@ToString()")]
        private UIContract[] windowsContracts;
        
        [SerializeField]
        [ListDrawerSettings(DraggableItems = false, ShowPaging = false, DefaultExpandedState = true, ListElementLabelName = "@ToString()")]
        private UIContract[] widgetsContracts;

        public UIContract[] WindowsContracts() => windowsContracts;
        public UIContract[] WidgetsContracts() => widgetsContracts;


        #if UNITY_EDITOR
        [Button("Validate")]
        private void ValidateEditor()
        {
          if (windowsContracts != null)
          {
              foreach (var contract in windowsContracts)
              {
                  Assert.IsNotNull(contract.Presenter, "No presenter found.");
                  Assert.IsTrue(contract.Configuration.ViewAsset.IsValid());
              }
          }
            
          if (widgetsContracts != null)
          {
              foreach (var contract in widgetsContracts)
              {
                  Assert.IsNotNull(contract.Presenter, "No presenter found.");
                  Assert.IsTrue(contract.Configuration.ViewAsset.RuntimeKeyIsValid());
              }
          } 
          
          Debug.Log("Check complete.");
        }
        #endif
    }
}