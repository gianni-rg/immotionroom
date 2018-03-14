namespace ImmotionAR.ImmotionRoom.DataSourceService
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Helpers;
    using Helpers.Messaging;
    using Interfaces;
    using Logger;
    using Model;

    public class CommandProcessor : ICommandProcessor
    {
        private static readonly object LockObj = new object();
        private bool m_IsStopping;

        private readonly ILogger m_Logger;
        private readonly IMessenger m_Messenger;
        private readonly IConfigurationService m_ConfigurationService;

        private AutoResetEvent m_CommandReceived;

        // See: http://blogs.msdn.com/b/pfxteam/archive/2012/05/08/concurrentqueue-lt-t-gt-holding-on-to-a-few-dequeued-elements.aspx
        private ConcurrentQueue<Command> m_Commands;
        private ConcurrentDictionary<string, CommandResult<object>> m_Responses;
        private CancellationTokenSource m_TokenSource;

        public CommandProcessor(IConfigurationService configurationService)
        {
            m_Logger = LoggerService.GetLogger<CommandProcessor>();
            m_Messenger = MessengerService.Messenger;
            m_ConfigurationService = configurationService;

            m_Messenger.Register<Command>(this, HandleCommand);
            m_Messenger.Register<CommandResult<object>>(this, ReceiveCommandResult);
            m_Messenger.Register<CommandResultRequest>(this, RetrieveRequestStatus);
        }

        public void Start()
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("Start called");
            }

            m_Commands = new ConcurrentQueue<Command>();
            m_Responses = new ConcurrentDictionary<string, CommandResult<object>>();
            m_TokenSource = new CancellationTokenSource();

            m_CommandReceived = new AutoResetEvent(false);

            Task.Factory.StartNew(CommandHandler, m_TokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            lock (LockObj)
            {
                m_IsStopping = false;
            }
        }

        public void Stop()
        {
            lock (LockObj)
            {
                m_IsStopping = true;
            }

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("Stop called");
            }

            if (m_TokenSource != null)
            {
                m_TokenSource.Cancel();
            }
            
            m_Messenger.Unregister<Command>(this);
            m_Messenger.Unregister<CommandResult<object>>(this);
            m_Messenger.Unregister<CommandResultRequest>(this);

            if (m_CommandReceived != null)
            {
                m_CommandReceived.Dispose();
                m_CommandReceived = null;
            }
       }

        public void EnqueueCommand(Command command)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("EnqueueCommand called");
            }

            lock (LockObj)
            {
                if (m_IsStopping)
                {
                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("CommandProcessor is stopping.. ignore command");
                    }
                    return;
                }
            }

            if (command == null)
            {
                return;
            }

            command.Timestamp = DateTime.UtcNow;

            m_Commands.Enqueue(command);

            try
            {
                if (m_CommandReceived != null)
                {
                    m_CommandReceived.Set();
                }
            }
            catch (ObjectDisposedException)
            {
                // Ignore
            }
        }

        private void CommandHandler()
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("CommandHandler started");
            }

            int cleanerCounter = m_ConfigurationService.CurrentConfiguration.ReceivedCommandsCleanerIntervalInMinutes/60;
            while (true)
            {
                var events = new WaitHandle[] {m_TokenSource.Token.WaitHandle, m_CommandReceived};

                var index = WaitHandle.WaitAny(events);

                if (index == 0)
                {
                    // Stop event has been set. Exit task
                    break;
                }

                Command dequeuedCommand;
                while (m_Commands.TryDequeue(out dequeuedCommand))
                {
                    Command command = dequeuedCommand;
                    Task.Factory.StartNew(() => ProcessCommand(command));
                }

                // Removes read or old CommandResults every "cleanerCounter" seconds
                cleanerCounter--;
                if (cleanerCounter == 0)
                {
                    foreach (var commandResult in m_Responses.Values)
                    {
                        if (commandResult.Read || (DateTime.UtcNow - commandResult.Timestamp).TotalSeconds >= m_ConfigurationService.CurrentConfiguration.MaxMessageAliveTimeInSeconds)
                        {
                            CommandResult<object> removed;
                            m_Responses.TryRemove(commandResult.RequestId, out removed);
                        }
                    }

                    cleanerCounter = m_ConfigurationService.CurrentConfiguration.ReceivedCommandsCleanerIntervalInMinutes/60;
                }
            }
            
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("CommandHandler terminated");
            }
        }

        private void ProcessCommand(Command dequeuedCommand)
        {
            if (dequeuedCommand == null || dequeuedCommand.CommandType == CommandType.Undefined)
            {
                return;
            }

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("Dequeued command (RequestID: {0} Type: {1})", dequeuedCommand.RequestId, dequeuedCommand.CommandType);
            }

            // TODO: implement commands
            switch (dequeuedCommand.CommandType)
            {
                case CommandType.EnableAutoDiscovery:
                    m_Messenger.Send(new EnableAutoDiscoveryEventArgs {RequestId = dequeuedCommand.RequestId});
                    break;
                case CommandType.ServiceStatus:
                    m_Messenger.Send(new GetServiceStatusEventArgs {RequestId = dequeuedCommand.RequestId});
                    break;
                case CommandType.StartTracking:
                    m_Messenger.Send(new StartTrackingSystemEventArgs { RequestId = dequeuedCommand.RequestId, Configuration = (TrackingSessionConfiguration)dequeuedCommand.Data["TrackingSessionConfiguration"] });
                    break;
                case CommandType.StopTracking:
                    m_Messenger.Send(new StopTrackingSystemEventArgs {RequestId = dequeuedCommand.RequestId});
                    break;
                case CommandType.SystemReboot:
                    m_Messenger.Send(new SystemRebootEventArgs {RequestId = dequeuedCommand.RequestId});
                    break;
            }
        }

        private void ReceiveCommandResult(CommandResult<object> result)
        {
            if (result == null)
            {
                return;
            }

            if (!m_Responses.ContainsKey(result.RequestId))
            {
                m_Responses[result.RequestId] = result;
            }
        }

        private void RetrieveRequestStatus(CommandResultRequest request)
        {
            if (!m_Responses.ContainsKey(request.RequestId))
            {
                request.Execute(new object[] { null });
                return;
            }

            m_Responses[request.RequestId].Read = true;

            request.Execute(m_Responses[request.RequestId]);
        }

        private void HandleCommand(Command c)
        {
            EnqueueCommand(c);
        }
    }
}
