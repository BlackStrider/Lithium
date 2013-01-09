using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Net;

namespace Lithium
{
    /// <summary>
    /// Логика взаимодействия для ServerWindow.xaml
    /// </summary>
    public partial class ServerWindow : Window
    {
        public ServerWindow()
        {
            InitializeComponent();
        }

        public void ShowMessage(string message)
        {
            Thread mChanger = new Thread(new ThreadStart(delegate() { this.ChangeTextProperly(message); }));
            mChanger.Start();
        }

        private Socket _serverSocket;
        private int _port = 11000;
        private delegate void TextChanger();

        public void SetupServerSocket()
        {
            // Получаем информацию о локальном компьютере
            IPAddress addr = IPAddress.Parse("172.17.135.60");
            IPEndPoint myEndpoint = new IPEndPoint(addr, _port);

            // Создаем сокет, привязываем его к адресу
            // и начинаем прослушивание
            _serverSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, 0);
            _serverSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, _port));
            _serverSocket.Listen(10);
            ShowMessage("Server Started");

            IPHostEntry host = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = host.AddressList[0];
            foreach (var adder in host.AddressList)
                ShowMessage("\n" + adder.ToString());
        }

        private class UserConnectionInfo
        {
            public Socket Socket;
            public byte[] Buffer;
        }

        private List<UserConnectionInfo> _connections = new List<UserConnectionInfo>();

        public void Start()
        {
            SetupServerSocket();
            for (int i = 0; i < 10; i++)
                _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), _serverSocket);
        }

        private void AcceptCallback(IAsyncResult result)
        {
            UserConnectionInfo connection = new UserConnectionInfo();
            try
            {
                // Завершение операции Accept
                Socket s = (Socket)result.AsyncState;
                connection.Socket = s.EndAccept(result);
                connection.Buffer = new byte[255];
                lock (_connections)
                    _connections.Add(connection);

                // Начало операции Receive и новой операции Accept
                connection.Socket.BeginReceive(connection.Buffer, 0, connection.Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), connection);
                _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), result.AsyncState);
            }
            catch (SocketException exc)
            {
                CloseConnection(connection);
                ShowMessage("Socket exception: " + exc.SocketErrorCode);  // словим краш
            }
            catch (Exception exc)
            {
                CloseConnection(connection);
                ShowMessage("Exception: " + exc);                           // словим краш
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            UserConnectionInfo connection = (UserConnectionInfo)result.AsyncState;
            try
            {
                int bytesRead = connection.Socket.EndReceive(result);
                if (bytesRead != 0)
                {
                    ShowMessage(Encoding.UTF8.GetString(connection.Buffer, 0, bytesRead));
                    lock (_connections)
                    {
                        foreach (UserConnectionInfo conn in _connections)
                        {
                            //if (connection != conn)
                            {
                                conn.Socket.Send(connection.Buffer, bytesRead, SocketFlags.None);
                            }
                        }
                    }
                    connection.Socket.BeginReceive(connection.Buffer, 0, connection.Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), connection);
                }
                else
                    CloseConnection(connection);
            }
            catch (SocketException exc)
            {
                CloseConnection(connection);
                ShowMessage("Socket exception: " + exc.SocketErrorCode);
            }
            catch (Exception exc)
            {
                CloseConnection(connection);
                ShowMessage("Exception: " + exc);
            }
        }

        private void CloseConnection(UserConnectionInfo user)
        {
            user.Socket.Close();
            lock (_connections)
                _connections.Remove(user);
        }

        private void ChangeTextProperly(string msg)
        {
            if (ServerBox.Dispatcher.CheckAccess())
            {
                ServerBox.AppendText(msg);
            }
            else
            {
                ServerBox.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new TextChanger(delegate() { this.ChangeTextProperly(msg); }));
            }
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            ServerBox.IsReadOnly = true;
            ServerBox.Document.Blocks.Clear();
        }
    }
}
