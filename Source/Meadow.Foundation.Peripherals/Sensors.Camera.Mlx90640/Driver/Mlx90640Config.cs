using System;
using System.Collections.Generic;

namespace Meadow.Foundation.Sensors.Camera
{
    public class Mlx90640Config
    {
        public short KVdd { get; internal set; }
        public short Vdd25 { get; internal set; }
        public float KvPTAT { get; internal set; }
        public float KtPTAT { get; internal set; }
        public ushort VPTAT25 { get; internal set; }
        public float AlphaPTAT { get; internal set; }
        public short GainEE { get; internal set; }
        public float Tgc { get; internal set; }
        public float CpKv { get; internal set; }
        public float CpKta { get; internal set; }
        public byte ResolutionEE { get; internal set; }
        public byte CalibrationModeEE { get; internal set; }
        public float KsTa { get; internal set; }
        public float[] KsTo { get; internal set; } = new float[5];
        public short[] Ct { get; internal set; } = new short[5];
        public short[] Alpha { get; internal set; } = new short[768];
        public byte AlphaScale { get; internal set; }
        public short[] Offset { get; internal set; } = new short[768];
        public sbyte[] Kta { get; internal set; } = new sbyte[768];
        public byte KtaScale { get; internal set; }
        public sbyte[] Kv { get; internal set; } = new sbyte[768];
        public byte KvScale { get; internal set; }
        public float[] CpAlpha { get; internal set; } = new float[2];
        public short[] CpOffset { get; internal set; } = new short[2];
        public float[] IlChessC { get; internal set; } = new float[3];
        public List<ushort> BrokenPixels { get; internal set; } = new List<ushort>();
        public List<ushort> OutlierPixels { get; internal set; } = new List<ushort>();

        public bool BrokenPixelHasAdjacentBrokenPixel { get; internal set; }

        public bool OutlierPixelHasAdjacentOutlierPixel { get; internal set; }

        public bool BrokenPixelHasAdjacentOutlierPixel { get; internal set; }

    }
}
