using System;
using Meadow.Hardware;
using System.Threading;

namespace Meadow.Foundation.Sensors.Camera
{
    /// <summary>
    /// *** Driver is untested but should be fully working ***
    /// Represents the MLX90640 32x24 IR array.
    /// The MLX90640 is a fully calibrated 32x24 pixels thermal IR array.
    /// </summary>
    /// <remarks>
    /// Based on https://github.com/adafruit/Adafruit_MLX90640 and https://github.com/melexis/mlx90640-library/tree/master/functions
    /// </remarks>
    public class Mlx90640
    {
        public enum Mode
        {
            Interleaved,
            Chess
        }

        public enum Resolution
        {
            SixteenBit,
            SeventeenBit,
            EighteenBit,
            NineteenBit
        }

        public enum RefreshRate
        {
            HalfHZ,
            OneHZ,
            TwoHZ,
            FourHZ,
            EightHZ,
            SixteenHZ,
            ThirtyTwoHZ,
            SixtyFourHZ
        }

        public enum Units
        {
            Celsius,
            Fahrenheit,
            Kelvin
        }

        public string SerialNumber { get; private set; }
        public float Emissivity { get => emissivity; 
            set
            {
                if (value > 1)
                    emissivity = 1;
                else if (value < 0.01)
                    emissivity = 0.01f;
                else
                    emissivity = value;
            } 
        }
        public float ReflectedTemperature { get; set; }
        public Units MeasurementUnit { get; set; }
        public Mlx90640Config Config { get; private set; }

        const short MaxBufferSize = 32;
        const float ScaleAlpha = 0.000001f;
        const byte OpenAirTaShift = 8;
        const short DeviceId1Register = 0x2407;

        readonly II2cPeripheral i2CPeripheral;

        float emissivity;

        public Mlx90640(II2cBus i2cBus, byte address = 0x33, Units measurementUnit = Units.Celsius, float emissivity = 0.95f)
        {
            i2CPeripheral = new I2cPeripheral(i2cBus, address);
            Emissivity = emissivity;
            MeasurementUnit = measurementUnit;
            ReflectedTemperature = 23.15f;
            Config = new Mlx90640Config();
        }

        /// <summary>
        /// Initialize the MLX90640. Read the MLX90640 serial number and EEProm
        /// </summary>
        public virtual void Initialize()
        {
            ushort[] serialNumber = new ushort[3];
            I2CRead(DeviceId1Register, 3, ref serialNumber);

            SerialNumber = string.Empty;
            foreach (var b in serialNumber)
                SerialNumber = SerialNumber + " " + b.ToString("X2");

            ushort[] eeMLX90640 = new ushort[832];

            ReadEEProm(ref eeMLX90640);

            ExtractParameters(ref eeMLX90640);
        }

        /// <summary>
        /// Gets all 768 sensor readings
        /// </summary>
        /// <returns>Float array containing each individual reading</returns>
        public float[] Read()
        {
            ushort[] mlx90640Frame = new ushort[834];
            float[] framebuf =new float[32 * 24];

            for (byte page = 0; page < 2; page++)
            {
                GetFrameData(ref mlx90640Frame);

                // For a MLX90640 in the open air the shift is -8
                ReflectedTemperature = GetTa(mlx90640Frame, Config) - OpenAirTaShift;

                CalculateTo(mlx90640Frame, Config, Emissivity, ReflectedTemperature, ref framebuf);
            }

            return framebuf;
        }

        /// <summary>
        /// Set the reading pattern mode
        /// </summary>
        /// <param name="mode">Chess or Interleaved</param>
        public void SetMode(Mode mode)
        {
            if (mode == Mode.Chess)
                SetChessMode();
            else
                SetInterleavedMode();
        }

        /// <summary>
        /// Get the current reading pattern mode
        /// </summary>
        /// <returns>Chess or Interleaved</returns>
        public Mode GetMode()
        {
            return (Mode)GetCurrentMode();
        }

        /// <summary>
        /// Sets the resolution
        /// </summary>
        /// <param name="res">Resolution type</param>
        public void SetResolution(Resolution res)
        {
            SetResolution((byte)res);
        }

        /// <summary>
        /// Get the current resolution mode
        /// </summary>
        /// <returns>Resolution mode</returns>
        public Resolution GetResolution()
        {
            return (Resolution)GetCurrentResolution();
        }

        /// <summary>
        /// Gets the Refresh rate
        /// </summary>
        /// <returns>RefreshRate type</returns>
        public RefreshRate GetRefreshRate()
        {
            return (RefreshRate)GetCurrentRefreshRate();
        }

        /// <summary>
        /// Sets the refresh rate
        /// </summary>
        /// <param name="rate">RefreshRate type</param>
        public void SetRefreshRate(RefreshRate rate)
        {
            SetCurrentRefreshRate((byte)rate);
        }

