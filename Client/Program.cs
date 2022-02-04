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
        private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static void Main(string[] args)
        {
            Console.Title = "Client";
            Console.WriteLine("Hello World!");
            LoopConnect();//gestisco la connessione con un loop in modo da attendere in caso il server non sia ancora stato messo in piedi
            SendLoop();//gestisco la send in loop in modo da poter mandare più messaggi
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
                    _clientSocket.Connect(IPAddress.Loopback, 9000);

                }
                catch (SocketException)
                {
                    Console.Clear();
                    Console.WriteLine("Connection attempts:" + attempts.ToString());
                }
            }

            Console.Clear();
            Console.WriteLine("Connected");
        }

        private static void SendLoop()
        {
            bool exit = false;
            while (exit == false)
            {
                //per mandare i messaggi da console
                Console.WriteLine("Enter a request: ");
                string req = Console.ReadLine();

                if (req == "close")
                {
                    exit = true;
                }
                else
                {

                    byte[] buffer = Encoding.ASCII.GetBytes(req);
                    _clientSocket.Send(buffer);

                    byte[] receivedBuf = new byte[1024];
                    int rec = _clientSocket.Receive(receivedBuf);
                    if (rec == 0)
                    {
                        exit = true;
                    }
                    else
                    {
                        byte[] data = new byte[rec];
                        Array.Copy(receivedBuf, data, rec);
                        Console.WriteLine("Received: " + Encoding.ASCII.GetString(data));
                    }
                }
            }
        }
    }
}
