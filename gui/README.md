# GoodbyeDPI-Turkey GUI: geliştirici notları

[GoodbyeDPI-Turkey](https://github.com/cagritaskn/GoodbyeDPI-Turkey) için Windows arayüzü:
`.cmd` dosyalarıyla uğraşmadan hizmeti açıp kapatır, profil seçer, Windows ile başlatmayı ayarlar.
Profiller `windows\service_install_dnsredir_turkey*.cmd` betikleriyle birebir aynı parametreleri kullanır.

## Kurulum (v1.0)

**[GoodbyeDPI-Turkey-GUI-v1.0-Setup.exe](https://github.com/arnoldgodsx/GoodbyeDPI-Turkey-GUI/releases/latest/download/GoodbyeDPI-Turkey-GUI-v1.0-Setup.exe)** dosyasını indirip çalıştırın.
Program `C:\Program Files\GoodbyeDPI-Turkey` klasörüne kurulur, masaüstüne ve Başlat menüsüne kısayol eklenir;
ek bir şey kurmanız gerekmez. Kaldırmak için Ayarlar → Uygulamalar → GoodbyeDPI-Turkey (hizmet ve görevler de silinir).

## Arayüz

Uygulamaları tek pencerede toplayan panel yapısındadır. Koyu, siyah-beyaz arayüz:
solda uygulamalar, sağda seçilen uygulamanın sayfası. Bildirim alanında simgesi yoktur;
pencere kapatılınca program da kapanır (GoodbyeDPI hizmeti etkilenmez).

| Uygulama        | Ne yapar |
|-----------------|----------|
| GoodbyeDPI      | `GoodbyeDPI` Windows hizmetini açar/kapatır, profil ve otomatik başlatma |

## Yetki ve açılış

Arayüz yönetici olarak çalışır (GoodbyeDPI hizmeti için). Her açılışta UAC sormamak için
Görev Zamanlayıcı'da tetikleyicisiz bir `GoodbyeDPI-Turkey GUI` görevi kullanır.
Kısayol normal yetkiyle `GoodbyeDPI-Turkey-GUI.exe`'yi açar, o da görevi tetikleyip çıkar.
v1.0'daki eski `Hub` görevleri ve önceki sürümün `GoodbyeDPI-Turkey GUI Autostart` görevi güncellemede kurulum tarafından kaldırılır.

## Klasör yapısı

```
App.xaml(.cs)        açılış, tek kopya, modül listesi
MainWindow.xaml      pencere: kenar çubuğu + sayfa alanı
Core\Theme.xaml      ortak renkler ve stiller (anahtar, kart, düğme, menü…)
Core\                IModule, ikon üretici, halka, komut çalıştırıcı, yetki/görevler
Modules\<Ad>\        her uygulama kendi klasöründe
engine\goodbyedpi\   orijinal GoodbyeDPI paketi (exe'nin yanına kopyalanır)
tools\               make-icon.ps1, install.ps1 (geliştirme kurulumu)
installer\setup.iss  kurulum exe'si (Inno Setup)
dist\                derleme çıktısı (GoodbyeDPI-Turkey-GUI.exe, repoda tutulmaz)
```

## Yeni uygulama eklemek

1. `Modules\Yeni\YeniModule.cs` içinde `IModule`'ü uygulayın (başlık, `Start`/`Stop`, `CreatePage`).
2. `Modules\Yeni\YeniPage.xaml` sayfasını mevcut sayfalar gibi kurun: üstte `Title` + `Chip` durum kapsülü,
   altında sadece işlevsel satırlar. Açılışta belirecek öğelere `hub:Motion.Reveal="True"` verin.
   Renk sadece durum için (`Ok`, `Danger`); stiller `Core\Theme.xaml`'da.
3. `App.xaml.cs` → `Modules = [ ..., new YeniModule() ];`

Klavye: `Ctrl+1…9` sayfa seçer, `Esc` pencereyi kapatır.

## Derleme ve kurulum

`gui\` klasöründe (.NET SDK gerekir; hedef .NET Framework 4.8, Windows'ta hazır gelir). Kurulum exe'si Actions → "Build GUI" → Run workflow ("release" işaretli)
ile ya da `v*` etiketi push'lanınca derlenip Release'e eklenir; elle derlemek için publish'ten sonra
`iscc installer\setup.iss` (Inno Setup 6). Geliştirme kurulumu için:

```
dotnet publish GoodbyeDPI-Turkey-GUI.csproj -c Release -o dist
powershell -ExecutionPolicy Bypass -File tools\install.ps1   # yönetici olarak
```

**Güvenlik kuralı:** GoodbyeDPI hizmeti SYSTEM olarak, arayüz de UAC'siz görevle yönetici olarak çalışır.
Bu yüzden program, dosyaları yönetici olmayanların (ya da kendi hesabınızın yükseltilmemiş süreçlerinin)
değiştirebildiği bir klasörden çalışıyorsa hizmeti kurmayı/değiştirmeyi reddeder ve UAC'siz açılış görevini
kurmaz (`Core\Acl.cs`). `dist\` gibi kullanıcı klasörlerindeki kopyalar yalnızca arayüzü denemek içindir;
hizmeti yönetmek için kurulum dosyasıyla Program Files'a kurun.

Tasarım kontrolü için yetki gerekmeden:
- `dist\GoodbyeDPI-Turkey-GUI.exe --snapshot <klasör>` her sayfayı PNG kaydeder (hata olursa `error.txt`).
- `dist\GoodbyeDPI-Turkey-GUI.exe --preview` pencereyi modülleri başlatmadan açar. Anahtarlar yine gerçek komut çalıştırır.
