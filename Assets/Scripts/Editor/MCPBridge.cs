using UnityEngine;
using UnityEditor;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MCPBridge
{
    public class MCPBridgeWindow : EditorWindow
    {
        private static TcpListener listener;
        private static Thread listenerThread;
        private static bool isRunning = false;
        private static int port = 3000;

        [MenuItem("Window/MCP Bridge")]
        public static void ShowWindow()
        {
            GetWindow<MCPBridgeWindow>("MCP Bridge");
        }

        void OnGUI()
        {
            GUILayout.Label("MCP Bridge Server", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            port = EditorGUILayout.IntField("Port:", port);
            
            EditorGUILayout.Space();
            
            if (isRunning)
            {
                EditorGUILayout.HelpBox($"Server is running on port {port}", MessageType.Info);
                
                if (GUILayout.Button("Stop Server"))
                {
                    StopServer();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Server is not running", MessageType.Warning);
                
                if (GUILayout.Button("Start Server"))
                {
                    StartServer();
                }
            }
        }

        private void StartServer()
        {
            if (!isRunning)
            {
                isRunning = true;
                listenerThread = new Thread(ListenForClients);
                listenerThread.Start();
                Debug.Log($"MCP Bridge Server started on port {port}");
            }
        }

        private void StopServer()
        {
            if (isRunning)
            {
                isRunning = false;
                if (listener != null)
                {
                    listener.Stop();
                }
                if (listenerThread != null)
                {
                    listenerThread.Join(1000);
                }
                Debug.Log("MCP Bridge Server stopped");
            }
        }

        private static void ListenForClients()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();

                while (isRunning)
                {
                    if (listener.Pending())
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        Thread clientThread = new Thread(HandleClientComm);
                        clientThread.Start(client);
                    }
                    Thread.Sleep(100);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"MCP Bridge Server error: {e.Message}");
            }
        }

        private static void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }

                string jsonString = Encoding.UTF8.GetString(message, 0, bytesRead);
                
                // Process the request and send response
                string response = ProcessRequest(jsonString);
                byte[] responseData = Encoding.UTF8.GetBytes(response);
                clientStream.Write(responseData, 0, responseData.Length);
                clientStream.Flush();
            }

            tcpClient.Close();
        }

        private static string ProcessRequest(string request)
        {
            // Simple echo for now - can be extended to handle actual Unity operations
            return JsonConvert.SerializeObject(new { success = true, message = "Bridge connected" });
        }

        void OnDestroy()
        {
            StopServer();
        }
    }
}
