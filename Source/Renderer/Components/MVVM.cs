using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Renderer.Components;

internal abstract partial class MVVM : ObservableRecipient, IDisposable
{
    private volatile uint _isDisposed;

    ~MVVM()
    {
        Dispose();
    }

    public string Id { get; } = Guid.NewGuid().ToString();

    public bool IsDisposed => _isDisposed != 0;

    public void SendMessage<TMessage>(TMessage message) where TMessage : class
    {
        Messenger.Send(message);
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
        {
            return;
        }

        Destroy();

        GC.SuppressFinalize(this);
    }

    protected abstract void Destroy();
}
