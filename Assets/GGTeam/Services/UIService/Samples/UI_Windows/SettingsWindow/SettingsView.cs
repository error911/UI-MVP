using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GGTeam.Services.UIService.Samples
{
    public class SettingsView : UIWindowView
    {
        [SerializeField] private TMP_Text captionText;
        [SerializeField] private Button popupBtn;
        [SerializeField] private Button closeBtn;
        
        public void SetCaption(string message) => captionText.text = message;
        public void SetCloseBtn(UnityAction onClick) => closeBtn.onClick.AddListener(onClick);
        public void SetPopupBtn(UnityAction onClick) => popupBtn.onClick.AddListener(onClick);
    }
}