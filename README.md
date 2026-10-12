# GoodbyeDPI-Turkey Arayüz

[![Sürüm](https://img.shields.io/github/v/release/arnoldgodsx/hub?label=s%C3%BCr%C3%BCm)](https://github.com/arnoldgodsx/hub/releases/latest)
[![Lisans](https://img.shields.io/badge/lisans-Apache%202.0-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-lightgrey.svg)](#sistem-gereksinimleri)

**GoodbyeDPI'ı komut dosyalarıyla uğraşmadan, tek tıkla açıp kapatın.**

Bu proje, [cagritaskn/GoodbyeDPI-Turkey](https://github.com/cagritaskn/GoodbyeDPI-Turkey)'in arayüzlü bir koludur.
GoodbyeDPI; Discord gibi erişimi engellenen site ve uygulamalara **VPN kullanmadan ve internetinizi
yavaşlatmadan** girmenizi sağlar. Orijinal sürümde hizmeti kurmak, profil değiştirmek ya da kaldırmak için
`.cmd` dosyalarını yönetici olarak çalıştırmak gerekir. Bu sürümde aynı işleri sade bir pencereden yaparsınız.

<p align="center">
  <a href="https://github.com/arnoldgodsx/hub/releases/latest/download/GoodbyeDPI-Turkey-GUI-v1.0-Setup.exe">
    <b>⬇ Kurulum dosyasını indir (v1.0)</b>
  </a>
</p>

---

## İçindekiler

- [Neler yapabilir?](#neler-yapabilir)
- [Kurulum](#kurulum)
- [Kullanım](#kullanım)
- [Hangi profili seçmeliyim?](#hangi-profili-seçmeliyim)
- [Sık sorulan sorular](#sık-sorulan-sorular)
- [Kaldırma](#kaldırma)
- [Nasıl çalışır?](#nasıl-çalışır)
- [Kaynaktan derleme](#kaynaktan-derleme)
- [Teşekkür ve lisans](#teşekkür-ve-lisans)

## Neler yapabilir?

| | |
|---|---|
| **Tek tıkla aç / kapat** | Büyük güç düğmesiyle GoodbyeDPI'ı anında başlatıp durdurursunuz. |
| **7 hazır profil** | Orijinal projedeki standart ve Superonline alternatif yöntemlerinin hepsi hazır; birinden diğerine tek tıkla geçilir. |
| **Windows ile başlat** | İsterseniz GoodbyeDPI bilgisayar açılınca kendiliğinden çalışır. |
| **Arka planda çalışır** | Pencereyi kapatınca program gizlenir, GoodbyeDPI çalışmaya devam eder. Bildirim alanını kalabalıklaştırmaz. |
| **Hafif** | Kurulum dosyası birkaç MB; Windows'ta hazır gelen .NET Framework 4.8 kullanılır, ek bir şey indirilmez. |
| **Temiz kaldırma** | Program kaldırılınca GoodbyeDPI hizmeti, WinDivert sürücüsü ve zamanlanmış görevler de silinir. |
| **Eski kurulumla uyumlu** | `.cmd` dosyalarıyla daha önce hizmet kurduysanız program onu tanır, seçili profili gösterir ve kendi kopyasına taşır. |

## Kurulum

1. **[GoodbyeDPI-Turkey-GUI-v1.0-Setup.exe](https://github.com/arnoldgodsx/hub/releases/latest/download/GoodbyeDPI-Turkey-GUI-v1.0-Setup.exe)** dosyasını indirin.
2. Dosyayı çalıştırın ve kurulum sihirbazındaki adımları izleyin.
   - Program `C:\Program Files\GoodbyeDPI-Turkey` klasörüne kurulur.
   - Masaüstüne ve Başlat menüsüne **GoodbyeDPI-Turkey** kısayolu eklenir.
3. Kurulumun son ekranında programı başlatın. Hepsi bu kadar.

> [!NOTE]
> Kurulum dosyası dijital olarak imzalı olmadığı için Windows SmartScreen *"Windows bilgisayarınızı korudu"*
> uyarısı gösterebilir. **Ek bilgi → Yine de çalıştır** ile devam edebilirsiniz.

### Sistem gereksinimleri

- Windows 10 (1903 ve sonrası) veya Windows 11, 64 ya da 32 bit
- Yönetici hesabı (GoodbyeDPI bir Windows hizmeti olarak çalışır)
- .NET Framework 4.8 (bu Windows sürümlerinde zaten yüklüdür; yoksa kurulum sizi indirme sayfasına yönlendirir)

## Kullanım

Program açıldığında solda uygulamalar, sağda seçili sayfa görünür.

### GoodbyeDPI sayfası

- **Güç düğmesi:** GoodbyeDPI'ı açar ya da kapatır. İşlem sürerken düğme yavaşça yanıp söner;
  birkaç saniye içinde açık (dolu) ya da kapalı (boş) duruma geçer.
- **Windows ile başlat:** Açıksa GoodbyeDPI hizmeti bilgisayar her açıldığında kendiliğinden çalışır.
  Programı açmanıza gerek kalmaz.
- **Profil:** Sağdaki listeden yöntemi seçin. GoodbyeDPI çalışırken profil değiştirirseniz yeni ayarla
  hemen yeniden başlatılır. Bir hata olursa açıklaması düğmenin altında kırmızı yazıyla görünür.

### Ayarlar sayfası

- **Windows ile başlat:** *Arayüzün* oturum açılınca arka planda başlamasını sağlar. (GoodbyeDPI'ın
  kendisinin açılışta çalışması için GoodbyeDPI sayfasındaki ayar yeterlidir; bu ayar sadece pencereyi hazır tutar.)
- **Klasörü aç:** Programın kurulu olduğu klasörü açar.
- **Hub'ı kapat:** Arayüzü tamamen kapatır. GoodbyeDPI hizmeti açıksa çalışmaya devam eder.

### Kısayollar

| Tuş | İşlev |
|---|---|
| `Ctrl` + `1` … `9` | Sayfalar arasında geçiş |
| `Esc` | Pencereyi arka plana gönderir |

> [!TIP]
> Pencereyi kapattığınızda program kapanmaz, arka planda bekler. Masaüstü kısayoluna yeniden tıkladığınızda
> aynı pencere öne gelir. İlk açılıştan sonra yönetici izni (UAC) tekrar sorulmaz.

## Hangi profili seçmeliyim?

Önce **Standart** profili deneyin. Çoğu internet sağlayıcısında yeterlidir. Çalışmazsa sırayla alternatifleri deneyin.

| Profil | GoodbyeDPI parametreleri | DNS | Ne zaman? |
|---|---|---|---|
| **Standart** | `-5 --set-ttl 5` | Yandex DNS (otomatik) | Önerilen, ilk deneyeceğiniz profil |
| Alternatif 1 | `--set-ttl 3` | Windows ayarı | Superonline |
| Alternatif 2 | `-5` | Windows ayarı | Superonline · bazı siteler yavaşsa |
| Alternatif 3 | `--set-ttl 3` | Yandex DNS (otomatik) | Superonline |
| Alternatif 4 | `-5` | Yandex DNS (otomatik) | Superonline · bazı siteler yavaşsa |
| Alternatif 5 | `-9` | Yandex DNS (otomatik) | Superonline |
| Alternatif 6 | `-9` | Windows ayarı | Superonline |

- **"Yandex DNS (otomatik)"** yazan profillerde DNS ayarı yapmanıza gerek yoktur. DNS sorguları GoodbyeDPI
  tarafından Yandex DNS'e (77.88.8.8, port 1253) yönlendirilir.
- **"Windows ayarı"** yazan profillerde (Alternatif 1, 2 ve 6) Windows'un ağ ayarlarından DNS'i kendiniz
  değiştirmelisiniz. Örneğin Yandex `77.88.8.8 / 77.88.8.1` ya da Cloudflare `1.1.1.1 / 1.0.0.1` kullanabilirsiniz.
- Bazı siteler **yavaş açılıyor ya da hiç açılmıyorsa** TTL ayarı içermeyen **Alternatif 2** veya **Alternatif 4**'ü deneyin.

Profiller, orijinal projedeki `service_install_dnsredir_turkey*.cmd` dosyalarıyla birebir aynı parametreleri kullanır.

## Sık sorulan sorular

<details>
<summary><b>Bu bir VPN mi? İnternetimi yavaşlatır mı?</b></summary>

Hayır. GoodbyeDPI trafiğinizi başka bir sunucuya yönlendirmez. Yalnızca internet sağlayıcısının "Derin Paket
İnceleme" (DPI) sistemini şaşırtacak şekilde paketleri düzenler. Hız ve oyunlardaki gecikme (ping) değişmez.
</details>

<details>
<summary><b>Kaspersky kullanıyorum, çalışmıyor.</b></summary>

Kaspersky, GoodbyeDPI'ın kullandığı WinDivert sürücüsünü engeller. Devre dışı bırakmak ya da dışlamalara eklemek
genellikle yetmez. GoodbyeDPI'ı kullanmak için Kaspersky'yi tamamen kaldırmanız gerekir. Windows Defender
yeterli bir koruma sağlar.
</details>

<details>
<summary><b>Antivirüs programım virüs uyarısı veriyor.</b></summary>

`WinDivert.dll` ve `WinDivert64.sys` ağ paketlerine eriştiği için bazı antivirüs programları bunları yanlışlıkla
zararlı olarak işaretleyebilir. Hem GoodbyeDPI hem de [WinDivert](https://github.com/basil00/WinDivert) açık
kaynaklıdır, kodlarını inceleyebilirsiniz. Gerekirse kurulum klasörünü (`C:\Program Files\GoodbyeDPI-Turkey`)
antivirüsünüzün dışlamalarına ekleyin.
</details>

<details>
<summary><b>"GoodbyeDPI açıldı ama hemen kapandı" hatası alıyorum.</b></summary>

Seçili profil bağlantınızda çalışmıyor ya da WinDivert sürücüsü başka bir program tarafından kullanılıyor
olabilir. Başka bir profil seçip tekrar deneyin. Bilgisayarınızda GoodbyeDPI'ın başka bir kopyası (ör. eski bir
ZIP klasörü) çalışıyorsa önce onu kaldırın.
</details>

<details>
<summary><b>Discord tarayıcıda açılıyor ama uygulama "update failed" diyor.</b></summary>

Bu genellikle Superonline fiber hatlarda görülür. Alternatif profilleri sırayla deneyin ve her denemeden sonra
Discord'u tamamen kapatıp yeniden açın. Sorun sürerse [orijinal rehberdeki](GOODBYEDPI.md#superonline-alternatif-yöntemler)
ek yöntemlere bakın.
</details>

<details>
<summary><b>Daha önce .cmd dosyalarıyla kurulum yapmıştım. Ne yapmalıyım?</b></summary>

Hiçbir şey yapmanıza gerek yok. Program mevcut `GoodbyeDPI` hizmetini tanır ve seçili profili gösterir. Hizmeti
kendi klasöründeki GoodbyeDPI kopyasına yönlendirir; yeni yol bir sonraki başlatmada geçerli olur. Sonrasında eski
ZIP klasörünü silebilirsiniz.
</details>

<details>
<summary><b>DNS ayarlarımı eski haline nasıl getiririm?</b></summary>

Programı kaldırmak GoodbyeDPI'ı tamamen siler. Windows'ta DNS'i elle değiştirdiyseniz onu da geri almak için
[GoodbyeDPI-Turkey Doğru Şekilde Kaldırma Rehberi](REVERT.md)'ni izleyin.
</details>

## Kaldırma

**Ayarlar → Uygulamalar → Yüklü uygulamalar → GoodbyeDPI-Turkey → Kaldır** yolunu izleyin.
Kaldırıcı programı kapatır ve şunları siler:

- `GoodbyeDPI` Windows hizmetini
- `WinDivert` sürücüsünü
- Programın zamanlanmış görevlerini (`Hub`, `Hub Autostart`)
- Program dosyalarını ve kısayolları

## Nasıl çalışır?

Arayüz, GoodbyeDPI'ı orijinal `.cmd` dosyalarının yaptığı gibi **`GoodbyeDPI` adlı bir Windows hizmeti** olarak
kurar ve yönetir. Açma, kapatma ve profil değiştirme işlemleri bu hizmet üzerinden yapılır. GoodbyeDPI'ın
kendisi (`goodbyedpi.exe` ve WinDivert) orijinal Türkiye sürümüyle aynıdır ve programın kurulu olduğu klasördeki
`engine\goodbyedpi\` altında durur.

Hizmeti yönetmek yönetici yetkisi gerektirir. Her açılışta UAC penceresiyle uğraşmamak için program ilk açılışta
Görev Zamanlayıcı'ya "en yüksek yetkiyle" çalışan bir `Hub` görevi ekler ve kısayol programı bu görev üzerinden
açar. *Ayarlar → Windows ile başlat* açıksa oturum açıldığında arayüzü arka planda başlatan bir `Hub Autostart`
görevi de eklenir.

## Kaynaktan derleme

```
.
├── gui/          Windows arayüzü (C#, WPF, .NET Framework 4.8)
│   ├── installer/   Kurulum betiği (Inno Setup)
│   └── engine/      Arayüzle birlikte dağıtılan GoodbyeDPI paketi
├── src/          GoodbyeDPI kaynak kodu (C)
├── windows/      Orijinal .cmd komut dosyaları
└── macos/        macOS desteği
```

Arayüzün nasıl derleneceği ve kurulum dosyasının nasıl üretileceği [`gui/README.md`](gui/README.md) dosyasında
anlatılıyor. Kısaca:

```
cd gui
dotnet publish Hub.csproj -c Release -o dist
iscc installer\setup.iss
```

Her sürümün kurulum dosyası GitHub Actions'taki **Build GUI** iş akışı tarafından derlenip
[Releases](https://github.com/arnoldgodsx/hub/releases) sayfasına eklenir.

## Teşekkür ve lisans

- **[GoodbyeDPI](https://github.com/ValdikSS/GoodbyeDPI)**: ValdikSS (asıl proje)
- **[GoodbyeDPI-Turkey](https://github.com/cagritaskn/GoodbyeDPI-Turkey)**: cagritaskn (Türkiye uyarlaması ve
  profiller). Komut dosyalarıyla kullanım ve macOS için [orijinal README](GOODBYEDPI.md)'ye bakın.
  Projeyi desteklemek isterseniz: [GitHub Sponsor](https://github.com/sponsors/cagritaskn) · [Patreon](https://www.patreon.com/cagritaskn/membership)
- **[WinDivert](https://github.com/basil00/WinDivert)**: basil00
- Arayüz yazı tipi **[Geist](https://vercel.com/font)**: OFL lisanslı (`gui/Assets/Fonts/OFL.txt`)

GoodbyeDPI kaynak kodu [Apache 2.0](LICENSE) lisanslıdır. Paketteki diğer bileşenlerin lisansları
`gui/engine/goodbyedpi/licenses/` klasöründedir.

> [!IMPORTANT]
> **Yasal uyarı:** Bu uygulamanın kullanımından doğan her türlü yasal sorumluluk kullanan kişiye aittir. Uygulama
> eğitim ve araştırma amacıyla geliştirilmiştir. Kullanıp kullanmamak kullanıcının kendi seçimidir.
