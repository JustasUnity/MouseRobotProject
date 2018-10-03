using System;
using System.Diagnostics;

namespace RobotMouseLibrary
{
    public class UnityEditorProcess : IDisposable
    {
        private Process m_editorProcess;
        public int Port;
        public bool AttachDebugger;
        public string EditorPath;
        public string ProjectPath;
        public UnityCodeEval CodeEval;


        public UnityEditorProcess()
        {
            Port = 9001;
            AttachDebugger = false;
            EditorPath = @"C:\UnityRepo\unity\build\WindowsEditor\Unity.exe";
            ProjectPath = @"C:\RandomDelete";

            m_editorProcess = SetUpEditorProcess();
            CodeEval = new UnityCodeEval();
        }

        private Process SetUpEditorProcess()
        {
            var psi = new ProcessStartInfo(EditorPath);
            psi.UseShellExecute = false;
            psi.Arguments = $@"-createProject {ProjectPath} -connectToMacroClient {Port}";

            if (AttachDebugger)
            {
                psi.EnvironmentVariables["UNITY_GIVE_CHANCE_TO_ATTACH_DEBUGGER"] = "1";
            }
            
            m_editorProcess = new Process()
            {
                StartInfo = psi
            };

            return m_editorProcess;
        }

        public void Start()
        {
            m_editorProcess.Start();
            CodeEval.ConnectToEditor();
            if (!CodeEval.IsConnected.WaitOne(TimeSpan.FromSeconds(30)))
            {
                Dispose();
            }
        }

        public void KeepAlive()
        {
            while (!m_editorProcess.HasExited)
                CodeEval.KeepAlive();
        }

        public void Dispose()
        {
            CodeEval.Dispose();
            m_editorProcess?.Kill();
        }
    }
}
