// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.

using System.IO;

namespace System.Util.Zlib
{
	public class ZDeflaterStream : Stream
	{

		private const Int32 BUFSIZE = 4192;
		private readonly Byte[] _buffer = new Byte[BUFSIZE];
		private readonly Int32 _flushLevel = JZlib.Z_NO_FLUSH;

		private readonly ZStream _zstream = new ZStream();
		private readonly Stream _output;

		public ZDeflaterStream(Stream outStream, Int32 compressionLevel = 1, Boolean nowrap = false)
		{
			_output = outStream;
			_zstream.deflateInit(compressionLevel, nowrap);
		}

		public override Boolean CanRead => throw new NotImplementedException(nameof(CanRead));

		public override Boolean CanSeek => throw new NotImplementedException(nameof(CanSeek));

		public override Boolean CanWrite => true;

		public override Int64 Length => throw new NotImplementedException(nameof(Length));

		public override Int64 Position { get => throw new NotImplementedException(nameof(Position)); set => throw new NotImplementedException(nameof(Position)); }

		public override void Flush()
		{
			_output.Flush();
		}

		public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count)
		{
			throw new NotImplementedException(nameof(Read));
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
			if (count == 0)
				return;
			Int32 err;
			_zstream.next_in = buffer;
			_zstream.next_in_index = offset;
			_zstream.avail_in = count;
			do
			{
				_zstream.next_out = _buffer;
				_zstream.next_out_index = 0;
				_zstream.avail_out = BUFSIZE;
				err = _zstream.deflate(_flushLevel);
				if (err != JZlib.Z_OK)
					throw new IOException($"Deflating: {_zstream.msg}");
				if (_zstream.avail_out < BUFSIZE)
					_output.Write(_buffer, 0, BUFSIZE - _zstream.avail_out);
			}
			while (_zstream.avail_in > 0 || _zstream.avail_out == 0);
		}

		public override void WriteByte(Byte value)
		{
			var buf = new Byte[1];
			buf[0] = (Byte)value;
			Write(buf, 0, 1);
		}


		public override void Close()
		{
			try
			{
				Finish();
			}
			finally
			{
				End();
				_output.Close();
			}
		}

		public virtual void Finish()
		{
			do
			{
				_zstream.next_out = _buffer;
				_zstream.next_out_index = 0;
				_zstream.avail_out = BUFSIZE;
				Int32 err = _zstream.deflate(JZlib.Z_FINISH);
				if (err != JZlib.Z_STREAM_END && err != JZlib.Z_OK)
					throw new IOException($"Deflating: {_zstream.msg}");
				if (BUFSIZE - _zstream.avail_out > 0)
				{
					_output.Write(_buffer, 0, BUFSIZE - _zstream.avail_out);
				}
			}
			while (_zstream.avail_in > 0 || _zstream.avail_out == 0);
			Flush();
		}

		public virtual void End()
		{
			if (_zstream == null)
				return;
			_zstream.deflateEnd();
			_zstream.free();
		}

	}
}
