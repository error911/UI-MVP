using System;
using System.Collections.Generic;
using GGTeam.Services.UIService.Settings;
using UnityEngine;

namespace GGTeam.Services.UIService
{
    public class UIService: MonoBehaviour, IUIService
    {
        [Header("Roots")]
        [SerializeField] private Canvas uIRoot;
        [SerializeField] private Transform windowsRoot;
        [SerializeField] private Transform widgetsRoot;

        private readonly Dictionary<string, WindowRegistration> _windowRegistry = new();
        private readonly Dictionary<string, WidgetRegistration> _widgetRegistry = new();

        private ActiveWindow _activeWindow;
        private readonly Dictionary<Guid, ActiveWidget> _activeWidgets = new();

        public Canvas UIRoot => uIRoot;


        #region Регистрация

        public void Register(UIContractSettings uiContractSettings)
        {
            if (uiContractSettings == null)
            {
                Debug.LogError("UI contract settings cannot be null.");
                return;
            }
            
            var contracts = uiContractSettings.GetUIContracts();
            
            if (contracts.Length == 0)
            {
                Debug.LogWarning("UI contract settings cannot be empty.");
                return;
            }
            
            foreach (var uiLink in contracts)
            {
                if (uiLink.Presenter == null || uiLink.ViewPrefab == null)
                {
                    Debug.LogWarning($"Register presenter or view is missing");
                    continue;
                }

                Register(uiLink.Presenter.GetType(), uiLink.ViewPrefab);
                
                // Если использовать ID, то регистрация такая:
                /*if (uiLink.ViewPrefab is UIWindowView)
                    uiService.RegisterWindow(uiLink.Presenter.GetType(), (UIWindowView)uiLink.ViewPrefab, uiLink.Key);
                else if (uiLink.ViewPrefab is UIWidgetView)
                    uiService.RegisterWidget(uiLink.Presenter.GetType(), (UIWidgetView)uiLink.ViewPrefab, uiLink.Key);*/
            }
        }
        
        private void Register(Type presenterType, UIView prefab, string id = null)
        {
            if (prefab as UIWindowView)
            {
                RegisterWindow(presenterType, (UIWindowView)prefab);
            }
            else if (prefab as UIWidgetView)
            {
                RegisterWidget(presenterType, (UIWidgetView)prefab);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="presenterType"></param>
        /// <param name="prefab"></param>
        /// <param name="id">If Null Window registeret by Type</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private void RegisterWindow(Type presenterType, UIWindowView prefab, string id = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                id = presenterType.Name;
            //throw new ArgumentException("Window id is null or empty.", nameof(id));
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));
            if (presenterType == null)
                throw new ArgumentNullException(nameof(presenterType));
            if (!typeof(ITypedPresenterWindow).IsAssignableFrom(presenterType))
                throw new ArgumentException(
                    $"Type {presenterType.Name} must implement {nameof(ITypedPresenterWindow)}.",
                    nameof(presenterType));
            if (presenterType.GetConstructor(Type.EmptyTypes) == null)
                throw new ArgumentException(
                    $"Type {presenterType.Name} must have parameterless constructor.",
                    nameof(presenterType));

            if (_windowRegistry.ContainsKey(id))
                Debug.LogWarning($"Window with id '{id}' already registered.  Replace previous.");
            
            var prototype = (ITypedPresenterWindow)Activator.CreateInstance(presenterType);
            
