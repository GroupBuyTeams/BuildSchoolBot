using Microsoft.Bot.Schema.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.StoreModels
{
    public static class TaskModuleResponseFactory
    {
        public static TaskModuleResponse CreateResponse(string message)
        {
            return new TaskModuleResponse
            {
                Task = new TaskModuleMessageResponse()
                {
                    Value = message,
                },
            };
        }

        public static TaskModuleResponse CreateResponse(TaskModuleTaskInfo taskInfo)
        {
            return new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse()
                {
                    Value = taskInfo,
                },
            };
        }

        public static TaskModuleResponse ToTaskModuleResponse(this TaskModuleTaskInfo taskInfo)
        {
            return CreateResponse(taskInfo);
        }
    }
}

