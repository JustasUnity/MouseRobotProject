using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace RobotMouseLibrary
{
    public class UnityCodeEval
    {
        private Socket m_Socket;
        public ManualResetEvent IsConnected;
        private int m_Port;
        private TcpListener m_tcpListener;

        public UnityCodeEval()
        {
            IsConnected = new ManualResetEvent(false);
            m_Port = 9001;
        }

        private void AcceptEditorSocketConnectionCallback(IAsyncResult ar)
        {
           
            var listener = (TcpListener)ar.AsyncState;
            m_Socket = listener.EndAcceptSocket(ar);

            Console.WriteLine("Unity connected. Local address: {0} Client Address {1}",
                m_Socket.LocalEndPoint, m_Socket.RemoteEndPoint);
            IsConnected.Set();

            m_tcpListener.Server.Close();
            m_tcpListener.Stop();
        }

        private static readonly BinaryFormatter s_Formatter =
            new BinaryFormatter { AssemblyFormat = FormatterAssemblyStyle.Simple };

        private static string GetFunctionCallParcel(object target, MethodBase method, IEnumerable args)
        {
            var type = (target?.GetType() ?? method.DeclaringType) ?? typeof(void);
            using (var stream = new MemoryStream())
            {
                s_Formatter.Serialize(stream, "com.unity3d.automation");
                s_Formatter.Serialize(stream, type.Assembly.CodeBase);
                s_Formatter.Serialize(stream, type);
                s_Formatter.Serialize(stream, method.Name);
                s_Formatter.Serialize(stream, method.GetParameters()
                    .Select(p => p.ParameterType).ToArray());
                s_Formatter.Serialize(stream, args);
                stream.Flush();
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        private static string GetEvalRequest(object target, MethodInfo method, object[] args)
        {
            return GetFunctionCallParcel(target, method, args);
        }
        private static string GetEvalRequest(Delegate call, object[] args)
        {
            return GetEvalRequest(call.Target, call.Method, args);
        }

        private string EvaluateDelegate(Delegate code, params object[] args)
        {
            return InternalEval(GetEvalRequest(code, args));
        }

        public string Eval(Delegate code, Type[] types, params object[] args)
        {
            return EvaluateDelegate(code, args);
        }

        public string Eval(Action code)
        {
            return EvaluateDelegate(code);
        }

        public string Eval<A1>(Action<A1> code, A1 a1)
        {
            return EvaluateDelegate(code, a1);
        }

        public string Eval<R>(Func<R> code)
        {
            return EvaluateDelegate(code);
        }

        public string Eval<A1, R>(Func<A1, R> code, A1 a1)
        {
            return EvaluateDelegate(code, a1);
        }

        public void Dispose()
        {
            m_Socket?.Close();
            m_Socket?.Dispose();


            m_tcpListener?.Server.Close();
            m_tcpListener?.Stop();
        }

        public void ConnectToEditor()
        {
            try
            {
                m_tcpListener = new TcpListener(new IPEndPoint(IPAddress.Loopback, m_Port));
                m_tcpListener.Start();
                m_tcpListener.BeginAcceptSocket(AcceptEditorSocketConnectionCallback, m_tcpListener);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        public string InternalEval(string code)
        {
            code += "\nFLUSH\n";
            try
            {
                m_Socket.Send(Encoding.UTF8.GetBytes(code));
            }
            catch (SocketException se)
            {
                Console.WriteLine("Socket.Send failed with error code: " + se.ErrorCode);
                Console.WriteLine("Message: " + se.Message);
                if (se.Data.Count > 0)
                {
                    Console.WriteLine("Details: ");
                }
                throw;
            }

            var ms = new MemoryStream();
            const int buffersize = 1024 * 1024;
            var buffer = new byte[buffersize];
            while (true)
            {
                try
                {
                    var read = m_Socket.Receive(buffer, 0, buffersize, SocketFlags.None);

                    if (read > 0)
                    {
                        ms.Write(buffer, 0, read);

                        // String will be zero terminated
                        if (buffer[read - 1] == 0)
                            break;
                    }
                    else
                    {
                        return "No return value from Editor.CodeEval.Eval";
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }

            var returnedString = Encoding.UTF8.GetString(ms.ToArray(), 0, (int)ms.Length - 1);
            const string prefix = "RESPONSE: ";
            if (!returnedString.StartsWith(prefix))
                throw new Exception("Expected a response to the evaluation request that started with '" + prefix +
                    "' but was: " + returnedString);
            Console.WriteLine(returnedString);
            return returnedString.Substring(prefix.Length);
        }

        public void KeepAlive()
        {
            m_Socket.Send(new byte[] { });
        }
    }
}
