# Hub

Küçük uygulamalarımı tek pencerede toplayan panel. Koyu, siyah-beyaz arayüz:
solda uygulamalar, sağda seçilen uygulamanın sayfası. Bildirim alanında simgesi yoktur;
pencere kapatılınca arka planda çalışmaya devam eder (Görev Yöneticisi'nde "Hub").

| Uygulama        | Ne yapar |
|-----------------|----------|
| GoodbyeDPI      | `GoodbyeDPI` Windows hizmetini açar/kapatır, profil ve otomatik başlatma |

## Yetki ve açılış

Hub yönetici olarak çalışır (GoodbyeDPI hizmeti için). Her açılışta UAC sormamak için
Görev Zamanlayıcı'da iki görev kullanır: `Hub` (kısayoldan açılış) ve `Hub Autostart`
(Ayarlar → Windows ile başlat açıksa oturum açılınca arka planda başlatır).
Kısayol normal yetkiyle `Hub.exe`'yi açar, o da `Hub` görevini tetikleyip çıkar.

## Klasör yapısı

```
App.xaml(.cs)        açılış, tek kopya, modül listesi
MainWindow.xaml      pencere: kenar çubuğu + sayfa alanı
Core\Theme.xaml      ortak renkler ve stiller (anahtar, kart, düğme, menü…)
Core\                IModule, ikon üretici, halka, komut çalıştırıcı, yetki/görevler
Modules\<Ad>\        her uygulama kendi klasöründe
engine\goodbyedpi\   orijinal GoodbyeDPI paketi (exe'nin yanına kopyalanır)
tools\               make-icon.ps1, install.ps1
dist\                derleme çıktısı (Hub.exe, repoda tutulmaz)
```

## Yeni uygulama eklemek

1. `Modules\Yeni\YeniModule.cs` içinde `IModule`'ü uygulayın (başlık, `Start`/`Stop`, `CreatePage`).
2. `Modules\Yeni\YeniPage.xaml` sayfasını mevcut sayfalar gibi kurun: üstte `Title` + `Chip` durum kapsülü,
   altında sadece işlevsel satırlar. Açılışta belirecek öğelere `hub:Motion.Reveal="True"` verin.
   Renk sadece durum için (`Ok`, `Danger`); stiller `Core\Theme.xaml`'da.
3. `App.xaml.cs` → `Modules = [ ..., new YeniModule() ];`

Klavye: `Ctrl+1…9` sayfa seçer, `Esc` pencereyi arka plana gönderir.

## Derleme ve kurulum

```
dotnet publish Hub.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o dist
powershell -ExecutionPolicy Bypass -File tools\install.ps1   # yönetici olarak
```

Tasarım kontrolü için yetki gerekmeden:
- `dist\Hub.exe --snapshot <klasör>` her sayfayı PNG kaydeder (hata olursa `error.txt`).
- `dist\Hub.exe --preview` pencereyi modülleri başlatmadan açar. Anahtarlar yine gerçek komut çalıştırır.
