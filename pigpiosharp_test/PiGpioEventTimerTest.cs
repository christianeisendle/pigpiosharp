using NUnit.Framework;
using PiGpio;
using System.Timers;

namespace PiGpio.Test
{
    [TestFixture]
    public class PiGpioEventTimerTest
    {
        static PiGpioSharp pi;
        static EventTimer timer;
        static Gpio gpio;
        static Timer asyncGpioSetTimer;

        [SetUp]
        public static void Setup()
        {
            pi = new PiGpioSharp("raspi", 8888);
            timer = new EventTimer(pi);
            gpio = new Gpio(pi);
            asyncGpioSetTimer = new Timer(100);
            asyncGpioSetTimer.AutoReset = false;
            asyncGpioSetTimer.Elapsed += AsyncGpioSetTimer_Elapsed;
            gpio.SetMode(26, GpioMode.OUTPUT);
            gpio.SetMode(19, GpioMode.OUTPUT);
            gpio.Write(26, 0);
            gpio.Write(19, 1);
        }

        [Test]
        public void RunTimerNeverStart()
        {
            var result = timer.Run(10);
            Assert.That(result.Status, Is.EqualTo(EventTimerStatus.NeverStarted));
        }

        [Test]
        public void RunTimerStartNeverStop()
        {
            timer.SetStartEvent(19, GpioEdge.FALLING_EDGE);
            asyncGpioSetTimer.Start();
            var result = timer.Run(1000);
            Assert.That(result.Status, Is.EqualTo(EventTimerStatus.OnlyStartedNeverStopped));
        }

        [Test]
        public void RunTimerStopOnly()
        {
            timer.SetStartEvent(26, GpioEdge.RISING_EDGE);
            asyncGpioSetTimer.Start();
            var result = timer.Run(1000);
            Assert.That(result.Status, Is.EqualTo(EventTimerStatus.NeverStarted));
        }

        [Test]
        public void RunTimerSuccess()
        {
            timer.SetStartEvent(19, GpioEdge.FALLING_EDGE);
            timer.SetStopEvent(26, GpioEdge.RISING_EDGE);
            asyncGpioSetTimer.Start();
            var result = timer.Run(1000);
            Assert.That(result.Status, Is.EqualTo(EventTimerStatus.Success));
            Assert.That(result.ElapsedTimeInMicroseconds, Is.GreaterThan(90000));
            Assert.That(result.ElapsedTimeInMicroseconds, Is.LessThan(110000));
        }

        static void AsyncGpioSetTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            gpio.Write(19, 0);
            System.Threading.Thread.Sleep(100);
            gpio.Write(26, 1);
        }

    }
}
