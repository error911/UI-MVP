using UnityEngine;

namespace GGTeam.Services.UIService
{
    public interface IUIView
    {
        void Show();
        void Hide();
    }

    public interface IWindowView : IUIView { }
    public interface IWidgetView : IUIView { }

    public abstract class UIView : MonoBehaviour, IUIView
    {
        public virtual void Show() => gameObject.SetActive(true);
        public virtual void Hide() => gameObject.SetActive(false);
    }

    public abstract class UIWindowView : UIView, IWindowView { }
    public abstract class UIWidgetView : UIView, IWidgetView { }
}