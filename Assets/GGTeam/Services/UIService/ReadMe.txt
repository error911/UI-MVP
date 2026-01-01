
https://docs.google.com/document/d/1hCDj4gA6DLOLEaYNOLs5sPx0HM68kixNxu_-O1cUjeQ/edit?usp=sharing




=== UI система по паттерну MVP ===

Поддержка 2х типов UI-элементов:
1. UIWindow (взаимоисключающие: при открытии одного — остальные закрываются)
2. UIWidget (независимые, количество не ограничено).

Функционал сервиса: открыть, закрыть и обновить окно/виджет.


=== Процесс создания кода ===
1. Создаем View класс, который будет висеть на префабе.
	Префабы окон/виджетов должны наследовать UIWindowView / UIWidgetView

	Пример окна: class MainMenuView: UIWindowView
	Пример виджета: class ProgressView: UIWidgetView

	В нем можно объявить публичные методы:
	public void SetTitle(string title) => titleText.text = title;

2. Создаем Model. Обычный класс с данными, ни от кого не наследуется. Может содержать данные и бизнес-логику.
	Пример: 
	public class MainMenuModel {
    		public string Title;
    		public Action OnExit;
	}

3. Создаем Presenter, который соединяет View и Model (Что в нашей системе не работало, а это базовая функция презентера)
	Имеет абстрактные методы:
		OnBind()
		OnOpen(object model)
		OnUpdate(object model)
		OnClose()

	Пример:
	public class MainMenuPresenter : WindowPresenterBase<MainMenuView, MainMenuModel> {
    		protected override void OnOpen(MainMenuModel model)
    		{
        		View.SetTitle(model.Title);
        		View.SetPlayCallback(model.OnPlay);
        		View.SetExitCallback(model.OnExit);
    		}

    		protected override void OnUpdate(MainMenuModel model)
    		{
        		View.SetTitle(model.Title);
    		}
	}

	Презентер для Виджета отличается наследуемым классм: WindowPresenterBase -> WidgetPresenterBase 
	


=== Работа с UI ===
Все созданные окна необходимо зарегистрировать, что проще возложить на CORE и опишу чуть ниже.

Пример работы с UI:

// Пример открытия окна с данными
uiService.OpenWindow("MainMenu", new MainMenuModel
{
Title = "Main Menu",
OnExit = () => Debug.Log("Exit pressed")
});


var widgetId = uiService.OpenWidget("Notification", new NotificationModel
{
Message = "Hello!"
});


// Пример обновления виджета
uiService.UpdateWidget(widgetId, new NotificationModel
{
Message = "Updated message."
});

Важно: var widgetId это не String , а Guid. Поэтому всегда уникален и позволяет открывать одинаковые панели в неограниченном количестве.



