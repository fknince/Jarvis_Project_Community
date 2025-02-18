using System.Text.Json;
using Jarvis_Project_Community.Models;

namespace Jarvis_Project_Community.Controllers
{
    public class NisusDialogueController
    {
        private UiPathInstance _uiPathInstance;
        private List<NisusDialogueModel> _nisusDialogueModelList;

        public NisusDialogueController()
        {
            _uiPathInstance = new UiPathInstance();
            _nisusDialogueModelList = new List<NisusDialogueModel>();

        }

        public NisusDialogueModel GetNewDialogueModel()
        {
            NisusDialogueModel nisusDialogueModel = new NisusDialogueModel();
            var projectList = _uiPathInstance.GetProcesses();
            nisusDialogueModel.UiPathProjects = projectList;
            return nisusDialogueModel;
        }

        public void ResetDialogueModelList()
        {
            _nisusDialogueModelList = new List<NisusDialogueModel>();
        }

        public List<NisusDialogueModel> InsertDialogueModel(NisusDialogueModel newModel)
        {
            _nisusDialogueModelList.Add(newModel);
            return _nisusDialogueModelList;
        }

        public List<NisusDialogueModel> GetDialogueModelList(int? keepLast = null)
        {
            if (keepLast != null)
            {
                int keepLastValue = Convert.ToInt32(keepLast);
                if (_nisusDialogueModelList.Count > keepLastValue)
                {
                    return _nisusDialogueModelList = _nisusDialogueModelList
                    .Skip(Math.Max(0, _nisusDialogueModelList.Count - keepLastValue))
                    .ToList();
                }
            }
            return _nisusDialogueModelList;

        }

        public string GetDialogueModelListAsJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            return JsonSerializer.Serialize(GetDialogueModelList(2));
        }

        public string GetDialogueModelListAsJson(List<NisusDialogueModel> list)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            return JsonSerializer.Serialize(list);
        }

        public bool LoadDialogueModelListFromJson(string jsonString)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                _nisusDialogueModelList = JsonSerializer.Deserialize<List<NisusDialogueModel>>(jsonString, options);
                return _nisusDialogueModelList != null;
            }
            catch (Exception)
            {
                return false;
            }
        }


    }

}
