using System;
using Quartz;
using BuildSchoolBot.Scheduler.Jobs;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Schema;

namespace BuildSchoolBot.Service
{
    public class ScheduleCreator
    {
        private IScheduler _sched { get; set; }
        private string _OrderId { get; set; }
        private string _UserId { get; set; }
        private string _SchedId { get; set; }
        private string _TeamsChannelId { get; set; }

        public ScheduleCreator(IScheduler scheduler, string UserId, string orderId, string schedId = null)
        {
            _sched = scheduler;
            _UserId = UserId;
            _OrderId = orderId;
            _SchedId = schedId;
        }

        public void CreateSingleGroupBuy(DateTime EndTime)
        {
            CreateSingleGroupBuy(DateTime.UtcNow, EndTime, null);
        }

        public void CreateSingleGroupBuy(DateTime startAt, DateTime endAt, string teamsChannelId)
        {
            
            DateTimeOffset startDate = new DateTimeOffset(startAt);
            DateTimeOffset endDate = new DateTimeOffset(endAt);
            // TimeSpan ten = new TimeSpan(0, 10, 0);

            // only for demo
            TimeSpan ten = new TimeSpan(0, 0, 10);

            _TeamsChannelId = teamsChannelId;
            if (teamsChannelId != null)
            {
                //ScheduleSingleJob<NoteBuy>(startDate - ten, ScheduleText.StartState, ScheduleText.NoteStartMsg);
                ScheduleSingleJob<StartBuy>(startDate, ScheduleText.NoteStartState, teamsChannelId);
                ScheduleSingleJob<NoteBuy>(startDate + new TimeSpan(0,0,5), ScheduleText.StartState, ScheduleText.StartMsg);
            }
            else
            {
                ScheduleSingleJob<NoteBuy>(startDate, ScheduleText.StartState, ScheduleText.StartMsg);//only notify everyone    
                // ScheduleSingleJob<NoteBuy>(endAt - ten, ScheduleText.NoteStopState, ScheduleText.NoteStopMsg);
                // ScheduleSingleJob<StopBuy>(endAt, ScheduleText.StopState, null);
            }
            ScheduleSingleJob<NoteBuy>(endAt - ten, ScheduleText.NoteStopState, ScheduleText.NoteStopMsg);
            ScheduleSingleJob<StopBuy>(endAt, ScheduleText.StopState, null);
        }

        public void CreateRepeatedGroupBuy(int startAt, int Duration, int WeekDays)
        {
            ScheduleRepeatJob<NoteBuy>(startAt - 10, WeekDays, ScheduleText.NoteStartState, ScheduleText.NoteStartMsg);
            ScheduleRepeatJob<StartBuy>(startAt, WeekDays, ScheduleText.StartState, ScheduleText.StartMsg);
            ScheduleRepeatJob<NoteBuy>(startAt + Duration - 10, WeekDays, ScheduleText.NoteStopState, ScheduleText.NoteStopMsg);
            ScheduleRepeatJob<StopBuy>(startAt + Duration, WeekDays, ScheduleText.StopState, null);
        }


        private void ScheduleSingleJob<T>(DateTimeOffset start, string stateInfo, string NotificationText) where T : IJob
        {

            var job = GetJobBuilder<T>(stateInfo, NotificationText);
            var trigger = GetTriggerBuilder(stateInfo)
                            .StartAt(start);

            ScheduleJob(job, trigger);
        }

        private void ScheduleRepeatJob<T>(int startAt, int WeekDaysFlag, string stateInfo, string NotificationText) where T : IJob
        {
            string hour = (startAt / 60).ToString();
            string min = (startAt % 60).ToString();
            string week = new WeekdaysEnum().GetWeekDays(WeekDaysFlag);
            
            var job = GetJobBuilder<T>(stateInfo, NotificationText);
            var trigger = GetTriggerBuilder(stateInfo)
                            .WithCronSchedule($"0 {min} {hour} ? * {week}");

            ScheduleJob(job, trigger);
        }
        private void ScheduleJob(JobBuilder job, TriggerBuilder trigger)
        {
            _sched.ScheduleJob(job.Build(), trigger.Build());
        }
        private JobBuilder GetJobBuilder<T>(string stateInfo, string NotificationText) where T : IJob
        {
            var jb = JobBuilder.Create<T>().WithIdentity(_OrderId + stateInfo, _UserId).UsingJobData("UserId",_UserId).UsingJobData("OrderId",_OrderId);

            if (_SchedId != null)
            {
                jb.UsingJobData("ScheduleId", _SchedId);
            }
            
            if (NotificationText != null) {
                jb.UsingJobData("Information", NotificationText);
            }

            return jb;
        }
        private TriggerBuilder GetTriggerBuilder(string stateInfo)
        {
            return TriggerBuilder.Create().WithIdentity(_OrderId + stateInfo, _UserId);
        }
    }

    public static class ScheduleText
    {
        public const string NoteStartMsg = "The group buying is about to start.";
        public const string StartMsg = "The group buying just started. Let's buy something!";
        public const string NoteStopMsg = "The group buying is about to close in 10 minutes.";
        public const string NoteStartState = "noteStart";
        public const string StartState = "Start";
        public const string NoteStopState = "noteStop";
        public const string StopState = "Stop";
    }
}