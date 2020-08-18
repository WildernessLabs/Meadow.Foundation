using Meadow.Hardware;
using System;
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
            RF24_1MBPS = 0,
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

        protected ISpiBus spiBus;

        protected IDigitalOutputPort chipEnablePort;

        protected IDigitalOutputPort chipSelectPort;

        protected IDigitalInterruptPort interruptPort;

        protected bool IsInitialized;

        protected byte[] Address;

        public Nrf24l01(
            IIODevice device, 
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
            this.spiBus = spiBus;

            this.chipEnablePort = chipEnablePort;

            this.chipSelectPort = chipSelectPort;

            this.interruptPort = interruptPort;

            // Module reset time
            Thread.Sleep(100);

            IsInitialized = true;

            // Set reasonable default values
            Address = Encoding.UTF8.GetBytes("NRF1");
            dataRate = DataRate.RF24_2MBPS;
            //IsDynamicPayload = true;
            //IsAutoAcknowledge = true;

            //FlushReceiveBuffer();
            //FlushTransferBuffer();
            //ClearIrqMasks();
            //SetRetries(5, 60);
        }

        //        void csn(int mode)
        //        {
        //            // Minimum ideal SPI bus speed is 2x data rate
        //            // If we assume 2Mbs data rate and 16Mhz clock, a
        //            // divider of 4 is the minimum we want.
        //            // CLK:BUS 8Mhz:2Mhz, 16Mhz:4Mhz, or 20Mhz:5Mhz
        //# ifdef ARDUINO
        //            SPI.setBitOrder(MSBFIRST);
        //            SPI.setDataMode(SPI_MODE0);
        //            SPI.setClockDivider(SPI_CLOCK_DIV4);
        //#endif
        //            digitalWrite(csn_pin, mode);
        //        }

        //        void ce(int level)
        //        {
        //            digitalWrite(ce_pin, level);
        //        }

        //        byte read_register(byte reg, byte buf, byte len)
        //        {
        //            byte status;

        //            csn(LOW);
        //            status = SPI.transfer(R_REGISTER | (REGISTER_MASK & reg));
        //            while (len--)
        //                *buf++ = SPI.transfer(0xff);

        //            csn(HIGH);

        //            return status;
        //        }

        //        byte read_register(byte reg)
        //        {
        //            csn(LOW);
        //            SPI.transfer(R_REGISTER | (REGISTER_MASK & reg));
        //            byte result = SPI.transfer(0xff);

        //            csn(HIGH);
        //            return result;
        //        }

        //        byte write_register(byte reg, byte buf, byte len)
        //        {
        //            byte status;

        //            csn(LOW);
        //            status = SPI.transfer(W_REGISTER | (REGISTER_MASK & reg ) );

        //            while (len--)
        //            {
        //                SPI.transfer(buf++);
        //            }

        //            csn(HIGH);

        //            return status;
        //        }

        //        byte write_register(byte reg, byte value)
        //        {
        //            byte status;

        //            csn(LOW);
        //            status = SPI.transfer(W_REGISTER | (REGISTER_MASK & reg));
        //            SPI.transfer(value);
        //            csn(HIGH);

        //            return status;
        //        }

        //        byte write_payload(byte buf, byte len)
        //        {
        //            byte status;

        //            const byte current = reinterpret_cast <const byte*> (buf);

        //            byte data_len = min(len, payload_size);
        //            byte blank_len = dynamic_payloads_enabled ? 0 : payload_size - data_len;

        //            //printf("[Writing %u bytes %u blanks]",data_len,blank_len);

        //            csn(LOW);
        //            status = SPI.transfer(W_TX_PAYLOAD);
        //            while (data_len--)
        //                SPI.transfer(*current++);
        //            while (blank_len--)
        //                SPI.transfer(0);
        //            csn(HIGH);

        //            return status;
        //        }

        //        byte read_payload(void* buf, byte len)
        //        {
        //            byte status;
        //            byte* current = reinterpret_cast<byte*>(buf);

        //            byte data_len = min(len, payload_size);
        //            byte blank_len = dynamic_payloads_enabled ? 0 : payload_size - data_len;

        //            //printf("[Reading %u bytes %u blanks]",data_len,blank_len);

        //            csn(LOW);
        //            status = SPI.transfer(R_RX_PAYLOAD);
        //            while (data_len--)
        //                *current++ = SPI.transfer(0xff);
        //            while (blank_len--)
        //                SPI.transfer(0xff);
        //            csn(HIGH);

        //            return status;
        //        }

        //        byte flush_rx()
        //        {
        //            byte status;

        //            csn(LOW);
        //            status = SPI.transfer(FLUSH_RX);
        //            csn(HIGH);

        //            return status;
        //        }

        //        byte flush_tx()
        //        {
        //            byte status;

        //            csn(LOW);
        //            status = SPI.transfer(FLUSH_TX);
        //            csn(HIGH);

        //            return status;
        //        }

        //        byte get_status()
        //        {
        //            byte status;

        //            csn(LOW);
        //            status = SPI.transfer(NOP);
        //            csn(HIGH);

        //            return status;
        //        }

        //        void print_status(byte status)
        //        {
        //            printf_P(PSTR("STATUS\t\t = 0x%02x RX_DR=%x TX_DS=%x MAX_RT=%x RX_P_NO=%x TX_FULL=%x\r\n"),
        //                     status,
        //                     (status & _BV(RX_DR)) ? 1 : 0,
        //                     (status & _BV(TX_DS)) ? 1 : 0,
        //                     (status & _BV(MAX_RT)) ? 1 : 0,
        //                     ((status >> RX_P_NO) & B111),
        //                     (status & _BV(TX_FULL)) ? 1 : 0
        //                    );
        //        }

        //        void print_observe_tx(byte value)
        //        {
        //            printf_P(PSTR("OBSERVE_TX=%02x: POLS_CNT=%x ARC_CNT=%x\r\n"),
        //                     value,
        //                     (value >> PLOS_CNT) & B1111,
        //                     (value >> ARC_CNT) & B1111
        //                    );
        //        }

        //        void print_byte_register(const char* name, byte reg, byte qty)
        //        {
        //            char extra_tab = strlen_P(name) < 8 ? '\t' : 0;
        //            printf_P(PSTR(PRIPSTR"\t%c ="), name, extra_tab);
        //            while (qty--)
        //                printf_P(PSTR(" 0x%02x"), read_register(reg++));
        //            printf_P(PSTR("\r\n"));
        //        }

        //        void print_address_register(const char* name, byte reg, byte qty)
        //        {
        //            char extra_tab = strlen_P(name) < 8 ? '\t' : 0;
        //            printf_P(PSTR(PRIPSTR"\t%c ="), name, extra_tab);

        //            while (qty--)
        //            {
        //                byte buffer[5];
        //                read_register(reg++, buffer, sizeof buffer);

        //                printf_P(PSTR(" 0x"));
        //                byte* bufptr = buffer + sizeof buffer;
        //                while (--bufptr >= buffer)
        //                    printf_P(PSTR("%02x"), *bufptr);
        //            }

        //            printf_P(PSTR("\r\n"));
        //        }

        //        RF24(byte _cepin, byte _cspin):
        //            ce_pin(_cepin), csn_pin(_cspin), wide_band(true), p_variant(false), 
        //            payload_size(32), ack_payload_available(false), dynamic_payloads_enabled(false),
        //            pipe0_reading_address(0)
        //        {
        //        }

        //        void setChannel(byte channel)
        //        {
        //            // TODO: This method could take advantage of the 'wide_band' calculation
        //            // done in setChannel() to require certain channel spacing.

        //            const byte max_channel = 127;
        //            write_register(RF_CH, min(channel, max_channel));
        //        }

        //        void setPayloadSize(byte size)
        //        {
        //            const byte max_payload_size = 32;
        //            payload_size = min(size, max_payload_size);
        //        }

        //        byte getPayloadSize()
        //        {
        //            return payload_size;
        //        }

        //        void printDetails()
        //        {
        //            print_status(get_status());

        //            print_address_register(PSTR("RX_ADDR_P0-1"), RX_ADDR_P0, 2);
        //            print_byte_register(PSTR("RX_ADDR_P2-5"), RX_ADDR_P2, 4);
        //            print_address_register(PSTR("TX_ADDR"), TX_ADDR);

        //            print_byte_register(PSTR("RX_PW_P0-6"), RX_PW_P0, 6);
        //            print_byte_register(PSTR("EN_AA"), EN_AA);
        //            print_byte_register(PSTR("EN_RXADDR"), EN_RXADDR);
        //            print_byte_register(PSTR("RF_CH"), RF_CH);
        //            print_byte_register(PSTR("RF_SETUP"), RF_SETUP);
        //            print_byte_register(PSTR("CONFIG"), CONFIG);
        //            print_byte_register(PSTR("DYNPD/FEATURE"), DYNPD, 2);

        //            printf_P(PSTR("Data Rate\t = %S\r\n"), pgm_read_word(&rf24_datarate_e_str_P[getDataRate()]));
        //            printf_P(PSTR("Model\t\t = %S\r\n"), pgm_read_word(&rf24_model_e_str_P[isPVariant()]));
        //            printf_P(PSTR("CRC Length\t = %S\r\n"), pgm_read_word(&rf24_crclength_e_str_P[getCRCLength()]));
        //            printf_P(PSTR("PA Power\t = %S\r\n"), pgm_read_word(&rf24_pa_dbm_e_str_P[getPALevel()]));
        //        }

        //        void begin()
        //        {
        //            // Initialize pins
        //            pinMode(ce_pin, OUTPUT);
        //            pinMode(csn_pin, OUTPUT);

        //            // Initialize SPI bus
        //            SPI.begin();

        //            ce(LOW);
        //            csn(HIGH);

        //            // Must allow the radio time to settle else configuration bits will not necessarily stick.
        //            // This is actually only required following power up but some settling time also appears to
        //            // be required after resets too. For full coverage, we'll always assume the worst.
        //            // Enabling 16b CRC is by far the most obvious case if the wrong timing is used - or skipped.
        //            // Technically we require 4.5ms + 14us as a worst case. We'll just call it 5ms for good measure.
        //            // WARNING: Delay is based on P-variant whereby non-P *may* require different timing.
        //            delay(5);

        //            // Set 1500uS (minimum for 32B payload in ESB@250KBPS) timeouts, to make testing a little easier
        //            // WARNING: If this is ever lowered, either 250KBS mode with AA is broken or maximum packet
        //            // sizes must never be used. See documentation for a more complete explanation.
        //            write_register(SETUP_RETR, (B0100 << ARD) | (B1111 << ARC));

        //            // Restore our default PA level
        //            setPALevel(RF24_PA_MAX);

        //            // Determine if this is a p or non-p RF24 module and then
        //            // reset our data rate back to default value. This works
        //            // because a non-P variant won't allow the data rate to
        //            // be set to 250Kbps.
        //            if (setDataRate(RF24_250KBPS))
        //            {
        //                p_variant = true;
        //            }

        //            // Then set the data rate to the slowest (and most reliable) speed supported by all
        //            // hardware.
        //            setDataRate(RF24_1MBPS);

        //            // Initialize CRC and request 2-byte (16bit) CRC
        //            setCRCLength(RF24_CRC_16);

        //            // Disable dynamic payloads, to match dynamic_payloads_enabled setting
        //            write_register(DYNPD, 0);

        //            // Reset current status
        //            // Notice reset and flush is the last thing we do
        //            write_register(STATUS, _BV(RX_DR) | _BV(TX_DS) | _BV(MAX_RT));

        //            // Set up default configuration.  Callers can always change it later.
        //            // This channel should be universally safe and not bleed over into adjacent
        //            // spectrum.
        //            setChannel(76);

        //            // Flush buffers
        //            flush_rx();
        //            flush_tx();
        //        }

        //        void startListening()
        //        {
        //            write_register(CONFIG, read_register(CONFIG) | _BV(PWR_UP) | _BV(PRIM_RX));
        //            write_register(STATUS, _BV(RX_DR) | _BV(TX_DS) | _BV(MAX_RT));

        //            // Restore the pipe0 adddress, if exists
        //            if (pipe0_reading_address)
        //                write_register(RX_ADDR_P0, reinterpret_cast <const byte*> (&pipe0_reading_address), 5);

        //            // Flush buffers
        //            flush_rx();
        //            flush_tx();

        //            // Go!
        //            ce(HIGH);

        //            // wait for the radio to come up (130us actually only needed)
        //            delayMicroseconds(130);
        //        }

        //        void stopListening()
        //        {
        //            ce(LOW);
        //            flush_tx();
        //            flush_rx();
        //        }

        //        void powerDown()
        //        {
        //            write_register(CONFIG, read_register(CONFIG) & ~_BV(PWR_UP));
        //        }

        //        void powerUp()
        //        {
        //            write_register(CONFIG, read_register(CONFIG) | _BV(PWR_UP));
        //        }

        //        bool write( const void* buf, byte len)
        //        {
        //            bool result = false;

        //            // Begin the write
        //            startWrite(buf, len);

        //            // ------------
        //            // At this point we could return from a non-blocking write, and then call
        //            // the rest after an interrupt

        //            // Instead, we are going to block here until we get TX_DS (transmission completed and ack'd)
        //            // or MAX_RT (maximum retries, transmission failed).  Also, we'll timeout in case the radio
        //            // is flaky and we get neither.

        //            // IN the end, the send should be blocking.  It comes back in 60ms worst case, or much faster
        //            // if I tighted up the retry logic.  (Default settings will be 1500us.
        //            // Monitor the send
        //            byte observe_tx;
        //            byte status;
        //            uint32_t sent_at = millis();
        //            const uint32_t timeout = 500; //ms to wait for timeout
        //            do
        //            {
        //                status = read_register(OBSERVE_TX, &observe_tx, 1);
        //                IF_SERIAL_DEBUG(Serial.print(observe_tx, HEX));
        //            }
        //            while (!(status & (_BV(TX_DS) | _BV(MAX_RT))) && (millis() - sent_at < timeout));

        //            // The part above is what you could recreate with your own interrupt handler,
        //            // and then call this when you got an interrupt
        //            // ------------

        //            // Call this when you get an interrupt
        //            // The status tells us three things
        //            // * The send was successful (TX_DS)
        //            // * The send failed, too many retries (MAX_RT)
        //            // * There is an ack packet waiting (RX_DR)
        //            bool tx_ok, tx_fail;
        //            whatHappened(tx_ok, tx_fail, ack_payload_available);

        //            //printf("%u%u%u\r\n",tx_ok,tx_fail,ack_payload_available);

        //            result = tx_ok;
        //            IF_SERIAL_DEBUG(Serial.print(result ? "...OK." : "...Failed"));

        //            // Handle the ack packet
        //            if (ack_payload_available)
        //            {
        //                ack_payload_length = getDynamicPayloadSize();
        //                IF_SERIAL_DEBUG(Serial.print("[AckPacket]/"));
        //                IF_SERIAL_DEBUG(Serial.println(ack_payload_length, DEC));
        //            }

        //            // Yay, we are done.

        //            // Power down
        //            powerDown();

        //            // Flush buffers (Is this a relic of past experimentation, and not needed anymore??)
        //            flush_tx();

        //            return result;
        //        }

        //        void startWrite( const void* buf, byte len)
        //        {
        //            // Transmitter power-up
        //            write_register(CONFIG, (read_register(CONFIG) | _BV(PWR_UP)) & ~_BV(PRIM_RX));
        //            delayMicroseconds(150);

        //            // Send the payload
        //            write_payload(buf, len);

        //            // Allons!
        //            ce(HIGH);
        //            delayMicroseconds(15);
        //            ce(LOW);
        //        }

        //        byte getDynamicPayloadSize()
        //        {
        //            byte result = 0;

        //            csn(LOW);
        //            SPI.transfer(R_RX_PL_WID);
        //            result = SPI.transfer(0xff);
        //            csn(HIGH);

        //            return result;
        //        }

        //        bool available()
        //        {
        //            return available(NULL);
        //        }

        //        bool available(byte pipe_num)
        //        {
        //            byte status = get_status();

        //            // Too noisy, enable if you really want lots o data!!
        //            //IF_SERIAL_DEBUG(print_status(status));

        //            bool result = (status & _BV(RX_DR));

        //            if (result)
        //            {
        //                // If the caller wants the pipe number, include that
        //                if (pipe_num)
        //                    *pipe_num = (status >> RX_P_NO) & B111;

        //                // Clear the status bit

        //                // ??? Should this REALLY be cleared now?  Or wait until we
        //                // actually READ the payload?

        //                write_register(STATUS, _BV(RX_DR));

        //                // Handle ack payload receipt
        //                if (status & _BV(TX_DS))
        //                {
        //                    write_register(STATUS, _BV(TX_DS));
        //                }
        //            }

        //            return result;
        //        }

        //        bool read(void* buf, byte len)
        //        {
        //            // Fetch the payload
        //            read_payload(buf, len);

        //            // was this the last of the data available?
        //            return read_register(FIFO_STATUS) & _BV(RX_EMPTY);
        //        }

        //        void whatHappened(bool& tx_ok, bool& tx_fail, bool& rx_ready)
        //        {
        //            // Read the status & reset the status in one easy call
        //            // Or is that such a good idea?
        //            byte status = write_register(STATUS, _BV(RX_DR) | _BV(TX_DS) | _BV(MAX_RT));

        //            // Report to the user what happened
        //            tx_ok = status & _BV(TX_DS);
        //            tx_fail = status & _BV(MAX_RT);
        //            rx_ready = status & _BV(RX_DR);
        //        }

        //        void openWritingPipe(uint64_t value)
        //        {
        //            // Note that AVR 8-bit uC's store this LSB first, and the NRF24L01(+)
        //            // expects it LSB first too, so we're good.

        //            write_register(RX_ADDR_P0, reinterpret_cast<byte*>(&value), 5);
        //            write_register(TX_ADDR, reinterpret_cast<byte*>(&value), 5);

        //            const byte max_payload_size = 32;
        //            write_register(RX_PW_P0, min(payload_size, max_payload_size));
        //        }

        //    enum AdressSlot : byte
        //    {
        //        RX_ADDR_P0, RX_ADDR_P1, RX_ADDR_P2, RX_ADDR_P3, RX_ADDR_P4, RX_ADDR_P5
        //    };

        //    enum PayloadSize : byte
        //    {
        //        RX_PW_P0, RX_PW_P1, RX_PW_P2, RX_PW_P3, RX_PW_P4, RX_PW_P5
        //    };

        //    enum EnablePipe : byte
        //    {
        //        ERX_P0, ERX_P1, ERX_P2, ERX_P3, ERX_P4, ERX_P5
        //    };

        //    void openReadingPipe(byte child, uint64_t address)
        //    {
        //        // If this is pipe 0, cache the address.  This is needed because
        //        // openWritingPipe() will overwrite the pipe 0 address, so
        //        // startListening() will have to restore it.
        //        if (child == 0)
        //            pipe0_reading_address = address;

        //        if (child <= 6)
        //        {
        //            // For pipes 2-5, only write the LSB
        //            if (child < 2)
        //                write_register(pgm_read_byte(&child_pipe[child]), reinterpret_cast <const byte*> (&address), 5);
        //        else
        //                write_register(pgm_read_byte(&child_pipe[child]), reinterpret_cast <const byte*> (&address), 1);

        //            write_register(pgm_read_byte(&child_payload_size[child]), payload_size);

        //            // Note it would be more efficient to set all of the bits for all open
        //            // pipes at once.  However, I thought it would make the calling code
        //            // more simple to do it this way.
        //            write_register(EN_RXADDR, read_register(EN_RXADDR) | _BV(pgm_read_byte(&child_pipe_enable[child])));
        //        }
        //    }

        //    void toggle_features()
        //    {
        //        csn(LOW);
        //        SPI.transfer(ACTIVATE);
        //        SPI.transfer(0x73);
        //        csn(HIGH);
        //    }

        //    void enableDynamicPayloads()
        //    {
        //        // Enable dynamic payload throughout the system
        //        write_register(FEATURE, read_register(FEATURE) | _BV(EN_DPL));

        //        // If it didn't work, the features are not enabled
        //        if (!read_register(FEATURE))
        //        {
        //            // So enable them and try again
        //            toggle_features();
        //            write_register(FEATURE, read_register(FEATURE) | _BV(EN_DPL));
        //        }

        //        IF_SERIAL_DEBUG(printf("FEATURE=%i\r\n", read_register(FEATURE)));

        //        // Enable dynamic payload on all pipes
        //        //
        //        // Not sure the use case of only having dynamic payload on certain
        //        // pipes, so the library does not support it.
        //        write_register(DYNPD, read_register(DYNPD) | _BV(DPL_P5) | _BV(DPL_P4) | _BV(DPL_P3) | _BV(DPL_P2) | _BV(DPL_P1) | _BV(DPL_P0));

        //        dynamic_payloads_enabled = true;
        //    }

        //    void enableAckPayload()
        //    {
        //        //
        //        // enable ack payload and dynamic payload features
        //        //

        //        write_register(FEATURE, read_register(FEATURE) | _BV(EN_ACK_PAY) | _BV(EN_DPL));

        //        // If it didn't work, the features are not enabled
        //        if (!read_register(FEATURE))
        //        {
        //            // So enable them and try again
        //            toggle_features();
        //            write_register(FEATURE, read_register(FEATURE) | _BV(EN_ACK_PAY) | _BV(EN_DPL));
        //        }

        //        IF_SERIAL_DEBUG(printf("FEATURE=%i\r\n", read_register(FEATURE)));

        //        //
        //        // Enable dynamic payload on pipes 0 & 1
        //        //

        //        write_register(DYNPD, read_register(DYNPD) | _BV(DPL_P1) | _BV(DPL_P0));
        //    }

        //    void writeAckPayload(byte pipe, const void* buf, byte len)
        //    {
        //        const byte* current = reinterpret_cast <const byte*> (buf);

        //        csn(LOW);
        //        SPI.transfer(W_ACK_PAYLOAD | (pipe & B111));
        //        const byte max_payload_size = 32;
        //        byte data_len = min(len, max_payload_size);
        //        while (data_len--)
        //            SPI.transfer(*current++);

        //        csn(HIGH);
        //    }

        //    bool isAckPayloadAvailable()
        //    {
        //        bool result = ack_payload_available;
        //        ack_payload_available = false;
        //        return result;
        //    }

        //    bool isPVariant()
        //    {
        //        return p_variant;
        //    }

        //    void setAutoAck(bool enable)
        //    {
        //        if (enable)
        //            write_register(EN_AA, B111111);
        //        else
        //            write_register(EN_AA, 0);
        //    }

        //    void setAutoAck(byte pipe, bool enable)
        //    {
        //        if (pipe <= 6)
        //        {
        //            byte en_aa = read_register(EN_AA);
        //            if (enable)
        //            {
        //                en_aa |= _BV(pipe);
        //            }
        //            else
        //            {
        //                en_aa &= ~_BV(pipe);
        //            }
        //            write_register(EN_AA, en_aa);
        //        }
        //    }

        //    bool testCarrier()
        //    {
        //        return (read_register(CD) & 1);
        //    }

        //    bool testRPD()
        //    {
        //        return (read_register(RPD) & 1);
        //    }

        //    void setPALevel(rf24_pa_dbm_e level)
        //    {
        //        byte setup = read_register(RF_SETUP);
        //        setup &= ~(_BV(RF_PWR_LOW) | _BV(RF_PWR_HIGH));

        //        // switch uses RAM (evil!)
        //        if (level == RF24_PA_MAX)
        //        {
        //            setup |= (_BV(RF_PWR_LOW) | _BV(RF_PWR_HIGH));
        //        }
        //        else if (level == RF24_PA_HIGH)
        //        {
        //            setup |= _BV(RF_PWR_HIGH);
        //        }
        //        else if (level == RF24_PA_LOW)
        //        {
        //            setup |= _BV(RF_PWR_LOW);
        //        }
        //        else if (level == RF24_PA_MIN)
        //        {
        //            // nothing
        //        }
        //        else if (level == RF24_PA_ERROR)
        //        {
        //            // On error, go to maximum PA
        //            setup |= (_BV(RF_PWR_LOW) | _BV(RF_PWR_HIGH));
        //        }

        //        write_register(RF_SETUP, setup);
        //    }

        //    Nrf24l01.PowerAmplifierLevel getPALevel()
        //    {
        //        rf24_pa_dbm_e result = RF24_PA_ERROR;
        //        byte power = read_register(RF_SETUP) & (_BV(RF_PWR_LOW) | _BV(RF_PWR_HIGH));

        //        // switch uses RAM (evil!)
        //        if (power == (_BV(RF_PWR_LOW) | _BV(RF_PWR_HIGH)))
        //        {
        //            result = RF24_PA_MAX;
        //        }
        //        else if (power == _BV(RF_PWR_HIGH))
        //        {
        //            result = RF24_PA_HIGH;
        //        }
        //        else if (power == _BV(RF_PWR_LOW))
        //        {
        //            result = RF24_PA_LOW;
        //        }
        //        else
        //        {
        //            result = RF24_PA_MIN;
        //        }

        //        return result;
        //    }

        //    bool setDataRate(rf24_datarate_e speed)
        //    {
        //        bool result = false;
        //        byte setup = read_register(RF_SETUP);

        //        // HIGH and LOW '00' is 1Mbs - our default
        //        wide_band = false;
        //        setup &= ~(_BV(RF_DR_LOW) | _BV(RF_DR_HIGH));
        //        if (speed == RF24_250KBPS)
        //        {
        //            // Must set the RF_DR_LOW to 1; RF_DR_HIGH (used to be RF_DR) is already 0
        //            // Making it '10'.
        //            wide_band = false;
        //            setup |= _BV(RF_DR_LOW);
        //        }
        //        else
        //        {
        //            // Set 2Mbs, RF_DR (RF_DR_HIGH) is set 1
        //            // Making it '01'
        //            if (speed == RF24_2MBPS)
        //            {
        //                wide_band = true;
        //                setup |= _BV(RF_DR_HIGH);
        //            }
        //            else
        //            {
        //                // 1Mbs
        //                wide_band = false;
        //            }
        //        }
        //        write_register(RF_SETUP, setup);

        //        // Verify our result
        //        if (read_register(RF_SETUP) == setup)
        //        {
        //            result = true;
        //        }
        //        else
        //        {
        //            wide_band = false;
        //        }

        //        return result;
        //    }

        //    Nrf24l01.DataRate getDataRate(void )
        //    {
        //        rf24_datarate_e result;
        //        byte dr = read_register(RF_SETUP) & (_BV(RF_DR_LOW) | _BV(RF_DR_HIGH));

        //        // switch uses RAM (evil!)
        //        // Order matters in our case below
        //        if (dr == _BV(RF_DR_LOW))
        //        {
        //            // '10' = 250KBPS
        //            result = RF24_250KBPS;
        //        }
        //        else if (dr == _BV(RF_DR_HIGH))
        //        {
        //            // '01' = 2MBPS
        //            result = RF24_2MBPS;
        //        }
        //        else
        //        {
        //            // '00' = 1MBPS
        //            result = RF24_1MBPS;
        //        }
        //        return result;
        //    }

        //    void setCRCLength(rf24_crclength_e length)
        //    {
        //        byte config = read_register(CONFIG) & ~(_BV(CRCO) | _BV(EN_CRC));

        //        // switch uses RAM (evil!)
        //        if (length == RF24_CRC_DISABLED)
        //        {
        //            // Do nothing, we turned it off above. 
        //        }
        //        else if (length == RF24_CRC_8)
        //        {
        //            config |= _BV(EN_CRC);
        //        }
        //        else
        //        {
        //            config |= _BV(EN_CRC);
        //            config |= _BV(CRCO);
        //        }
        //        write_register(CONFIG, config);
        //    }

        //    rf24_crclength_e getCRCLength()
        //    {
        //        rf24_crclength_e result = RF24_CRC_DISABLED;
        //        byte config = read_register(CONFIG) & (_BV(CRCO) | _BV(EN_CRC));

        //        if (config & _BV(EN_CRC))
        //        {
        //            if (config & _BV(CRCO))
        //                result = RF24_CRC_16;
        //            else
        //                result = RF24_CRC_8;
        //        }

        //        return result;
        //    }

        //    void disableCRC(void )
        //    {
        //        byte disable = read_register(CONFIG) & ~_BV(EN_CRC);
        //        write_register(CONFIG, disable);
        //    }

        //    void setRetries(byte delay, byte count)
        //    {
        //        write_register(SETUP_RETR, (delay & 0xf) << ARD | (count & 0xf) << ARC);
        //    }    
    }
}