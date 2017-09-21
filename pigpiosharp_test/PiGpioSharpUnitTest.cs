﻿using System;
using NUnit.Framework;

namespace PiGpio.Test
{
	public static class PiGpioSharpUnitTest
	{
		[Test]
		public static void ConstructorWithEnvironmentVariableSet()
		{
			string tst_addr = "raspi";
			string tst_port = "8888";
			Environment.SetEnvironmentVariable("PIGPIO_ADDR", tst_addr);
			Environment.SetEnvironmentVariable("PIGPIO_PORT", tst_port);
			var pi = new PiGpioSharp();
			Assert.That(pi.Port, Is.EqualTo(int.Parse(tst_port)));
			Assert.That(pi.Host, Is.EqualTo(tst_addr));
		}

        [Test]
        public static void DefaultConstructor()
		{
			string tst_addr = "raspi";
			int tst_port = 8888;
            var pi = new PiGpioSharp(tst_addr, tst_port);
			Assert.That(pi.Port, Is.EqualTo(tst_port));
			Assert.That(pi.Host, Is.EqualTo(tst_addr));
		}

		[Test]
		public static void ConstructorWithEnvironmentVariableNotSet()
		{
			Environment.SetEnvironmentVariable("PIGPIO_ADDR", null);
			Environment.SetEnvironmentVariable("PIGPIO_PORT", null);
			Assert.That(() => new PiGpioSharp(), Throws.TypeOf<System.Net.Sockets.SocketException>());
		}
	}
}