            _windowRegistry[id] = new WindowRegistration
            {
                Prefab = prefab,
                PresenterFactory = () => (ITypedPresenterWindow)Activator.CreateInstance(presenterType),
                ModelType = prototype.ModelType
            };
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="presenterType"></param>
        /// <param name="prefab"></param>
        /// <param name="id">If Null Widget registeret by Type</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private void RegisterWidget(Type presenterType, UIWidgetView prefab, string id = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                id = presenterType.Name;
            //throw new ArgumentException("Widget id is null or empty.", nameof(id));
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));
            if (presenterType == null)
                throw new ArgumentNullException(nameof(presenterType));
            if (!typeof(ITypedPresenterWidget).IsAssignableFrom(presenterType))
                throw new ArgumentException(
                    $"Type {presenterType.Name} must implement {nameof(ITypedPresenterWidget)}.",
                    nameof(presenterType));
            if (presenterType.GetConstructor(Type.EmptyTypes) == null)
                throw new ArgumentException(
                    $"Type {presenterType.Name} must have parameterless constructor.",
                    nameof(presenterType));
            
            if (_widgetRegistry.ContainsKey(id))
                Debug.LogWarning($"Widget with id '{id}' already registered. Replace previous.");

            var prototype = (ITypedPresenterWidget)Activator.CreateInstance(presenterType);
            _widgetRegistry[id] = new WidgetRegistration
            {
                Prefab = prefab,
                PresenterFactory = () => (ITypedPresenterWidget)Activator.CreateInstance(presenterType),
                ModelType = prototype.ModelType
            };
        }
        
        /*public void RegisterWindowOld<TPresenter>(string id, UIWindowView prefab)
            where TPresenter : ITypedPresenterWindow, new()
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Window id is null or empty.", nameof(id));
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            var prototype = new TPresenter();
            if (!prototype.ViewType.IsAssignableFrom(prefab.GetType()))
            {
                Debug.LogError($"[UIService] Window '{id}': prefab of type '{prefab.GetType().Name}' " +
                               $"is not compatible with presenter view type '{prototype.ViewType.Name}'.");
                return;
            }

            _windowRegistry[id] = new WindowRegistration
            {
                Prefab = prefab,
                PresenterFactory = () => new TPresenter(),
                ModelType = prototype.ModelType
            };
        }*/

        /*public void RegisterWidgetOld<TPresenter>(string id, UIWidgetView prefab)
            where TPresenter : ITypedPresenterWidget, new()
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Widget id is null or empty.", nameof(id));
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            var prototype = new TPresenter();
            if (!prototype.ViewType.IsAssignableFrom(prefab.GetType()))
            {
                Debug.LogError($"[UIService] Widget '{id}': prefab of type '{prefab.GetType().Name}' " +
                               $"is not compatible with presenter view type '{prototype.ViewType.Name}'.");
                return;
            }

            _widgetRegistry[id] = new WidgetRegistration
            {
                Prefab = prefab,
                PresenterFactory = () => new TPresenter(),
                ModelType = prototype.ModelType
            };
        }*/
        

        #endregion

        
        #region Окна

        public void OpenWindow<T>(object model)
        {
            var id = typeof(T).Name;
            OpenWindow(id, model);
        }
        
        public void OpenWindow(string id, object model)
        {
            if (!_windowRegistry.TryGetValue(id, out var registration))
            {
                Debug.LogError($"[UIService] Failed to open Window. Window with id '{id}' is not registered.");
                return;
            }

            if (model != null && !registration.ModelType.IsInstanceOfType(model))
            {
                Debug.LogError($"[UIService] Window '{id}': model type mismatch. Expected '{registration.ModelType.Name}', got '{model.GetType().Name}'.");
                return;
            }

            if (_activeWindow != null)
            {
                CloseWindow(_activeWindow.Id);
            }

            var view = Instantiate(registration.Prefab, windowsRoot);
            var presenter = registration.PresenterFactory();

            presenter.Bind(view);
            presenter.OnOpen(model);

            _activeWindow = new ActiveWindow
            {
                Id = id,
                View = view,
                Presenter = presenter
            };
        }

        public void UpdateWindow(object model)
        {
            if (_activeWindow == null)
            {
                Debug.LogWarning("[UIService] No active window to update.");
                return;
            }

            var presenter = _activeWindow.Presenter;
            if (model != null && !presenter.ModelType.IsInstanceOfType(model))
            {
                Debug.LogError($"[UIService] Window '{_activeWindow.Id}': model type mismatch. Expected '{presenter.ModelType.Name}', got '{model.GetType().Name}'.");
                return;
            }

            presenter.OnUpdate(model);
        }

        public void CloseWindow(string id = null)
        {
            if (_activeWindow == null)
                return;

            if (id != null && _activeWindow.Id != id)
                return;

            _activeWindow.Presenter.OnClose();
            Destroy(_activeWindow.View.gameObject);
            _activeWindow = null;
        }

        #endregion
        
        
        #region Виджеты

        public Guid OpenWidget<T>(object model)
        {
            var id = typeof(T).Name;
            return OpenWidget(id, model);
        }
        
        public Guid OpenWidget(string id, object model)
        {
            if (!_widgetRegistry.TryGetValue(id, out var registration))
            {
                Debug.LogError($"[UIService] Failed to open Widget. Widget with id '{id}' is not registered.");
                return Guid.Empty;
            }

            if (model != null && !registration.ModelType.IsInstanceOfType(model))
            {
                Debug.LogError($"[UIService] Widget '{id}': model type mismatch. Expected '{registration.ModelType.Name}', got '{model.GetType().Name}'.");
                return Guid.Empty;
            }

            var view = Instantiate(registration.Prefab, widgetsRoot);
            var presenter = registration.PresenterFactory();
            presenter.Bind(view);
            presenter.OnOpen(model);

            var guid = Guid.NewGuid();
            _activeWidgets[guid] = new ActiveWidget
            {
                Id = id,
                Guid = guid,
                View = view,
                Presenter = presenter
            };

            return guid;
        }

        public void UpdateWidget(Guid instanceId, object model)
        {
            if (!_activeWidgets.TryGetValue(instanceId, out var widget))
            {
                Debug.LogWarning($"[UIService] Widget instance '{instanceId}' does not exist.");
                return;
            }

            var presenter = widget.Presenter;
            if (model != null && !presenter.ModelType.IsInstanceOfType(model))
            {
                Debug.LogError($"[UIService] Widget '{widget.Id}': model type mismatch. Expected '{presenter.ModelType.Name}', got '{model.GetType().Name}'.");
                return;
            }

            presenter.OnUpdate(model);
        }

        public void CloseWidget(Guid instanceId)
        {
            if (!_activeWidgets.TryGetValue(instanceId, out var widget))
                return;

            widget.Presenter.OnClose();
            Destroy(widget.View.gameObject);
            _activeWidgets.Remove(instanceId);
        }

        #endregion

        
        #region Внутренние структуры

        private sealed class WindowRegistration
        {
            public UIWindowView Prefab;
            public Func<ITypedPresenterWindow> PresenterFactory;
            public Type ModelType;
        }

        private sealed class WidgetRegistration
        {
            public UIWidgetView Prefab;
            public Func<ITypedPresenterWidget> PresenterFactory;
            public Type ModelType;
        }

        private sealed class ActiveWindow
        {
            public string Id;
            public UIWindowView View;
            public ITypedPresenterWindow Presenter;
        }

        private sealed class ActiveWidget
        {
            public string Id;
            public Guid Guid;
            public UIWidgetView View;
            public ITypedPresenterWidget Presenter;
        }

        #endregion
    }
}