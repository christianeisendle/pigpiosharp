namespace PiGpio
{
    public class I2C
    {
        readonly PiGpioSharp m_pi;
        int m_handle;

        public I2C(PiGpioSharp pi)
        {
            m_pi = pi;
        }

        public void Open(int bus, int address, int i2c_flags = 0)
        {
            m_handle = m_pi.ExecuteCommand(CommandCode.PI_CMD_I2CO, bus, address, i2c_flags);
        }

        public void Close()
        {
            m_pi.ExecuteCommand(CommandCode.PI_CMD_I2CC, m_handle, 0);
        }

        public void WriteDevice(byte[] data)
        {
            m_pi.ExecuteCommand(CommandCode.PI_CMD_I2CWD, m_handle, 0, data.Length, data);
        }

        public byte[] ReadDevice(int count)
        {
            byte[] readData = null;
            try
            {
                var numReceivedBytes = m_pi.ExecuteCommand(CommandCode.PI_CMD_I2CRD, m_handle, count, 0, null, false);
                readData = m_pi.GetMessage(numReceivedBytes);
            }
            finally
            {
                m_pi.ReleaseLock();
            }
            return readData;
        }
    }
}
