using System;
namespace PiGpio
{
	public class I2C
	{
#region Commands
		private const int I2CO = 54;
		private const int I2CC = 55;
		private const int I2CRD = 56;
		private const int I2CWD = 57;
		private const int I2CWQ = 58;
		private const int I2CRS = 59;
		private const int I2CWS = 60;
		private const int I2CRB = 61;
		private const int I2CWB = 62;
		private const int I2CRW = 63;
		private const int I2CWW = 64;
		private const int I2CRK = 65;
		private const int I2CWK = 66;
		private const int I2CRI = 67;
		private const int I2CWI = 68;
		private const int I2CPC = 69;
		private const int I2CPK = 70;
#endregion

		private PiGpioSharp m_pi;
		private int m_handle;

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
	}
}
