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
		// TODO: Better name would be cool. Dont use ReadString, it wont be called then (BinaryReader has its own ReadString method)
		public static string ReadString2(this BinaryReader br)
        {
			// TODO: This is the same implementation as BitReader's ReadString. Try to consolidate someday
            var len = br.ReadInt32();
			if (len > 0)
			{
				var bytes = br.ReadBytes(len);
				return Encoding.ASCII.GetString(bytes, 0, len - 1);
			}
			else if ( len < 0 )
			{
				var bytes = br.ReadBytes(len * -2);
				return Encoding.Unicode.GetString(bytes, 0, (len * -2) - 2);
			}
            return "";
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