        protected virtual bool GetFrameData(ref ushort[] frameData)
        {
            ushort dataReady = 1;
            ushort[] controlRegister1 = new ushort[1];
            ushort[] statusRegister = new ushort[1];
            bool error = false;
            byte cnt = 0;

            dataReady = 0;
            while (dataReady == 0)
            {
                I2CRead(0x8000, 1, ref statusRegister);
                dataReady = (ushort)(statusRegister[0] & 0x0008);
            }

            while (dataReady != 0 && cnt < 5)
            {
                error = I2CWrite(0x8000, 0x0030);

                //if (!error)
                //    return false;

                I2CRead(0x0400, 832, ref frameData);

                I2CRead(0x8000, 1, ref statusRegister);

                dataReady = (ushort)(statusRegister[0] & 0x0008);

                cnt = (byte)(cnt + 1);
            }

            if (cnt > 4)
                return false;

           I2CRead(0x800D, 1, ref controlRegister1);
            frameData[832] = controlRegister1[0];
            frameData[833] = (ushort)(statusRegister[0] & 0x0001);

            return true;
        }

        protected virtual void I2CRead(int startAddress, ushort readLen, ref ushort[] data)
        {
            byte[] cmd = new byte[2];
            ushort bufferIndex = 0;

            while (readLen > 0)
            {
                ushort toRead16 = (int)(MaxBufferSize / 2);

                if (readLen < (int)(MaxBufferSize / 2))
                    toRead16 = readLen;

                cmd[0] = (byte)(startAddress >> 8);
                cmd[1] = (byte)(startAddress & 0x00FF);

                Span<byte> writeBuffer = new Span<byte>(cmd);
                Span<byte> tempBuf = new Span<byte>(new byte[toRead16 * 2]);
                i2CPeripheral.Exchange(writeBuffer, tempBuf);

                // we now have to swap every two bytes
                int index = 0;
                for (int i = bufferIndex; i < bufferIndex + toRead16; i++)
                {
                    data[i] = (ushort)((ushort)(tempBuf[index] * 256) + (ushort)tempBuf[index + 1]); ;
                    index = index + 2;
                }

                bufferIndex += toRead16;
                startAddress += toRead16;
                readLen -= toRead16;
            }
        }

        protected virtual bool I2CWrite(ushort writeAddress, ushort data)
        {
            byte[] cmd = new byte[4];
            ushort[] dataCheck = new ushort[1];

            cmd[0] = (byte)(writeAddress >> 8);
            cmd[1] = (byte)(writeAddress & 0x00FF);
            cmd[2] = (byte)(data >> 8);
            cmd[3] = (byte)(data & 0x00FF);

            i2CPeripheral.WriteBytes(cmd);

            Thread.Sleep(1);

            I2CRead(writeAddress, 1, ref dataCheck);

            // check echo
            if (dataCheck[0] != data)
                return false;

            // OK!
            return true;
        }

