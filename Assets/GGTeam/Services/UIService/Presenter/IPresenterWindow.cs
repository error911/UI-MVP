using System;

namespace GGTeam.Services.UIService
{
    public interface IPresenterWindow : IPresenter
    {
        void Bind(IWindowView view);
        void OnOpen(object model);
        void OnUpdate(object model);
        void OnClose();
    }
    
    public interface ITypedPresenterWindow : IPresenterWindow
    {
        Type ViewType { get; }
        Type ModelType { get; }
    }

    
}