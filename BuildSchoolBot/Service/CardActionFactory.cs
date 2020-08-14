using AdaptiveCards;
using BuildSchoolBot.StoreModels;

namespace BuildSchoolBot.Service
{
    public static class CardActionFactory
    {
        /// <summary>
        /// 使用此方法後，觸發此Action會進入OnTaskModuleFetchAsync的方法(開啟TaskModule)
        /// </summary>
        /// <param name="action">要開啟TaskModule的將AdaptiveSubmitAction</param>
        /// <param name="title">顯示在介面上按鈕內的string</param>
        /// <param name="data">要傳入下個Turn的文字string</param>
        /// <returns>會開啟TaskModule的AdaptiveSubmitAction</returns>
        public static AdaptiveSubmitAction SetOpenTaskModule(this AdaptiveSubmitAction action, string title, string data)
        {
            action.Title = title;
            action.Data = new AdaptiveCardTaskFetchValue<string>() { Data = data };
            return action;
        }
        
        public static AdaptiveSubmitAction SetSubmitTaskModule(this AdaptiveSubmitAction action, string title, string data)
        {
            action.Title = title;
            action.Data = new AdaptiveCardTaskSubmitValue<string>() { Data = data };
            return action;
        }
        /// <summary>
        /// 為AdaptiveCard新增一個AdaptiveActionSet
        /// </summary>
        /// <param name="card">要被掛上Set的AdaptiveCard</param>
        /// <param name="set">要掛上的AdaptiveActionSet</param>
        /// <returns>加入AdaptiveActionSet後的AdaptiveCard</returns>
        public static AdaptiveCard AddActionsSet(this AdaptiveCard card, AdaptiveActionSet set)
        {
            card.Body.Add(set);
            return card;
        }
        /// <summary>
        /// 快速新增一ActionSet(List<AdaptiveAction>)
        /// </summary>
        /// <returns>回傳一基本的ActionSet</returns>
        public static AdaptiveActionSet NewActionsSet()
        {
            return new AdaptiveActionSet(){ Separator = true, Type = AdaptiveActionSet.TypeName };
        }
        /// <summary>
        /// 為AdaptiveActionSet加入一個AdaptiveAction
        /// </summary>
        /// <param name="set">要被加入AdaptiveAction的AdaptiveActionSet</param>
        /// <param name="action">要加入的AdaptiveAction</param>
        /// <returns>加入一個AdaptiveAction後的AdaptiveActionSet</returns>
        public static AdaptiveActionSet AddActionToSet(this AdaptiveActionSet set, AdaptiveAction action)
        {
            set.Actions.Add(action);
            return set;
        } 
    }
}