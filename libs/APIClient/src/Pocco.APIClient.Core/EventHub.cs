using System.Reactive.Linq;
using System.Reactive.Subjects;
using static Pocco.APIClient.Core.Events;

namespace Pocco.APIClient.Core;

public class EventHub : IDisposable {
    private readonly Subject<object> _eventPool = new();

    /// <summary>
    /// イベントを発行します。
    /// </summary>
    /// <typeparam name="TEvent"><see cref="BaseEvent"/>を継承したイベントレコード</typeparam>
    /// <param name="e">イベントデータ</param>
    public void Publish<TEvent>(TEvent e) where TEvent : BaseEvent {
        _eventPool.OnNext(e);
    }
    /// <summary>
    /// 特定のイベントの購読を取得します。
    /// </summary>
    /// <typeparam name="TEvent"><see cref="BaseEvent"/>を継承したイベントレコード</typeparam>
    /// <returns><seealso cref="IObservable{TEvent}"/>インスタンス</returns>
    public IObservable<TEvent> GetObservable<TEvent>() where TEvent : BaseEvent {
        return _eventPool.OfType<TEvent>();
    }

    public void Dispose() {
        _eventPool.OnCompleted();
        _eventPool.Dispose();

        GC.SuppressFinalize(this);
    }
}
