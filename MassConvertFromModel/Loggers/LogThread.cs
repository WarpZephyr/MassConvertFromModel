using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace MassConvertFromModel.Loggers
{
    /// <summary>
    /// A logger that logs on a separate thread.
    /// </summary>
    internal class LogThread : ILogger
    {
        /// <summary>
        /// The write queue.
        /// </summary>
        private readonly BlockingCollection<string> Queue;

        /// <summary>
        /// The write event.
        /// </summary>
        private event EventHandler<string>? WriteEvent;

        /// <summary>
        /// Creates a new <see cref="LogThread"/>.
        /// </summary>
        internal LogThread()
        {
            Queue = [];
            var thread = new Thread(
              () =>
              {
                  while (true)
                      while (Queue.Count > 0)
                        WriteEvent?.Invoke(this, Queue.Take());
              })
            {
                IsBackground = true
            };

            thread.Start();
        }

        #region Write

        /// <summary>
        /// Write the specified value.
        /// </summary>
        /// <param name="value">The value to write.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(string value)
            => Queue.Add(value);

        /// <summary>
        /// Write the specified value to a line.
        /// </summary>
        /// <param name="value">The value to write.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteLine(string value)
            => Queue.Add(value + '\n');

        #endregion

        #region Actions

        /// <summary>
        /// Add a write action.
        /// </summary>
        /// <param name="action">The action to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddAction(Action<string> action)
            => WriteEvent += (sender, args) => action(args);

        /// <summary>
        /// Remove a write action.
        /// </summary>
        /// <param name="action">The action to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAction(Action<string> action)
            => WriteEvent -= (sender, args) => action(args);

        /// <summary>
        /// Clear all write actions.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearActions()
            => WriteEvent = null;

        #endregion
    }
}
