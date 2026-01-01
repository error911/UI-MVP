using System;

namespace GGTeam.Services.UIService
{
    public interface IPresenterWidget : IPresenter
    {
        void Bind(IWidgetView view);
        void OnOpen(object model);
        void OnUpdate(object model);
        void OnClose();
    }
    
    public interface ITypedPresenterWidget : IPresenterWidget
    {
        Type ViewType { get; }
        Type ModelType { get; }
    }
}