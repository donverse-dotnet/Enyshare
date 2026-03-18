namespace Pocco.Svc.CoreAPI.Services;

public interface IHotStartableService {
  /// <summary>
  /// IHotStartableServiceを継承したサービスクラスを起動するために、Kestrelが起動した後に呼び出される。
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns><seealso cref="Task"/></returns>
  Task WarmUpAsync(IServiceProvider sp, CancellationToken cancellationToken);
  /// <summary>
  /// IHotStartableServiceを継承したサービスクラスを停止するために、Kestrelが停止する前に呼び出される。
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns><seealso cref="Task"/></returns>
  Task CoolDownAsync(CancellationToken cancellationToken);
}
