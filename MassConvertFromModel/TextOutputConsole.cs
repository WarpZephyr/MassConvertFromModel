using System.Diagnostics;

namespace MassConvertFromModel
{
    public class TextOutputConsole
    {
        /// <summary>
        /// Where should output go.
        /// </summary>
        [Flags]
        public enum OutputLocationFlags
        {
            /// <summary>
            /// Output to console.
            /// </summary>
            Console = 1,

            /// <summary>
            /// Output to debug.
            /// </summary>
            Debug = 2,

            /// <summary>
            /// Output to a text file.
            /// </summary>
            Text = 4
        }

        private string Buffer;
        public int BufferThreshold;
        public OutputLocationFlags OutputLocation;
        public string NewLine;
        private TextWriter? TextWriter;

        public TextOutputConsole()
        {
            Buffer = string.Empty;
            BufferThreshold = 1000;
            OutputLocation = OutputLocationFlags.Console;
            NewLine = Environment.NewLine;
        }

        public TextOutputConsole(int bufferThreshold = 1000, OutputLocationFlags flags = OutputLocationFlags.Console, string? newLine = null, string? filePath = null)
        {
            Buffer = string.Empty;
            BufferThreshold = bufferThreshold;
            OutputLocation = flags;
            NewLine = newLine ?? Environment.NewLine;
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                TextWriter = new StreamWriter(filePath);
            }
        }

        public void Write(string text)
        {
            Buffer += text;
            if (Buffer.Length >= BufferThreshold)
            {
                WriteBuffer();
            }
        }

        public void WriteLine(string text)
        {
            Buffer += text + NewLine;
            if (Buffer.Length >= BufferThreshold)
            {
                WriteBuffer();
            }
        }

        private void WriteBuffer()
        {
            if ((OutputLocation & OutputLocationFlags.Console) != 0)
            {
                Console.Write(Buffer);
            }

            if ((OutputLocation & OutputLocationFlags.Debug) != 0)
            {
                Debug.Write(Buffer);
            }

            if (TextWriter != null && (OutputLocation & OutputLocationFlags.Text) != 0)
            {
                TextWriter.Write(Buffer);
            }

            Buffer = string.Empty;
        }

        public void Finish()
        {
            WriteBuffer();
        }

        public void SetTextFile(string path)
        {
            TextWriter = new StreamWriter(path);
        }
    }
}
