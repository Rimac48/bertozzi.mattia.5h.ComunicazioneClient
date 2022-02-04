using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace multiple_clients
{
    class Program
    {
        
        static IPEndPoint _endPoint = new IPEndPoint(IPAddress.Loopback, 9000);//server
        static Socket _serverSocket = new Socket(_endPoint.AddressFamily,SocketType.Stream, ProtocolType.Tcp);//server
        static List<Socket> _clientSocketList = new List<Socket>();
        static byte[] _buffer = new byte[1024];


        static void Main(string[] args)
        {
            //metto in piedi il server e poi attendo con un Console.ReadLine ( dato che tutte le funzioni sono asincrone, se non attendo il programma terminerebbe immediatamente)
            Console.WriteLine("Server");
            SetupServer();
            Console.ReadLine();
            _serverSocket.Close();//chiudo la socket di ascolto e termino il programma
        }

        private static void SetupServer()
        {
            _serverSocket.Bind(_endPoint);
            _serverSocket.Listen(10);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback),null);//inizio ad accettare il primo client e gestisco l'evento con una callback asincrona
        }

        private static void AcceptCallback(IAsyncResult AR) 
        {
            Socket clientSocket = _serverSocket.EndAccept(AR); //come la Accept sincrona, anche la EndAccept mi ritorna la socket di comunicazione con il client

            if (_clientSocketList.Count<2)
            {
                _clientSocketList.Add(clientSocket);
                clientSocket.Send(Encoding.UTF8.GetBytes($"Sei il Client N° {_clientSocketList.Count}"));
                _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

                if(_clientSocketList.Count == 2)//quando i client connessi sono 2 comincio la comunicazione
                {
                    ClientToClientCommunication();
                }
            }
            else
            {
                clientSocket.Send(Encoding.UTF8.GetBytes("Coda Server (2/2) Piena"));
            }
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);//FORSE SERVE FORSE NO

        }

        static void ClientToClientCommunication()//comincio ad ascoltare da entrambi i client
        {
            _clientSocketList[0].BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(Client0to1), _clientSocketList[0]);
            _clientSocketList[1].BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(Client1to0), _clientSocketList[1]);            
        }

        //private static void ReceiveCallback(IAsyncResult AR)
        //{
        //    Socket socket = (Socket)AR.AsyncState; //l'oggetto che ho passato nella BeginReceive lo trovo dentro AR come AsyncState
        //    int receivedMsgLng = socket.EndReceive(AR);
        //    //gestisco l'evento di disconnessione lato Client
        //    if (receivedMsgLng == 0)
        //    {
        //        socket.Close();
        //    }
        //    else
        //    {
        //        byte[] dataBufRcv = new byte[receivedMsgLng];
        //        Array.Copy(_buffer, dataBufRcv, receivedMsgLng);

        //        string receivedText = Encoding.UTF8.GetString(dataBufRcv);
        //        string responseText = string.Empty;
        //        //se il client mi chiede l'orario lo mando altrimenti echo
        //        if (receivedText == "time")
        //        {
        //            responseText = DateTime.Now.ToLongTimeString();
        //            byte[] dataBufSnd = Encoding.UTF8.GetBytes(responseText);
                    
        //            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(Client0to1), socket);
        //            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(Client1to0), socket);
        //        }
        //        else
        //        {
        //            socket.Close();
        //        }
        //    }
        //}

        static void Client0to1(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            int receivedMsgLng = socket.EndReceive(AR);

            if (receivedMsgLng == 0)
            {
                socket.Close();
            }
            else
            {
                byte[] dataBufRcv = new byte[receivedMsgLng];
                Array.Copy(_buffer, dataBufRcv, receivedMsgLng);

                string receivedText = Encoding.UTF8.GetString(dataBufRcv);

                if (receivedText != "")
                {
                    byte[] dataBufSnd = Encoding.UTF8.GetBytes(receivedText);
                    _clientSocketList[1].BeginSend(dataBufSnd, 0, dataBufSnd.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                }
                else
                {
                    socket.Close();
                }
            }
        }

        static void Client1to0(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            int receivedMsgLng = socket.EndReceive(AR);

            if (receivedMsgLng == 0)
            {
                socket.Close();
            }
            else
            {
                byte[] dataBufRcv = new byte[receivedMsgLng];
                Array.Copy(_buffer, dataBufRcv, receivedMsgLng);

                string receivedText = Encoding.UTF8.GetString(dataBufRcv);

                if (receivedText != "")
                {
                    byte[] dataBufSnd = Encoding.UTF8.GetBytes(receivedText);
                    _clientSocketList[0].BeginSend(dataBufSnd, 0, dataBufSnd.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                }
                else
                {
                    socket.Close();
                }
            }
        }


        private static void SendCallback(IAsyncResult AR)
        {
            ClientToClientCommunication();
        }
    }
}
