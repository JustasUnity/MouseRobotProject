using System;
using System.Linq;
using UnityEditor;
using Json;

namespace RobotMouseLibrary
{
    public class RobotMouseHelper
    {
        public static Type[] GetWindowTypes()
        {
            var unityAssembly = typeof(EditorWindow).Assembly;
            var windowTypes = unityAssembly.GetTypes().Where(t => typeof(EditorWindow).IsAssignableFrom(t)).Where(t => t.IsPublic);
            return windowTypes.ToArray();
        }

        public string PackToSend<T>(T object)
        {
            Json
            return string.Empty;
        }
    }
}
