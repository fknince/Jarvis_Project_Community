using System.Collections.ObjectModel;
using System.ComponentModel;
using Jarvis_Project_Community.Models;

namespace Jarvis_Project_Community.Controllers
{
    public class ChatController : INotifyPropertyChanged
    {
        private ObservableCollection<ChatMessage> _chatMessages;
        public ObservableCollection<ChatMessage> ChatMessages
        {
            get => _chatMessages;
            set { _chatMessages = value; OnPropertyChanged(nameof(ChatMessages)); }
        }

        public ChatController()
        {
            ChatMessages = new ObservableCollection<ChatMessage>();
        }

        public void AddMessage(string message, bool isAssistant)
        {
            ChatMessages.Add(new ChatMessage { Message = message, IsAssistant = isAssistant });
        }

        public void ResetChat()
        {
            ChatMessages.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
