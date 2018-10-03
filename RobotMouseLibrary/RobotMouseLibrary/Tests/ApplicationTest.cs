using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System.Linq;


namespace RobotMouseLibrary.Tests
{
    [TestFixture]
    class ApplicationTest
    {

        [Test]
        public void GetAllWindowLabels()
        {
            var windowTypes = RobotMouseHelper.GetWindowTypes().ToArray();
            
            using (var editor = new UnityEditorProcess())
            {
                editor.Start();


                var titles2 = editor.CodeEval.Eval((wt) =>
                {
                    var titles = new List<string>();

                    foreach (var windowType in wt)
                    {
                        try
                        {
                            Debug.Log("Opening:" + windowType.FullName);
                            EditorWindow.FocusWindowIfItsOpen(windowType);
                            var currentWindow = EditorWindow.focusedWindow;
                            titles.Add(currentWindow.titleContent.text);
                            var position = currentWindow.position.position;
                            
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                    return titles;
                }, windowTypes);

                editor.KeepAlive();
            }
        }

        [Test]
        public void GetAllUnityWindowTypes()
        {
            var res = RobotMouseHelper.GetWindowTypes();
            foreach (var t in res)
            {
                Console.WriteLine(t.FullName);
            }
        }

        [Test]
        public void JustATest()
        {
            using (var editor = new UnityEditorProcess())
            {
                editor.Start();
                editor.CodeEval.Eval(() =>
                {
                     new GameObject("Test this!");
                     new GameObject("Test this!2");
                });
                editor.CodeEval.Eval(() => new GameObject("TADADAA!"));
                editor.KeepAlive();
            }
        }
    }
}
