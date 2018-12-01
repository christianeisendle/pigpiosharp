using System.Threading;
using System.Timers;

namespace PiGpio
{
    public delegate void EventTimerHandler(EventTimerResult result);

    public enum EventTimerStatus
    { 
        Success,
        OnlyStartedNeverStopped,
        NeverStarted
    }

    public class EventTimerResult
    {
        public EventTimerResult(EventTimerStatus status, uint elapsedTime)
        {
            Status = status;
            ElapsedTimeInMicroseconds = elapsedTime;
        }

        public EventTimerStatus Status { get; }
        public uint ElapsedTimeInMicroseconds { get; } 
    }

    public class EventTimer
    {
        private int startGpioNumber = 0;
        private int stopGpioNumber = 0;
        private GpioEdge startGpioEdge = GpioEdge.FALLING_EDGE;
        private GpioEdge stopGpioEdge = GpioEdge.FALLING_EDGE;
        private EventTimerHandler callback;
        private Gpio gpioManager;
        private int startGpioCallbackHandle;
        private int stopGpioCallbackHandle;
        private EventTimerStatus timerStatus;
        private uint timerStartTick;
        private System.Timers.Timer guardTimer;
        private bool guardTimerElapsed = false;
        private EventWaitHandle endOfRunTimer;
        private EventTimerResult blockingTimerResult;

        public EventTimer(PiGpioSharp pi)
        {
            gpioManager = new Gpio(pi);
            gpioManager.StartGpioChangeListener();
        }

        ~EventTimer()
        {
            gpioManager.StopGpioChangeListener();
        }

        private bool MatchLevelAndEdge(uint level, GpioEdge edge)
        { 
            switch (edge)
            {
                case GpioEdge.EITHER_EDGE:
                    return true;
                case GpioEdge.FALLING_EDGE:
                    if (level == 0)
                    {
                        return true;
                    }
                    break;
                case GpioEdge.RISING_EDGE:
                    if (level == 1)
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }

        private void GpioChangeHandler(int gpio, uint level, uint tick)
        {
            switch (timerStatus)
            {
                case EventTimerStatus.NeverStarted:
                    if ((gpio == startGpioNumber) && (MatchLevelAndEdge(level, startGpioEdge)))
                    {
                        timerStatus = EventTimerStatus.OnlyStartedNeverStopped;
                        timerStartTick = tick;
                    }
                    break;
                case EventTimerStatus.OnlyStartedNeverStopped:
                    if ((gpio == stopGpioNumber) && (MatchLevelAndEdge(level, stopGpioEdge)))
                    {
                        Disable();
                        if (!guardTimerElapsed)
                        {
                            timerStatus = EventTimerStatus.Success;
                            uint timerDuration = 0;
                            if (tick > timerStartTick)
                            {
                                timerDuration = tick - timerStartTick;
                            }
                            else
                            {
                                timerDuration = 2 ^ 32 - (timerStartTick - tick);
                            }
                            callback(new EventTimerResult(timerStatus, timerDuration));
                            UnregisterGpioCallbacksAndStopListener();
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Sets the GPIO which is used to start the timer when the level
        /// changes according to the configured edge (falling, rising, either).
        /// </summary>
        /// <param name="gpioNumber">Raspberry Pi Gpio number.</param>
        /// <param name="gpioEdge">Gpio edge.</param>
        public void SetStartEvent(int gpioNumber, GpioEdge gpioEdge)
        {
            startGpioNumber = gpioNumber;
            startGpioEdge = gpioEdge;
            startGpioCallbackHandle = gpioManager.RegisterLevelChangeCallback(startGpioNumber, startGpioEdge, GpioChangeHandler);
        }

        /// <summary>
        /// Sets the GPIO which is used to start the timer when the level
        /// changes according to the configured edge (falling, rising, either).
        /// </summary>
        /// <param name="gpioNumber">Raspberry Pi Gpio number.</param>
        /// <param name="gpioEdge">Gpio edge.</param>
        public void SetStopEvent(int gpioNumber, GpioEdge gpioEdge)
        {
            stopGpioNumber = gpioNumber;
            stopGpioEdge = gpioEdge;
            stopGpioCallbackHandle = gpioManager.RegisterLevelChangeCallback(stopGpioNumber, stopGpioEdge, GpioChangeHandler);
        }

        private void SetCallback(EventTimerHandler cb)
        {
            callback = cb;
        }

        /// <summary>
        /// Enable the Timer and immediately return. Requires proper configuration
        /// of start and stop timer event using SetStartEvent and SetStopEvent.
        /// The supplied callback is called when the timer elapses (start and stop event detected)
        /// or timer was not started or stopped within given max. waiting time.
        /// </summary>
        /// <param name="maxWaitingTimeInMilliseconds">Max waiting time in milliseconds for the timer to start and stop.</param>
        /// <param name="callback">Callback which is executed upon timer has elapsed or waiting for the timer times out.</param>
        public void Enable(uint maxWaitingTimeInMilliseconds, EventTimerHandler callback)
        {
            SetCallback(callback);
            timerStatus = EventTimerStatus.NeverStarted;
            guardTimer = new System.Timers.Timer(maxWaitingTimeInMilliseconds);
            guardTimer.Elapsed += GuardTimer_Elapsed;
            guardTimerElapsed = false;
            guardTimer.Start();
        }

        void GuardTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            guardTimerElapsed = true;
            callback(new EventTimerResult(timerStatus, 0));
            UnregisterGpioCallbacksAndStopListener();
        }

        /// <summary>
        /// Disable the timer, which has been enabled before. This is only required for enabled
        /// timer, i.e. an already elapsed timer is automatically disabled.
        /// </summary>
        public void Disable()
        {
            guardTimer.Stop();
            UnregisterGpioCallbacksAndStopListener();
        }

        /// <summary>
        /// Enable the Timer and block until either the Timer elapses (start and stop event detected)
        /// or waiting for the events times out.
        /// </summary>
        /// <returns>Status of the timer.</returns>
        /// <param name="maxWaitingTimeInMilliseconds">Max time to wait for the timer to elapse.</param>
        public EventTimerResult Run(uint maxWaitingTimeInMilliseconds)
        {
            endOfRunTimer = new EventWaitHandle(false, EventResetMode.ManualReset);
            Enable(maxWaitingTimeInMilliseconds, RunTimerCallback);
            endOfRunTimer.WaitOne();
            return blockingTimerResult;
        }

        private void RunTimerCallback(EventTimerResult result)
        {
            blockingTimerResult = result;
            endOfRunTimer.Set();
        }

        private void UnregisterGpioCallbacksAndStopListener()
        {
            gpioManager.UnregisterLevelChangeCallback(startGpioCallbackHandle);
            gpioManager.UnregisterLevelChangeCallback(stopGpioCallbackHandle);
        }
    }
}
