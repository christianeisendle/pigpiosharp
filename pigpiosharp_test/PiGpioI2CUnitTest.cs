using NUnit.Framework;

namespace PiGpio.Test
{
    public static class PiGpioI2CUnitTest
    {
        static PiGpioSharp pi;
        static I2C i2c;

        static void Open(int slaveAddr)
        {
            i2c.Open(1, slaveAddr);
        }

        [OneTimeSetUp]
        public static void Setup()
        {
            pi = new PiGpioSharp("raspi", 8888);
            i2c = new I2C(pi);
        }

        [Test, Order(1)]
        public static void Open()
        {
            Open(0x28);
        }

        [Test, Order(2)]
        public static void Close()
        {
            i2c.Close();
        }

        [Test]
        public static void OpenBadI2CBus()
        {
            Assert.That(() => i2c.Open(10, 0x28), Throws.Exception.TypeOf<CommandFailedException>().With.Property("Error").EqualTo(ErrorCode.PI_BAD_I2C_BUS));
        }

        [Test]
        public static void WriteDeviceToNonExistingDevice()
        {
            byte[] tmp = { 0, 1, 2 };
            Open(0x30);
            Assert.That(() => i2c.WriteDevice(tmp), Throws.Exception.TypeOf<CommandFailedException>().With.Property("Error").EqualTo(ErrorCode.PI_I2C_WRITE_FAILED));
            Close();
        }

        [Test]
        public static void WriteDeviceWithClosedHandle()
        {
            byte[] tmp = { 0, 1, 2 };
            Assert.That(() => i2c.WriteDevice(tmp), Throws.Exception.TypeOf<CommandFailedException>().With.Property("Error").EqualTo(ErrorCode.PI_BAD_HANDLE));
        }

        [Test]
        public static void ReadDeviceFromNonExistingDevice()
        {
            Open(0x30);
            Assert.That(() => i2c.ReadDevice(10), Throws.Exception.TypeOf<CommandFailedException>().With.Property("Error").EqualTo(ErrorCode.PI_I2C_READ_FAILED));
            Close();
        }

        [Test]
        public static void ReadDevice()
        {
            Open(0x28);
            i2c.ReadDevice(10);
            Close();
        }
    }
}
