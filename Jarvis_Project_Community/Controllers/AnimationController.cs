using System.Windows;
using System.Windows.Media.Animation;

namespace Jarvis_Project_Community.Controllers
{
    public class AnimationController
    {
        private readonly FrameworkElement resourceProvider;
        private Storyboard currentAnimation;
        private string currentAnimationKey;

        public AnimationController(FrameworkElement resourceProvider)
        {
            this.resourceProvider = resourceProvider;
        }

        public string GetCurrentAnimationKey()
        {
            return currentAnimationKey;
        }

        public void StopCurrentAnimation()
        {
            if (currentAnimation != null)
            {
                currentAnimation.Stop();
                currentAnimation = null;
            }
        }

        public void PlayAnimation(string animationKey)
        {
            StopCurrentAnimation();
            currentAnimation = resourceProvider.TryFindResource(animationKey) as Storyboard;
            if (currentAnimation == null)
                return;
            currentAnimationKey = animationKey;
            currentAnimation.Begin();
        }
    }
}
