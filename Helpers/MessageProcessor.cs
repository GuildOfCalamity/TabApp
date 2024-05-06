using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace TabApp.Helpers;

/// <summary>
/// A simple file logger utilizing <see cref="System.Collections.Concurrent.BlockingCollection{T}"/>.
/// </summary>
public class MessageProcessor : IDisposable
{
    const int _maxQueuedMessages = 1024;
    readonly string _fileName = "Messages.log";
    readonly Thread _outputThread;
    readonly BlockingCollection<MessageEntry> _messageQueue = new BlockingCollection<MessageEntry>(_maxQueuedMessages);

    public MessageProcessor()
    {
        _outputThread = new Thread(ProcessLogQueue)
        {
            IsBackground = true,
            Priority = ThreadPriority.BelowNormal,
            Name = "Message queue processing thread", 
        };
        _outputThread.Start();
    }

    public virtual void Enqueue(MessageEntry message)
    {
        if (!_messageQueue.IsAddingCompleted)
        {
            try
            {
                _messageQueue.Add(message);
                return;
            }
            catch (InvalidOperationException) { }
        }

        WriteMessage(message); // Adding is completed so just log the message
    }

    
    void WriteMessage(MessageEntry message)
    {
        try
        {
            if (App.IsPackaged)
                System.IO.File.AppendAllText(System.IO.Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, _fileName), $"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff tt")}]--{message.Level}--{message.Message}{Environment.NewLine}");
            else
                System.IO.File.AppendAllText(System.IO.Path.Combine(System.AppContext.BaseDirectory, _fileName), $"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff tt")}] [{message.Level}] [{message.Message}]{Environment.NewLine}");
        }
        catch (Exception)
        {
            Debug.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff tt")}] [{message.Level}] [{message.Message}]");
        }
    }

    void ProcessLogQueue()
    {
        try
        {
            foreach (var message in _messageQueue.GetConsumingEnumerable())
                WriteMessage(message);
        }
        catch
        {
            try { _messageQueue.CompleteAdding(); }
            catch { }
        }
    }

    public void Dispose()
    {
        _messageQueue.CompleteAdding();
        try { _outputThread.Join(1500); }
        catch (ThreadStateException) { }
    }
}

public struct MessageEntry
{
    public string Level;
    public string Message;
}