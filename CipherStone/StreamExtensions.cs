using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CipherStone
{
    public static class StreamExtensions
    {
        public static void Write(this Stream @this, byte[] arr, int offset = 0)
        {
            @this.Write(arr, offset, arr.Length);
        }
        public static byte[] Read(this Stream @this, int length)
        {
            var ret = new byte[length];
            if (@this.Read(ret, 0, length) != length)
                throw new EndOfStreamException();
            return ret;
        }
        public static void CopyToLimited(this Stream @this, Stream output, int bytes, int bufferSize = 1024)
        {
            byte[] buffer = new byte[bufferSize];
            int read;
            while (bytes > 0 &&
                   (read = @this.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
            {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }
    }
}
