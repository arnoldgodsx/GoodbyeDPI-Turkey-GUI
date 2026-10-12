using System.Windows;
using System.Windows.Controls;

namespace Hub.Modules.GoodbyeDpi;

public partial class GoodbyeDpiPage : UserControl
{
    readonly GoodbyeDpiModule _m;
    readonly List<(Profile Profile, RadioButton Row)> _rows = [];

    public GoodbyeDpiPage(GoodbyeDpiModule module)
    {
        InitializeComponent();
        _m = module;

        foreach (var p in Profile.All)
        {
            var row = new RadioButton
            {
                Style = (Style)FindResource("OptionRow"),
                GroupName = "profile",
                Content = p.Name,
                ToolTip = p.Hint,
            };
            row.Click += (_, _) => _m.SetProfile(p);
            ProfileList.Children.Add(row);
            _rows.Add((p, row));
        }

        _m.Changed += () => Dispatcher.BeginInvoke(Render);
        Render();
    }

    void Render()
    {
        // Açılırken/kapanırken güç anahtarı atar; ayrı bir durum yazısı yok.
        Motion.Pulse(MainSwitch, _m.Busy);

        // Meşgulken anahtar kullanıcının istediği konumda bekler.
        if (!_m.Busy)
        {
            MainSwitch.IsChecked = _m.Active;
            AutoSwitch.IsChecked = _m.AutoStart;
        }
        // Soluklaştırmak yerine sadece tıklamaya kapatılır.
        MainSwitch.IsHitTestVisible = AutoSwitch.IsHitTestVisible = ProfileList.IsHitTestVisible = !_m.Busy;

        // Elle ayarlanmış parametrelerde hiçbir satır seçili görünmez.
        foreach (var (p, row) in _rows) row.IsChecked = p.Args == _m.Profile.Args;

        ErrorText.Text = _m.Error;
        ErrorText.Visibility = _m.Error is null ? Visibility.Collapsed : Visibility.Visible;
    }

    void OnMainToggle(object sender, RoutedEventArgs e) => _m.SetActive(MainSwitch.IsChecked == true);
    void OnAutoToggle(object sender, RoutedEventArgs e) => _m.SetAutoStart(AutoSwitch.IsChecked == true);
}
