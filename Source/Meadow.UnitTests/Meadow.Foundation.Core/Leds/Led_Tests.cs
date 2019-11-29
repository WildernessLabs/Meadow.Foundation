using AutoFixture.Xunit2;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace Meadow.UnitTests.Meadow.Foundation.Core.Leds
{
	public class Led_Tests
	{
		[Theory]
		[AutoData]
		public void StartBlink_Sets_Pin_State(Mock<IDigitalOutputPort> port)
		{
			var sut = new Led(port.Object);

			sut.StartBlink();
			Thread.Sleep(500);
			sut.Stop();

			port.VerifySet(o => o.State = It.IsAny<bool>());
		}

		[Theory]
		[AutoData]
		public void StartBlink_Sets_Pin_State_And_Led_IsOn(Mock<IDigitalOutputPort> port)
		{
			var sut = new Led(port.Object);

			var stateToggleTracking = new List<bool>();
			port.SetupSet(o => o.State = It.IsAny<bool>())
				.Callback<bool>(o =>
				{
					stateToggleTracking.Add(sut.IsOn == o);
				});

			sut.StartBlink();
			Thread.Sleep(500);
			sut.Stop();

			port.VerifySet(o => o.State = It.IsAny<bool>());
			Assert.DoesNotContain(stateToggleTracking, o => o == false);
		}

		[Theory]
		[AutoData]
		public void Stop_Turns_Led_Off(Mock<IDigitalOutputPort> port)
		{
			var sut = new Led(port.Object);

			sut.StartBlink();
			Thread.Sleep(500);
			sut.Stop();

			port.VerifySet(o => o.State = It.IsAny<bool>());
			Assert.False(sut.IsOn);
		}


		[Theory]
		[AutoData]
		public void IsOn_Sets_Pin_State(Mock<IDigitalOutputPort> port)
		{
			var sut = new Led(port.Object);

			sut.IsOn = true;

			port.VerifySet(o => o.State = It.IsAny<bool>());
		}
	}
}
