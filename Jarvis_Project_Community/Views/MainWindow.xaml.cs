using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using Jarvis_Project_Community.Controllers;
using Jarvis_Project_Community.Models;
using Jarvis_Project_Community.Resources;

namespace Jarvis_Project_Community.Views
{
    public partial class MainWindow : Window
    {
        private AnimationController animationController;
        private ChatController chatController;
        private NisusDialogueController nisusDialogueController;
        private NisusInstance nisusInstance;
        private UiPathInstance uiPathInstance;
        private SpeechController speechController;
        private App _app;

        public MainWindow(App app)
        {
            InitializeComponent();

            animationController = new AnimationController(this);

            chatController = new ChatController();
            this.DataContext = chatController;
            chatController.ChatMessages.CollectionChanged += ChatMessages_CollectionChanged;

            nisusDialogueController = new NisusDialogueController();
            nisusInstance = new NisusInstance();
            uiPathInstance = new UiPathInstance();
            _app = app;
            speechController = new SpeechController(_app);
        }



        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            string awakeMessage = "Merhaba efendim, Size nasıl yardımcı olabilirim?";
            AddAssistantMessage(awakeMessage);
            SpeechAssistantMessageAsync(awakeMessage, true);
        }

        private void ChatMessages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                chatScrollViewer.Dispatcher.InvokeAsync(() =>
                {
                    chatScrollViewer.ScrollToEnd();
                });
            }
        }

        public void PlayAnimation(string animationKey)
        {
            animationController.PlayAnimation(animationKey);
        }

        public void AddUserMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) { return; }
            chatController.AddMessage(message, false);
        }
        public void AddAssistantMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) { return; }
            chatController.AddMessage(message, true);
        }

        public async Task SendMessageAsync(string userMessage)
        {

            try
            {
                NisusDialogueModel tempModel = nisusDialogueController.GetNewDialogueModel();
                tempModel.UserMessage = userMessage;
                var modelList = nisusDialogueController.InsertDialogueModel(tempModel);
                string modelListAsJson = nisusDialogueController.GetDialogueModelListAsJson();
                AddAssistantMessage("🧠 Düşünüyor...");
                _app._tracerFlags._hasActiveAction = true;
                var response = await nisusInstance.StartAndSendMessageAsync(new SendDialogueMessageRequest()
                {
                    ModelDeploymentEndpointId = ProjectResources.NisusAssistantEndpointID,
                    Message = modelListAsJson
                });
                _app._tracerFlags._hasActiveAction = false;
                nisusDialogueController.LoadDialogueModelListFromJson(response.CallResponse);
                var nisusDialogueModel = nisusDialogueController.GetDialogueModelList();
                var lastModel = nisusDialogueModel.Last();
                if (!String.IsNullOrEmpty(lastModel.AssistantMessage))
                {
                    chatController.AddMessage(lastModel.AssistantMessage, true);
                    SpeechAssistantMessageAsync(lastModel.AssistantMessage);
                }
                switch (lastModel.DetectedAction)
                {
                    case "Run UiPath":
                        {
                            _app._tracerFlags._hasActiveAction = true;
                            AddAssistantMessage("🤖 UiPath süreci çalıştırılıyor...");
                            SpeechAssistantMessageAsync(lastModel.ActionStartMessage);
                            chatController.AddMessage(lastModel.ActionStartMessage, true);
                            var allProcesses = uiPathInstance.GetProcesses();
                            if (allProcesses.Any(p => p.Name == lastModel.DetectedUiPathProjectName))
                            {
                                string processKey = allProcesses.First(p => p.Name == lastModel.DetectedUiPathProjectName).Key;
                                string jobKey = uiPathInstance.StartProcess(processKey);
                                await Task.Delay(5000);
                                var jobStatus = uiPathInstance.GetJobStatus(jobKey);
                                while (jobStatus == "Running")
                                {
                                    await Task.Delay(3000);
                                    jobStatus = uiPathInstance.GetJobStatus(jobKey);
                                }
                            }
                            SpeechAssistantMessageAsync(lastModel.ActionEndMessage);
                            chatController.AddMessage(lastModel.ActionEndMessage, true);
                            _app._tracerFlags._hasActiveAction = false;
                            break;
                        }
                    case "Run Browser-Use":
                        {
                            _app._tracerFlags._hasActiveAction = true;
                            AddAssistantMessage("🐍 Browser-Use scripti çalıştırılıyor...");
                            SpeechAssistantMessageAsync(lastModel.ActionStartMessage);
                            chatController.AddMessage(lastModel.ActionStartMessage, true);
                            try
                            {
                                string pytonScriptResponse = await new PythonRunner().RunAsync(lastModel.PreparedUseBrowserTaskPrompt);
                                if (lastModel.UseBrowserTaskIncludeDataExtraction)
                                {
                                    tempModel = nisusDialogueController.GetNewDialogueModel();
                                    tempModel.UserMessage = $@"
Son talep ettiğim Browser-Use işleminin sonucundan elde edilen çıktı aşağıdadır:
---------
{ExtractFinalResult(pytonScriptResponse)}
---------
Bu çıktıyı dikkate alarak son talep ettiğim Browser-Use işlemi içerisindeki soruya türkçe ve anlamlı bir cevap vermeni istiyorum.";
                                    File.WriteAllText("pythonresponse.txt", pytonScriptResponse); //sil
                                    File.WriteAllText("test.txt", tempModel.UserMessage); //sil
                                    nisusDialogueController.InsertDialogueModel(tempModel);
                                    modelListAsJson = nisusDialogueController.GetDialogueModelListAsJson();
                                    AddAssistantMessage("🧠 Düşünüyor...");
                                    response = await nisusInstance.StartAndSendMessageAsync(new SendDialogueMessageRequest()
                                    {
                                        ModelDeploymentEndpointId = ProjectResources.NisusAssistantEndpointID,
                                        Message = modelListAsJson
                                    });
                                    nisusDialogueController.LoadDialogueModelListFromJson(response.CallResponse);
                                    nisusDialogueModel = nisusDialogueController.GetDialogueModelList();
                                    lastModel = nisusDialogueModel.Last();
                                    if (!String.IsNullOrEmpty(lastModel.AssistantMessage))
                                    {
                                        chatController.AddMessage(lastModel.AssistantMessage, true);
                                        SpeechAssistantMessageAsync(lastModel.AssistantMessage);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                AddAssistantMessage("Run Browser-Use is failed:" + Environment.NewLine + ex.Message);
                            }

                            SpeechAssistantMessageAsync(lastModel.ActionEndMessage);
                            AddAssistantMessage(lastModel.ActionEndMessage);
                            _app._tracerFlags._hasActiveAction = false;
                            break;
                        }
                    case "Just Speak":
                        {
                            break;
                        }
                    default:
                        {
                            throw new Exception("Not specified action case detected: [" + lastModel.DetectedAction + "]");
                        }
                }


                System.Diagnostics.Debug.WriteLine($"🤖 Asistan Yanıtı: {response.CallResponse}");
            }
            catch (Exception ex)
            {
                chatController.AddMessage("❌ Hata: " + ex.Message, true);
            }
        }
        public async void SpeechAssistantMessageAsync(string ActionMessage, bool isProactive = false)
        {
            if (String.IsNullOrEmpty(ActionMessage))
            {
                return;
            }
            PlayAnimation("SpeakingAnimation");
            await speechController.SpeakTextAsync(ActionMessage);
            if (isProactive)
            {
                PlayAnimation("IdleAnimation");
            }
            else
            {
                PlayAnimation("ThinkingAnimation");
            }

        }
        private string ExtractFinalResult(string log)
        {
            string pattern = @"INFO\s+\[agent\]\s+📄\s+Result:\s+(.*)";
            Match match = Regex.Match(log, pattern);

            return match.Success ? match.Groups[1].Value.Trim() : "Sonuç bulunamadı.";
        }



    }
}
