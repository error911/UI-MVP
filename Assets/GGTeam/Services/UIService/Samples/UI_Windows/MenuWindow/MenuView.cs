using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GGTeam.Services.UIService.Samples
{
    public class MenuView : UIWindowView
    {
        [SerializeField] private TMP_Text captionText;
        [SerializeField] private Button closeBtn;
        
        public void SetCaption(string message) => captionText.text = message;
        public void SetCloseBtn(UnityAction onClose) => closeBtn.onClick.AddListener(onClose);
    }
}