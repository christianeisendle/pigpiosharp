using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace PiGpio
{
	public class PiGpioSharp
	{
		private string m_host;
		private int m_port;
		private Socket m_socket;
		private const int SOCKET_CMD_RESP_LENGTH = 16;

		public PiGpioSharp(string host = "localhost", int port = 8888)
		{
			m_host = host;
			m_port = port;
            connect();
		}

		public PiGpioSharp()
		{
			m_host = Environment.GetEnvironmentVariable("PIGPIO_ADDR");
			var port = Environment.GetEnvironmentVariable("PIGPIO_PORT");
			try
			{
				m_port = int.Parse(port);
			}
			catch (Exception)
			{
				m_port = 8888;
			}
			if (m_host == null)
			{
				m_host = "localhost";
			}
			connect();
		}

		public int Port
		{ 
			get { return m_port; }
		}

		public string Host
		{ 
			get { return m_host; }
		}

		private void connect()
		{

			IPAddress ipAddress = Dns.GetHostAddresses(m_host)[0];
			IPEndPoint remote = new IPEndPoint(ipAddress, m_port);

			m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			m_socket.Connect(remote);
			m_socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
		}

		public int executeCommand(int command, int p1, int p2, int p3 = 0, byte[] ext = null)
		{
			List<byte> data = new List<byte>();
			byte[] resp = new byte[SOCKET_CMD_RESP_LENGTH];

			data.AddRange(BitConverter.GetBytes(command));
			data.AddRange(BitConverter.GetBytes(p1));
			data.AddRange(BitConverter.GetBytes(p2));
			data.AddRange(BitConverter.GetBytes(p3));
			if (ext != null)
			{
				data.AddRange(ext);
			}
			m_socket.Send(data.ToArray());
			m_socket.Receive(resp, SOCKET_CMD_RESP_LENGTH, SocketFlags.None);
			return BitConverter.ToInt32(resp, 12);
		}
	}
}