        void CalculateTo(ushort[] frameData, Mlx90640Config mlx90640, float emissivity, float tr, ref float[] result)
        {
            float vdd;
            float ta;
            float ta4;
            float tr4;
            float taTr;
            float gain;
            float[] irDataCP = new float[2];
            float irData;
            float alphaCompensated;
            byte mode;
            sbyte ilPattern;
            sbyte chessPattern;
            sbyte pattern;
            sbyte conversionPattern;
            float Sx;
            float To;
            float[] alphaCorrR = new float[4];
            sbyte range;
            ushort subPage;
            float ktaScale;
            float kvScale;
            float alphaScale;
            float kta;
            float kv;

            subPage = frameData[833];
            vdd = GetVdd(frameData, mlx90640);
            ta = GetTa(frameData, mlx90640);

            ta4 = (ta + 273.15f);
            ta4 = ta4 * ta4;
            ta4 = ta4 * ta4;
            tr4 = (tr + 273.15f);
            tr4 = tr4 * tr4;
            tr4 = tr4 * tr4;
            taTr = tr4 - (tr4 - ta4) / emissivity;

            ktaScale = (float)(Math.Pow(2, (double)mlx90640.KtaScale));
            kvScale = (float)(Math.Pow(2, (double)mlx90640.KvScale));
            alphaScale = (float)(Math.Pow(2, (double)mlx90640.AlphaScale));

            alphaCorrR[0] = 1 / (1 + mlx90640.KsTo[0] * 40);
            alphaCorrR[1] = 1;
            alphaCorrR[2] = (1 + mlx90640.KsTo[1] * mlx90640.Ct[2]);
            alphaCorrR[3] = alphaCorrR[2] * (1 + mlx90640.KsTo[2] * (mlx90640.Ct[3] - mlx90640.Ct[2]));

            //Gain calculation 
            gain = frameData[778];
            if (gain > 32767)
                gain = gain - 65536;

            gain = mlx90640.GainEE / gain;

            //To calculation   
            mode = (byte)((frameData[832] & 0x1000) >> 5);

            irDataCP[0] = frameData[776];
            irDataCP[1] = frameData[808];
            for (int i = 0; i < 2; i++)
            {
                if (irDataCP[i] > 32767)
                    irDataCP[i] = irDataCP[i] - 65536;
                
                irDataCP[i] = irDataCP[i] * gain;
            }

            irDataCP[0] = (float)(irDataCP[0] - mlx90640.CpOffset[0] * (1 + mlx90640.CpKta * (ta - 25)) * (1 + mlx90640.CpKv * (vdd - 3.3)));

            if (mode == mlx90640.CalibrationModeEE)
                irDataCP[1] = (float)(irDataCP[1] - mlx90640.CpOffset[1] * (1 + mlx90640.CpKta * (ta - 25)) * (1 + mlx90640.CpKv * (vdd - 3.3)));
            else
                irDataCP[1] = (float)(irDataCP[1] - (mlx90640.CpOffset[1] + mlx90640.IlChessC[0]) * (1 + mlx90640.CpKta * (ta - 25)) * (1 + mlx90640.CpKv * (vdd - 3.3)));

            for (int pixelNumber = 0; pixelNumber < 768; pixelNumber++)
            {
                ilPattern = (sbyte)(pixelNumber / 32 - (pixelNumber / 64) * 2);
                chessPattern = (sbyte)(ilPattern ^ (pixelNumber - (pixelNumber / 2) * 2));
                conversionPattern = (sbyte)(((pixelNumber + 2) / 4 - (pixelNumber + 3) / 4 + (pixelNumber + 1) / 4 - pixelNumber / 4) * (1 - 2 * ilPattern));

                if (mode == 0)
                    pattern = ilPattern;
                else
                    pattern = chessPattern;

                if (pattern == frameData[833])
                {
                    irData = frameData[pixelNumber];
                    if (irData > 32767)
                        irData = irData - 65536;

                    irData = irData * gain;

                    kta = mlx90640.Kta[pixelNumber] / ktaScale;
                    kv = mlx90640.Kv[pixelNumber] / kvScale;
                    irData = (float)(irData - mlx90640.Offset[pixelNumber] * (1 + kta * (ta - 25)) * (1 + kv * (vdd - 3.3)));

                    if (mode != mlx90640.CalibrationModeEE)
                        irData = irData + mlx90640.IlChessC[2] * (2 * ilPattern - 1) - mlx90640.IlChessC[1] * conversionPattern;

                    irData = irData - mlx90640.Tgc * irDataCP[subPage];
                    irData = irData / emissivity;

                    alphaCompensated = ScaleAlpha * alphaScale / mlx90640.Alpha[pixelNumber];
                    alphaCompensated = alphaCompensated * (1 + mlx90640.KsTa * (ta - 25));

                    Sx = alphaCompensated * alphaCompensated * alphaCompensated * (irData + alphaCompensated * taTr);
                    Sx = (float)(Math.Sqrt(Math.Sqrt(Sx)) * mlx90640.KsTo[1]);

                    To = (float)(Math.Sqrt(Math.Sqrt(irData / (alphaCompensated * (1 - mlx90640.KsTo[1] * 273.15) + Sx) + taTr)) - 273.15);

                    if (To < mlx90640.Ct[1])
                        range = 0;
                    else if (To < mlx90640.Ct[2])
                        range = 1;
                    else if (To < mlx90640.Ct[3])
                        range = 2;
                    else
                        range = 3;

                    To = (float)(Math.Sqrt(Math.Sqrt(irData / (alphaCompensated * alphaCorrR[range] * (1 + mlx90640.KsTo[range] * (To - mlx90640.Ct[range]))) + taTr)) - 273.15);

                    if(MeasurementUnit == Units.Fahrenheit)
                        result[pixelNumber] = To * 9 / 5 + 32;
                    else if (MeasurementUnit == Units.Kelvin)
                        result[pixelNumber] = To - 273.15f;
                    else 
                        result[pixelNumber] = To;
                }
            }
        }

        float GetTa(ushort[] frameData, Mlx90640Config mlx90640)
        {
            float ptat;
            float ptatArt;
            float vdd;
            float ta;

            vdd = GetVdd(frameData, mlx90640);

            ptat = frameData[800];
            if (ptat > 32767)
                ptat = ptat - 65536;

            ptatArt = frameData[768];
            if (ptatArt > 32767)
                ptatArt = ptatArt - 65536;
            
            ptatArt = (float)((ptat / (ptat * mlx90640.AlphaPTAT + ptatArt)) * Math.Pow(2, (double)18));

            ta = (float)((ptatArt / (1 + mlx90640.KvPTAT * (vdd - 3.3)) - mlx90640.VPTAT25));
            ta = ta / mlx90640.KtPTAT + 25;

            return ta;
        }

