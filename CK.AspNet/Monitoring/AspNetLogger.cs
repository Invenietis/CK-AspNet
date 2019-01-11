using CK.Monitoring;
using Microsoft.Extensions.Logging;
using System;

namespace CK.AspNet
{
    /// <summary>
    /// The <see cref="ILogger"/> for the <see cref="GrandOutput"/>.
    /// </summary>
    sealed class AspNetLogger : ILogger
    {
        readonly string _categoryName;
        readonly GrandOutput _output;
        readonly AspNetLoggerProvider _provider;

        static Core.LogLevel[] _mapLevels = new Core.LogLevel[]
        {
            Core.LogLevel.Debug, // <= LogLevel.Trace
            Core.LogLevel.Trace, // <= LogLevel.Debug
            Core.LogLevel.Info,  // <= LogLevel.Info
            Core.LogLevel.Warn,  // <= LogLevel.Warn
            Core.LogLevel.Error, // <= LogLevel.Error
            Core.LogLevel.Fatal, // <= LogLevel.Fatal
            Core.LogLevel.None   // <= LogLevel.None
        };

        public AspNetLogger( AspNetLoggerProvider provider, string categoryName, GrandOutput output )
        {
            _categoryName = categoryName ?? String.Empty;
            _output = output;
            _provider = provider;
        }

        /// <summary>
        /// Justs logs the state of the new scope as an external log.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state"></param>
        /// <returns></returns>
        public IDisposable BeginScope<TState>( TState state )
        {
            if( _provider.Running ) _output.ExternalLog( Core.LogLevel.Trace, state?.ToString() );
            return Core.Util.EmptyDisposable;
        }

        /// <summary>
        /// Challenges <see cref="GrandOutput.IsExternalLogEnabled(Core.LogLevel)"/>
        /// (using <see cref="FromAspNetCoreLogLevel(Microsoft.Extensions.Logging.LogLevel)"/>).
        /// </summary>
        /// <param name="logLevel">The log level to challenge.</param>
        /// <returns>True if this level is active, false if it should not be logged.</returns>
        public bool IsEnabled( LogLevel logLevel )
        {
            return _provider.Running && _output.IsExternalLogEnabled( FromAspNetCoreLogLevel( logLevel ) );
        }

        /// <summary>
        /// Maps the AspNet <see cref="Microsoft.Extensions.Logging.LogLevel"/> to
        /// ActivityMonitor <see cref="Core.LogLevel"/>.
        /// Trace and Debug are inverted.
        /// </summary>
        /// <param name="l">The AspNet level.</param>
        /// <returns>The corresponding <see cref="Core.LogLevel"/>.</returns>
        public static Core.LogLevel FromAspNetCoreLogLevel( Microsoft.Extensions.Logging.LogLevel l ) => _mapLevels[(int)l];

        /// <summary>
        /// Logs to the <see cref="GrandOutput"/> as an external log entry.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="exception"></param>
        /// <param name="formatter"></param>
        public void Log<TState>( LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter )
        {
            if( _provider.Running )
            {
                _output.ExternalLog( FromAspNetCoreLogLevel( logLevel ), $"[{_categoryName}] {formatter( state, exception )}", exception );
            }
        }
    }
}
