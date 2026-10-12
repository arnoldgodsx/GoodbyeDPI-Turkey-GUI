using System.Windows;

namespace Hub;

/// <summary>
/// Arayüzdeki her sayfa bir modüldür. Yeni modül eklemek için bu arayüzü uygulayıp
/// App.xaml.cs içindeki listeye eklemek yeterli; kenar çubuğu ve sayfa geçişi otomatik gelir.
/// </summary>
public interface IModule
{
    string Title { get; }

    /// <summary>Arayüz açılırken bir kez çağrılır; arka plan işi burada başlar.</summary>
    void Start();

    void Stop();

    /// <summary>Sayfa ilk açılışta bir kez oluşturulur ve sonra saklanır.</summary>
    FrameworkElement CreatePage();
}
