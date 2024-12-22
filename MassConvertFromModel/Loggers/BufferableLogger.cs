using System.Runtime.CompilerServices;
using System.Text;

namespace MassConvertFromModel.Loggers
{
    /// <summary>
    /// A logger that supports buffering, then dumping contents all at once into the log.
    /// </summary>
    internal class BufferableLogger : ILogger
    {
        /// <summary>
        /// The actions for writing.
        /// </summary>
        private readonly List<Action<string>> WriteActions;

        /// <summary>
        /// The buffer to save writes before dump.
        /// </summary>
        private readonly StringBuilder Buffer;

        /// <summary>
        /// The writes since the last update.
        /// </summary>
        private int WritesSinceLastUpdate;

        /// <summary>
        /// The number of writes the logger will wait before updating the log.
        /// </summary>
        public int UpdateInterval { get; set; }

        /// <summary>
        /// Creates a new <see cref="BufferableLogger"/> with the specified update interval.
        /// </summary>
        /// <param name="updateInterval">The number of writes the logger will wait before updating the log.</param>
        public BufferableLogger(int updateInterval)
        {
            WriteActions = new List<Action<string>>();
            Buffer = new StringBuilder();
            UpdateInterval = updateInterval;
            WritesSinceLastUpdate = 0;
        }

        /// <summary>
        /// Add a write action.
        /// </summary>
        /// <param name="action">The action to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddAction(Action<string> action)
            => WriteActions.Add(action);

        /// <summary>
        /// Remove a write action.
        /// </summary>
        /// <param name="action">The action to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAction(Action<string> action)
            => WriteActions.Remove(action);

        /// <summary>
        /// Clear all write actions.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearActions()
            => WriteActions.Clear();

        /// <summary>
        /// Write the specified value.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void Write(string value)
        {
            Buffer.Append(value);
            UpdateBuffer();
        }

        /// <summary>
        /// Write the specified value to a line.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteLine(string value)
        {
            Buffer.AppendLine(value);
            UpdateBuffer();
        }

        /// <summary>
        /// Flush all queued writes to the logger.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Flush()
         => UpdateLog();

        /// <summary>
        /// Update the buffer.
        /// </summary>
        private void UpdateBuffer()
        {
            WritesSinceLastUpdate++;
            if (WritesSinceLastUpdate == UpdateInterval)
            {
                UpdateLog();
            }
        }

        /// <summary>
        /// Update the log by flushing the entire buffer.
        /// </summary>
        private void UpdateLog()
        {
            string buffer = Buffer.ToString();
            foreach (var write in WriteActions)
            {
                write(buffer);
            }

            WritesSinceLastUpdate = 0;
            Buffer.Clear();
        }
    }
}
