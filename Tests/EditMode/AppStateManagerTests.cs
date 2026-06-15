using NUnit.Framework;
using ARUnity.AR;

namespace ARUnity.Tests
{
    public class AppStateManagerTests
    {
        [Test]
        public void AppState_Enum_HasExpectedValues()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(AppState), AppState.Initializing));
            Assert.IsTrue(System.Enum.IsDefined(typeof(AppState), AppState.PermissionCheck));
            Assert.IsTrue(System.Enum.IsDefined(typeof(AppState), AppState.PermissionDenied));
            Assert.IsTrue(System.Enum.IsDefined(typeof(AppState), AppState.ARStarting));
            Assert.IsTrue(System.Enum.IsDefined(typeof(AppState), AppState.Scanning));
            Assert.IsTrue(System.Enum.IsDefined(typeof(AppState), AppState.Placement));
            Assert.IsTrue(System.Enum.IsDefined(typeof(AppState), AppState.Placed));
        }

        [Test]
        public void AppState_Scanning_ComesAfter_ARStarting()
        {
            Assert.That((int)AppState.ARStarting, Is.LessThan((int)AppState.Scanning));
        }

        [Test]
        public void AppState_Placed_IsLastState()
        {
            var states = System.Enum.GetValues(typeof(AppState));
            int maxValue = 0;
            foreach (int v in states) maxValue = System.Math.Max(maxValue, v);
            Assert.That((int)AppState.Placed, Is.EqualTo(maxValue));
        }
    }
}
