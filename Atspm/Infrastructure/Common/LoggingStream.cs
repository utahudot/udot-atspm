#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Common/LoggingStream.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Text;

namespace Utah.Udot.Atspm.Infrastructure.Common
{
    /// <summary>
    /// A wrapper stream that intercepts writes to an inner stream
    /// and logs metrics about the response body.
    /// Specifically designed for ND‑JSON streaming responses.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="LoggingStream"/> class.
    /// </remarks>
    /// <param name="inner">The underlying stream to wrap and forward writes to.</param>
    public class LoggingStream(Stream inner) : Stream
    {
        private readonly Stream _inner = inner;
        private long _bytesWritten = 0;
        private int _ndJsonCount = 0;
        private string _lineBuffer = "";

        /// <summary>
        /// Gets the total number of bytes written through this stream.
        /// </summary>
        public long BytesWritten => _bytesWritten;

        /// <summary>
        /// Gets the number of ND‑JSON objects written.
        /// This is incremented whenever a complete line starting with '{' is detected.
        /// </summary>
        public int NdJsonCount => _ndJsonCount;

        /// <summary>
        /// Asynchronously writes a sequence of bytes to the current stream,
        /// increments the byte counter, and counts ND‑JSON objects by line.
        /// </summary>
        /// <param name="buffer">The buffer containing data to write.</param>
        /// <param name="offset">The zero‑based byte offset in <paramref name="buffer"/> at which to begin copying bytes.</param>
        /// <param name="count">The number of bytes to write.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _bytesWritten += count;
            var chunk = Encoding.UTF8.GetString(buffer, offset, count);

            _lineBuffer += chunk;
            var lines = _lineBuffer.Split('\n');

            // Keep the last partial line in the buffer
            _lineBuffer = lines[^1];

            // Count complete ND‑JSON objects
            for (int i = 0; i < lines.Length - 1; i++)
            {
                var line = lines[i];
                if (!string.IsNullOrWhiteSpace(line) && line.TrimStart().StartsWith('{'))
                    _ndJsonCount++;
            }

            await _inner.WriteAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc />
        public override bool CanRead => _inner.CanRead;

        /// <inheritdoc />
        public override bool CanSeek => _inner.CanSeek;

        /// <inheritdoc />
        public override bool CanWrite => _inner.CanWrite;

        /// <inheritdoc />
        public override long Length => _inner.Length;

        /// <inheritdoc />
        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        /// <inheritdoc />
        public override Task FlushAsync(CancellationToken cancellationToken) => _inner.FlushAsync(cancellationToken);

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);

        /// <inheritdoc />
        public override void SetLength(long value) => _inner.SetLength(value);

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);

        /// <inheritdoc />
        public override void Flush() => _inner.Flush();
    }
}
