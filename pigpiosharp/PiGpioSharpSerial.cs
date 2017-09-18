using System.Text;

namespace PiGpio
{
    public class Serial
    {
        readonly PiGpioSharp m_pi;
        int m_handle;

        public Serial(PiGpioSharp pi)
        {
            m_pi = pi;
        }

        public void Open(string tty, int baudrate, int i2c_flags = 0)
        {
            var byteString = Encoding.ASCII.GetBytes(tty);
            m_handle = m_pi.ExecuteCommand(CommandCode.PI_CMD_SERO, baudrate, i2c_flags, byteString.Length, byteString);
        }

        public void Close()
        {
            m_pi.ExecuteCommand(CommandCode.PI_CMD_SERC, m_handle, 0);
        }

        public void Write(byte[] data)
        {
            m_pi.ExecuteCommand(CommandCode.PI_CMD_SERW, m_handle, 0, data.Length, data);
        }

        public void WriteByte(byte data)
        {
            m_pi.ExecuteCommand(CommandCode.PI_CMD_SERWB, m_handle, data);
        }

        public byte[] Read(int count)
        {
            var numReceivedBytes = m_pi.ExecuteCommand(CommandCode.PI_CMD_SERR, m_handle, count, 0, null, false);
            var readData = m_pi.GetMessage(numReceivedBytes);
            m_pi.ReleaseLock();
            return readData;
        }

        public byte ReadByte()
        {
            return (byte)m_pi.ExecuteCommand(CommandCode.PI_CMD_SERRB, m_handle, 0);
        }
    }
}
