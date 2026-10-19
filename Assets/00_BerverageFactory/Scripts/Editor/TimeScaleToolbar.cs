using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace FBFactory.Editor
{
    public static class TimeScaleToolbar
    {
        private const string ToolbarElementId = "FBFactory/TimeScale";
        private const float MinTimeScale = 0f;
        private const float MaxTimeScale = 10f;

        [MainToolbarElement(
            ToolbarElementId,
            defaultDockPosition = MainToolbarDockPosition.Middle)]
        public static MainToolbarElement CreateTimeScaleSlider()
        {
            MainToolbarContent content = new MainToolbarContent("Time Scale", "Time Scale");
            return new MainToolbarSlider(
                content,
                Time.timeScale,
                MinTimeScale,
                MaxTimeScale,
                SetTimeScale);
        }

        private static void SetTimeScale(float timeScale)
        {
            Time.timeScale = timeScale;
        }
    }
}
