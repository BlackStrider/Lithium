using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Timers;

namespace Lithium
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        public ChatWindow()
        {
            InitializeComponent();
        }

        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        System.Timers.Timer ReconnectionTimer = new System.Timers.Timer();
        TextManager txtmgr = new TextManager();
        private const int port = 11000;

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Send(client, ClientWriteBox.Text);
            ClientBox.AppendText('\n' + "[" + ChatNameBox.Content + "]" + ClientWriteBox.Text);  //косяк, потом переписать
            ClientWriteBox.Clear();
        }

        public class StateObject
        {
            // Client socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 256;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
            // Received data string.
            public StringBuilder sb = new StringBuilder();
        }

        // The response from the remote device.
        private static String response = String.Empty;

        public void StartClient()
        {
            // Connect to a remote device.
            try
            {
                IPAddress ipAddress = IPAddress.Parse("10.44.5.33");
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
        
                // Connect to the remote endpoint.
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
            }
            catch (Exception e)
            {
                //ShowMessage(e.ToString()); ToDo: add to log file
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);
                txtmgr.ShowMessage(ClientBox, "Socket connected to " + client.RemoteEndPoint.ToString());
                Receive(client);
                ReconnectionTimer.Stop();
            }
            catch (Exception e)
            {
                ReconnectInTime(5000);
                txtmgr.ShowMessage(ClientBox, "Unable connect to server");
            }
        }

        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                txtmgr.ShowMessage(ClientBox, e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                MemoryStream memstr = new MemoryStream(state.buffer, 0, bytesRead);
                Packets newReadPacket = new Packets();
                if (bytesRead > 5)//получаем длину хедера
                {
                    Packets dataViewer = newReadPacket.HandleMessagePacket(bytesRead, memstr);

                    if (dataViewer != null)
                    {
                        txtmgr.ShowMessage(ClientBox, dataViewer.GetNickname);
                        txtmgr.ShowMessage(ClientBox, dataViewer.GetMessage);
                    }
                    else
                        client.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
                    client.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
                }
                else
                    client.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                txtmgr.ShowMessage(ClientBox, e.ToString());
            }
        }

        private void Send(Socket client, String data)
        {
            Packets newPacket = new Packets(0, ChatNameBox.Content.ToString(), data);
            byte[] newData = newPacket.PrepareMessageToSending();
            client.BeginSend(newData, 0, newData.Length, 0, new AsyncCallback(SendCallback), client);    
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
            }
            catch (Exception e)
            {
                txtmgr.ShowMessage(ClientBox, e.ToString());
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ServerWindow srw = new ServerWindow();
            srw.Show();
        }

        private void Button_Loaded_1(object sender, RoutedEventArgs e)
        {
            this.StartClient();
        }

        private void Lit_Loaded(object sender, RoutedEventArgs e)
        {
            ClientBox.Document.Blocks.Clear();
            NameBox.Document.Blocks.Clear();
            ClientBox.IsReadOnly = true;
            NameBox.IsReadOnly = true;
            this.StartClient();
        }

        public void ReconnectInTime(Int32 Time)
        {
            ReconnectionTimer.Interval = Time;
            ReconnectionTimer.Elapsed += new ElapsedEventHandler(TimerEvent);
            ReconnectionTimer.Enabled = true;
        }

        private void TimerEvent(object source, ElapsedEventArgs e)
        {
            StartClient();
        }
    }
}
