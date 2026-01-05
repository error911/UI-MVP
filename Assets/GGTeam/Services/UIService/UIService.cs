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
            
            /*if (contractsWindows.Length == 0)
            {
                Debug.LogWarning("UI contract settings cannot be empty.");
                return;
            }*/
            
            /*
            foreach (var uiLink in contractsWindows)
            {
                if (uiLink.Presenter == null || uiLink.ViewPrefab == null)
                {
                    Debug.LogWarning($"Register presenter or view is missing");
                    continue;
                }
                
                var prefab = uiLink.ViewPrefab;
                var asset = uiLink.ViewAsset;
                var presenterType = uiLink.Presenter.GetType();

                //Debug.Log("!>> 0> " + asset.GetType());
                //if (asset.GetType() == typeof(UIWindowView))
                //{
                //    Debug.Log("!>> 1");
                //}
                //else if (asset.Asset as UIWidgetView)
                //{
                //    Debug.Log("!>> 2");
                //}
                
                if (prefab as UIWindowView)
                {
//                    RegisterWindow(presenterType, (UIWindowView)prefab);
                    RegisterWindowAsset(presenterType, asset);
                }
                else if (prefab as UIWidgetView)
                {
//                    RegisterWidget(presenterType, (UIWidgetView)prefab);
                    RegisterWidgetAsset(presenterType, asset);
                }
            }
            */
        }


        private void RegWindow(UIContract[] contracts)  //UIWindowView
        {
            foreach (var uiLink in contracts)
            {
                if (uiLink.Presenter == null/* || uiLink.ViewPrefab == null*/)
                {
                    Debug.LogWarning($"Register presenter or view is missing");
                    continue;
                }
                
                //var prefab = uiLink.ViewPrefab;
                var asset = uiLink.ViewAsset;
                var presenterType = uiLink.Presenter.GetType();
                RegisterWindowAsset(presenterType, asset);
            }
        }
        
        private void RegWidget(UIContract[] contracts)  //UIWidgetView
        {
            foreach (var uiLink in contracts)
            {
                if (uiLink.Presenter == null/* || uiLink.ViewPrefab == null*/)
                {
                    Debug.LogWarning($"Register presenter or view is missing");
                    continue;
                }
                
                //var prefab = uiLink.ViewPrefab;
                var asset = uiLink.ViewAsset;
                var presenterType = uiLink.Presenter.GetType();
                RegisterWidgetAsset(presenterType, asset);
            }
        }
        
        
        /*
        private void RegisterWindow(Type presenterType, UIWindowView prefab)
        {
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

            if (_windowRegistry.ContainsKey(presenterType))
                Debug.LogWarning($"Window with id '{presenterType}' already registered.  Replace previous.");
            
            var prototype = (ITypedPresenterWindow)Activator.CreateInstance(presenterType);
            
            _windowRegistry[presenterType] = new WindowRegistration
            {
                Prefab = prefab,
                PresenterFactory = () => (ITypedPresenterWindow)Activator.CreateInstance(presenterType),
                ModelType = prototype.ModelType
            };
        }
        */
        
        private void RegisterWindowAsset(Type presenterType, AssetReference asset) //UIWindowView
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
//                Prefab = prefab,
                PresenterFactory = () => (ITypedPresenterWindow)Activator.CreateInstance(presenterType),
                ModelType = prototype.ModelType
            };
        }
        

        /*private void RegisterWidget(Type presenterType, UIWidgetView prefab)
        {
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
            
            if (_widgetRegistry.ContainsKey(presenterType))
                Debug.LogWarning($"Widget with id '{presenterType}' already registered. Replace previous.");

            var prototype = (ITypedPresenterWidget)Activator.CreateInstance(presenterType);
            _widgetRegistry[presenterType] = new WidgetRegistration
            {
                Prefab = prefab,
                PresenterFactory = () => (ITypedPresenterWidget)Activator.CreateInstance(presenterType),
                ModelType = prototype.ModelType
            };
        }*/
        
        private void RegisterWidgetAsset(Type presenterType, AssetReference asset) //UIWidgetView prefab
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
//                Prefab = prefab,
                PresenterFactory = () => (ITypedPresenterWidget)Activator.CreateInstance(presenterType),
                ModelType = prototype.ModelType
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

ReadyToSpawnWindow(registration, model);
            
//Debug.Log("!>>!!! " + registration.Prefab.GetType());
//            var view = Instantiate(registration.Prefab, windowsRoot);
            
            
//            var loadHandle = registration.AssetReference.InstantiateAsync();
            //loadHandle.Completed += OnUILoaded;
            
//            var v2 = LoadWindowAsset(registration.AssetReference, windowsRoot);
            
            /*
            var presenter = registration.PresenterFactory();
            presenter.Bind(view);
            presenter.OnOpen(model);

            _activeWindow = new ActiveWindow
            {
                View = view,
                Presenter = presenter
            };
            */
        }


        private void ReadyToSpawnWindow(WindowRegistration registration, object model)
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
                        Presenter = presenter
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
            ReadyToSpawnWidget(registration, model, guid);

            /*var view = Instantiate(registration.Prefab, widgetsRoot);
            var presenter = registration.PresenterFactory();
            presenter.Bind(view);
            presenter.OnOpen(model);

            var guid = Guid.NewGuid();
            _activeWidgets[guid] = new ActiveWidget
            {
                View = view,
                Presenter = presenter
            };*/

            return guid;
        }

        private void ReadyToSpawnWidget(WidgetRegistration registration, object model, Guid guid)
        {
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
                        Presenter = presenter
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
            _activeWidgets.Remove(instanceId);
        }

        #endregion

        
        #region === Internal structures ===

        private sealed class WindowRegistration
        {
            public AssetReference AssetReference;
//            public UIWindowView Prefab;
            public Func<ITypedPresenterWindow> PresenterFactory;
            public Type ModelType;
        }

        private sealed class WidgetRegistration
        {
            public AssetReference AssetReference;
//            public UIWidgetView Prefab;
            public Func<ITypedPresenterWidget> PresenterFactory;
            public Type ModelType;
        }

        private sealed class ActiveWindow
        {
            public UIWindowView View;
            public ITypedPresenterWindow Presenter;
        }

        private sealed class ActiveWidget
        {
            public UIWidgetView View;
            public ITypedPresenterWidget Presenter;
        }

        #endregion
    }
}