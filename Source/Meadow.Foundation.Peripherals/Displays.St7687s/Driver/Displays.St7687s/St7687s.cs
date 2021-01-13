using Meadow.Foundation;
using Meadow.Foundation.Displays;
using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Displays
{
    public class St7687s : DisplayBase
    {
        public override DisplayColorMode ColorMode => DisplayColorMode.Format16bppRgb565;

        public override uint Width => 128;

        public override uint Height => 128;

        /// <summary>
        ///     SPI object
        /// </summary>
        protected ISpiPeripheral spiPerihperal;

        protected IDigitalOutputPort lckPort;
        protected IDigitalOutputPort chipSelectPort;
        protected IDigitalOutputPort wrPort;
        protected IDigitalOutputPort rsPort;

        /*
         * GND 
         * VCC 3.3-5V
         * LCK  - LCD internal register clock line
         * RS   - LCD internal register selection 
         * CS   - Chip select 
         * WR   - LCD data input
         * SCL  - Clock signal
         * MOSI - Device data output
         */

        public St7687s(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin lckPin, IPin wrPin, IPin rsPin)
        {
            lckPort = device.CreateDigitalOutputPort(lckPin, false);
            wrPort = device.CreateDigitalOutputPort(wrPin, false);
            rsPort = device.CreateDigitalOutputPort(rsPin, false);

            chipSelectPort = device.CreateDigitalOutputPort(chipSelectPin);
            spiPerihperal = new SpiPeripheral(spiBus, chipSelectPort);

            Init();
        }

        void Init()
        {

        }

        void Initialize()
        {
            delay(120);

            writeCmd(0xd7);
            writeDat(0x9f);

            writeCmd(0xE0);
            writeDat(0x00);
            delay(10);

            writeCmd(0xFA);
            writeDat(0x01);
            delay(20);

            writeCmd(0xE3);
            delay(20);
            writeCmd(0xE1);

            writeCmd(0x28);
            writeCmd(0x11);
            delay(30);
            writeCmd(0xc0);
            writeDat(0x17);  //ctrL=0x1b 080416 5PCS 0X1E; 8PCS 0X2A
            writeDat(0x01);

            writeCmd(0x25);
            writeDat(0x1E);
            writeCmd(0xC3);
            writeDat(0x03);

            writeCmd(0xC4);
            writeDat(0x07);

            writeCmd(0xC5);
            writeDat(0x01);

            writeCmd(0xCB);
            writeDat(0x01);

            writeCmd(0xB7);
            writeDat(0x00);

            writeCmd(0xD0);
            writeDat(0x1d);
            writeCmd(0xB5);
            writeDat(0x89);

            writeCmd(0xBD);
            writeDat(0x02);

            writeCmd(0xF0);
            writeDat(0x07);
            writeDat(0x0C);
            writeDat(0x0C);
            writeDat(0x12);

            writeCmd(0xF4);
            writeDat(0x33);
            writeDat(0x33);
            writeDat(0x33);
            writeDat(0x00);
            writeDat(0x33);
            writeDat(0x66);
            writeDat(0x66);
            writeDat(0x66);

            writeCmd(0x20);
            writeCmd(0x2A);
            writeDat(0x00);
            writeDat(0x7F);

            writeCmd(0x2B);
            writeDat(0x00);
            writeDat(0x7f);

            writeCmd(0x3A);
            writeDat(0x05);

            writeCmd(0x36);
            writeDat(0x80); //0xc8

            writeCmd(0xB0);
            writeDat(0x7F);

            writeCmd(0x29);
            ////////	
            writeCmd(0xF9);
            writeDat(0x00);
            writeDat(0x02);
            writeDat(0x04);
            writeDat(0x06);
            writeDat(0x08);
            writeDat(0x0a);
            writeDat(0x0c);
            writeDat(0x0e);
            writeDat(0x10);
            writeDat(0x12);
            writeDat(0x14);
            writeDat(0x16);
            writeDat(0x18);
            writeDat(0x1A);
            writeDat(0x1C);
            writeDat(0x1E);

            writeCmd(0x29);
        }

        void writeCmd(byte command)
        {
            //ST7687S_SPIBEGIN(4000000);
            //digitalWrite(pin_cd, 0);
            //digitalWrite(pin_cs, 0);
            dataCommandPort.State = false;

            //HC595_writeReg(&sHC595, cmd, 1);

           // digitalWrite(pin_wr, 0);
            wrPort.State = false;
            //hc595_delayUS();

            //digitalWrite(pin_wr, 1);
            wrPort.State = true;
            //digitalWrite(pin_cs, 1);
            
            //ST7687S_SPIEND();
        }

        void writeDat(byte data)
        {

        }

        void delay(int ms)
        {
            Thread.Sleep(ms);
        }

        void writeDatBytes(byte[] data, int length)
        {

        }

        void writeToRam()
        {
            writeCmd(0x2c);//wtf?
        }

        void displayON()
        {
            writeCmd(0x29);
        }

        void displayOFF()
        {
            writeCmd(0x28);
        }

        void displaySleepIN()
        {
            writeCmd(0x10);
        }

        void displaySleepOUT()
        {
            writeCmd(0x11);
        }

        public override void Clear(bool updateDisplay = false)
        {
            throw new NotImplementedException();
        }

        public override void DrawPixel(int x, int y, Color color)
        {
            throw new NotImplementedException();
        }

        public override void DrawPixel(int x, int y, bool colored)
        {
            throw new NotImplementedException();
        }

        public override void DrawPixel(int x, int y)
        {
            throw new NotImplementedException();
        }

        public override void SetPenColor(Color pen)
        {
            throw new NotImplementedException();
        }

        public override void Show()
        {
            throw new NotImplementedException();
        }
    }
}