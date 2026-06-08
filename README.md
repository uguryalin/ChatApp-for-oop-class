# Console Chat Application / Konsol Sohbet Uygulaması

An Object-Oriented Programming (OOP) project for C# console-based multi-client chat room application using TCP/IP Sockets. No GUI, No DevExpress required.

TCP/IP Sockets kullanan, C# konsol tabanlı ve çoklu istemci destekli bir Nesne Yönelimli Programlama (OOP) ödev projesidir. Herhangi bir arayüz veya DevExpress bağımlılığı gerektirmez.

---

## Language Selection / Dil Seçimi
- [English Documentation](#english-documentation)
- [Türkçe Dokümantasyon](#türkçe-dokümantasyon)

---

# English Documentation

This application provides a simple yet complete multi-client chat environment running fully within a console terminal. It implements a TCP-based Server-Client protocol where clients can register with nicknames, send public messages, send private messages, and fetch the list of online users.

## Features
- **TCP/IP Socket Communication**: Robust, asynchronous communication utilizing `TcpListener` and `TcpClient`.
- **Headless & GUI-free**: Lightweight, portable, runs inside standard terminal consoles without complex dependencies.
- **Multithreading & Async**: Leverages C# `async/await` and task-based concurrency to handle multiple active clients simultaneously.
- **Bilingual & Automated Testing**: Includes a built-in programmatic test command (`--test`) to demonstrate client connection, listing, private messaging, and disconnection without manual input.
- **Commands**:
  - `/list` - Displays all online users.
  - `/msg <username> <content>` - Sends a private message to a specific user.
  - `/help` - Displays help menu instructions.
  - `/quit` or `/exit` - Safely closes connection and exits.

## OOP Principles Demonstrated
1. **Encapsulation**:
   - `ChatServer` hides active client lists, sockets, and lock synchronization blocks, exposing only `StartAsync()` and `Stop()`.
   - The server maintains a private nested class `ConnectedClient` implementing `IChatParticipant` which is completely encapsulated.
2. **Inheritance**:
   - The abstract base class `Message` provides common headers (`Sender`, `Timestamp`).
   - `TextMessage`, `SystemMessage`, and `PrivateMessage` inherit from `Message` to specialize message structures.
3. **Polymorphism**:
   - The base `Message` class defines the abstract method `public abstract string Format()`.
   - Concrete message subclasses override this method to provide specialized output structures (e.g., prefixing `[SYSTEM]` or indicating a private send `Alice -> Bob`).
4. **Abstraction**:
   - The interface `IChatParticipant` acts as a contract representing any chat client node. This decouples the core routing engine from the underlying socket transport level.

## How to Run

Ensure you have the [.NET 9 SDK](https://dotnet.microsoft.com/download) installed.

### 1. Run the Programmatic Integration Test
You can run a pre-recorded test scenario (Alice and Bob conversing automatically) to verify compile and socket binding:
```bash
dotnet run -- --test
```

### 2. Run Interactively
Run the application normal way:
```bash
dotnet run
```
At startup, choose your mode:
- Select `1` to run the **Server**. (Enter the port, default `8080`).
- Select `2` to run a **Client**. (Enter your nickname, server IP e.g. `127.0.0.1`, and port).
- Launch multiple terminal windows to run one Server and multiple Clients to chat together!

---

# Türkçe Dokümantasyon

Bu uygulama, tamamen konsol terminali üzerinde çalışan basit ama tam özellikli bir çoklu istemcili sohbet ortamı sağlar. İstemcilerin takma adlarla (nickname) kaydolabileceği, genel mesajlar atabileceği, özel mesajlar gönderebileceği ve çevrimiçi kullanıcı listesini çekebileceği TCP tabanlı bir Sunucu-İstemci protokolü uygular.

## Özellikler
- **TCP/IP Soket İletişimi**: `TcpListener` ve `TcpClient` kullanarak asenkron iletişim.
- **Arayüzsüz Tasarım**: Karmaşık grafik arayüz veya DevExpress bağımlılıkları olmadan, standart konsolda çalışan taşınabilir ve hafif yapı.
- **Çoklu İş Parçacığı (Async/Await)**: Birden fazla aktif istemciyi aynı anda yönetmek için C# asenkron programlama ve görev tabanlı eşzamanlılık kullanır.
- **Programlı Test Modu**: Manuel giriş gerektirmeden istemci bağlantılarını, listelemeyi, özel mesajlaşmayı ve bağlantı kesilmesini doğrulamak için entegre bir test komutuna (`--test`) sahiptir.
- **Sohbet Komutları**:
  - `/list` - Çevrimiçi tüm kullanıcıları gösterir.
  - `/msg <kullanıcı_adı> <mesaj>` - Belirtilen kullanıcıya özel (özel mesaj) gönderir.
  - `/help` - Komut kılavuzunu gösterir.
  - `/quit` veya `/exit` - Bağlantıyı güvenli bir şekilde kapatır ve çıkar.

## OOP Prensiplerinin Kullanımı
1. **Kapsülleme (Encapsulation)**:
   - `ChatServer` sınıfı aktif istemci listelerini, soketleri ve kilit senkronizasyon bloklarını gizli tutar; dışarıya yalnızca `StartAsync()` ve `Stop()` metotlarını sunar.
   - Sunucu, dışarıdan doğrudan örneklenemeyen ve `IChatParticipant` arayüzünü uygulayan gizli bir iç sınıf (`ConnectedClient`) barındırır.
2. **Kalıtım (Inheritance)**:
   - Soyut temel sınıf `Message`, ortak bilgileri (`Sender`, `Timestamp`) barındırır.
   - `TextMessage`, `SystemMessage` ve `PrivateMessage` sınıfları bu temel sınıftan türeyerek mesaj yapılarını özelleştirir.
3. **Çok Biçimlilik (Polymorphism)**:
   - Temel `Message` sınıfı, soyut `public abstract string Format()` metodunu tanımlar.
   - Alt sınıflar bu metodu ezerek (override) kendi çıktı yapılarını oluşturur (Örn. `[SYSTEM]` ön eki ekleme veya özel mesajlarda `Alice -> Bob` yönlendirmesi gösterme).
4. **Soyutlama (Abstraction)**:
   - `IChatParticipant` arayüzü, sohbet katılımcısı olan her varlığın uyması gereken sözleşmeyi tanımlar. Bu sayede sunucu mesaj yönlendirme motoru, ağ soket taşıma katmanının detaylarından bağımsız kalır.

## Nasıl Çalıştırılır

Bilgisayarınızda [.NET 9 SDK](https://dotnet.microsoft.com/download) yüklü olduğundan emin olun.

### 1. Programatik Entegrasyon Testini Çalıştırma
Sistem bağlantılarını ve soket işlemlerini (Alice ve Bob arasında otomatik bir sohbeti canlandırır) doğrulamak için aşağıdaki komutu kullanın:
```bash
dotnet run -- --test
```

### 2. Etkileşimli Çalıştırma
Uygulamayı normal şekilde başlatın:
```bash
dotnet run
```
Başlangıç menüsünde:
- **Sunucu (Server)** olarak çalıştırmak için `1` yazın (Port girin, varsayılan `8080`).
- **İstemci (Client)** olarak çalıştırmak için `2` yazın (Takma adınızı, Sunucu IP'sini e.g. `127.0.0.1` ve Portunu girin).
- Sohbet etmek için birden fazla terminal ekranı açarak bir Sunucu ve birden fazla İstemci çalıştırabilirsiniz!