        float GetVdd(ushort[] frameData, Mlx90640Config mlx90640)
        {
            float vdd;
            float resolutionCorrection;

            int resolutionRAM;

            vdd = frameData[810];
            if (vdd > 32767)
                vdd = vdd - 65536;

            resolutionRAM = (frameData[832] & 0x0C00) >> 10;
            resolutionCorrection = (float)(Math.Pow(2, (double)mlx90640.ResolutionEE) / Math.Pow(2, (double)resolutionRAM));
            vdd = (float)((resolutionCorrection * vdd - mlx90640.Vdd25) / mlx90640.KVdd + 3.3);

            return vdd;
        }

        void ReadEEProm(ref ushort[] eeData)
        {
            I2CRead(0x2400, 832, ref eeData);
        }

        void ExtractParameters(ref ushort[] eeData)
        {

            ExtractVDDParameters(ref eeData);
            ExtractPTATParameters(ref eeData);
            ExtractGainParameters(ref eeData);
            ExtractTgcParameters(ref eeData);
            ExtractResolutionParameters(ref eeData);
            ExtractKsTaParameters(ref eeData);
            ExtractKsToParameters(ref eeData);
            ExtractCPParameters(ref eeData);
            ExtractAlphaParameters(ref eeData);
            ExtractOffsetParameters(ref eeData);
            ExtractKtaPixelParameters(ref eeData);
            ExtractKvPixelParameters(ref eeData);
            ExtractCILCParameters(ref eeData);
            ExtractDeviatingPixels(ref eeData);
        }

        void ExtractVDDParameters(ref ushort[] eeData)
        {
            short kVdd;
            short vdd25;

            kVdd = (short)eeData[51];

            kVdd = (short)((eeData[51] & 0xFF00) >> 8);

            if (kVdd > 127)
                kVdd = (short)(kVdd - 256);

            kVdd = (short)(32 * kVdd);
            vdd25 = (short)(eeData[51] & 0x00FF);
            vdd25 = (short)(((vdd25 - 256) << 5) - 8192);

            Config.KVdd = kVdd;
            Config.Vdd25 = vdd25;

        }

        void ExtractPTATParameters(ref ushort[] eeData)
        {
            float KvPTAT;
            float KtPTAT;
            ushort vPTAT25;
            float alphaPTAT;

            KvPTAT = (eeData[50] & 0xFC00) >> 10;
            if (KvPTAT > 31)
                KvPTAT = KvPTAT - 64;
            
            KvPTAT = KvPTAT / 4096;

            KtPTAT = eeData[50] & 0x03FF;
            if (KtPTAT > 511)
                KtPTAT = KtPTAT - 1024;

            KtPTAT = KtPTAT / 8;

            vPTAT25 = eeData[49];

            alphaPTAT = (float)((eeData[16] & 0xF000) / Math.Pow(2, (double)14) + 8.0f);

            Config.KvPTAT = KvPTAT;
            Config.KtPTAT = KtPTAT;
            Config.VPTAT25 = vPTAT25;
            Config.AlphaPTAT = alphaPTAT;

        }

        void ExtractGainParameters(ref ushort[] eeData)
        {
            short gainEE;

            gainEE = (short)eeData[48];
            if (gainEE > 32767)
                gainEE = (short)(gainEE - 65536);

            Config.GainEE = gainEE;
        }

        void ExtractTgcParameters(ref ushort[] eeData)
        {
            float tgc;
            tgc = eeData[60] & 0x00FF;
            if (tgc > 127)
                tgc = tgc - 256;
            
            tgc = tgc / 32.0f;

            Config.Tgc = tgc;

        }

        void ExtractResolutionParameters(ref ushort[] eeData)
        {
            byte resolutionEE;
            resolutionEE = (byte)((eeData[56] & 0x3000) >> 12);

            Config.ResolutionEE = resolutionEE;

        }

        void ExtractKsTaParameters(ref ushort[] eeData)
        {
            float KsTa;
            KsTa = (eeData[60] & 0xFF00) >> 8;
            if (KsTa > 127)
                KsTa = KsTa - 256;
            
            KsTa = KsTa / 8192.0f;

            Config.KsTa = KsTa;
        }

        void ExtractKsToParameters(ref ushort[] eeData)
        {
            int KsToScale;
            byte step;

            step = (byte)(((eeData[63] & 0x3000) >> 12) * 10);

            Config.Ct[0] = -40;
            Config.Ct[1] = 0;
            Config.Ct[2] = (short)((eeData[63] & 0x00F0) >> 4);
            Config.Ct[3] = (short)((eeData[63] & 0x0F00) >> 8);

            Config.Ct[2] = (short)(Config.Ct[2] * step);
            Config.Ct[3] = (short)(Config.Ct[2] + Config.Ct[3] * step);
            Config.Ct[4] = 400;

            KsToScale = (eeData[63] & 0x000F) + 8;
            KsToScale = 1 << KsToScale;

            Config.KsTo[0] = eeData[61] & 0x00FF;
            Config.KsTo[1] = (eeData[61] & 0xFF00) >> 8;
            Config.KsTo[2] = eeData[62] & 0x00FF;
            Config.KsTo[3] = (eeData[62] & 0xFF00) >> 8;

            for (int i = 0; i < 4; i++)
            {
                if (Config.KsTo[i] > 127)
                    Config.KsTo[i] = Config.KsTo[i] - 256;

                Config.KsTo[i] = Config.KsTo[i] / KsToScale;
            }

            Config.KsTo[4] = -0.0002f;
        }

