using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace PiGpio
{
    public delegate void GpioLevelChangeHandler(int gpioNum, uint level, uint tick);

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

    public enum GpioEdge
    {
        RISING_EDGE = 0,
        FALLING_EDGE = 1,
        EITHER_EDGE = 2
    }

    public class Gpio : IDisposable
    {
        class GpioSubscriber : IDisposable
        {
            GpioLevelChangeHandler m_callback;
            int m_gpioNumber;
            GpioEdge m_edge;
            ManualResetEvent m_event;

            public GpioSubscriber(int gpioNumber, GpioEdge edge, GpioLevelChangeHandler callback)
            {
                m_callback = callback;
                m_gpioNumber = gpioNumber;
                m_edge = edge;
            }

            public GpioSubscriber(int gpioNumber, GpioEdge edge)
            {
                m_callback = EdgeDetected;
                m_gpioNumber = gpioNumber;
                m_edge = edge;
                m_event = new ManualResetEvent(false);
            }

            void EdgeDetected(int gpioNum, uint level, uint tick)
            {
                m_event.Set();
            }

            public bool WaitForChange(int maxTimeInMs)
            {
                return m_event.WaitOne(maxTimeInMs);
            }

            public GpioLevelChangeHandler Callback
            {
                get
                {
                    return m_callback;
                }
            }

            public int GpioNumber
            {
                get
                {
                    return m_gpioNumber;
                }
            }

            public GpioEdge Edge
            {
                get
                {
                    return m_edge;
                }
            }
            #region IDisposable Support
            private bool disposedValue = false;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        m_event.Close();
                    }

                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(true);
            }
            #endregion
    }

    readonly PiGpioSharp m_pi;
        Thread m_listenerThread;
        uint m_lastLevel;
        bool m_run;
        int m_handle;
        uint m_monitor;
        Socket m_socket;
        Mutex m_lock;
        Dictionary<int, GpioSubscriber> m_changeSubscribers;

        public Gpio(PiGpioSharp pi)
        {
            m_pi = pi;
            m_run = false;
            m_lock = new Mutex();
        }

        public void StartGpioChangeListener()
        {
            if (!m_run)
            {
                m_run = true;
                m_socket = m_pi.Connect();
                m_lastLevel = (uint)m_pi.ExecuteCommand(m_socket, CommandCode.PI_CMD_BR1, 0, 0);
                m_handle = m_pi.ExecuteCommand(m_socket, CommandCode.PI_CMD_NOIB, 0, 0);
                m_monitor = 0;
                m_listenerThread = new Thread(new ParameterizedThreadStart(GpioChangeListener));
                m_changeSubscribers = new Dictionary<int, GpioSubscriber>();
                m_listenerThread.Start();
            }
        }

        public void StopGpioChangeListener()
        {
            if (m_run)
            {
                m_run = false;
                m_pi.ExecuteCommand(CommandCode.PI_CMD_NC, m_handle, 0);
                m_socket.Close();
                m_listenerThread.Join();
            }
        }

        void GpioChangeListener(object param)
        {
            
            int recvSize = 12;
            byte[] buf = new byte[recvSize];
            int messageSize = 12;

            while (m_run == true)
            {
                try
                {
                    var bytesReceived = m_socket.Receive(buf, recvSize, SocketFlags.None);
                    int offset = 0;

                    while ((bytesReceived - offset) >= messageSize)
                    {
                        var seq = (ushort)PiGpioSharp.GetInt16FromByteArrayEndianessCorrected(buf, 0 + offset);
                        var flags = (ushort)PiGpioSharp.GetInt16FromByteArrayEndianessCorrected(buf, 2 + offset);
                        var tick = (uint)PiGpioSharp.GetInt32FromByteArrayEndianessCorrected(buf, 4 + offset);
                        var level = (uint)PiGpioSharp.GetInt32FromByteArrayEndianessCorrected(buf, 8 + offset);
                        offset += messageSize;

                        if (flags == 0)
                        {
                            var changes = m_lastLevel ^ level;
                            m_lastLevel = level;
                            m_lock.WaitOne();
                            var tmpsubscribers = new Dictionary<int, GpioSubscriber>(m_changeSubscribers);
                            m_lock.ReleaseMutex();
                            foreach (var subscriber in tmpsubscribers)
                            {
                                if (((1 << subscriber.Value.GpioNumber) & changes) > 0)
                                {
                                    var newLevel = (uint)0;
                                    if (((1 << subscriber.Value.GpioNumber) & level) > 0)
                                    {
                                        newLevel = 1;
                                    }
                                    if ((newLevel ^ (uint)subscriber.Value.Edge) > 0)
                                    {
                                        subscriber.Value.Callback(subscriber.Value.GpioNumber, newLevel, tick);
                                    }
                                }
                            }
                        }
					}
                    /* TODO: Add other flags */
                }

                catch (SocketException)
                { }
            }
        }

        int AddChangeSubscriber(GpioSubscriber subscriber)
        {
            int hash = subscriber.GetHashCode();
            m_monitor |= (uint)1 << subscriber.GpioNumber;
            m_lock.WaitOne();
            m_changeSubscribers.Add(hash, subscriber);
            m_lock.ReleaseMutex();
            m_pi.ExecuteCommand(CommandCode.PI_CMD_NB, m_handle, (int)m_monitor);
            return hash;
        }

		void RemoveChangeSubscriber(int hash)
		{
            uint newMonitor = 0;
            m_lock.WaitOne();
            try
            {
                m_changeSubscribers.Remove(hash);
                foreach (var s in m_changeSubscribers)
                {
                    newMonitor |= (uint)1 << s.Value.GpioNumber;
                }
            }
            finally
            {
                m_lock.ReleaseMutex();
            }
            if (newMonitor != m_monitor)
            {
                m_monitor = newMonitor;
                m_pi.ExecuteCommand(CommandCode.PI_CMD_NB, m_handle, (int)m_monitor);
            }
		}

        public void SetMode(int gpioNum, GpioMode mode)
        {
            m_pi.ExecuteCommand(CommandCode.PI_CMD_MODES, gpioNum, (int)mode);
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
            m_pi.ExecuteCommand(CommandCode.PI_CMD_WRITE, gpioNum, value);
        }

        public int RegisterLevelChangeCallback(int gpioNum, GpioEdge edge, GpioLevelChangeHandler callback)
        {
            return AddChangeSubscriber(new GpioSubscriber(gpioNum, edge, callback));
        }

        public void UnregisterLevelChangeCallback(int id)
        {
            RemoveChangeSubscriber(id);
        }

        public void WaitForEdge(int gpioNum, GpioEdge edge, int maxWaitingTimeInMs)
        {
            if (!m_run)
            {
                throw new Exception("GPIO Listener Thread not started!");
            }
            var tmpSubscriber = new GpioSubscriber(gpioNum, edge);
            int hash = AddChangeSubscriber(tmpSubscriber);
            var result = tmpSubscriber.WaitForChange(maxWaitingTimeInMs);
            RemoveChangeSubscriber(hash);
            if (!result)
            {
                throw new TimeoutException("Timeout waiting for level change on GPIO " + gpioNum);
            }
        }

    #region IDisposable Support
    private bool disposedValue = false;

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          m_lock.Close();
        }

        disposedValue = true;
      }
    }

    public void Dispose()
    {
      Dispose(true);
    }
    #endregion
  }
}
