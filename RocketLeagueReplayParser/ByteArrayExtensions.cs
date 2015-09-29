using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public static class BinaryReaderExtensions
    {
        public static string ReadAsciiString(this BinaryReader bs)
        {
            var len = bs.ReadInt32();
            var r = (new ASCIIEncoding()).GetString(bs.ReadBytes(len-1));
            bs.ReadByte(); // Discard trailing null char
            return r;
        }
    }

    public class ByteStream
    {
        byte[] _bytes;
        int _position = 0;

        public ByteStream(byte[] bytes)
        {
            _bytes = bytes;
        }

        public Int32 ReadInt()
        {
            var r = BitConverter.ToInt32(_bytes, _position);
            _position += 4;
            return r;
        }

        public byte ReadByte()
        {
            var r = _bytes[_position];
            _position += 1;
            return r;
        }

        public float ReadFloat()
        {
            var r = BitConverter.ToSingle(_bytes, _position);
            _position += 4;
            return r;
        }

        public string ReadAsciiString()
        {
            var len = ReadInt();
            var r = (new ASCIIEncoding()).GetString(_bytes, _position, len-1);
            _position += len - 1;
            ReadByte(); // Discard trailing null char
            return r;
        }

        public bool EndOfStream
        {
            get
            {
                return _position == _bytes.Length;
            }
        }
    }
}
