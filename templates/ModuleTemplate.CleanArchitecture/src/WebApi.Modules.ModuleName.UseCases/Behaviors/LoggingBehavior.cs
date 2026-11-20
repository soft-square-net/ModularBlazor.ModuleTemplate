using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace soft.square.net.WebApi.Modules.ModuleName.UseCases.Behaviors;

/// <summary>
/// Logs every command and query the mediator handles, with its duration (the template's LoggingBehavior).
/// Takes <see cref="ILoggerFactory"/>, not <c>ILogger&lt;T&gt;</c>: an <c>ILogger&lt;T&gt;</c> of a module type would be
/// built by the host's container and cached there under that type, which would keep the module from unloading.
/// </summary>
public sealed class LoggingBehavior<TMessage, TResponse>(ILoggerFactory loggers) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    private readonly ILogger _log = loggers.CreateLogger("soft.square.net.WebApi.Modules.ModuleName.UseCases");

    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        var name = typeof(TMessage).Name;
        _log.LogDebug("Handling {Message}", name);
        var started = Stopwatch.GetTimestamp();
        var response = await next(message, cancellationToken);
        _log.LogDebug("Handled {Message} in {Elapsed} ms", name, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        return response;
    }
}
