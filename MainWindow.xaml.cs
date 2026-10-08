using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Hub;

public partial class MainWindow : Window
{
    readonly Dictionary<IModule, FrameworkElement> _pages = [];
    bool _switching;

    public MainWindow()
    {
        InitializeComponent();
        var app = App.Current;
        ModuleList.ItemsSource = app.Modules;
        FooterList.ItemsSource = new IModule[] { app.Settings };
        SourceInitialized += (_, _) => Dwm.Apply(this);
        ModuleList.SelectedIndex = 0;
    }

    void OnModuleSelected(object sender, SelectionChangedEventArgs e)
    {
        if (_switching || ((ListBox)sender).SelectedItem is not IModule module) return;

        // İki liste tek bir seçim gibi davranır.
        _switching = true;
        (sender == ModuleList ? FooterList : ModuleList).SelectedItem = null;
        _switching = false;

        if (!_pages.TryGetValue(module, out var page))
            _pages[module] = page = module.CreatePage();
        PageHost.Content = page;
        Motion.Play(page);
    }

    // ------------------------------------------------------------------ --snapshot için

    public int PageCount => ModuleList.Items.Count + FooterList.Items.Count;

    public void SelectPage(int i)
    {
        if (i < ModuleList.Items.Count) ModuleList.SelectedIndex = i;
        else FooterList.SelectedIndex = i - ModuleList.Items.Count;
    }

    public void SaveSnapshot(string path)
    {
        var dpi = VisualTreeHelper.GetDpi(this);
        var bmp = new System.Windows.Media.Imaging.RenderTargetBitmap(
            (int)(Root.ActualWidth * dpi.DpiScaleX), (int)(Root.ActualHeight * dpi.DpiScaleY),
            dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32);
        bmp.Render(Root);
        var enc = new System.Windows.Media.Imaging.PngBitmapEncoder();
        enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bmp));
        using var fs = File.Create(path);
        enc.Save(fs);
    }

    // ------------------------------------------------------------------ pencere

    void OnDrag(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    void OnHide(object sender, RoutedEventArgs e) => Hide();
    void OnMinimize(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        // Ctrl+1…9 sayfa seçer, Esc arka plana gönderir.
        if (e.Key == Key.Escape) { Hide(); e.Handled = true; }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key is >= Key.D1 and <= Key.D9 && e.Key - Key.D1 < PageCount)
        {
            SelectPage(e.Key - Key.D1);
            e.Handled = true;
        }
        base.OnPreviewKeyDown(e);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // Pencere kapanınca sadece arayüz gizlenir; modüller arka planda çalışmaya devam eder.
        if (!App.Current.Quitting)
        {
            e.Cancel = true;
            Hide();
        }
        base.OnClosing(e);
    }
}
