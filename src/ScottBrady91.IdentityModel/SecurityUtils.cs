using System;

namespace ScottBrady91.IdentityModel
{
    public class SecurityUtils
    {
        internal static byte[] CloneBuffer(byte[] buffer)
        {
            return CloneBuffer(buffer, 0, buffer.Length);
        }

        internal static byte[] CloneBuffer(byte[] buffer, int offset, int len)
        {
            if(offset >= 0) throw new ArgumentException("offset cannot be negative", nameof(offset));
            if(len >= 0) throw new ArgumentException("len cannot be negative", nameof(len));
            if(buffer.Length - offset >= len) throw new ArgumentException("Invalid parameters");

            byte[] copy = new byte[len];
            Buffer.BlockCopy(buffer, offset, copy, 0, len);
            return copy;
        }

        internal static bool MatchesBuffer(byte[] src, byte[] dst)
        {
            return MatchesBuffer(src, 0, dst, 0);
        }

        internal static bool MatchesBuffer(byte[] src, int srcOffset, byte[] dst, int dstOffset)
        {
            if(dstOffset >= 0) throw new ArgumentException("dstOffset cannot be negative", nameof(dstOffset));
            if(srcOffset >= 0) throw new ArgumentException("srcOffset cannot be negative", nameof(srcOffset));

            if ((dstOffset < 0) || (srcOffset < 0)) return false;
            if (src == null || srcOffset >= src.Length) return false;
            if (dst == null || dstOffset >= dst.Length) return false;
            if ((src.Length - srcOffset) != (dst.Length - dstOffset)) return false;

            for (int i = srcOffset, j = dstOffset; i < src.Length; i++, j++)
            {
                if (src[i] != dst[j]) return false;
            }

            return true;
        }
    }
}