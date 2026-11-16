//using TickerQ.Utilities.Enums;
//using TickerQ.Utilities.Interfaces;

//namespace SorteFederation.Builders.TickerQ;

//public class TickerExceptionHandler : ITickerExceptionHandler
//{
//    private readonly ILogger<TickerExceptionHandler> _logger;

//    public TickerExceptionHandler(ILogger<TickerExceptionHandler> logger)
//    {
//        _logger = logger;
//    }

//    public async Task HandleExceptionAsync(Exception exception, Guid tickerId, TickerType tickerType)
//    {
//        _logger.LogError(exception, "Error occurred while processing ticker {TickerId} of type {TickerType}", tickerId, tickerType);
//        await Task.CompletedTask;
//    }

//    public async Task HandleCanceledExceptionAsync(Exception exception, Guid tickerId, TickerType tickerType)
//    {
//        _logger.LogWarning(exception, "Processing of ticker {TickerId} of type {TickerType} was canceled", tickerId, tickerType);
//        await Task.CompletedTask;
//    }
//} 
