using StreamJsonRpc;
using StreamJsonRpc.Protocol;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers;
namespace OnlyHumans.Acp
{
    /// <summary>
    /// Handles JSON-RPC messages delimited by either newlines or headers.
    /// Allows selection of newline sequence (CRLF or LF).
    /// </summary>
    public class JsonRpcMessageHandler : PipeMessageHandler
    {
        public enum DelimiterType
        {
            NewLine,
            Header
        }

        private readonly DelimiterType delimiterType;
        private readonly string newline;
        private readonly byte[] newlineBytes;
        private readonly Encoding encoding;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonRpcMessageHandler"/> class.
        /// </summary>
        /// <param name="pipe">The duplex pipe.</param>
        /// <param name="formatter">The message formatter.</param>
        /// <param name="delimiterType">Delimiter type: NewLine or Header.</param>
        /// <param name="useCrLf">If true, use CRLF; otherwise, use LF.</param>
        /// <param name="encoding">Text encoding (default: UTF8).</param>
        public JsonRpcMessageHandler(
            IDuplexPipe pipe,
            IJsonRpcMessageFormatter formatter,
            DelimiterType delimiterType = DelimiterType.NewLine,
            bool useCrLf = false,
            Encoding? encoding = null)
            : base(pipe, formatter)
        {
            this.delimiterType = delimiterType;
            this.encoding = encoding ?? Encoding.UTF8;
            this.newline = useCrLf ? "\r\n" : "\n";
            this.newlineBytes = this.encoding.GetBytes(this.newline);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonRpcMessageHandler"/> class.
        /// </summary>
        public JsonRpcMessageHandler(
            PipeWriter? writer,
            PipeReader? reader,
            IJsonRpcMessageFormatter formatter,
            DelimiterType delimiterType = DelimiterType.NewLine,
            bool useCrLf = false,
            Encoding? encoding = null)
            : base(writer, reader, formatter)
        {
            this.delimiterType = delimiterType;
            this.encoding = encoding ?? Encoding.UTF8;
            this.newline = useCrLf ? "\r\n" : "\n";
            this.newlineBytes = this.encoding.GetBytes(this.newline);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonRpcMessageHandler"/> class.
        /// </summary>
        public JsonRpcMessageHandler(
            Stream? writer,
            Stream? reader,
            IJsonRpcMessageFormatter formatter,
            DelimiterType delimiterType = DelimiterType.NewLine,
            bool useCrLf = false,
            Encoding? encoding = null)
            : base(writer, reader, formatter)
        {
            this.delimiterType = delimiterType;
            this.encoding = encoding ?? Encoding.UTF8;
            this.newline = useCrLf ? "\r\n" : "\n";
            this.newlineBytes = this.encoding.GetBytes(this.newline);
        }

        protected override void Write(JsonRpcMessage content, CancellationToken cancellationToken)
        {
            var buffer = new ArrayBufferWriter<byte>();
            Formatter.Serialize(buffer, content);

            if (delimiterType == DelimiterType.NewLine)
            {
                Writer!.Write(buffer.WrittenSpan);
                Writer!.Write(newlineBytes);
            }
            else // Header
            {
                // Write Content-Length header
                var header = $"Content-Length: {buffer.WrittenCount}{newline}{newline}";
                var headerBytes = encoding.GetBytes(header);
                Writer!.Write(headerBytes);
                Writer!.Write(buffer.WrittenSpan);
            }
        }

        protected override async ValueTask<JsonRpcMessage?> ReadCoreAsync(CancellationToken cancellationToken)
        {
            if (delimiterType == DelimiterType.NewLine)
            {
                while (true)
                {
                    var result = await Reader!.ReadAsync(cancellationToken).ConfigureAwait(false);
                    var buffer = result.Buffer;
                    var pos = buffer.PositionOf(newlineBytes[0]);
                    if (pos != null)
                    {
                        var messageBytes = buffer.Slice(0, pos.Value);
                        var message = Formatter.Deserialize(messageBytes);
                        Reader.AdvanceTo(buffer.GetPosition(newlineBytes.Length, pos.Value));
                        return message;
                    }
                    if (result.IsCompleted)
                    {
                        if (buffer.Length > 0)
                        {
                            var message = Formatter.Deserialize(buffer);
                            Reader.AdvanceTo(buffer.End);
                            return message;
                        }
                        break;
                    }
                    Reader.AdvanceTo(buffer.Start, buffer.End);
                }
            }
            else // Header
            {
                while (true)
                {
                    var result = await Reader!.ReadAsync(cancellationToken).ConfigureAwait(false);
                    var buffer = result.Buffer;
                    var headerEnd = FindHeaderEnd(buffer, newlineBytes);
                    if (headerEnd != null)
                    {
                        var headerBytes = buffer.Slice(0, headerEnd.Value);
                        var headerText = encoding.GetString(headerBytes.ToArray());
                        var contentLength = ParseContentLength(headerText);
                        var messageStart = buffer.GetPosition(newlineBytes.Length, headerEnd.Value);
                        var messageEnd = buffer.GetPosition(contentLength, messageStart);
                        if (buffer.Slice(messageStart, contentLength).Length == contentLength)
                        {
                            var messageBytes = buffer.Slice(messageStart, contentLength);
                            var message = Formatter.Deserialize(messageBytes);
                            Reader.AdvanceTo(messageEnd);
                            return message;
                        }
                    }
                    if (result.IsCompleted)
                    {
                        break;
                    }
                    Reader.AdvanceTo(buffer.Start, buffer.End);
                }
            }
            return null;
        }

        private static SequencePosition? FindHeaderEnd(ReadOnlySequence<byte> buffer, byte[] newlineBytes)
        {
            var reader = new SequenceReader<byte>(buffer);
            int consecutiveNewlines = 0;
            while (!reader.End)
            {
                if (reader.IsNext(newlineBytes, advancePast: true))
                {
                    consecutiveNewlines++;
                    if (consecutiveNewlines == 2)
                    {
                        return reader.Position;
                    }
                }
                else
                {
                    reader.Advance(1);
                    consecutiveNewlines = 0;
                }
            }
            return null;
        }

        private static int ParseContentLength(string headerText)
        {
            foreach (var line in headerText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                {
                    var value = line.Substring("Content-Length:".Length).Trim();
                    if (int.TryParse(value, out int length))
                    {
                        return length;
                    }
                }
            }
            throw new InvalidOperationException("Content-Length header not found or invalid.");
        }
    }
}
