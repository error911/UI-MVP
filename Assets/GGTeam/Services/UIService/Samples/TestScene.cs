using System;
using System.Collections.Generic;
using UnityEngine;

namespace GGTeam.Services.UIService.Samples
{
    public class TestScene : MonoBehaviour
    {
        [SerializeField] private UIService uiService;
        
        private Queue<Guid> _popugGuids = new ();
        private int cnt = 0;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                OpenTestWindow();
            if (Input.GetKeyDown(KeyCode.Alpha2))
                OpenTestWindow2();
            if (Input.GetKeyDown(KeyCode.Alpha3))
                OpenTestWidget();
        }
        
        
        private void OpenTestWindow()
        {
            uiService.OpenWindow<SettingsWindowPresenter>(new SettingsModel()
            {
                Caption = "SETTINGS Window",
                OnCloseClick = () => { Debug.Log("CLICK"); uiService.CloseWindow(); },
                OnPopupWidgetClick = OpenTestWidget,
            });
            
            // Если использовать ID то вот так:
            /*uiService.OpenWindow("Settings", new SettingsModel()
            {
                Caption = "SETTINGS Window",
                OnCloseClick = () => { Debug.Log("CLICK"); uiService.CloseWindow(); },
                OnPopupWidgetClick = OpenTestWidget,
            });*/
        }
        
        private void OpenTestWindow2()
        {
            uiService.OpenWindow<MenuWindowPresenter>(new MenuModel()
            {
                Caption = "Menu Window",
                OnClick = () => uiService.CloseWindow()
            });
        }

        private void OpenTestWidget()
        {
            var widgetId = uiService.OpenWidget<NotificationWidgetPresenter>(new NotificationModel
            {
                Message = "Hello!",
                OnClick = OnBtnWidgetClick
            });

            // Пример обновления виджета
            cnt++;
            uiService.UpdateWidget(widgetId, new NotificationModel
            {
                Message = $"Ты открыл меня {cnt} раз.\r\n"
                +$"Одновременно открыто: {_popugGuids.Count+1}"
            });

            _popugGuids.Enqueue(widgetId);
        }

        private void OnBtnWidgetClick()
        {
            Debug.Log("Widget btn Click");
            var guid = _popugGuids.Dequeue();
            uiService.CloseWidget(guid);
        }
    }
}