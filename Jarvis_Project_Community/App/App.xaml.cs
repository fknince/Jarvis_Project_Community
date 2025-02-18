using System.Windows;
using System.Timers;
using Jarvis_Project_Community.Controllers;
using Jarvis_Project_Community.Views;
using Timer = System.Timers.Timer;
using Jarvis_Project_Community.Models;

namespace Jarvis_Project_Community
{
    public partial class App : Application
    {
        private SpeechController? _speechController;
        private System.Timers.Timer _silenceTimer;
        private string _lastHeardCommand = "";
        private MainWindow? _mainWindow;
        public TracerFlags _tracerFlags { get; set; }


        [STAThread]
        public static void Main()
        {
            App app = new App();
            app.InitializeComponent();
            app.Run();


        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);
                _tracerFlags = new TracerFlags();
                _mainWindow = new MainWindow(this);

                _speechController = new SpeechController(this);
                _speechController.OnCommandRecognized += ProcessCommand;

                _silenceTimer = new Timer(3000);
                _silenceTimer.Elapsed += OnSilenceDetected;
                _silenceTimer.AutoReset = false;

                await InitializeSpeechRecognitionAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Uygulama başlatılırken bir hata oluştu: {ex.Message}",
                                "Başlatma Hatası",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        private async Task InitializeSpeechRecognitionAsync()
        {
            if (_speechController != null)
            {
                try
                {
                    await _speechController.StartListeningAsync();
                    System.Diagnostics.Debug.WriteLine("Ses tanıma başarıyla başlatıldı.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ses tanıma başlatılırken hata: {ex.Message}");
                    MessageBox.Show("Ses tanıma sistemi başlatılamadı. Mikrofonunuzu kontrol edin ve uygulamayı yeniden başlatın.",
                                    "Ses Tanıma Hatası",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                }
            }
        }

        private void ProcessCommand(string command)
        {
            if (_tracerFlags._isSpeaking || _tracerFlags._hasActiveAction)
            {
                return;
            }

            Dispatcher.Invoke(async () =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"📢 Kullanıcı Dedi: {command}");

                    if (command.Contains("uyku moduna geç"))
                    {
                        if (_tracerFlags._isOpened)
                        {
                            _tracerFlags._isOpened = false;
                            _tracerFlags._isListening = false;
                            _mainWindow.Hide();

                        }
                    }
                    if (!_tracerFlags._isListening)
                    {
                        if ((command.Contains("hey") || command.Contains("tamam") || command.Contains("evet") ||
                             command.Contains("selam") || command.Contains("merhaba") || command.Contains("sağol")
                        || command.Contains("peki") || command.Contains("teşekkürler"))
                        && command.Contains("cavit"))
                        {
                            System.Diagnostics.Debug.WriteLine("✅ Cavit Uyandı! Komut bekleniyor...");
                            _mainWindow?.PlayAnimation("IdleAnimation");
                            _tracerFlags._isListening = true;

                            if (!_tracerFlags._isOpened)
                            {
                                _tracerFlags._isOpened = true;
                                _mainWindow.Show();

                            }

                        }
                    }
                    else
                    {
                        _lastHeardCommand = command;
                        _silenceTimer.Stop();
                        _silenceTimer.Start();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Komut işlenirken hata: {ex.Message}");
                }
            });
        }

        private async void OnSilenceDetected(object sender, ElapsedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_lastHeardCommand))
            {
                Dispatcher.Invoke(async () =>
                {
                    System.Diagnostics.Debug.WriteLine($"💬 Komut işlemesi başlatılıyor: {_lastHeardCommand}");
                    _mainWindow?.AddUserMessage(_lastHeardCommand);
                    _mainWindow?.PlayAnimation("ThinkingAnimation");

                    await _mainWindow?.SendMessageAsync(_lastHeardCommand);

                    _lastHeardCommand = "";
                    _tracerFlags._isListening = false;
                });
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            try
            {
                if (_speechController != null)
                {
                    await _speechController.StopListeningAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Uygulama kapatılırken hata: {ex.Message}");
            }
            finally
            {
                base.OnExit(e);
            }
        }
    }
}
