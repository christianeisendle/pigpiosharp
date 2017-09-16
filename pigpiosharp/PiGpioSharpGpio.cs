namespace PiGpio
{
    public enum GpioMode
    {
        INPUT = 0,
        OUTPUT = 1,
        ALT0 = 4,
        ALT1 = 5,
        ALT2 = 6,
        ALT3 = 7,
        ALT4 = 3,
        ALT5 = 2
    }

    public enum GpioPullUpDown
    {
        PUD_OFF = 0,
        PUD_DOWN = 1,
        PUD_UP = 2
    }

    public class Gpio
    {
        readonly PiGpioSharp m_pi;
        int m_handle;

        public Gpio(PiGpioSharp pi)
        {
            m_pi = pi;
        }

        public void SetMode(int gpioNum, GpioMode mode)
        {
            m_handle = m_pi.ExecuteCommand(CommandCode.PI_CMD_MODES, gpioNum, (int)mode);
        }

        public GpioMode GetMode(int gpioNum)
        {
            return (GpioMode)m_pi.ExecuteCommand(CommandCode.PI_CMD_MODEG, gpioNum, 0);
        }

        public void SetPullUpPullDown(int gpioNum, GpioPullUpDown pud)
        {
            m_pi.ExecuteCommand(CommandCode.PI_CMD_PUD, gpioNum, (int)pud);
        }

        public int Read(int gpioNum)
        {
            return m_pi.ExecuteCommand(CommandCode.PI_CMD_READ, gpioNum, 0);
        }

        public void Write(int gpioNum, int value)
        {
            m_pi.ExecuteCommand(CommandCode.PI_CMD_PUD, gpioNum, value);
        }
    }
}
