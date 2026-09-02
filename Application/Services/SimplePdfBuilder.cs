using System.Text;
using System.IO;

namespace APISales.Application.Services
{
    public static class SimplePdfBuilder
    {
        public static byte[] BuildFromLines(IReadOnlyList<string> lines, string title = "Comprovante")
        {
            var normalizedLines = (lines ?? Array.Empty<string>())
                .Select(l => (l ?? string.Empty).TrimEnd())
                .ToList();

            if (normalizedLines.Count == 0)
            {
                normalizedLines.Add(title);
            }

            var content = BuildContentStream(PrepareLinesForRender(normalizedLines));
            return BuildPdf(content);
        }

        private static IReadOnlyList<string> PrepareLinesForRender(IReadOnlyList<string> lines)
        {
            var statusLine = lines.FirstOrDefault(l => l.StartsWith("Status atual:", StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
            var isReadyForPickup = statusLine.IndexOf("PRONTA PARA RETIRADA", StringComparison.OrdinalIgnoreCase) >= 0;
            var prepared = new List<string>();

            foreach (var raw in lines)
            {
                var line = (raw ?? string.Empty).TrimEnd();
                if (line.StartsWith("Entregue", StringComparison.OrdinalIgnoreCase))
                    continue;

                var isPolicyLine = line.StartsWith("Politica da loja", StringComparison.OrdinalIgnoreCase)
                    || line.StartsWith("Retirada:", StringComparison.OrdinalIgnoreCase)
                    || line.StartsWith("Reparo:", StringComparison.OrdinalIgnoreCase);
                if (isPolicyLine && !isReadyForPickup)
                    continue;

                prepared.AddRange(WrapLine(line, 72));
            }

            return prepared;
        }

        private static IEnumerable<string> WrapLine(string line, int maxLen)
        {
            var value = (line ?? string.Empty).TrimEnd();
            if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLen)
                return new[] { value };

            var chunks = new List<string>();
            var remaining = value;
            while (remaining.Length > maxLen)
            {
                var splitAt = remaining.LastIndexOf(" - ", maxLen, StringComparison.Ordinal);
                if (splitAt <= 0) splitAt = remaining.LastIndexOf(' ', maxLen);
                if (splitAt <= 0) splitAt = maxLen;

                chunks.Add(remaining[..splitAt].Trim());
                remaining = remaining[splitAt..].TrimStart(' ', '-');
            }
            if (!string.IsNullOrWhiteSpace(remaining))
                chunks.Add(remaining);

            return chunks;
        }

        private static byte[] BuildContentStream(IReadOnlyList<string> lines)
        {
            var sb = new StringBuilder();

            // Page background (same light tone used in app screens)
            sb.AppendLine("q");
            sb.AppendLine("0.9608 0.9686 0.9804 rg");
            sb.AppendLine("0 0 595 842 re f");
            sb.AppendLine("Q");

            // Simple vector/text brand mark (no external image lib needed)
            sb.AppendLine("q");
            sb.AppendLine("0.3059 0.1255 0.9451 rg");
            sb.AppendLine("272 800 50 24 re f");
            sb.AppendLine("Q");
            sb.AppendLine("BT");
            sb.AppendLine("/F1 14 Tf");
            sb.AppendLine("1 1 1 rg");
            sb.AppendLine("285 806 Td");
            sb.AppendLine("(KS) Tj");
            sb.AppendLine("ET");

            var maxLines = Math.Min(lines.Count, 56);
            var headerSeparatorIndex = lines
                .Select((line, index) => new { line, index })
                .FirstOrDefault(x => x.line.StartsWith("---", StringComparison.Ordinal))?.index ?? 2;
            var mainTextStarted = false;
            for (var i = 0; i < maxLines; i++)
            {
                var line = EscapePdfText(lines[i]);
                var lowered = line.ToLowerInvariant();
                if (i <= headerSeparatorIndex)
                {
                    var x = Math.Max(40, 297 - (line.Length * 2.7));
                    var y = 760 - (i * 14);
                    sb.AppendLine("BT");
                    sb.AppendLine("/F1 10 Tf");
                    if (i == 0)
                    {
                        sb.AppendLine("0.3059 0.1255 0.9451 rg");
                    }
                    else
                    {
                        sb.AppendLine("0.9451 0.3059 0.1255 rg");
                    }
                    sb.AppendLine($"{x:0} {y:0} Td");
                    sb.AppendLine($"({line}) Tj");
                    sb.AppendLine("ET");
                    continue;
                }

                if (i == headerSeparatorIndex + 1)
                {
                    mainTextStarted = true;
                    sb.AppendLine("BT");
                    sb.AppendLine("/F1 11 Tf");
                    sb.AppendLine("14 TL");
                    sb.AppendLine("50 690 Td");
                }

                if (lowered.Contains("entrega prevista:") || lowered.StartsWith("- "))
                {
                    sb.AppendLine("0.3059 0.1255 0.9451 rg");
                }
                else if (lowered.Contains(" - ") && (lowered.Contains("@") || lowered.Contains("(")))
                {
                    sb.AppendLine("0.9451 0.3059 0.1255 rg");
                }
                else
                {
                    sb.AppendLine("0.12 0.16 0.2 rg");
                }
                if (i == headerSeparatorIndex + 1)
                {
                    sb.AppendLine($"({line}) Tj");
                }
                else
                {
                    sb.AppendLine("T*");
                    sb.AppendLine($"({line}) Tj");
                }
            }

            if (mainTextStarted)
            {
                sb.AppendLine("ET");
            }

            // Footer brand mark
            sb.AppendLine("q");
            sb.AppendLine("0.3059 0.1255 0.9451 rg");
            sb.AppendLine("275 44 44 20 re f");
            sb.AppendLine("Q");
            sb.AppendLine("BT");
            sb.AppendLine("/F1 12 Tf");
            sb.AppendLine("1 1 1 rg");
            sb.AppendLine("289 49 Td");
            sb.AppendLine("(KS) Tj");
            sb.AppendLine("ET");

            // Footer brand text
            sb.AppendLine("BT");
            sb.AppendLine("/F1 10 Tf");
            sb.AppendLine("0.12 0.16 0.2 rg");
            sb.AppendLine("265 24 Td");
            sb.AppendLine("(KuwenSys) Tj");
            sb.AppendLine("ET");
            return EncodeLatin1(sb.ToString());
        }

        private static byte[] BuildPdf(byte[] contentStream)
        {
            var objects = new List<byte[]>
            {
                EncodeLatin1("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n"),
                EncodeLatin1("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n"),
                EncodeLatin1("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>\nendobj\n"),
                EncodeLatin1("4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>\nendobj\n"),
                BuildContentObject(contentStream),
            };

            using var stream = new MemoryStream();
            Write(stream, EncodeLatin1("%PDF-1.4\n%\u00E2\u00E3\u00CF\u00D3\n"));

            var offsets = new List<long> { 0 };
            foreach (var obj in objects)
            {
                offsets.Add(stream.Position);
                Write(stream, obj);
            }

            var xrefPosition = stream.Position;
            Write(stream, EncodeLatin1($"xref\n0 {objects.Count + 1}\n"));
            Write(stream, EncodeLatin1("0000000000 65535 f \n"));
            for (var i = 1; i <= objects.Count; i++)
            {
                Write(stream, EncodeLatin1($"{offsets[i]:D10} 00000 n \n"));
            }

            Write(stream, EncodeLatin1("trailer\n"));
            Write(stream, EncodeLatin1($"<< /Size {objects.Count + 1} /Root 1 0 R >>\n"));
            Write(stream, EncodeLatin1("startxref\n"));
            Write(stream, EncodeLatin1($"{xrefPosition}\n"));
            Write(stream, EncodeLatin1("%%EOF"));

            return stream.ToArray();
        }

        private static byte[] BuildContentObject(byte[] contentStream)
        {
            using var stream = new MemoryStream();
            Write(stream, EncodeLatin1($"5 0 obj\n<< /Length {contentStream.Length} >>\nstream\n"));
            Write(stream, contentStream);
            Write(stream, EncodeLatin1("endstream\nendobj\n"));
            return stream.ToArray();
        }

        private static void Write(Stream stream, byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }

        private static string EscapePdfText(string value)
        {
            var safe = (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)");

            var bytes = Encoding.Latin1.GetBytes(safe);
            return Encoding.Latin1.GetString(bytes);
        }

        private static byte[] EncodeLatin1(string text)
        {
            return Encoding.Latin1.GetBytes(text);
        }
    }
}
