using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Windows;

namespace Jarvis_Project_Community.ViewModels
{
    public class AsistanViewModel : INotifyPropertyChanged
    {
        private Storyboard _currentAnimation;
        public event PropertyChangedEventHandler PropertyChanged;

        public void SetAnimation(Storyboard animation, Window window)
        {
            if (_currentAnimation != null)
                _currentAnimation.Stop(window);

            _currentAnimation = animation;
            _currentAnimation.Begin(window, true);
        }
    }
}