        void ExtractAlphaParameters(ref ushort[] eeData)
        {
            int[] accRow = new int[24];
            int[] accColumn = new int[32];
            int p = 0;
            int alphaRef;
            byte alphaScale;
            byte accRowScale;
            byte accColumnScale;
            byte accRemScale;
            float[] alphaTemp = new float[768];
            float temp;

            accRemScale = (byte)(eeData[32] & 0x000F);
            accColumnScale = (byte)((eeData[32] & 0x00F0) >> 4);
            accRowScale = (byte)((eeData[32] & 0x0F00) >> 8);
            alphaScale = (byte)(((eeData[32] & 0xF000) >> 12) + 30);
            alphaRef = eeData[33];

            for (int i = 0; i < 6; i++)
            {
                p = i * 4;
                accRow[p + 0] = (eeData[34 + i] & 0x000F);
                accRow[p + 1] = (eeData[34 + i] & 0x00F0) >> 4;
                accRow[p + 2] = (eeData[34 + i] & 0x0F00) >> 8;
                accRow[p + 3] = (eeData[34 + i] & 0xF000) >> 12;
            }

            for (int i = 0; i < 24; i++)
            {
                if (accRow[i] > 7)
                    accRow[i] = accRow[i] - 16;
            }

            for (int i = 0; i < 8; i++)
            {
                p = i * 4;
                accColumn[p + 0] = (eeData[40 + i] & 0x000F);
                accColumn[p + 1] = (eeData[40 + i] & 0x00F0) >> 4;
                accColumn[p + 2] = (eeData[40 + i] & 0x0F00) >> 8;
                accColumn[p + 3] = (eeData[40 + i] & 0xF000) >> 12;
            }

            for (int i = 0; i < 32; i++)
            {
                if (accColumn[i] > 7)
                    accColumn[i] = accColumn[i] - 16;
            }

            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    p = 32 * i + j;
                    alphaTemp[p] = (eeData[64 + p] & 0x03F0) >> 4;
                    if (alphaTemp[p] > 31)
                        alphaTemp[p] = alphaTemp[p] - 64;
                    
                    alphaTemp[p] = alphaTemp[p] * (1 << accRemScale);
                    alphaTemp[p] = (alphaRef + (accRow[i] << accRowScale) + (accColumn[j] << accColumnScale) + alphaTemp[p]);
                    alphaTemp[p] = (float)(alphaTemp[p] / Math.Pow(2, (double)alphaScale));
                    alphaTemp[p] = alphaTemp[p] - Config.Tgc * (Config.CpAlpha[0] + Config.CpAlpha[1]) / 2;
                    alphaTemp[p] = ScaleAlpha / alphaTemp[p];
                }
            }

            temp = alphaTemp[0];
            for (int i = 1; i < 768; i++)
            {
                if (alphaTemp[i] > temp)
                    temp = alphaTemp[i];
                
            }

            alphaScale = 0;

            if (temp < 0)
            {
                //error
                temp = Math.Abs(temp);
            }

            while (temp < 32768)
            {
                temp = temp * 2;
                alphaScale = (byte)(alphaScale + 1);
            }

            for (int i = 0; i < 768; i++)
            {
                temp = (float)(alphaTemp[i] * Math.Pow(2, (double)alphaScale));
                Config.Alpha[i] = (short)(temp + 0.5);

            }

