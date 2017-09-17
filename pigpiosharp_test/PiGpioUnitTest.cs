using NUnit.Framework;
using System.Threading;

namespace PiGpio.Test
{
    public static class PiGpioUnitTest
    {
        static PiGpioSharp pi;
        static Gpio gpio;

        [OneTimeSetUp]
        public static void Setup()
        {
            pi = new PiGpioSharp("raspi", 8888);
            gpio = new Gpio(pi);
        }

        [OneTimeTearDown]
        public static void TearDown()
        {
            gpio.StopGpioChangeListener();
        }

        [Test]
        public static void SetMode()
        {
            gpio.SetMode(26, GpioMode.OUTPUT);
        }

        [Test]
        public static void GetMode()
        {
            gpio.SetMode(26, GpioMode.OUTPUT);
            Assert.That(() => gpio.GetMode(26), Is.EqualTo(GpioMode.OUTPUT));
            gpio.SetMode(26, GpioMode.INPUT);
            Assert.That(() => gpio.GetMode(26), Is.EqualTo(GpioMode.INPUT));
        }

        [Test]
        public static void SetPullUp()
        {
            gpio.SetMode(26, GpioMode.INPUT);
            gpio.SetPullUpPullDown(26, GpioPullUpDown.PUD_UP);
            Assert.That(() => gpio.Read(26), Is.EqualTo(1));
        }

        [Test]
        public static void SetPullDown()
        {
            gpio.SetMode(26, GpioMode.INPUT);
            gpio.SetPullUpPullDown(26, GpioPullUpDown.PUD_DOWN);
            Assert.That(() => gpio.Read(26), Is.EqualTo(0));
        }

        [TestCase(0)]
        [TestCase(1)]
        public static void Write(int value)
        {
            gpio.SetMode(19, GpioMode.OUTPUT);
            gpio.SetMode(26, GpioMode.INPUT);
            gpio.SetPullUpPullDown(26, GpioPullUpDown.PUD_OFF);
            gpio.Write(19, value);
            Assert.That(() => gpio.Read(26), Is.EqualTo(value));
        }

        static void ToggleGpioDelayed(object gpioNum)
        {
            Thread.Sleep(100);
            gpio.Write(19, 1);
            Thread.Sleep(100);
            gpio.Write(19, 0);
            Thread.Sleep(100);
            gpio.Write(19, 1);
        }

        [Test]
        public static void WaitForSignalChange()
        {
            gpio.SetMode(26, GpioMode.INPUT);
            gpio.SetMode(19, GpioMode.OUTPUT);
            gpio.Write(19, 0);
            var thread = new Thread(new ParameterizedThreadStart(PiGpioUnitTest.ToggleGpioDelayed));
            thread.Start(26);
            gpio.WaitForEdge(26, GpioEdge.RISING_EDGE, 1000);
        }


    }
}
