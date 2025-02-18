using System.Windows;
using System.Windows.Controls;
using Jarvis_Project_Community.Models;

namespace Jarvis_Project_Community.Views
{
    public class ChatMessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AssistantTemplate { get; set; }
        public DataTemplate UserTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var chatMessage = item as ChatMessage;
            if (chatMessage == null)
                return base.SelectTemplate(item, container);

            return chatMessage.IsAssistant ? AssistantTemplate : UserTemplate;
        }
    }
}
