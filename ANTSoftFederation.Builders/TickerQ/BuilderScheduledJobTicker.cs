//using SorteFederation.Shared.Contracts;
//using TickerQ.Utilities;
//using TickerQ.Utilities.Base;

//namespace SorteFederation.Builders.TickerQ;

//public sealed class BuilderScheduledJobTicker(IScheduledJobActionDispatcher dispatcher)
//{
//    [TickerFunction("ExecuteBuilderScheduledJob")]
//    public Task Execute(TickerFunctionContext<BuilderScheduledJobMessage> context) =>
//        dispatcher.DispatchAsync(context.Request);
//}
