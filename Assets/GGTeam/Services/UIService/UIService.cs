using System;
using System.Collections.Generic;
using GGTeam.Services.UIService.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GGTeam.Services.UIService
{
    public class UIService: MonoBehaviour, IUIService
    {
        [Header("Roots")]
        [SerializeField] private Canvas uIRoot;
        [SerializeField] private Transform windowsRoot;
        [SerializeField] private Transform widgetsRoot;

        private readonly Dictionary<Type, WindowRegistration> _windowRegistry = new();
        private readonly Dictionary<Type, WidgetRegistration> _widgetRegistry = new();

        private ActiveWindow _activeWindow;
        private readonly Dictionary<Guid, ActiveWidget> _activeWidgets = new();
        
        private readonly List<Guid> _loadingProcessWidgetsGuids = new List<Guid>();
        private Dictionary<Guid, object> _lateDataUpdateWidgets = new Dictionary<Guid, object>();

        public Canvas UIRoot => uIRoot;


        #region === Register ===

        public void Register(UIContractSettings uiContractSettings)
        {
            if (uiContractSettings == null)
            {
                Debug.LogError("UI contract settings cannot be null.");
                return;
            }
            
            var contractsWindows = uiContractSettings.WindowsContracts();
            var contractsWidgets = uiContractSettings.WidgetsContracts();
            
            RegWindow(contractsWindows);
            RegWidget(contractsWidgets);
        }


        private void RegWindow(UIContract[] contracts)
        {
            foreach (var uiLink in contracts)
            {
                if (uiLink.Presenter == null)
                {
                    Debug.LogWarning($"Register presenter or view is missing");
                    continue;
                }
                
                var asset = uiLink.Configuration.ViewAsset;
                var presenterType = uiLink.Presenter.GetType();
                RegisterWindowAsset(presenterType, asset, uiLink.Configuration.UnloadOnHide);
            }
        }
        
        private void RegWidget(UIContract[] contracts)
        {
            foreach (var uiLink in contracts)
            {
                if (uiLink.Presenter == null)
                {
                    Debug.LogWarning($"Register presenter or view is missing");
                    continue;
                }
                
                var asset = uiLink.Configuration.ViewAsset;
                var presenterType = uiLink.Presenter.GetType();
                RegisterWidgetAsset(presenterType, asset, uiLink.Configuration.UnloadOnHide);
            }
        }
        
        private void RegisterWindowAsset(Type presenterType, AssetReference asset, bool unloadOnHide)
        {
            if (!asset.RuntimeKeyIsValid())
            {
                Debug.LogError("AssetReference не настроен!");
                return;
            }
            
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));
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

            if (_windowRegistry.ContainsKey(presenterType))
                Debug.LogWarning($"Window with id '{presenterType}' already registered.  Replace previous.");
            
            var prototype = (ITypedPresenterWindow)Activator.CreateInstance(presenterType);
            
            _windowRegistry[presenterType] = new WindowRegistration
            {
                AssetReference = asset,
                PresenterFactory = () => (ITypedPresenterWindow)Activator.CreateInstance(presenterType),
                ModelType = prototype.ModelType,
                UnloadOnHide = unloadOnHide
            };
        }
        
        private void RegisterWidgetAsset(Type presenterType, AssetReference asset, bool unloadOnHide)
        {
            if (!asset.RuntimeKeyIsValid())
            {
                Debug.LogError("AssetReference не настроен!");
                return;
            }
            
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));
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
            
            if (_widgetRegistry.ContainsKey(presenterType))
                Debug.LogWarning($"Widget with id '{presenterType}' already registered. Replace previous.");

            var prototype = (ITypedPresenterWidget)Activator.CreateInstance(presenterType);
            _widgetRegistry[presenterType] = new WidgetRegistration
            {
                AssetReference = asset,
                PresenterFactory = () => (ITypedPresenterWidget)Activator.CreateInstance(presenterType),
                ModelType = prototype.ModelType,
                UnloadOnHide = unloadOnHide
            };
        }
        
        #endregion

        
        #region === Windows ===

        public void OpenWindow<T>(object model)
        {
            if (!_windowRegistry.TryGetValue(typeof(T), out var registration))
            {
                Debug.LogError($"[UIService] Failed to open Window. Window with type '{typeof(T)}' is not registered.");
                return;
            }

            if (model != null && !registration.ModelType.IsInstanceOfType(model))
            {
                Debug.LogError($"[UIService] Window '{typeof(T)}': model type mismatch. Expected '{registration.ModelType.Name}', got '{model.GetType().Name}'.");
                return;
            }
            
            if (_activeWindow != null)
            {
                if (_activeWindow.Presenter.GetType() == typeof(T))
                {
                    //Debug.LogWarning("Window is already opened. Use Update only.");
                    //UpdateWindow(model);
                    return;
                }
                CloseWindow();
            }

            LoadWindow(registration, model);
        }


        private void LoadWindow(WindowRegistration registration, object model)
        {
            var assetRef = registration.AssetReference;
            var loadHandle = assetRef.InstantiateAsync(windowsRoot);
            // Создаем замыкание для передачи данных в коллбэк
            loadHandle.Completed += (AsyncOperationHandle<GameObject> handle) => 
            {
                OnWindowLoaded(handle, registration, model);
            };
        }

        private void OnWindowLoaded(AsyncOperationHandle<GameObject> handle, 
            WindowRegistration registration, 
            object model)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject uiInstance = handle.Result;
                var view = uiInstance.GetComponent<UIWindowView>();
        
                if (view != null)
                {
                    var presenter = registration.PresenterFactory();
                    presenter.Bind(view);
                    presenter.OnOpen(model);

                    _activeWindow = new ActiveWindow
                    {
                        View = view,
                        Presenter = presenter,
                        Handle = handle,
                        UnloadOnHide = registration.UnloadOnHide
                    };
                }
                else
                {
                    Debug.LogError("UIView component not found on loaded prefab");
                }
            }
            else
            {
                Debug.LogError($"Ошибка загрузки: {handle.OperationException}");
            }
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
                Debug.LogError($"[UIService] Window view: '{_activeWindow.View.GetType().Name}': model type mismatch presenter: {_activeWindow.Presenter.GetType().Name}. Expected '{presenter.ModelType.Name}', got '{model.GetType().Name}'.");
                return;
            }

            presenter.OnUpdate(model);
        }

        public void CloseWindow()
        {
            if (_activeWindow == null)
                return;

            _activeWindow.Presenter.OnClose();
            
            Destroy(_activeWindow.View.gameObject);
            
            if(_activeWindow.UnloadOnHide)
                Addressables.Release(_activeWindow.Handle);
            
            _activeWindow = null;
        }

        #endregion
        
        
        #region === Widgets ===
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Guid link to Update widget</returns>
        public Guid OpenWidget<T>(object model)
        {
            if (!_widgetRegistry.TryGetValue(typeof(T), out var registration))
            {
                Debug.LogError($"[UIService] Failed to open Widget. Widget with type '{typeof(T)}' is not registered.");
                return Guid.Empty;
            }

            if (model != null && !registration.ModelType.IsInstanceOfType(model))
            {
                Debug.LogError($"[UIService] Widget '{typeof(T)}': model type mismatch. Expected '{registration.ModelType.Name}', got '{model.GetType().Name}'.");
                return Guid.Empty;
            }
            
            var guid = Guid.NewGuid();
            LoadWidget(registration, model, guid);

            return guid;
        }


        private void LoadWidget(WidgetRegistration registration, object model, Guid guid)
        {
            _loadingProcessWidgetsGuids.Add(guid);
            
            var assetRef = registration.AssetReference;
            var loadHandle = assetRef.InstantiateAsync(widgetsRoot);

            // Создаем замыкание для передачи данных в коллбэк
            loadHandle.Completed += (AsyncOperationHandle<GameObject> handle) => 
            {
                OnWidgetLoaded(handle, registration, model, guid);
            };
        }
        
        private void OnWidgetLoaded(AsyncOperationHandle<GameObject> handle, 
            WidgetRegistration registration, 
            object model, Guid guid)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject uiInstance = handle.Result;
                var view = uiInstance.GetComponent<UIWidgetView>();
        
                if (view != null)
                {
                    var presenter = registration.PresenterFactory();
                    presenter.Bind(view);
                    presenter.OnOpen(model);

                    _activeWidgets[guid] = new ActiveWidget
                    {
                        View = view,
                        Presenter = presenter,
                        Handle = handle,
                        UnloadOnHide = registration.UnloadOnHide
                    };

                    if (_lateDataUpdateWidgets.ContainsKey(guid))
                    {
                        // Есть обновление данных
                        var data = _lateDataUpdateWidgets[guid];
                        UpdateWidget(guid, data);
                        _lateDataUpdateWidgets.Remove(guid);
                    }
                }
                else
                {
                    Debug.LogError("UIView component not found on loaded prefab");
                }
            }
            else
            {
                Debug.LogError($"Ошибка загрузки: {handle.OperationException}");
            }
            
            if (_loadingProcessWidgetsGuids.Contains(guid)) // Проверку можно убрать
                _loadingProcessWidgetsGuids.Remove(guid);
        }
        

        public void UpdateWidget(Guid instanceId, object model)
        {
            if (!_activeWidgets.TryGetValue(instanceId, out var widget))
            {
                // UI в процессе загрузки. Отложим обновление
                if (_loadingProcessWidgetsGuids.Contains(instanceId))
                {
                    _lateDataUpdateWidgets.Add(instanceId, model);
                    return;
                }
                Debug.LogWarning($"[UIService] Widget instance '{instanceId}' does not exist.");
                return;
            }

            var presenter = widget.Presenter;
            if (model != null && !presenter.ModelType.IsInstanceOfType(model))
            {
                Debug.LogError($"[UIService] Widget view:'{widget.View.GetType().Name}' model type mismatch presenter: {widget.Presenter.GetType().Name}. Expected '{presenter.ModelType.Name}', got '{model.GetType().Name}'.");
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
            
            if(widget.UnloadOnHide)
                Addressables.Release(widget.Handle);
                
            _activeWidgets.Remove(instanceId);
        }

        #endregion

        
        #region === Internal structures ===

        private sealed class WindowRegistration
        {
            public AssetReference AssetReference;
            public Func<ITypedPresenterWindow> PresenterFactory;
            public Type ModelType;
            public bool UnloadOnHide;
        }

        private sealed class WidgetRegistration
        {
            public AssetReference AssetReference;
            public Func<ITypedPresenterWidget> PresenterFactory;
            public Type ModelType;
            public bool UnloadOnHide;
        }

        private sealed class ActiveWindow
        {
            public UIWindowView View;
            public ITypedPresenterWindow Presenter;
            
            public AsyncOperationHandle<GameObject> Handle;
            public bool UnloadOnHide;
        }

        private sealed class ActiveWidget
        {
            public UIWidgetView View;
            public ITypedPresenterWidget Presenter;
            
            public AsyncOperationHandle<GameObject> Handle;
            public bool UnloadOnHide;
        }

        #endregion
    }
}