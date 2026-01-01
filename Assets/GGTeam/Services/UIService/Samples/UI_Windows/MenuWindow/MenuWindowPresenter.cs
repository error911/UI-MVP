namespace GGTeam.Services.UIService.Samples
{
    public class MenuWindowPresenter : UIPresenterWindow<MenuView, MenuModel>
    {
        protected override void OnOpen(MenuModel model)
        {
            base.OnOpen(model);
            View.SetCaption(model.Caption);
            View.SetCloseBtn(model.OnClick.Invoke);
        }
    }
}