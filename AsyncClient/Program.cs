using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static IPEndPoint _server = new IPEndPoint(IPAddress.Loopback, 9000);
        static IPEndPoint _client = new IPEndPoint(IPAddress.Loopback, 0);

        static byte[] _Receivebuffer = new byte[1024];

        static Socket _clientSocket = new(_client.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        static void Main(string[] args)
        {
            Console.WriteLine("Client");
            Console.Title = "Client";
            LoopConnect();//gestisco la connessione con un loop in modo da attendere in caso il server non sia ancora stato messo in piedi
            Comunicazione();//gestisco la send in loop in modo da poter mandare più messaggi

            _clientSocket.Close();
            Console.ReadLine();
        }

        private static void LoopConnect()
        {
            int attempts = 0;
            while (!_clientSocket.Connected)
            {
                try
                {
                    attempts++;
                    _clientSocket.Connect(_server);

                }
                catch (SocketException)
                {
                    Console.Clear();
                    Console.WriteLine("Connection attempts:" + attempts.ToString());
                }
            }

            Console.Clear();
            Console.WriteLine("Connected");

            _clientSocket.BeginReceive(_Receivebuffer, 0, _Receivebuffer.Length, SocketFlags.None, new AsyncCallback(InfoConnessione), _clientSocket);
        }

        private static void Comunicazione()
        {
            //ricevo il primo messaggio
            _clientSocket.BeginReceive(_Receivebuffer, 0, _Receivebuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), _clientSocket);

            while (true)
            {
                byte[] _Sendbuffer = new byte[1024];

                Console.WriteLine("Enter a request: ");
                string req = Console.ReadLine();

                if (req.Length <= 0)
                {
                    _clientSocket.Send(System.Text.Encoding.UTF8.GetBytes("Disconnesso"));
                    _clientSocket.Close();
                    break;
                }

                _Sendbuffer = System.Text.Encoding.UTF8.GetBytes(req);

                //_clientSocket.BeginSend(_Sendbuffer, 0, _Sendbuffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), _clientSocket);
                _clientSocket.Send(_Sendbuffer);
            }
        }

        private static void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }

        private static void ReceiveCallBack(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;//contiene il socket di connessione (il client socket)

            int reqLength = socket.EndReceive(AR);
            string reqReceived = System.Text.Encoding.UTF8.GetString(_Receivebuffer[0..reqLength]);

            Console.WriteLine($"Messaggio ricevuto: {reqReceived}");

            socket.BeginReceive(_Receivebuffer, 0, _Receivebuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), socket);
        }

        //gestisco le eventuali informazioni iniziali dal server
        //In questo casoi il mio numero di client
        private static void InfoConnessione(IAsyncResult AR) 
        {
            Socket socket = (Socket)AR.AsyncState;

            int reqLength = socket.EndReceive(AR);
            string reqReceived = System.Text.Encoding.UTF8.GetString(_Receivebuffer[0..reqLength]);

            Console.WriteLine(reqReceived);
        }

        //private static void Comunicazione()
        //{
        //    bool exit = false;

        //    //RICEZIONE ASICRONA 
        //    while (exit == false)
        //    {
        //        //per mandare i messaggi da console
        //        Console.WriteLine("Enter a request: ");
        //        string req = Console.ReadLine();

        //        if (req == "close")
        //        {
        //            exit = true;
        //        }
        //        else
        //        {

        //            byte[] buffer = Encoding.ASCII.GetBytes(req);
        //            _clientSocket.Send(buffer);

        //            //byte[] receivedBuf = new byte[1024];
        //            //int rec = _clientSocket.Receive(receivedBuf);
        //            //if (rec == 0)
        //            //{
        //            //    exit = true;

        //            //    _clientSocket.Close();

        //            //}
        //            //else
        //            //{
        //            //    byte[] data = new byte[rec];
        //            //    Array.Copy(receivedBuf, data, rec);
        //            //    Console.WriteLine("Received: " + Encoding.ASCII.GetString(data));
        //            //}
        //        }
        //    }
        //}

        //CallBackReceive
        //{
        //EndReceive
        //gestisco il pacchetto ricevuto
        //beginReceive(CallbackReceive)
        //}

    }
}
