using Jarvis_Project_Community.Resources;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;


namespace Jarvis_Project_Community.Controllers
{
    public class SpeechController
    {
        private SpeechRecognizer? _recognizer;
        private readonly CancellationTokenSource _cancellationTokenSource;
        public event Action<string>? OnCommandRecognized;
        private readonly string _subscriptionKey = ProjectResources.AzureSpeechServiceSubscriptionKey;
        private readonly string _region = ProjectResources.AzureSpeechServiceRegion;
        private App _app;

        public SpeechController(App app)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            InitializeSpeechRecognition();
            _app = app;
        }

        private async void InitializeSpeechRecognition()
        {
            try
            {
                var config = SpeechConfig.FromSubscription(_subscriptionKey, _region);
                config.SpeechRecognitionLanguage = "tr-TR";

                var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
                _recognizer = new SpeechRecognizer(config, audioConfig);

                _recognizer.Recognized += Recognizer_Recognized;
                _recognizer.Recognizing += Recognizer_Recognizing;
                _recognizer.Canceled += Recognizer_Canceled;

                System.Diagnostics.Debug.WriteLine("Konuşma tanıma motoru başarıyla başlatıldı.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Başlatma hatası: {ex.Message}");
                throw;
            }
        }

        public async Task StartListeningAsync()
        {
            if (_recognizer == null)
                throw new InvalidOperationException("Konuşma tanıma motoru başlatılmamış!");

            try
            {
                System.Diagnostics.Debug.WriteLine("Dinleme başlatılıyor...");
                await _recognizer.StartContinuousRecognitionAsync();

                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Dinleme hatası: {ex.Message}");
            }
        }

        private void Recognizer_Recognized(object? sender, SpeechRecognitionEventArgs e)
        {
            if (e.Result == null) return;
            string command = e.Result.Text.ToLower();
            OnCommandRecognized?.Invoke(command);
        }

        private void Recognizer_Recognizing(object? sender, SpeechRecognitionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Tanınıyor: {e.Result.Text}");
        }

        private void Recognizer_Canceled(object? sender, SpeechRecognitionCanceledEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Tanıma iptal edildi: {e.ErrorCode} - {e.ErrorDetails}");
        }

        public async Task StopListeningAsync()
        {
            try
            {
                if (_recognizer != null)
                {
                    await _recognizer.StopContinuousRecognitionAsync();
                    _recognizer.Dispose();
                }
                _cancellationTokenSource.Cancel();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Durdurma hatası: {ex.Message}");
            }
        }

        public async Task SpeakTextAsync(string text, string voiceName = "tr-TR-AhmetNeural")
        {
            _app._tracerFlags._isSpeaking = true;
            try
            {
                if (_recognizer != null)
                {
                    await StopListeningAsync();
                }

                var config = SpeechConfig.FromSubscription(_subscriptionKey, _region);
                config.SpeechSynthesisVoiceName = voiceName;

                using var synthesizer = new SpeechSynthesizer(config);
                var result = await synthesizer.SpeakTextAsync(text);

                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Seslendirme tamamlandı.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Seslendirme hatası: {result.Reason}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Seslendirme istisnası: {ex.Message}");
            }
            finally
            {
                if (_recognizer != null)
                {
                    await StartListeningAsync();
                }
                _app._tracerFlags._isSpeaking = false;
            }
        }
    }
}
