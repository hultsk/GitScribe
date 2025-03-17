using Microsoft.Extensions.Logging;
using System.Management.Automation;

namespace GitScribe.Cmdlets
{
   public class PowerShellLogger<T> : ILogger<T>
   {
      private readonly PSCmdlet _cmdlet;
      private readonly string _categoryName;

      public PowerShellLogger(PSCmdlet cmdlet)
      {
         _cmdlet = cmdlet ?? throw new ArgumentNullException(nameof(cmdlet));
         _categoryName = typeof(T).Name;
      }

      IDisposable ILogger.BeginScope<TState>(TState state) => NullScope.Instance;

      public bool IsEnabled(LogLevel logLevel) => true;

      public void Log<TState>(
          LogLevel logLevel,
          EventId eventId,
          TState state,
          Exception? exception,
          Func<TState, Exception?, string> formatter)
      {
         if (!IsEnabled(logLevel))
            return;

         string message = formatter(state, exception);

         switch (logLevel)
         {
            case LogLevel.Critical:
            case LogLevel.Error:
               _cmdlet.WriteError(new ErrorRecord(
                   exception ?? new Exception(message),
                   "GitScibre" + eventId.Id,
                   ErrorCategory.NotSpecified,
                   null));
               break;

            case LogLevel.Warning:
               _cmdlet.WriteWarning(message);
               break;

            case LogLevel.Information:
               _cmdlet.WriteInformation(
                   new InformationRecord(message, "GitScibre" + eventId.Id));
               break;

            case LogLevel.Debug:
            case LogLevel.Trace:
               _cmdlet.WriteVerbose(message);
               break;

            default:
               _cmdlet.WriteObject(message);
               break;
         }
      }

      // Null scope implementation for the BeginScope method
      private class NullScope : IDisposable
      {
         public static NullScope Instance { get; } = new NullScope();

         private NullScope()
         { }

         public void Dispose()
         { }
      }
   }

   // Factory to create PowerShell loggers
   public class PowerShellLoggerFactory
   {
      private readonly PSCmdlet _cmdlet;

      public PowerShellLoggerFactory(PSCmdlet cmdlet)
      {
         _cmdlet = cmdlet;
      }

      public ILogger<T> CreateLogger<T>()
      {
         return new PowerShellLogger<T>(_cmdlet);
      }
   }
}