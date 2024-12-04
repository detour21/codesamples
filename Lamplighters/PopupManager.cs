using System.Collections.Generic;
using Framework;

namespace UI
{
    public class PopupDataItemBase
    {
        public Metatext title;
        public Metatext content;
    }

    public class GenericPopupDataItem : PopupDataItemBase
    {
        public UiPopupGenericViewModel.StateControllerTextContainerType popupTextStyle;

        public readonly bool displayAsWarning;

        public Metatext tarotReplacementTitle;
        public Metatext subtitle;

        public int? tarotCost;

        public PopupButtonInfo[] buttons;

        public bool scrollable = false;

        public System.Action<UiPopupGenericController> onVisible;

        public GenericPopupDataItem(Metatext title, Metatext content, PopupButtonInfo[] buttons, bool displayAsWarning = false)
        {
            this.popupTextStyle = UiPopupGenericViewModel.StateControllerTextContainerType.TitleText;
            this.title = title;
            this.tarotReplacementTitle = Metatext.EMPTY_STRING;
            this.subtitle = Metatext.EMPTY_STRING;
            this.content = content;
            this.tarotCost = null;
            this.buttons = buttons;
            this.displayAsWarning = displayAsWarning;
        }
        
        public GenericPopupDataItem(Metatext title, Metatext content, PopupButtonInfo[] buttons, System.Action<UiPopupGenericController> onVisible)
        {
            this.popupTextStyle = UiPopupGenericViewModel.StateControllerTextContainerType.TitleText;
            this.title = title;
            this.tarotReplacementTitle = Metatext.EMPTY_STRING;
            this.subtitle = Metatext.EMPTY_STRING;
            this.content = content;
            this.tarotCost = null;
            this.buttons = buttons;
            this.onVisible = onVisible;
        }

        public GenericPopupDataItem(UiPopupGenericViewModel.StateControllerTextContainerType popupTextStyle, Metatext title, Metatext tarotReplacementTitle, Metatext subtitle, Metatext content, int? tarotCost, PopupButtonInfo[] buttons, bool displayAsWarning = false)
        {
            this.popupTextStyle = popupTextStyle;
            this.title = title;
            this.tarotReplacementTitle = tarotReplacementTitle;
            this.subtitle = subtitle;
            this.content = content;
            this.tarotCost = tarotCost;
            this.buttons = buttons;
            this.displayAsWarning = displayAsWarning;
        }
    }

    public class AutoSavePopupDataItem : PopupDataItemBase
    {
        // Where we are triggering this Save, used to determine where we position it.
        public AutoSaveSource triggerSource;

        // Callback we will trigger once the Notification is visible, used to actually trigger the Save and then Destroy the Popup when we're done
        public System.Action<UiSaveNotificationController> onVisible;

        public AutoSavePopupDataItem(Metatext title, Metatext content, AutoSaveSource triggerSource, System.Action<UiSaveNotificationController> onVisible)
        {
            this.title = title;
            this.content = content;

            this.triggerSource = triggerSource;

            this.onVisible = onVisible;
        }
    }

    public struct PopupButtonInfo
    {
        public Metatext buttonTitle;
        public System.Action callback;

        public PopupButtonInfo(Metatext title, System.Action callback)
        {
            this.buttonTitle = title;
            this.callback = callback;
        }
    }

    /// <summary>
    /// A simple static manager class allowing creation of popups 
    /// Can maintain a queue of popups to regulate the order of appearance
    /// The idea is only 1 popup shows at a time, additional popups wait in line so that we don't pile them up on screen
    /// </summary>

    public static class PopupManager
    {
        public static bool IsDisplaying(IModuleController controllerToIgnore) 
        {
            return (activePopupController != null && activePopupController != controllerToIgnore) || !Tutorials.TutorialManager.Instance.IsEmpty; 
        }

        public static bool IsDisplayingGenericPopup ()
        {
            return activePopupController != null && activePopupController is UiPopupGenericController && !activePopupController.IsDestroyed;
        }

        public static bool IsDisplayingPauseMenu ()
        {
            return activePopupController is UipauseMenuController;
        }
        
        private static Queue<PopupDataItemBase> popups = new Queue<PopupDataItemBase>();
        private static IModuleController activePopupController;
        private static List<IModuleController> popupStack = new List<IModuleController>();

        public static void Init ()
        {
            popups.Clear();
        }

        public static void Reset ()
        {
            popups.Clear();
            popupStack.Clear();
            activePopupController = null;
        }

