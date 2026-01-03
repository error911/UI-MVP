using System;
using UnityEngine;

namespace GGTeam.Services.UIService
{
    public abstract class UIPresenterWidget<TView, TModel> : /*IWidgetPresenter, */ITypedPresenterWidget
        where TView : class, IWidgetView
    {
        protected TView View { get; private set; }

        Type ITypedPresenterWidget.ViewType => typeof(TView);
        Type ITypedPresenterWidget.ModelType => typeof(TModel);

        public void Bind(IWidgetView view)
        {
            if (view is not TView typedView)
                throw new ArgumentException($"Expected view of type {typeof(TView).Name}, got {view.GetType().Name}");
            View = typedView;
            OnBind();
        }

        protected virtual void OnBind() { }

        public void OnOpen(object model)
        {
            ValidateModel(model);
            View?.Show();
            OnOpen((TModel)model);
        }

        public void OnUpdate(object model)
        {
            ValidateModel(model);
            OnUpdate((TModel)model);
        }

        public void OnClose()
        {
            OnClosed();
            View?.Hide();
        }

        protected virtual void OnOpen(TModel model) { }
        protected virtual void OnUpdate(TModel model) { }
        protected virtual void OnClosed() { }

        private static void ValidateModel(object model)
        {
            if (model != null && model is not TModel)
            {
                throw new ArgumentException(
                    $"Model must be of type {typeof(TModel).Name}, got {model.GetType().Name}");
            }
        }
    }
}