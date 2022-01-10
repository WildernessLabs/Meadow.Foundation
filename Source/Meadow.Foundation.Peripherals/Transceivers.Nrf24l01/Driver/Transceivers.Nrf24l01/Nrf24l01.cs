using Meadow.Devices;
using Meadow.Hardware;
using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace Meadow.Foundation.Transceivers
{
    public class Nrf24l01
    {
        /* Memory Map */
        static byte NRF_CONFIG = 0x00;
        static byte EN_AA = 0x01;
        static byte EN_RXADDR = 0x02;
        static byte SETUP_AW = 0x03;
        static byte SETUP_RETR = 0x04;
        static byte RF_CH = 0x05;
        static byte RF_SETUP = 0x06;
        static byte NRF_STATUS = 0x07;
        static byte OBSERVE_TX = 0x08;
        static byte CD = 0x09;
        static byte RX_ADDR_P0 = 0x0A;
        static byte RX_ADDR_P1 = 0x0B;
        static byte RX_ADDR_P2 = 0x0C;
        static byte RX_ADDR_P3 = 0x0D;
        static byte RX_ADDR_P4 = 0x0E;
        static byte RX_ADDR_P5 = 0x0F;
        static byte TX_ADDR = 0x10;
        static byte RX_PW_P0 = 0x11;
        static byte RX_PW_P1 = 0x12;
        static byte RX_PW_P2 = 0x13;
        static byte RX_PW_P3 = 0x14;
        static byte RX_PW_P4 = 0x15;
        static byte RX_PW_P5 = 0x16;
        static byte FIFO_STATUS = 0x17;
        static byte DYNPD = 0x1C;
        static byte FEATURE = 0x1D;

        /* Bit Mnemonics */
        static byte MASK_RX_DR = 6;
        static byte MASK_TX_DS = 5;
        static byte MASK_MAX_RT = 4;
        static byte EN_CRC = 3;
        static byte CRCO = 2;
        static byte PWR_UP = 1;
        static byte PRIM_RX = 0;
        static byte ENAA_P5 = 5;
        static byte ENAA_P4 = 4;
        static byte ENAA_P3 = 3;
        static byte ENAA_P2 = 2;
        static byte ENAA_P1 = 1;
        static byte ENAA_P0 = 0;
        static byte ERX_P5 = 5;
        static byte ERX_P4 = 4;
        static byte ERX_P3 = 3;
        static byte ERX_P2 = 2;
        static byte ERX_P1 = 1;
        static byte ERX_P0 = 0;
        static byte AW = 0;
        static byte ARD = 4;
        static byte ARC = 0;
        static byte PLL_LOCK = 4;
        static byte CONT_WAVE = 7;
        static byte RF_DR = 3;
        static byte RF_PWR = 6;
        static byte RX_DR = 6;
        static byte TX_DS = 5;
        static byte MAX_RT = 4;
        static byte RX_P_NO = 1;
        static byte TX_FULL = 0;
        static byte PLOS_CNT = 4;
        static byte ARC_CNT = 0;
        static byte TX_REUSE = 6;
        static byte FIFO_FULL = 5;
        static byte TX_EMPTY = 4;
        static byte RX_FULL = 1;
        static byte RX_EMPTY = 0;
        static byte DPL_P5 = 5;
        static byte DPL_P4 = 4;
        static byte DPL_P3 = 3;
        static byte DPL_P2 = 2;
        static byte DPL_P1 = 1;
        static byte DPL_P0 = 0;
        static byte EN_DPL = 2;
        static byte EN_ACK_PAY = 1;
        static byte EN_DYN_ACK = 0;

        /* Instruction Mnemonics */
        static byte R_REGISTER = 0x00;
        static byte W_REGISTER = 0x20;
        static byte REGISTER_MASK = 0x1F;
        static byte ACTIVATE = 0x50;
        static byte R_RX_PL_WID = 0x60;
        static byte R_RX_PAYLOAD = 0x61;
        static byte W_TX_PAYLOAD = 0xA0;
        static byte W_ACK_PAYLOAD = 0xA8;
        static byte FLUSH_TX = 0xE1;
        static byte FLUSH_RX = 0xE2;
        static byte REUSE_TX_PL = 0xE3;
        static byte RF24_NOP = 0xFF;

        /* Non-P omissions */
        static byte LNA_HCURR = 0;

        /* P model memory Map */
        static byte RPD = 0x09;
        static byte W_TX_PAYLOAD_NO_ACK = 0xB0;

        /* P model bit Mnemonics */
        static byte RF_DR_LOW = 5;
        static byte RF_DR_HIGH = 3;
        static byte RF_PWR_LOW = 1;
        static byte RF_PWR_HIGH = 2;

        public enum PowerAmplifierLevel : byte
        {
            RF24_PA_MIN = 0,
            RF24_PA_LOW,
            RF24_PA_HIGH,
            RF24_PA_MAX,
            RF24_PA_ERROR
        }

        public enum DataRate : byte
        {
            RF24_1MBPS,
            RF24_2MBPS,
            RF24_250KBPS
        }
        protected DataRate dataRate;

        public enum CrcLength : byte
        {
            RF24_CRC_DISABLED = 0,
            RF24_CRC_8,
            RF24_CRC_16
        }

        public enum PipeEnabled : byte
        { 
            ERX_P0,
            ERX_P1,
            ERX_P2,
            ERX_P3,
            ERX_P4,
            ERX_P5,
        }
        protected PipeEnabled pipeEnabled;

        protected ISpiBus spiBus;

        protected ISpiPeripheral rf24;

        protected IDigitalOutputPort chipEnablePort;

        protected IDigitalInterruptPort interruptPort;

        protected bool IsInitialized;

        protected byte[] Address;

        int csDelay, txDelay;

        bool dynamic_payloads_enabled;

        bool ack_payloads_enabled;

        byte config_reg;
       //byte addr_width;
        byte payload_size = 32;
        readonly byte[] address = new byte[6];
        readonly byte[] pipe0_reading_address = new byte[5];
        readonly Memory<byte> readBuffer = new byte[256]; //ToDo ... check size

        public Nrf24l01(
            IMeadowDevice device, 
            ISpiBus spiBus, 
            IPin chipEnablePin, 
            IPin chipSelectLine, 
            IPin interruptPin)
            : this (
                  spiBus, 
                  device.CreateDigitalOutputPort(chipEnablePin), 
                  device.CreateDigitalOutputPort(chipSelectLine), 
                  device.CreateDigitalInputPort(interruptPin))
        { }

        public Nrf24l01(
            ISpiBus spiBus,
            IDigitalOutputPort chipEnablePort,
            IDigitalOutputPort chipSelectPort,
            IDigitalInterruptPort interruptPort)
        {
            pipe0_reading_address[0] = 0;

            this.spiBus = spiBus;
            rf24 = new SpiPeripheral(spiBus, chipSelectPort);

            this.chipEnablePort = chipEnablePort;
            this.interruptPort = interruptPort;

            Initialize();
        }

        void Initialize()
        {
            chipEnablePort.State = false;

            Thread.Sleep(5);

            SetRetries(5, 15);

            SetDataRate(DataRate.RF24_1MBPS);

            ToggleFeatures();
            WriteRegister(FEATURE, 0);
            WriteRegister(DYNPD, 0);
            dynamic_payloads_enabled = false;
            ack_payloads_enabled = false;

            WriteRegister(NRF_STATUS, (byte) (1 << RX_DR | 1 << TX_DS | 1 << MAX_RT));

            SetChannel(76);

            FlushRx();
            FlushTx();

            config_reg = rf24.ReadRegister(NRF_CONFIG);

            WriteRegister(NRF_CONFIG, (byte)(1 << EN_CRC | 1 << CRCO));

            PowerUp();

            config_reg = ReadRegister(NRF_CONFIG);

            Console.WriteLine($"Initialize - Expected: {(byte)(1 << EN_CRC | 1 << CRCO | 1 << PWR_UP)} Result: {config_reg}");
        }

        void SetRetries(byte delay, byte count)
        {
            WriteRegister(SETUP_RETR, (byte)((delay & 0xf) << ARD | (count & 0xf) << ARC));
        }

        void SetDataRate(DataRate speed)
        {
            txDelay = 85;

            byte setup = ReadRegister(RF_SETUP);

            switch (speed)
            {
                case DataRate.RF24_1MBPS:
                    setup &= (byte)~(1 << RF_DR_LOW);  // 0
                    setup &= (byte)~(1 << RF_DR_HIGH); // 0
                    break;

                case DataRate.RF24_2MBPS:
                    setup &= (byte)~(1 << RF_DR_LOW);  // 0
                    setup |= (byte)(1 << RF_DR_HIGH);  // 1
                    break;

                case DataRate.RF24_250KBPS:
                    setup |= (byte)(1 << RF_DR_LOW);    // 1
                    setup &= (byte)~(1 << RF_DR_HIGH);  // 0
                    break;

                default:
                    throw new ArgumentOutOfRangeException("DataRate", speed, "An invalid DataRate was specified");
            }

            WriteRegister(RF_SETUP, setup);
        }

        void ToggleFeatures()
        {
            rf24.Write(ACTIVATE);
            rf24.Write(0x73);
        }

        public void SetChannel(byte channel)
        {
            const byte max_channel = 125;
            WriteRegister(RF_CH, Math.Min(channel, max_channel));            
            Console.WriteLine($"SetChannel - RF_CH: {ReadRegister(RF_CH)}");
        }

        public byte GetChannel()
        {
            return ReadRegister(RF_CH);
        }

        void FlushRx()
        {
            rf24.Write(FLUSH_RX);
        }

        void FlushTx()
        {
            rf24.Write(FLUSH_TX);
        }

        void PowerDown()
        {
            chipEnablePort.State = false;

            var regValue = rf24.ReadRegister(NRF_CONFIG);
            WriteRegister(NRF_CONFIG, (byte)(regValue | (0u << PWR_UP)));

            Thread.Sleep(5);
        }

        void PowerUp()
        {
            var regValue = rf24.ReadRegister(NRF_CONFIG);
            WriteRegister(NRF_CONFIG, (byte)(regValue | (1u << PWR_UP)));

            Thread.Sleep(5);
        }

        public void OpenReadingPipe(byte child, byte[] address)
        {
            if(child > 5)
            {
                throw new ArgumentOutOfRangeException("OpenReadingPipe child must be between 0 & 5 inclusive");
            }

            Array.Copy(address, pipe0_reading_address, address.Length);

            switch (child)
            {
                case 0:
                    WriteRegisters(RX_ADDR_P0, address);
                    WriteRegister(RX_PW_P0, payload_size);
                    break;
                case 1:
                    WriteRegisters(RX_ADDR_P1, address);
                    WriteRegister(RX_PW_P1, payload_size);
                    break;
                case 2:
                    WriteRegisters(RX_ADDR_P2, address);
                    WriteRegister(RX_PW_P2, payload_size);
                    break;
                case 3:
                    WriteRegisters(RX_ADDR_P3, address);
                    WriteRegister(RX_PW_P3, payload_size);
                    break;
                case 4:
                    WriteRegisters(RX_ADDR_P4, address);
                    WriteRegister(RX_PW_P4, payload_size);
                    break;
                case 5:
                    WriteRegisters(RX_ADDR_P5, address);
                    WriteRegister(RX_PW_P5, payload_size);
                    break;
            }

            //ToDo make less suck
            byte rxaddr = ReadRegister(EN_RXADDR);
            WriteRegister(EN_RXADDR, (byte)(rxaddr | 1 << ERX_P0));
        }

        public void CloseReadingPipe(byte child) 
        {
            var enrxaddr = ReadRegister(EN_RXADDR);

            byte childPipe = 0;
            switch (child)
            {
                case 0:
                    childPipe = (byte)(1 << ERX_P0);
                    break;
                case 1:
                    childPipe = (byte)(1 << ERX_P1);
                    break;
                case 2:
                    childPipe = (byte)(1 << ERX_P2);
                    break;
                case 3:
                    childPipe = (byte)(1 << ERX_P3);
                    break;
                case 4:
                    childPipe = (byte)(1 << ERX_P4);
                    break;
                case 5:
                    childPipe = (byte)(1 << ERX_P5);
                    break;
            }

            WriteRegister(EN_RXADDR, (byte)(enrxaddr & ~childPipe));
        }

        public void SetPALevel(byte level, byte lnaEnable = 1)
        {
            byte setup = (byte)(ReadRegister(RF_SETUP) & 0xF8);

            if (level > 3)
            {
                level = (byte)(((byte)PowerAmplifierLevel.RF24_PA_MAX << 1) + lnaEnable);
            }
            else
            {
                level = (byte)((level << 1) + lnaEnable);
            }

            byte value = setup |= level;            

            WriteRegister(RF_SETUP, value);
            Console.WriteLine($"SetPALevel - RF_SETUP: {ReadRegister(RF_SETUP)}");
        }

        void WriteRegisters(byte address, byte[] data)
        {
            rf24.WriteRegister((byte)(W_REGISTER | (REGISTER_MASK & address)), data);
        }

        void WriteRegister(byte address, byte value)
        {
            rf24.WriteRegister((byte)(W_REGISTER | (REGISTER_MASK & address)), value);
        }

        byte ReadRegister(byte address)
        {
            return rf24.ReadRegister((byte)(R_REGISTER | (REGISTER_MASK & address)));
        }

        public void StartListening()
        {
            config_reg |= (byte) (1 << PRIM_RX);
            WriteRegister(NRF_CONFIG, config_reg);
            WriteRegister(NRF_STATUS, (byte)(1 << RX_DR | 1 << TX_DS | 1 << MAX_RT));

            chipEnablePort.State = true;

            if (pipe0_reading_address[0] > 0)
            {
                WriteRegisters(RX_ADDR_P0, pipe0_reading_address);            
            }
            else
            {
                CloseReadingPipe(0);
            }

            if (ack_payloads_enabled)
            {
                FlushTx();
            }
        }

        public bool IsAvailable()
        {
            return IsAvailable(null);
        }

        bool IsAvailable(byte[] pipe_num)
        {
            if ((byte)(ReadRegister(FIFO_STATUS) & (1 << RX_EMPTY)) == 0)
            {
                if(pipe_num != null)
                {
                    byte status = GetStatus();
                    //    *pipe_num = (status >> RX_P_NO) & 0x07;
                }

                return true;
            }

            return false;
        }

        //ToDo clean this up
        public byte[] Read(byte length)
        {
            var data = ReadPayload(readBuffer.Span, length);
            WriteRegister(NRF_STATUS, (byte) (1 << RX_DR | 1 << MAX_RT | 1 << TX_DS));

            return readBuffer.Span[0..length].ToArray();
        }

        byte ReadPayload(Span<byte> buffer, byte length)
        {
            byte status;
            byte[] current = buffer.ToArray();

            if (length > payload_size)
            {
                length = payload_size;
            }

            byte blank_len = 0;

            if (!dynamic_payloads_enabled)
            {
                blank_len = (byte)(payload_size - length);
            }
            
            Console.WriteLine($"ReadPayload - data_len: {length}");
            Console.WriteLine($"ReadPayload - blank_len: {blank_len}");

            status = rf24.ReadRegister(R_RX_PAYLOAD);

            Console.Write($"ReadPayload - payload:");

            rf24.ReadRegister(R_RX_PAYLOAD, readBuffer.Span[0..length]);

            
            for (int i = 0; i < length; i++)
            {
                if (readBuffer.Span[i] == 0)
                {
                    current[i] = 32;
                    Console.Write(current[i]);
                }
                else
                {
                    current[i] = readBuffer.Span[i];
                    Console.Write(current[i]);
                }
            }

            Console.WriteLine("");

            Console.WriteLine($"ReadPayload - data: {Encoding.UTF8.GetString(current, 0, current.Length).Trim()}");

            Console.WriteLine("");

            while (blank_len > 0)
            {
                rf24.Write(0xFF);
                blank_len--;
            }

            Console.WriteLine($"ReadPayload - status: {status}");
            
            return status;
        }
        
        public void OpenWritingPipe(byte[] address)
        {
            WriteRegisters(RX_ADDR_P0, address);
            WriteRegisters(TX_ADDR, address);

            WriteRegister(RX_PW_P0, payload_size);
        }

        public void StopListening()
        {
            Thread.Sleep(5);

            byte value = (byte)(ReadRegister(FEATURE) & (1 << EN_ACK_PAY));

            if (value > 0) 
            {
                Thread.Sleep(5);
                FlushTx();
            }

            config_reg &= (byte)(~(1 << PRIM_RX));
            WriteRegister(NRF_CONFIG, (byte)(ReadRegister(NRF_CONFIG) & ~(1 << PRIM_RX)));
            WriteRegister(EN_RXADDR, (byte)(ReadRegister(EN_RXADDR) | 1 << ERX_P0));            
        }

        public bool Write(byte[] buf, byte len)
        {
            return Write(buf, len, false);
        }

        bool Write(byte[] buf, byte len, bool multicast)
        {
            StartFastWrite(buf, len, multicast, true);

            byte getStatus = GetStatus();
            byte x = (byte)(1 << TX_DS | 1 << MAX_RT);

            Console.WriteLine($"Write - status: {getStatus} Something: {x}");

            //while (!(get_status() & (_BV(TX_DS) | _BV(MAX_RT)))) { }

            //ce(LOW);

            WriteRegister(NRF_STATUS, (byte)(1 << RX_DR | 1 << TX_DS | 1 << MAX_RT));
            byte status = ReadRegister(NRF_STATUS);

            Console.WriteLine($"Write - status: {status} Something: {(byte)(1 << MAX_RT)}");

            byte r = (byte)(status & (byte)(1 << MAX_RT));
            if (r == 0)
            {
                FlushTx(); //Only going to be 1 packet int the FIFO at a time using this method, so just flush
                return false;
            }
            
            return true;
        }        

        void StartFastWrite(byte[] buf, byte len, bool multicast, bool startTx)
        { 
            WritePayload(buf, len, multicast? W_TX_PAYLOAD_NO_ACK : W_TX_PAYLOAD);
            if (startTx) 
            {
                //ce(HIGH);
            }
        }

        void WritePayload(byte[] buf, byte data_len, byte writeType)
        { 
            byte[] current = buf.ToArray();

            data_len = Math.Min(data_len, payload_size);

            byte blank_len = 0;
            if (dynamic_payloads_enabled)
                blank_len = (byte)(payload_size - data_len);

            rf24.Write(writeType);
            rf24.Write(current);
        }

        byte GetStatus()
        {
            return rf24.ReadRegister(RF24_NOP);
        }

        //void setPayloadSize(byte size)
        //{
        //    payload_size = rf24_min(size, 32);
        //}

        //byte getPayloadSize()
        //{
        //    return payload_size;
        //}       

        //bool isChipConnected()
        //{
        //    byte setup = read_register(SETUP_AW);
        //    if (setup >= 1 && setup <= 3)
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        //static const byte child_pipe_enable[]
        //PROGMEM = {ERX_P0, ERX_P1, ERX_P2, ERX_P3, ERX_P4, ERX_P5};           

        ////For general use, the interrupt flags are not important to clear
        //bool writeBlocking(byte buf, byte len, uint timeout)
        //{
        //    //Block until the FIFO is NOT full.
        //    //Keep track of the MAX retries and set auto-retry if seeing failures
        //    //This way the FIFO will fill up and allow blocking until packets go through
        //    //The radio will auto-clear everything in the FIFO as long as CE remains high

        //    uint timer = millis();                              //Get the time that the payload transmission started

        //    while ((get_status()
        //            & (_BV(TX_FULL))))
        //    {          //Blocking only if FIFO is full. This will loop and block until TX is successful or timeout

        //        if (get_status() & _BV(MAX_RT))
        //        {                      //If MAX Retries have been reached
        //            reUseTX();                                          //Set re-transmit and clear the MAX_RT interrupt flag
        //            if (millis() - timer > timeout)
        //            {
        //                return 0;
        //            }          //If this payload has exceeded the user-defined timeout, exit and return 0
        //        }
        //    }

        //    //Start Writing
        //    startFastWrite(buf, len, 0);                                  //Write the payload if a buffer is clear

        //    return 1;                                                  //Return 1 to indicate successful transmission
        //}

        //void reUseTX()
        //{
        //    write_register(NRF_STATUS, _BV(MAX_RT));              //Clear max retry flag
        //    spiTrans(REUSE_TX_PL);
        //    ce(LOW);                                          //Re-Transfer packet
        //    ce(HIGH);
        //}

        //bool writeFast(byte buf, byte len, bool multicast)
        //{
        //    //Block until the FIFO is NOT full.
        //    //Keep track of the MAX retries and set auto-retry if seeing failures
        //    //Return 0 so the user can control the retrys and set a timer or failure counter if required
        //    //The radio will auto-clear everything in the FIFO as long as CE remains high

        //    //Blocking only if FIFO is full. This will loop and block until TX is successful or fail
        //    while ((get_status() & (_BV(TX_FULL))))
        //    {
        //        if (get_status() & _BV(MAX_RT))
        //        {
        //            return 0; //Return 0. The previous payload has been retransmitted
        //            // From the user perspective, if you get a 0, just keep trying to send the same payload
        //        }
        //    }
        //    //Start Writing
        //    startFastWrite(buf, len, multicast);

        //    return 1;
        //}

        //bool writeFast(byte buf, byte len)
        //{
        //    return writeFast(buf, len, 0);
        //}

        ////Per the documentation, we want to set PTX Mode when not listening. Then all we do is write data and set CE high
        ////In this mode, if we can keep the FIFO buffers loaded, packets will transmit immediately (no 130us delay)
        ////Otherwise we enter Standby-II mode, which is still faster than standby mode
        ////Also, we remove the need to keep writing the config register over and over and delaying for 150 us each time if sending a stream of data

        //void startFastWrite(byte buf, byte len, const bool multicast, bool startTx)
        //{ //TMRh20
        //    //write_payload( buf,len);
        //    write_payload(buf, len, multicast ? W_TX_PAYLOAD_NO_ACK : W_TX_PAYLOAD);
        //    if (startTx)
        //    {
        //        ce(HIGH);
        //    }
        //}

        ////Added the original startWrite back in so users can still use interrupts, ack payloads, etc
        ////Allows the library to pass all tests
        //void startWrite(byte buf, byte len, const bool multicast)
        //{
        //    // Send the payload

        //    //write_payload( buf, len );
        //    write_payload(buf, len, multicast ? W_TX_PAYLOAD_NO_ACK : W_TX_PAYLOAD);
        //    ce(HIGH);
        //#if !defined(F_CPU) || F_CPU > 20000000
        //    delayMicroseconds(10);
        //#endif
        //    ce(LOW);
        //}

        //bool rxFifoFull()
        //{
        //    return read_register(FIFO_STATUS) & _BV(RX_FULL);
        //}

        //bool txStandBy()
        //{
        //    while (!(read_register(FIFO_STATUS) & _BV(TX_EMPTY)))
        //    {
        //        if (get_status() & _BV(MAX_RT))
        //        {
        //            write_register(NRF_STATUS, _BV(MAX_RT));
        //            ce(LOW);
        //            flush_tx();    //Non blocking, flush the data
        //            return 0;
        //        }
        //    }

        //    ce(LOW);               //Set STANDBY-I mode
        //    return 1;
        //}

        //bool txStandBy(uint timeout, bool startTx)
        //{
        //    if (startTx)
        //    {
        //        stopListening();
        //        ce(HIGH);
        //    }
        //    uint start = millis();

        //    while (!(read_register(FIFO_STATUS) & _BV(TX_EMPTY)))
        //    {
        //        if (get_status() & _BV(MAX_RT))
        //        {
        //            write_register(NRF_STATUS, _BV(MAX_RT));
        //            ce(LOW); // Set re-transmit
        //            ce(HIGH);
        //            if (millis() - start >= timeout)
        //            {
        //                ce(LOW);
        //                flush_tx();
        //                return 0;
        //            }
        //        }
        //    }

        //    ce(LOW);  //Set STANDBY-I mode
        //    return 1;
        //}

        //void maskIRQ(bool tx, bool fail, bool rx)
        //{
        //    /* clear the interrupt flags */
        //    config_reg &= ~(1 << MASK_MAX_RT | 1 << MASK_TX_DS | 1 << MASK_RX_DR);
        //    /* set the specified interrupt flags */
        //    config_reg |= fail << MASK_MAX_RT | tx << MASK_TX_DS | rx << MASK_RX_DR;
        //    write_register(NRF_CONFIG, config_reg);
        //}

        //byte getDynamicPayloadSize()
        //{
        //    byte result = 0;

        //    beginTransaction();
        //    _SPI.transfer(R_RX_PL_WID);
        //    result = _SPI.transfer(0xff);
        //    endTransaction();

        //    if (result > 32)
        //    {
        //        flush_rx();
        //        delay(2);
        //        return 0;
        //    }

        //    return result;
        //}

        //void whatHappened(bool& tx_ok, bool& tx_fail, bool& rx_ready)
        //{
        //    // Read the status & reset the status in one easy call
        //    // Or is that such a good idea?
        //    byte status = write_register(NRF_STATUS, _BV(RX_DR) | _BV(TX_DS) | _BV(MAX_RT));

        //    // Report to the user what happened
        //    tx_ok = status & _BV(TX_DS);
        //    tx_fail = status & _BV(MAX_RT);
        //    rx_ready = status & _BV(RX_DR);
        //}

        //void openWritingPipe(uint64_t value)
        //{
        //    // Note that AVR 8-bit uC's store this LSB first, and the NRF24L01(+)
        //    // expects it LSB first too, so we're good.

        //    write_register(RX_ADDR_P0, reinterpret_cast<byte*>(&value), addr_width);
        //    write_register(TX_ADDR, reinterpret_cast<byte*>(&value), addr_width);

        //    //const byte max_payload_size = 32;
        //    //write_register(RX_PW_P0,rf24_min(payload_size,max_payload_size));
        //    write_register(RX_PW_P0, payload_size);
        //}        

        //static const byte child_pipe[]
        //PROGMEM = {RX_ADDR_P0, RX_ADDR_P1, RX_ADDR_P2, RX_ADDR_P3, RX_ADDR_P4, RX_ADDR_P5};
        //static const byte child_payload_size[]
        //PROGMEM = {RX_PW_P0, RX_PW_P1, RX_PW_P2, RX_PW_P3, RX_PW_P4, RX_PW_P5};

        //void openReadingPipe(byte child, uint64_t address)
        //{
        //    // If this is pipe 0, cache the address.  This is needed because
        //    // openWritingPipe() will overwrite the pipe 0 address, so
        //    // startListening() will have to restore it.
        //    if (child == 0)
        //    {
        //        memcpy(pipe0_reading_address, &address, addr_width);
        //    }

        //    if (child <= 5)
        //    {
        //        // For pipes 2-5, only write the LSB
        //        if (child < 2)
        //        {
        //            write_register(pgm_read_byte(&child_pipe[child]), reinterpret_cast <const byte*> (&address), addr_width);
        //        }
        //        else
        //        {
        //            write_register(pgm_read_byte(&child_pipe[child]), reinterpret_cast <const byte*> (&address), 1);
        //        }

        //        write_register(pgm_read_byte(&child_payload_size[child]), payload_size);

        //        // Note it would be more efficient to set all of the bits for all open
        //        // pipes at once.  However, I thought it would make the calling code
        //        // more simple to do it this way.
        //        write_register(EN_RXADDR, read_register(EN_RXADDR) | _BV(pgm_read_byte(&child_pipe_enable[child])));
        //    }
        //}

        //void setAddressWidth(byte a_width)
        //{
        //    if (a_width -= 2)
        //    {
        //        write_register(SETUP_AW, a_width % 4);
        //        addr_width = (a_width % 4) + 2;
        //    }
        //    else
        //    {
        //        write_register(SETUP_AW, 0);
        //        addr_width = 2;
        //    }
        //}

        //void closeReadingPipe(byte pipe)
        //{
        //    write_register(EN_RXADDR, read_register(EN_RXADDR) & ~_BV(pgm_read_byte(&child_pipe_enable[pipe])));
        //}

        //void enableDynamicPayloads()
        //{
        //    // Enable dynamic payload throughout the system

        //    //toggle_features();
        //    write_register(FEATURE, read_register(FEATURE) | _BV(EN_DPL));

        //    // Enable dynamic payload on all pipes
        //    //
        //    // Not sure the use case of only having dynamic payload on certain
        //    // pipes, so the library does not support it.
        //    write_register(DYNPD, read_register(DYNPD) | _BV(DPL_P5) | _BV(DPL_P4) | _BV(DPL_P3) | _BV(DPL_P2) | _BV(DPL_P1) | _BV(DPL_P0));

        //    dynamic_payloads_enabled = true;
        //}

        //void disableDynamicPayloads()
        //{
        //    // Disables dynamic payload throughout the system.  Also disables Ack Payloads

        //    //toggle_features();
        //    write_register(FEATURE, 0);

        //    // Disable dynamic payload on all pipes
        //    //
        //    // Not sure the use case of only having dynamic payload on certain
        //    // pipes, so the library does not support it.
        //    write_register(DYNPD, 0);

        //    dynamic_payloads_enabled = false;
        //    ack_payloads_enabled = false;
        //}

        //void enableAckPayload()
        //{
        //    //
        //    // enable ack payload and dynamic payload features
        //    //

        //    //toggle_features();
        //    write_register(FEATURE, read_register(FEATURE) | _BV(EN_ACK_PAY) | _BV(EN_DPL));

        //    //
        //    // Enable dynamic payload on pipes 0 & 1
        //    //
        //    write_register(DYNPD, read_register(DYNPD) | _BV(DPL_P1) | _BV(DPL_P0));
        //    dynamic_payloads_enabled = true;
        //    ack_payloads_enabled = true;
        //}

        //void enableDynamicAck()
        //{
        //    //
        //    // enable dynamic ack features
        //    //
        //    //toggle_features();
        //    write_register(FEATURE, read_register(FEATURE) | _BV(EN_DYN_ACK));
        //}

        //void writeAckPayload(byte pipe, byte buf, byte len)
        //{
        //    const byte* current = reinterpret_cast <const byte*> (buf);

        //    byte data_len = rf24_min(len, 32);

        //    beginTransaction();
        //    _SPI.transfer(W_ACK_PAYLOAD | (pipe & 0x07));

        //    while (data_len--)
        //    {
        //        _SPI.transfer(*current++);
        //    }
        //    endTransaction();
        //}

        //bool isAckPayloadAvailable()
        //{
        //    return available(NULL);
        //}

        //bool isPVariant()
        //{
        //    rf24_datarate_e dR = getDataRate();
        //    bool result = setDataRate(RF24_250KBPS);
        //    setDataRate(dR);
        //    return result;
        //}

        //void setAutoAck(bool enable)
        //{
        //    if (enable)
        //    {
        //        write_register(EN_AA, 0x3F);
        //    }
        //    else
        //    {
        //        write_register(EN_AA, 0);
        //    }
        //}

        //void setAutoAck(byte pipe, bool enable)
        //{
        //    if (pipe <= 6)
        //    {
        //        byte en_aa = read_register(EN_AA);
        //        if (enable)
        //        {
        //            en_aa |= _BV(pipe);
        //        }
        //        else
        //        {
        //            en_aa &= ~_BV(pipe);
        //        }
        //        write_register(EN_AA, en_aa);
        //    }
        //}

        //bool testCarrier()
        //{
        //    return (read_register(CD) & 1);
        //}

        //bool testRPD()
        //{
        //    return (read_register(RPD) & 1);
        //}

        //byte getPALevel()
        //{
        //    return (read_register(RF_SETUP) & (_BV(RF_PWR_LOW) | _BV(RF_PWR_HIGH))) >> 1;
        //}

        //byte getARC()
        //{
        //    return read_register(OBSERVE_TX) & 0x0F;
        //}

        //rf24_datarate_e getDataRate()
        //{
        //    rf24_datarate_e result;
        //    byte dr = read_register(RF_SETUP) & (_BV(RF_DR_LOW) | _BV(RF_DR_HIGH));

        //    // switch uses RAM (evil!)
        //    // Order matters in our case below
        //    if (dr == _BV(RF_DR_LOW))
        //    {
        //        // '10' = 250KBPS
        //        result = RF24_250KBPS;
        //    }
        //    else if (dr == _BV(RF_DR_HIGH))
        //    {
        //        // '01' = 2MBPS
        //        result = RF24_2MBPS;
        //    }
        //    else
        //    {
        //        // '00' = 1MBPS
        //        result = RF24_1MBPS;
        //    }
        //    return result;
        //}

        //void setCRCLength(rf24_crclength_e length)
        //{
        //    config_reg &= ~(_BV(CRCO) | _BV(EN_CRC));

        //    // switch uses RAM (evil!)
        //    if (length == RF24_CRC_DISABLED)
        //    {
        //        // Do nothing, we turned it off above.
        //    }
        //    else if (length == RF24_CRC_8)
        //    {
        //        config_reg |= _BV(EN_CRC);
        //    }
        //    else
        //    {
        //        config_reg |= _BV(EN_CRC);
        //        config_reg |= _BV(CRCO);
        //    }

        //    write_register(NRF_CONFIG, config_reg);
        //}

        //rf24_crclength_e getCRCLength()
        //{
        //    rf24_crclength_e result = RF24_CRC_DISABLED;
        //    byte AA = read_register(EN_AA);
        //    config_reg = read_register(NRF_CONFIG);

        //    if (config_reg & _BV(EN_CRC) || AA)
        //    {
        //        if (config_reg & _BV(CRCO))
        //        {
        //            result = RF24_CRC_16;
        //        }
        //        else
        //        {
        //            result = RF24_CRC_8;
        //        }
        //    }

        //    return result;
        //}

        //void disableCRC()
        //{
        //    config_reg &= ~_BV(EN_CRC);
        //    write_register(NRF_CONFIG, config_reg);
        //}        

        //void startConstCarrier(rf24_pa_dbm_e level, byte channel)
        //{
        //    write_register(RF_SETUP, (read_register(RF_SETUP)) | _BV(CONT_WAVE));
        //    write_register(RF_SETUP, (read_register(RF_SETUP)) | _BV(PLL_LOCK));
        //    setPALevel(level);
        //    setChannel(channel);
        //    ce(HIGH);
        //}

        //void stopConstCarrier()
        //{
        //    write_register(RF_SETUP, (read_register(RF_SETUP)) & ~_BV(CONT_WAVE));
        //    write_register(RF_SETUP, (read_register(RF_SETUP)) & ~_BV(PLL_LOCK));
        //    ce(LOW);
        //}
    }
}