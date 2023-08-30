using Meadow.Hardware;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Meadow.Foundation.ICs.IOExpanders
{
	public partial class Pcx857x
	{
		/// <summary>
		/// Pin definitions for 8 pin MCP IO expanders
		/// </summary>
		public class PinDefinitions : IPinDefinitions
		{
			/// <summary>
			/// The controller for the pins
			/// </summary>
			public IPinController Controller { get; set; }

			/// <summary>
			/// List of pins
			/// </summary>
			public IList<IPin> AllPins { get; } = new List<IPin>();

			/// <summary>
			/// Pin R0
			/// </summary>
			public IPin R00 => new Pin(
				Controller,
				nameof(R00), (byte)0x00,
				new List<IChannelInfo> {
					new DigitalChannelInfo(nameof(R00)),
				}
			);

			/// <summary>
			/// Pin R1
			/// </summary>
			public IPin R01 => new Pin(
				Controller,
				nameof(R01), (byte)0x01,
				new List<IChannelInfo> {
					new DigitalChannelInfo(nameof(R01)),
				}
			);

			/// <summary>
			/// Pin R2
			/// </summary>
			public IPin R02 => new Pin(
				Controller,
				nameof(R02), (byte)0x02,
				new List<IChannelInfo> {
					new DigitalChannelInfo(nameof(R02)),
				}
			);


			/// <summary>
			/// Pin R3
			/// </summary>
			public IPin R03 => new Pin(
				Controller,
				nameof(R03), (byte)0x03,
				new List<IChannelInfo> {
					new DigitalChannelInfo(nameof(R03)),
				}
			);


			/// <summary>
			/// Pin R4
			/// </summary>
			public IPin R04 => new Pin(
				Controller,
				nameof(R04), (byte)0x04,
				new List<IChannelInfo> {
					new DigitalChannelInfo(nameof(R04)),
				}
			);

			/// <summary>
			/// Pin R5
			/// </summary>
			public IPin R05 => new Pin(
				Controller,
				nameof(R05), (byte)0x05,
				new List<IChannelInfo> {
					new DigitalChannelInfo(nameof(R05)),
				}
			);

			/// <summary>
			/// Pin R6
			/// </summary>
			public IPin R06 => new Pin(
				Controller,
				nameof(R06), (byte)0x06,
				new List<IChannelInfo> {
					new DigitalChannelInfo(nameof(R06)),
				}
			);

			/// <summary>
			/// Pin R7
			/// </summary>
			public IPin R07 => new Pin(
				Controller,
				nameof(R07), (byte)0x07,
				new List<IChannelInfo> {
					new DigitalChannelInfo(nameof(R07)),
				}
			);


			/// <summary>
			/// Pin R8
			/// </summary>
			public IPin R08 => new Pin(
				Controller,
				nameof(R08), (byte)0x08,
				new List<IChannelInfo> {
					new DigitalChannelInfo(nameof(R08)),
				}
			);

			/// <summary>
			/// Pin R9
			/// </summary>
			public IPin R09 => new Pin(
				Controller,
				nameof(R09), (byte)0x09,
				new List<IChannelInfo> {
					new DigitalChannelInfo(nameof(R09)),
				}
			);


			/// <summary>
			/// Pin R10
			/// </summary>
			public IPin R10 => new Pin(
				Controller,
				nameof(R10), (byte)0x0A,
				new List<IChannelInfo> {
					new DigitalChannelInfo(nameof(R10)),
				}
			);

			/// <summary>
			/// Pin R11
			/// </summary>
			public IPin R11 => new Pin(
				Controller,
				nameof(R11), (byte)0x0B,
				new List<IChannelInfo> {
					new DigitalChannelInfo(nameof(R11)),
				}
			);


			/// <summary>
			/// Pin R1
			/// </summary>
			public IPin R12 => new Pin(
				Controller,
				nameof(R12), (byte)0x0C,
				new List<IChannelInfo> {
					new DigitalChannelInfo(nameof(R12)),
				}
			);

			/// <summary>
			/// Pin R13
			/// </summary>
			public IPin R13 => new Pin(
				Controller,
				nameof(R13), (byte)0x0D,
				new List<IChannelInfo> {
					new DigitalChannelInfo(nameof(R13)),
				}
			);

			/// <summary>
			/// Pin R14
			/// </summary>
			public IPin R14 => new Pin(
				Controller,
				nameof(R14), (byte)0x0E,
				new List<IChannelInfo> {
					new DigitalChannelInfo(nameof(R14)),
				}
			);


			/// <summary>
			/// Pin R15
			/// </summary>
			public IPin R15 => new Pin(
				Controller,
				nameof(R15), (byte)0x0F,
				new List<IChannelInfo> {
					new DigitalChannelInfo(nameof(R15)),
				}
			);


			/// <summary>
			/// Create a new PinDefinitions object
			/// </summary>
			public PinDefinitions(Pcx857x controller)
			{
				Controller = controller;
				InitAllPins();
			}

			/// <summary>
			/// Initalize all pins
			/// </summary>
			protected void InitAllPins()
			{
				// add all our pins to the collection
				AllPins.Add(R00);
				AllPins.Add(R01);
				AllPins.Add(R02);
				AllPins.Add(R03);
				AllPins.Add(R04);
				AllPins.Add(R05);
				AllPins.Add(R06);
				AllPins.Add(R07);
				AllPins.Add(R08);
				AllPins.Add(R09);
				AllPins.Add(R10);
				AllPins.Add(R11);
				AllPins.Add(R12);
				AllPins.Add(R13);
				AllPins.Add(R14);
				AllPins.Add(R15);
			}

			/// <summary>
			/// Get Pins
			/// </summary>
			/// <returns>IEnumerator of IPin with all pins</returns>
			public IEnumerator<IPin> GetEnumerator() => AllPins.GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}