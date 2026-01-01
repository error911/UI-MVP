namespace GGTeam.Services.UIService.Samples
{
    public class SettingsWindowPresenter : UIPresenterWindow<SettingsView, SettingsModel>
    {
        protected override void OnOpen(SettingsModel model)
        {
            base.OnOpen(model);
            View.SetCaption(model.Caption);
            View.SetCloseBtn(model.OnCloseClick.Invoke);
            View.SetPopupBtn(model.OnPopupWidgetClick.Invoke);
        }
    }
}