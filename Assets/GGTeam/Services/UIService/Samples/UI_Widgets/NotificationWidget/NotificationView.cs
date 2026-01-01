using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GGTeam.Services.UIService.Samples
{
    public class NotificationView : UIWidgetView
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button closeBtn;

        public void SetMessage(string message) => messageText.text = message;
        public void SetCloseBtn(UnityAction onClick) => closeBtn.onClick.AddListener(onClick);
    }
}