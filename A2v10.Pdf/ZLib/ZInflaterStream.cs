// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.

using System.IO;

namespace System.Util.Zlib
{
	public class ZInflaterStream : Stream
	{
		private readonly ZStream _zstream = new ZStream();
		private readonly Stream _input;

		private const Int32 BUFSIZE = 4192;
		private readonly Byte[] _buffer = new Byte[BUFSIZE];

		private Boolean _nomoreinput = false;
		private readonly Int32 _flushLevel = JZlib.Z_NO_FLUSH;

		public ZInflaterStream(Stream input, Boolean nowrap = false)
		{
			_input = input;
			_zstream.inflateInit(nowrap);
			_zstream.next_in = _buffer;
			_zstream.next_in_index = 0;
			_zstream.avail_in = 0;
		}

		public static Byte[] FlatDecode(Byte[] input, Boolean strict = true)
		{
			MemoryStream stream = new MemoryStream(input);
			MemoryStream output = new MemoryStream();
			ZInflaterStream zip = new ZInflaterStream(stream);
			byte[] b = new Byte[strict ? 4092 : 1];
			try
			{
				int n;
				while ((n = zip.Read(b, 0, b.Length)) > 0)
				{
					output.Write(b, 0, n);
				}
				zip.Close();
				output.Close();
				return output.ToArray();
			}
			finally
			{
				output.Close();
				zip.Close();
			}
		}


		public override Boolean CanRead => true;

		public override Boolean CanSeek => false;

		public override Boolean CanWrite => false;

		public override Int64 Length => throw new NotImplementedException(nameof(Length));

		public override Int64 Position { get => throw new NotImplementedException(nameof(Position)); set => throw new NotImplementedException(); }

		public override void Flush()
		{
			_input.Flush();
		}

		public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count)
		{
			if (count == 0)
				return 0;
			Int32 err;
			_zstream.next_out = buffer;
			_zstream.next_out_index = offset;
			_zstream.avail_out = count;
			do
			{
				if ((_zstream.avail_in == 0) && (!_nomoreinput))
				{ // if buffer is empty and more input is avaiable, refill it
					_zstream.next_in_index = 0;
					_zstream.avail_in = _input.Read(_buffer, 0, BUFSIZE);//(BUFSIZE<z.avail_out ? BUFSIZE : z.avail_out));
					if (_zstream.avail_in <= 0)
					{
						_zstream.avail_in = 0;
						_nomoreinput = true;
					}
				}
				err = _zstream.inflate(_flushLevel);
				if (_nomoreinput && (err == JZlib.Z_BUF_ERROR))
					return (0);
				if (err != JZlib.Z_OK && err != JZlib.Z_STREAM_END)
					throw new IOException($"Inflating: {_zstream.msg}");
				if ((_nomoreinput || err == JZlib.Z_STREAM_END) && (_zstream.avail_out == count))
					return (0);
			}
			while (_zstream.avail_out == count && err == JZlib.Z_OK);
			return (count - _zstream.avail_out);
		}


		public override Int32 ReadByte()
		{
			var buf1 = new Byte[1];
			if (Read(buf1, 0, 1) <= 0)
				return -1;
			return (buf1[0] & 0xff);
		}

		public override Int64 Seek(Int64 offset, SeekOrigin origin)
		{
			throw new NotImplementedException(nameof(Seek));
		}

		public override void SetLength(Int64 value)
		{
			throw new NotImplementedException(nameof(SetLength));
		}

		public override void Write(Byte[] buffer, Int32 offset, Int32 count)
		{
			throw new NotImplementedException(nameof(Write));
		}

		public override void Close()
		{
			_input.Close();
		}
	}
}
