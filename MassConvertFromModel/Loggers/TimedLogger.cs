using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;
using Timer = System.Timers.Timer;

namespace MassConvertFromModel.Loggers
{
    /// <summary>
    /// A logger that logs on a timed interval.
    /// </summary>
    internal class TimedLogger : ILogger, IDisposable
    {
        /// <summary>
        /// The timer for when to write.
        /// </summary>
        private readonly Timer Timer;

        /// <summary>
        /// The write queue.
        /// </summary>
        private readonly BlockingCollection<string> Queue;

        /// <summary>
        /// The buffer used when writing.
        /// </summary>
        private readonly StringBuilder Buffer;

        /// <summary>
        /// The write event.
        /// </summary>
        private event EventHandler<string>? WriteEvent;

        /// <summary>
        /// Whether or not this has been disposed.
        /// </summary>
        private bool disposedValue;

        /// <summary>
        /// Whether or not this has been disposed.
        /// </summary>
        public bool IsDisposed => disposedValue;

        /// <summary>
        /// Gets or sets the interval, expressed in milliseconds, at which to write.
        /// </summary>
        public double Interval
        {
            get => Timer.Interval;
            set => Timer.Interval = value;
        }

        /// <summary>
        /// Create a new <see cref="TimedLogger"/> with the specified interval.
        /// </summary>
        /// <param name="interval">The time between writes.</param>
        public TimedLogger(double interval)
        {
            Queue = [];
            Buffer = new StringBuilder();
            Timer = new Timer(interval);
            Timer.Elapsed += Timer_Elapsed;
        }

        #region Timer

        /// <summary>
        /// The event for when the timer is elapsed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            while (Queue.Count != 0)
            {
                Buffer.Append(Queue.Take());
            }

            WriteEvent?.Invoke(this, Buffer.ToString());
            Buffer.Clear();
        }

        /// <summary>
        /// Start the timer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartTimer()
            => Timer.Start();

        /// <summary>
        /// Stop the timer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StopTimer()
            => Timer.Stop();

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

        /// <summary>
        /// Flush all queued writes to the logger.
        /// </summary>
        public void Flush()
        {
            Timer.Stop();
            while (Queue.Count != 0)
            {
                Buffer.Append(Queue.Take());
            }

            WriteEvent?.Invoke(this, Buffer.ToString());
            Buffer.Clear();
        }

        #endregion

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Timer.Dispose();
                    Buffer.Clear();
                }

                WriteEvent = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
