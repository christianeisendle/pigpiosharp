namespace PiGpio
{
    public class I2C
    {
        #region Commands
        const int I2CO = 54;
        const int I2CC = 55;
        const int I2CRD = 56;
        const int I2CWD = 57;
        const int I2CWQ = 58;
        const int I2CRS = 59;
        const int I2CWS = 60;
        const int I2CRB = 61;
        const int I2CWB = 62;
        const int I2CRW = 63;
        const int I2CWW = 64;
        const int I2CRK = 65;
        const int I2CWK = 66;
        const int I2CRI = 67;
        const int I2CWI = 68;
        const int I2CPC = 69;
        const int I2CPK = 70;
        #endregion

        readonly PiGpioSharp m_pi;
        int m_handle;

        public I2C(PiGpioSharp pi)
        {
            m_pi = pi;
        }

        public void open(int bus, int address, int i2c_flags = 0)
        {
            m_handle = m_pi.executeCommand(I2CO, bus, address, i2c_flags);
        }

        public void close()
        {
            m_pi.executeCommand(I2CC, m_handle, 0);
        }

        public void writeDevice(byte[] data)
        {
            m_pi.executeCommand(I2CWD, m_handle, 0, data.Length, data);
        }
    }
}
