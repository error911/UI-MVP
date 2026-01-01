namespace GGTeam.Services.UIService.Samples
{
    public class NotificationWidgetPresenter : UIPresenterWidget<NotificationView, NotificationModel>
    {
        protected override void OnOpen(NotificationModel model)
        {
            View.SetMessage(model.Message);
            View.SetCloseBtn(model.OnClick.Invoke);
        }

        protected override void OnUpdate(NotificationModel model)
        {
            View.SetMessage(model.Message);
        }
    }
}