            Config.AlphaScale = alphaScale;
        }

        void ExtractOffsetParameters(ref ushort[] eeData)
        {
            int[] occRow = new int[24];
            int[] occColumn = new int[32];
            int p = 0;
            short offsetRef;
            byte occRowScale;
            byte occColumnScale;
            byte occRemScale;

            occRemScale = (byte)(eeData[16] & 0x000F);
            occColumnScale = (byte)((eeData[16] & 0x00F0) >> 4);
            occRowScale = (byte)((eeData[16] & 0x0F00) >> 8);
            offsetRef = (short)eeData[17];

            if (offsetRef > 32767)
                offsetRef = (short)(offsetRef - 65536);

            for (int i = 0; i < 6; i++)
            {
                p = i * 4;
                occRow[p + 0] = (eeData[18 + i] & 0x000F);
                occRow[p + 1] = (eeData[18 + i] & 0x00F0) >> 4;
                occRow[p + 2] = (eeData[18 + i] & 0x0F00) >> 8;
                occRow[p + 3] = (eeData[18 + i] & 0xF000) >> 12;
            }

            for (int i = 0; i < 24; i++)
            {
                if (occRow[i] > 7)
                {
                    occRow[i] = occRow[i] - 16;
                }
            }

            for (int i = 0; i < 8; i++)
            {
                p = i * 4;
                occColumn[p + 0] = (eeData[24 + i] & 0x000F);
                occColumn[p + 1] = (eeData[24 + i] & 0x00F0) >> 4;
                occColumn[p + 2] = (eeData[24 + i] & 0x0F00) >> 8;
                occColumn[p + 3] = (eeData[24 + i] & 0xF000) >> 12;
            }

            for (int i = 0; i < 32; i++)
            {
                if (occColumn[i] > 7)
                {
                    occColumn[i] = occColumn[i] - 16;
                }
            }

            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    p = 32 * i + j;
                    Config.Offset[p] = (short)((eeData[64 + p] & 0xFC00) >> 10);
                    if (Config.Offset[p] > 31)
                    {
                        Config.Offset[p] = (short)(Config.Offset[p] - 64);
                    }
                    Config.Offset[p] = (short)(Config.Offset[p] * (1 << occRemScale));
                    Config.Offset[p] = (short)((offsetRef + (occRow[i] << occRowScale) + (occColumn[j] << occColumnScale) + Config.Offset[p]));
                }
            }
        }

        void ExtractKtaPixelParameters(ref ushort[] eeData)
        {
            int p = 0;
            sbyte[] KtaRC = new sbyte[4];
            sbyte KtaRoCo;
            sbyte KtaRoCe;
            sbyte KtaReCo;
            sbyte KtaReCe;
            byte ktaScale1;
            byte ktaScale2;
            byte split;
            float[] ktaTemp = new float[768];
            float temp;

            KtaRoCo = (sbyte)((eeData[54] & 0xFF00) >> 8);
            if (KtaRoCo > 127)
                KtaRoCo = (sbyte)(KtaRoCo - 256);

            KtaRC[0] = KtaRoCo;

            KtaReCo = (sbyte)((eeData[54] & 0x00FF));
            if (KtaReCo > 127)
                KtaReCo = (sbyte)(KtaReCo - 256);

            KtaRC[2] = KtaReCo;

            KtaRoCe = (sbyte)((eeData[55] & 0xFF00) >> 8);
            if (KtaRoCe > 127)
                KtaRoCe = (sbyte)(KtaRoCe - 256);
            
            KtaRC[1] = KtaRoCe;

            KtaReCe = (sbyte)((eeData[55] & 0x00FF));
            if (KtaReCe > 127)
                KtaReCe = (sbyte)(KtaReCe - 256);
            
            KtaRC[3] = KtaReCe;

            ktaScale1 = (byte)(((eeData[56] & 0x00F0) >> 4) + 8);
            ktaScale2 = (byte)((eeData[56] & 0x000F));

            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    p = 32 * i + j;
                    split = (byte)(2 * (p / 32 - (p / 64) * 2) + p % 2);
                    ktaTemp[p] = (eeData[64 + p] & 0x000E) >> 1;
                    if (ktaTemp[p] > 3)
                        ktaTemp[p] = ktaTemp[p] - 8;
                    
                    ktaTemp[p] = ktaTemp[p] * (1 << ktaScale2);
                    ktaTemp[p] = KtaRC[split] + ktaTemp[p];
                    ktaTemp[p] = (float)(ktaTemp[p] / Math.Pow(2, (double)ktaScale1));
                }
            }

            temp = Math.Abs(ktaTemp[0]);
            for (int i = 1; i < 768; i++)
            {
                if (Math.Abs(ktaTemp[i]) > temp)
                    temp = Math.Abs(ktaTemp[i]);
                
            }

            ktaScale1 = 0;
            while (temp < 64)
            {
                temp = temp * 2;
                ktaScale1 = (byte)(ktaScale1 + 1);
            }

            for (int i = 0; i < 768; i++)
            {
                temp = (float)(ktaTemp[i] * Math.Pow(2, (double)ktaScale1));
                if (temp < 0)
                    Config.Kta[i] = (sbyte)(temp - 0.5);
                else
                    Config.Kta[i] = (sbyte)(temp + 0.5);

            }

            Config.KtaScale = ktaScale1;
        }

        void ExtractKvPixelParameters(ref ushort[] eeData)
        {
            int p = 0;
            sbyte[] KvT = new sbyte[4];
            sbyte KvRoCo;
            sbyte KvRoCe;
            sbyte KvReCo;
            sbyte KvReCe;
            byte kvScale;
            byte split;
            float[] kvTemp = new float[768];
            float temp;

            KvRoCo = (sbyte)((eeData[52] & 0xF000) >> 12);
            if (KvRoCo > 7)
                KvRoCo = (sbyte)(KvRoCo - 16);
            
            KvT[0] = KvRoCo;

            KvReCo = (sbyte)((eeData[52] & 0x0F00) >> 8);
            if (KvReCo > 7)
                KvReCo = (sbyte)(KvReCo - 16);
            
            KvT[2] = KvReCo;

            KvRoCe = (sbyte)((eeData[52] & 0x00F0) >> 4);
            if (KvRoCe > 7)
                KvRoCe = (sbyte)(KvRoCe - 16);
            
            KvT[1] = KvRoCe;

            KvReCe = (sbyte)((eeData[52] & 0x000F));
            if (KvReCe > 7)
                KvReCe = (sbyte)(KvReCe - 16);
            
            KvT[3] = KvReCe;

            kvScale = (byte)((eeData[56] & 0x0F00) >> 8);

            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    p = 32 * i + j;
                    split = (byte)(2 * (p / 32 - (p / 64) * 2) + p % 2);
                    kvTemp[p] = KvT[split];
                    kvTemp[p] = (float)(kvTemp[p] / Math.Pow(2, (double)kvScale));
                }
            }

            temp = Math.Abs(kvTemp[0]);
            for (int i = 1; i < 768; i++)
            {
                if (Math.Abs(kvTemp[i]) > temp)
                    temp = Math.Abs(kvTemp[i]);
                
            }

            kvScale = 0;
            while (temp < 64)
            {
                temp = temp * 2;
                kvScale = (byte)(kvScale + 1);
            }

            for (int i = 0; i < 768; i++)
            {
                temp = (float)(kvTemp[i] * Math.Pow(2, (double)kvScale));
                if (temp < 0)
                    Config.Kv[i] = (sbyte)(temp - 0.5);
                else
                    Config.Kv[i] = (sbyte)(temp + 0.5);

            }

            Config.KvScale = kvScale;
        }

        void ExtractCILCParameters(ref ushort[] eeData)
        {
            float[] ilChessC = new float[3];
            byte calibrationModeEE;

            calibrationModeEE = (byte)((eeData[10] & 0x0800) >> 4);
            calibrationModeEE = (byte)(calibrationModeEE ^ 0x80);

            ilChessC[0] = (eeData[53] & 0x003F);
            if (ilChessC[0] > 31)
                ilChessC[0] = ilChessC[0] - 64;

            ilChessC[0] = ilChessC[0] / 16.0f;

            ilChessC[1] = (eeData[53] & 0x07C0) >> 6;
            if (ilChessC[1] > 15)
                ilChessC[1] = ilChessC[1] - 32;
            
            ilChessC[1] = ilChessC[1] / 2.0f;

            ilChessC[2] = (eeData[53] & 0xF800) >> 11;
            if (ilChessC[2] > 15)
                ilChessC[2] = ilChessC[2] - 32;
            
            ilChessC[2] = ilChessC[2] / 8.0f;

            Config.CalibrationModeEE = calibrationModeEE;
            Config.IlChessC[0] = ilChessC[0];
            Config.IlChessC[1] = ilChessC[1];
            Config.IlChessC[2] = ilChessC[2];
        }

        void ExtractCPParameters(ref ushort[] eeData)
        {
            float[] alphaSP = new float[2];
            short[] offsetSP = new short[2];
            float cpKv;
            float cpKta;
            byte alphaScale;
            byte ktaScale1;
            byte kvScale;

            alphaScale = (byte)(((eeData[32] & 0xF000) >> 12) + 27);

            offsetSP[0] = (short)((eeData[58] & 0x03FF));
            if (offsetSP[0] > 511)
                offsetSP[0] = (short)(offsetSP[0] - 1024);
            

            offsetSP[1] = (short)((eeData[58] & 0xFC00) >> 10);
            if (offsetSP[1] > 31)
                offsetSP[1] = (short)(offsetSP[1] - 64);
            
            offsetSP[1] = (short)(offsetSP[1] + offsetSP[0]);

            alphaSP[0] = (eeData[57] & 0x03FF);
            if (alphaSP[0] > 511)
                alphaSP[0] = alphaSP[0] - 1024;
            
            alphaSP[0] = (float)(alphaSP[0] / Math.Pow(2, (double)alphaScale));

            alphaSP[1] = (eeData[57] & 0xFC00) >> 10;
            if (alphaSP[1] > 31)
                alphaSP[1] = alphaSP[1] - 64;
            
            alphaSP[1] = (1 + alphaSP[1] / 128) * alphaSP[0];

            cpKta = (eeData[59] & 0x00FF);
            if (cpKta > 127)
                cpKta = cpKta - 256;
            
            ktaScale1 = (byte)(((eeData[56] & 0x00F0) >> 4) + 8);
            Config.CpKta = (float)(cpKta / Math.Pow(2, (double)ktaScale1));

            cpKv = (eeData[59] & 0xFF00) >> 8;
            if (cpKv > 127)
                cpKv = cpKv - 256;
            
            kvScale = (byte)((eeData[56] & 0x0F00) >> 8);
            Config.CpKv = (float)(cpKv / Math.Pow(2, (double)kvScale));

            Config.CpAlpha[0] = alphaSP[0];
            Config.CpAlpha[1] = alphaSP[1];
            Config.CpOffset[0] = offsetSP[0];
            Config.CpOffset[1] = offsetSP[1];
        }

        void ExtractDeviatingPixels(ref ushort[] eeData)
        {
            ushort pixCnt = 0;
            int i;

            pixCnt = 0;
            while (pixCnt < 768)
            {
                if (eeData[pixCnt + 64] == 0)
                {
                    Config.BrokenPixels.Add(pixCnt);
                }
                else if ((eeData[pixCnt + 64] & 0x0001) != 0)
                {
                    Config.OutlierPixels.Add(pixCnt);
                }

                pixCnt = (ushort)(pixCnt + 1);

            }

            if(Config.BrokenPixels.Count > 1 )
            {
                for (pixCnt = 0; pixCnt < Config.BrokenPixels.Count; pixCnt++)
                {
                    for (i = pixCnt + 1; i < Config.BrokenPixels.Count; i++)
                    {
                        if (!CheckAdjacentPixels(Config.BrokenPixels[pixCnt], Config.BrokenPixels[i]))
                        {
                            Config.BrokenPixelHasAdjacentBrokenPixel = true;
                        }
                    }
                }

                for (pixCnt = 0; pixCnt < Config.OutlierPixels.Count; pixCnt++)
                {
                    for (i = pixCnt + 1; i < Config.OutlierPixels.Count; i++)
                    {
                        if (!CheckAdjacentPixels(Config.OutlierPixels[pixCnt], Config.OutlierPixels[i]))
                        {
                            Config.OutlierPixelHasAdjacentOutlierPixel = true;
                        }
                    }
                }

                for (pixCnt = 0; pixCnt < Config.BrokenPixels.Count; pixCnt++)
                {
                    for (i = 0; i < Config.BrokenPixels.Count; i++)
                    {
                        if (!CheckAdjacentPixels(Config.BrokenPixels[pixCnt], Config.OutlierPixels[i]))
                        {
                            Config.BrokenPixelHasAdjacentOutlierPixel = true;
                        }
                    }
                }

            }

        }

        bool CheckAdjacentPixels(ushort pix1, ushort pix2)
        {
            int pixPosDif;

            pixPosDif = pix1 - pix2;
            if (pixPosDif > -34 && pixPosDif < -30)
                return false;

            if (pixPosDif > -2 && pixPosDif < 2)
                return false;

            if (pixPosDif > 30 && pixPosDif < 34)
                return false;

            return true;
        }

        bool SetCurrentRefreshRate(byte refreshRate)
        {
            ushort[] controlRegister1 = new ushort[1];
            ushort value;
            value = (ushort)((refreshRate & 0x07) << 7);

            I2CRead(0x800D, 1, ref controlRegister1);

            value = (ushort)((controlRegister1[0] & 0xFC7F) | value);
            return I2CWrite(0x800D, value);
        }

        int GetCurrentRefreshRate()
        {
            ushort[] controlRegister1 = new ushort[1];
            int refreshRate;

            I2CRead(0x800D, 1, ref controlRegister1);

            refreshRate = (controlRegister1[0] & 0x0380) >> 7;

            return refreshRate;
        }

        bool SetInterleavedMode()
        {
            ushort[] controlRegister1 = new ushort[1];
            ushort value;

            I2CRead(0x800D, 1, ref controlRegister1);

            value = (ushort)((controlRegister1[0] & 0xEFFF));

            return I2CWrite(0x800D, value);
        }

        bool SetChessMode()
        {
            ushort[] controlRegister1 = new ushort[1];
            ushort value;

            I2CRead(0x800D, 1, ref controlRegister1);

            value = (ushort)(controlRegister1[0] | 0x1000);

            return I2CWrite(0x800D, value);
        }

        int GetCurrentMode()
        {
            ushort[] controlRegister1 = new ushort[1];
            ushort modeRAM;

            I2CRead(0x800D, 1, ref controlRegister1);

            modeRAM = (ushort)((controlRegister1[0] & 0x1000) >> 12);

            return modeRAM;
        }

        bool SetResolution(byte resolution)
        {
            ushort[] controlRegister1 = new ushort[1];
            ushort value;

            value = (ushort)((resolution & 0x03) << 10);

            I2CRead(0x800D, 1, ref controlRegister1);

            value = (ushort)((controlRegister1[0] & 0xF3FF) | value);

            return I2CWrite(0x800D, value);
        }

        int GetCurrentResolution()
        {
            ushort[] controlRegister1 = new ushort[1];
            int resolutionRAM;

            I2CRead(0x800D, 1, ref controlRegister1);

            resolutionRAM = (controlRegister1[0] & 0x0C00) >> 10;

            return resolutionRAM;
        }

    }

}
