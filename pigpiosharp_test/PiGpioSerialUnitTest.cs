using NUnit.Framework;

namespace PiGpio.Test
{
    public static class PiGpioSerialUnitTest
    {
        static PiGpioSharp pi;
        static Serial uart;

        static void Open(string tty)
        {
            uart.Open(tty, 115200);
        }

        [OneTimeSetUp]
        public static void Setup()
        {
            pi = new PiGpioSharp("raspi", 8888);
            uart = new Serial(pi);
        }

        [Test, Order(1)]
        public static void Open()
        {
            Open("/dev/ttyUSB1");
        }

        [Test, Order(2)]
        public static void Close()
        {
            uart.Close();
        }

        [Test]
        public static void OpenBadTTY()
        {
            Assert.That(() => uart.Open("/dev/ttyUSB10", 115200), Throws.Exception.TypeOf<CommandFailedException>().With.Property("Error").EqualTo(ErrorCode.PI_SER_OPEN_FAILED));
        }


        [Test]
        public static void WriteDeviceWithClosedHandle()
        {
            byte[] tmp = { 0, 1, 2 };
            Assert.That(() => uart.Write(tmp), Throws.Exception.TypeOf<CommandFailedException>().With.Property("Error").EqualTo(ErrorCode.PI_BAD_HANDLE));
        }

        [Test]
        public static void WriteAndRead()
        {
            byte[] writeData = { 0, 1, 2 };
            Open("/dev/ttyUSB1");
            uart.Write(writeData);
            System.Threading.Thread.Sleep(10);
            var readData = uart.Read(1000);
            Assert.That(readData, Is.EqualTo(writeData));
            Close();
        }

        [Test]
        public static void AvailableBytes()
        {
            byte[] writeData = { 0, 1, 2, 3, 4 };

            Open("/dev/ttyUSB1");
            uart.Write(writeData);
            System.Threading.Thread.Sleep(10);
            Assert.That(uart.NumOfRxBytesAvailable, Is.EqualTo(writeData.Length));
            uart.Read(1000);
            Close();
        }

    }
}
