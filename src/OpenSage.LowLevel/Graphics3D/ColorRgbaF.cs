﻿using System.Runtime.InteropServices;

namespace OpenSage.LowLevel.Graphics3D
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ColorRgbaF
    {
        public static readonly ColorRgbaF Transparent = new ColorRgbaF();
        public static readonly ColorRgbaF White = new ColorRgbaF(1.0f, 1.0f, 1.0f, 1.0f);

        public float R;
        public float G;
        public float B;
        public float A;

        public ColorRgbaF(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public ColorRgbaF BlendMultiply(in ColorRgbaF color)
        {
            ColorRgbaF result;
            result.R = R * color.R;
            result.G = G * color.G;
            result.B = B * color.B;
            result.A = A * color.A;
            return result;
        }
    }
}
