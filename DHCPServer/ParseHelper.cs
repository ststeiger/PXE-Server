/*

Copyright (c) 2010 Jean-Paul Mikkers

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

*/
namespace GitHub.JPMikkers.DHCP
{
    public class ParseHelper
    {
        public static System.Net.IPAddress ReadIPAddress(System.IO.Stream s)
        {
            byte[] bytes = new byte[4];
            s.Read(bytes, 0, bytes.Length);
            return new System.Net.IPAddress(bytes);
        }

        public static void WriteIPAddress(System.IO.Stream s, System.Net.IPAddress v)
        {
            byte[] bytes = v.GetAddressBytes();
            s.Write(bytes, 0, bytes.Length);
        }

        public static byte ReadUInt8(System.IO.Stream s)
        {
            System.IO.BinaryReader br = new System.IO.BinaryReader(s);
            return br.ReadByte();
        }

        public static void WriteUInt8(System.IO.Stream s, byte v)
        {
            System.IO.BinaryWriter bw = new System.IO.BinaryWriter(s);
            bw.Write(v);
        }

        public static ushort ReadUInt16(System.IO.Stream s)
        {
            System.IO.BinaryReader br = new System.IO.BinaryReader(s);
            return (ushort)System.Net.IPAddress.NetworkToHostOrder((short)br.ReadUInt16());
        }

        public static void WriteUInt16(System.IO.Stream s, ushort v)
        {
            System.IO.BinaryWriter bw = new System.IO.BinaryWriter(s);
            bw.Write((ushort)System.Net.IPAddress.HostToNetworkOrder((short)v));
        }

        public static uint ReadUInt32(System.IO.Stream s)
        {
            System.IO.BinaryReader br = new System.IO.BinaryReader(s);
            return (uint)System.Net.IPAddress.NetworkToHostOrder((int)br.ReadUInt32());
        }

        public static void WriteUInt32(System.IO.Stream s, uint v)
        {
            System.IO.BinaryWriter bw = new System.IO.BinaryWriter(s);
            bw.Write((uint)System.Net.IPAddress.HostToNetworkOrder((int)v));
        }

        public static string ReadZString(System.IO.Stream s)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int c = s.ReadByte();
            while (c>0)
            {
                sb.Append((char)c);
                c = s.ReadByte();
            }
            return sb.ToString();
        }

        public static void WriteZString(System.IO.Stream s, string msg)
        {
            System.IO.TextWriter tw = new System.IO.StreamWriter(s, System.Text.Encoding.ASCII);
            tw.Write(msg);
            tw.Flush();
            s.WriteByte(0);
        }

        public static void WriteZString(System.IO.Stream s, string msg, int length)
        {
            if (msg == null)
                msg = string.Empty;

            if (msg.Length >= length)
            {
                msg = msg.Substring(0, length - 1);
            }

            System.IO.TextWriter tw = new System.IO.StreamWriter(s, System.Text.Encoding.ASCII);
            tw.Write(msg);
            tw.Flush();

            // write terminating and padding zero's
            for (int t = msg.Length; t < length; t++)
            {
                s.WriteByte(0);
            }
        }

        public static string ReadString(System.IO.Stream s, int maxLength)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int c = s.ReadByte();
            while (c > 0 && sb.Length < maxLength)
            {
                sb.Append((char)c);
                c = s.ReadByte();
            }
            return sb.ToString();
        }

        public static string ReadString(System.IO.Stream s)
        {
            return ReadString(s, 16*1024);
        }

        public static void WriteString(System.IO.Stream s, string msg)
        {
            WriteString(s, false, msg);
        }

        public static void WriteString(System.IO.Stream s, bool zeroTerminated, string msg)
        {
            System.IO.TextWriter tw = new System.IO.StreamWriter(s, System.Text.Encoding.ASCII);
            tw.Write(msg);
            tw.Flush();
            if(zeroTerminated) s.WriteByte(0);
        }
    }
}
