using NUnit.Framework;
using System.Threading;

namespace PiGpio.Test
{
    public static class PiGpioUnitTest
    {
        static PiGpioSharp pi;
        static Gpio gpio;
        static int m_count;

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

        [TestCase(GpioPullUpDown.PUD_UP, 1)]
        [TestCase(GpioPullUpDown.PUD_DOWN, 0)]
        public static void SetPullUpDown(GpioPullUpDown pullUpDown, int expectedValue)
        {
            gpio.SetMode(13, GpioMode.INPUT);
            gpio.SetPullUpPullDown(13, pullUpDown);
            Assert.That(() => gpio.Read(13), Is.EqualTo(expectedValue));
        }

        [Test]
        public static void SetPullUpDriver()
        {
            gpio.SetMode(19, GpioMode.OUTPUT);
            gpio.SetMode(26, GpioMode.INPUT);
            gpio.SetPullUpPullDown(26, GpioPullUpDown.PUD_UP);
            gpio.SetPullUpPullDown(19, GpioPullUpDown.PUD_UP);
            gpio.Write(19, 0);
            Assert.That(() => gpio.Read(26), Is.EqualTo(0));
            gpio.SetMode(19, GpioMode.INPUT);
            Assert.That(() => gpio.Read(26), Is.EqualTo(1));

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
            gpio.StartGpioChangeListener();
            gpio.WaitForEdge(26, GpioEdge.RISING_EDGE, 1000);
            gpio.StopGpioChangeListener();
        }

        static void GpioLevelChangeHandler(int gpioNum, uint level, uint tick)
        {
            m_count++;
        }

        [Test]
        public static void ToggleCount()
        {
            m_count = 0;
            int maxCount = 1000;
            bool val = false;

            gpio.SetMode(26, GpioMode.INPUT);
            gpio.SetMode(19, GpioMode.OUTPUT);
            gpio.Write(19, val ? 1 : 0);
            gpio.StartGpioChangeListener();
            gpio.RegisterLevelChangeCallback(26, GpioEdge.EITHER_EDGE, GpioLevelChangeHandler);
            for (int i = 0; i < maxCount; i++)
            {
                val = !val;
                gpio.Write(19, val ? 1 : 0);
            }
            Thread.Sleep(100);
			gpio.StopGpioChangeListener();
            Assert.That(m_count, Is.EqualTo(maxCount));
        }
    }
}