        /// <summary>
        /// Call this to add a new popup to the queue
        /// If there is nothing in the queue this popup will be created immediately, otherwise it will wait in the queue
        /// 
        /// This is a convenience helper to just create a simple title/body only generic popup. More elaborate ones can be created by calling the full dataItem version below
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="buttons"></param>
        /// <param name="displayAsWarning">If true, we will use a different color/style of background for this popup to warn the user </param>
        /// Currently the only "type" is generic
        /// The text which shows in the title area
        /// The text which shows in the body area
        /// An array of ButtonInfo structs representing the list of buttons to be shown
        public static void EnqueueGenericPopup(Metatext title, Metatext content, PopupButtonInfo[] buttons, bool displayAsWarning = false)
        {
            GenericPopupDataItem item = new GenericPopupDataItem(title, content, buttons, displayAsWarning);
            EnqueuePopup( item );
        }
        
        public static void EnqueueGenericPopup(Metatext title, Metatext content, PopupButtonInfo[] buttons, 
            System.Action<UiPopupGenericController> onVisible)
        {
            GenericPopupDataItem item = new GenericPopupDataItem(title, content, buttons, onVisible);
            EnqueuePopup( item );
        }

        public static void PresentGenericPopup(Metatext title, Metatext content, PopupButtonInfo[] buttons, bool displayAsWarning = false)
        {
            GenericPopupDataItem item = new GenericPopupDataItem(title, content, buttons, displayAsWarning);
            CreatePopup( item );
        }

        public static void EnqueueSavePopup(Metatext title, Metatext content, AutoSaveSource triggerSource, System.Action<UiSaveNotificationController> onVisible)
        {
            AutoSavePopupDataItem item = new AutoSavePopupDataItem(title, content, triggerSource, onVisible);
            EnqueuePopup( item );
        }

        public static void PresentSavePopup(Metatext title, Metatext content, AutoSaveSource triggerSource, System.Action<UiSaveNotificationController> onVisible)
        {
            AutoSavePopupDataItem item = new AutoSavePopupDataItem(title, content, triggerSource, onVisible);
            CreatePopup(item);
        }

        /// <summary>
        /// Adds a new popup to the queue. If there is nothing in the queue this popup will be created immediately, otherwise it will wait in the queue
        /// </summary>
        /// <param name="item">The GenericPopupDataItem to use</param>
        public static void EnqueuePopup(PopupDataItemBase item)
        {
            if (activePopupController == null || !(activePopupController is UiPopupGenericController))
            {
                CreatePopup(item);
            }
            else
            {
                popups.Enqueue(item);
            }
        }

        /// <summary>
        /// Check for anything in the queue and create the next popup if so
        /// </summary>
        public static void DequeuePopup ()
        {
            if (popups.Count > 0)
            {
                PopupDataItemBase item = popups.Dequeue();
                CreatePopup(item);
            }
        }

        private static void CreatePopup (PopupDataItemBase item)
        {
            if(item is GenericPopupDataItem)
            {
                activePopupController = UIUXManager.Instance.CreateModule<UiPopupGenericController>(RenderingLayerType.PopupRoot);
                UiPopupGenericController genericController = (UiPopupGenericController)activePopupController;
                genericController.InitPopup(item as GenericPopupDataItem);
                popupStack.Add(genericController);
            }
            else if(item is AutoSavePopupDataItem)
            {
                activePopupController = UIUXManager.Instance.CreateModule<UiSaveNotificationController>(RenderingLayerType.PopupRoot);
                UiSaveNotificationController saveNotificationController = (UiSaveNotificationController)activePopupController;
                saveNotificationController.Data = item as AutoSavePopupDataItem;
                popupStack.Add(saveNotificationController);
            }
            else
            {
                // Need to figure out which logger would be best to use here just in case we wanna log that we passed an data time we're not set to handle.
            }
        }

        /// <summary>
        /// Creates any UIUX module controller for presentation in the popup layer and marks IsDisplaying as true to prevent input on other layers
        /// </summary>
        public static T PresentPopup<T>() where T : class, IModuleController, new()
        {
            activePopupController = UIUXManager.Instance.CreateModule<T>(RenderingLayerType.PopupRoot);
            popupStack.Add(activePopupController);
            return (T)activePopupController;
        }

        public static void DestroyActivePopup()
        {
            if (activePopupController == null)
                return;
            DestroyPopup(activePopupController);
        }
        
        public static void DestroyPopup(IModuleController controller)
        {
            // If we're told to destroy the controller, destroy it.
            // Even if it isn't the activePopupControler.
            if (controller != null)
            {
                UIUXManager.Instance.DestroyModule(controller);
                if (popupStack.Contains(controller))
                {
                    popupStack.Remove(controller);
                }
            }

            // If the active controller just got destroyed, adjust the popuStack.
            if (activePopupController != null && activePopupController == controller)
            {
                if (popupStack.Count > 0)
                {
                    activePopupController = popupStack[popupStack.Count - 1];
                }
                else
                {
                    activePopupController = null;
                }
            }

        }

    }
}