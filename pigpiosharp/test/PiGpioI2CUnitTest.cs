using System;
using PiGpio;
using NUnit.Framework;

namespace PiGpio.Test
{
    public class PiGpioI2CUnitTest
    {
        private static PiGpioSharp pi;
        private static I2C i2c;

        private static void open(int slaveAddr)
        {
            i2c.open(1, slaveAddr);
        }

        [OneTimeSetUp]
        public static void setup()
        {
            pi = new PiGpioSharp("raspi", 8888);
            i2c = new I2C(pi);
        }

        [Test, Order(1)]
        public static void open()
        {
            open(0x28);
        }

        [Test, Order(2)]
        public static void close()
        {
            i2c.close();
        }

        [Test]
        public static void openBadI2CBus()
        {
            Assert.That(() => i2c.open(10, 0x28), Throws.Exception.TypeOf<CommandFailedException>().With.Property("Error").EqualTo(ErrorCode.PI_BAD_I2C_BUS));
        }

        [Test]
        public static void writeDeviceToNonExistingDevice()
        {
            byte[] tmp = { 0, 1, 2 };
            open(0x30);
            Assert.That(() => i2c.writeDevice(tmp), Throws.Exception.TypeOf<CommandFailedException>().With.Property("Error").EqualTo(ErrorCode.PI_I2C_WRITE_FAILED));
        }
    }
}
