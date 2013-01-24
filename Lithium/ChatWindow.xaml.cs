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

        private delegate void TextChanger();
        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Send(client, ClientWriteBox.Text);
            ClientBox.AppendText('\n' + "[" + ChatNameBox.Content + "]" + ClientWriteBox.Text);  //косяк, потом переписать
            ClientWriteBox.Clear();
        }

        public void ShowMessage(string message)
        {
            Thread mChanger = new Thread(new ThreadStart(delegate() { this.ChangeTextProperly(message); }));
            mChanger.Start();
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

        private void ChangeTextProperly(string msg)
        {
            if (ClientBox.Dispatcher.CheckAccess())
                {
                    ClientBox.AppendText(msg);
                    return;
                }
                else
                {
                    ClientBox.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new TextChanger(delegate() { this.ChangeTextProperly(msg); }));
                }
        }

        private const int port = 11000;

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
                ShowMessage(e.ToString());
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
                ShowMessage("Socket connected to " + client.RemoteEndPoint.ToString());
                Receive(client);
            }
            catch (Exception e)
            {
                ReconnectInTime(5000);
                ShowMessage(e.ToString());
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
                ShowMessage(e.ToString());
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

                if (bytesRead != 0)
                {
                    ShowMessage("\n[From Server]" + Encoding.UTF8.GetString(state.buffer, 0, bytesRead));
                    // Get the rest of the data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
            }
            catch (Exception e)
            {
                ShowMessage(e.ToString());
            }
        }

        private void Send(Socket client, String data)
        {
            Packets newPacket = new Packets(0, ChatNameBox.Content.ToString(), data);
            byte[] newData = new byte[newPacket.PacketLength];
            newData = newPacket.PrepareMessageToSending();
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
                ShowMessage(e.ToString());
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
            Timer tmer = new Timer(TimerCallback, null, Time, System.Threading.Timeout.Infinite);
        }

        void TimerCallback(object param)
        {
            this.StartClient();
        }
    }
}
