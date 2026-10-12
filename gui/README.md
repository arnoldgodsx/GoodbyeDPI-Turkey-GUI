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
pencere kapatılınca arka planda çalışmaya devam eder (Görev Yöneticisi'nde "GoodbyeDPI-Turkey GUI").

| Uygulama        | Ne yapar |
|-----------------|----------|
| GoodbyeDPI      | `GoodbyeDPI` Windows hizmetini açar/kapatır, profil ve otomatik başlatma |

## Yetki ve açılış

Arayüz yönetici olarak çalışır (GoodbyeDPI hizmeti için). Her açılışta UAC sormamak için
Görev Zamanlayıcı'da iki görev kullanır: `GoodbyeDPI-Turkey GUI` (kısayoldan açılış) ve `GoodbyeDPI-Turkey GUI Autostart`
(Ayarlar → Windows ile başlat açıksa oturum açılınca arka planda başlatır).
Kısayol normal yetkiyle `GoodbyeDPI-Turkey-GUI.exe`'yi açar, o da görevi tetikleyip çıkar.
v1.0'daki eski `Hub` görevleri güncellemede kurulum tarafından kaldırılır.

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

Klavye: `Ctrl+1…9` sayfa seçer, `Esc` pencereyi arka plana gönderir.

## Derleme ve kurulum

`gui\` klasöründe (.NET SDK gerekir; hedef .NET Framework 4.8, Windows'ta hazır gelir). Kurulum exe'si Actions → "Build GUI" → Run workflow ("release" işaretli)
ile ya da `v*` etiketi push'lanınca derlenip Release'e eklenir; elle derlemek için publish'ten sonra
`iscc installer\setup.iss` (Inno Setup 6). Geliştirme kurulumu için:

```
dotnet publish GoodbyeDPI-Turkey-GUI.csproj -c Release -o dist
powershell -ExecutionPolicy Bypass -File tools\install.ps1   # yönetici olarak
```

Tasarım kontrolü için yetki gerekmeden:
- `dist\GoodbyeDPI-Turkey-GUI.exe --snapshot <klasör>` her sayfayı PNG kaydeder (hata olursa `error.txt`).
- `dist\GoodbyeDPI-Turkey-GUI.exe --preview` pencereyi modülleri başlatmadan açar. Anahtarlar yine gerçek komut çalıştırır.
