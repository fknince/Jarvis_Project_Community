
namespace Jarvis_Project_Community.Models
{
    public class UiPathProject
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ProcessKey { get; set; }
        public string Key { get; set; }

        public UiPathProject()
        {
            Name = "";
            Description = "";
            Key = "";
            ProcessKey = "";

        }
    }

    public class NisusDialogueModel
    {
        public string DetectedAction { get; set; }
        public string AssistantMessage { get; set; }
        public string ActionEndMessage { get; set; }
        public string ActionStartMessage { get; set; }
        public string DetectedUiPathProjectName { get; set; }
        public string PreparedUseBrowserTaskPrompt { get; set; }
        public string UserMessage { get; set; }
        public bool UseBrowserTaskIncludeDataExtraction { get; set; }
        public List<UiPathProject> UiPathProjects { get; set; }

        public NisusDialogueModel()
        {
            ActionEndMessage = "";
            ActionStartMessage = "";
            DetectedUiPathProjectName = "";
            PreparedUseBrowserTaskPrompt = "";
            DetectedAction = "";
            AssistantMessage = "";
            UiPathProjects = new List<UiPathProject>();
            UseBrowserTaskIncludeDataExtraction = false;



        }
    }

}
