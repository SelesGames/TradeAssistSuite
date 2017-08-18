using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using static TradeAssist.Realtime.Candlestick.TimerConstants;

namespace TradeAssist.Realtime.Candlestick
{
    static class TimerConstants
    {
        public static TimeSpan TimerPeriod { get; } = TimeSpan.FromMinutes(1);
    }

    class Timer
    {
        static IEmitCandles OnEmitCandles;

        public Timer(IEmitCandles candlesReceiver)
        {
            OnEmitCandles = candlesReceiver;
        }

        public void InitTimer()
        {
            Common.Logging.LogManager.Adapter = new Common.Logging.Simple.ConsoleOutLoggerFactoryAdapter { Level = Common.Logging.LogLevel.Info };

            // Grab the Scheduler instance from the Factory 
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();

            // and start it off
            scheduler.Start();

            // define the job and tie it to our HelloJob class
            IJobDetail job = JobBuilder.Create<IntervalProc>()
                .WithIdentity("job1", "group1")
                .Build();


            var initialOffset = DateTimeOffset.Now;
            initialOffset.AddTicks(TimerPeriod.Ticks);

            // Trigger the job to run now, and then repeat every 10 seconds
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .StartAt(DateBuilder.FutureDate(1, IntervalUnit.Minute))
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds((int)TimerPeriod.TotalSeconds)
                    .RepeatForever())
                .Build();

            // Tell quartz to schedule the job using our trigger
            scheduler.ScheduleJob(job, trigger);
        }

        /// <summary>
        /// Takes a snapshot of the current candles at set intervals - also initiates the timing mechanism
        /// </summary>
        class IntervalProc : IJob
        {
            static readonly CandlestickBook book = CandlestickBook.Current;

            static readonly List<TimerState> timerStates = Enum.GetValues(typeof(CandlestickSize))
                .Cast<CandlestickSize>()
                .Select(o => new TimerState(o))
                .ToList();

            public void Execute(IJobExecutionContext context)
            {
                var proccedSizes = new List<CandlestickSize>();

                foreach (var timerState in timerStates)
                {
                    if (timerState.Increment())
                    {
                        proccedSizes.Add(timerState.CandlestickSize);
                    }
                }

                OnEmitCandles?.EmitCandles(proccedSizes);
            }
        }
    }

    class TimerState
    {
        readonly int candleLengthInMinutes;
        int counter = 0;

        public CandlestickSize CandlestickSize { get; }

        public TimerState(CandlestickSize size)
        {
            this.candleLengthInMinutes = (int)size.GetSizeInMilliseconds() / 60000;
            CandlestickSize = size;
        }

        public bool Increment()
        {
            counter++;
            if (counter % candleLengthInMinutes == 0)
            {
                counter = 0;
                return true;
            }
            return false;
        }
    }

    interface IEmitCandles
    {
        void EmitCandles(IEnumerable<CandlestickSize> candleSizes);
    }
}