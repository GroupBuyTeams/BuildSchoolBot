using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.StoreModels
{
    public class TaskModuleUIConstants
    {
        public static UISettings AdaptiveCard { get; set; } =
          new UISettings(900, 500, "", TaskModuleIds.AdaptiveCard, "Click");

        public static UISettings Update { get; set; } =
            new UISettings(600, 400, "", TaskModuleIds.Update, "Click");
    }
}
