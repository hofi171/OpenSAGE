﻿using System;
using System.IO;

namespace OpenSage.FileFormats.Big
{
    public class BigArchiveEntryStream : Stream
    {
        private readonly BigArchiveEntry _entry;
        private readonly BigArchive _archive;
        private readonly uint _offset;
        private bool _locked;
        private bool _write;
        private MemoryStream _writeBuffer;

        public BigArchiveEntryStream(BigArchiveEntry entry, uint offset)
        {
            _write = false;
            _entry = entry;
            _archive = entry.Archive;
            _offset = offset;

            _archive.AcquireLock();
            _locked = true;
        }

        public override void Flush()
        {
            if (_write)
            {
                _entry.Length = (uint) this.Length;
                _entry.WriteBuffer = _writeBuffer;
            }
            _archive.WriteToDisk();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int result = 0;
            if (_write == false)
            {
                _archive.Stream.Seek(_offset + Position, SeekOrigin.Begin);
                if (count > (Length - Position))
                {
                    count = (int) (Length - Position);
                }

                result = _archive.Stream.Read(buffer, offset, count);
                Position += result;
            }
            else
            {
                result = _writeBuffer.Read(buffer, offset, count);
                Position += result;
            }

            return result;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;

                case SeekOrigin.Current:
                    Position += offset;
                    break;

                case SeekOrigin.End:
                    Position = Length + offset;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }

            return Position;
        }

        private void EnsureWriteMode()
        {
            if (_write == false)
            {
                _writeBuffer = new MemoryStream();
                CopyTo(_writeBuffer);
                _write = true;
            }
        }

        public override void SetLength(long value)
        {
            EnsureWriteMode();

            _entry.OnDisk = false;
            _writeBuffer.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureWriteMode();

            _entry.OnDisk = false;
            _writeBuffer.Position = Position;
            _writeBuffer.Write(buffer, offset, count);
        }

        public override bool CanRead => _archive.Stream.CanRead;

        public override bool CanSeek => _archive.Stream.CanSeek;

        public override bool CanWrite => true;

        public override long Length => _write ? _writeBuffer.Length : _entry.Length;

        public override long Position { get; set; }

        public override void Close()
        {
            if (_locked)
            {
                _archive.ReleaseLock();
                _locked = false;
            }

            base.Close();
        }
    }
}
