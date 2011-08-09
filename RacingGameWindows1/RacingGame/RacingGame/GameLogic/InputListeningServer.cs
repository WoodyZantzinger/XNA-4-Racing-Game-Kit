using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace RacingGame.GameLogic
{
    class InputListeningServer
    {
        public int tilt = 0;
        public bool left = false;
        public bool right = false;
        public bool isConnected;
        private TcpListener tcpListener;
        private Thread listenThread;
        String data = "No Data yet";

        public InputListeningServer()
        {
            isConnected = false;
            this.tcpListener = new TcpListener(IPAddress.Any, 3000);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
        }

        private void ListenForClients()
        {
            this.tcpListener.Start();
            //blocks until a client has connected to the server
            TcpClient client = this.tcpListener.AcceptTcpClient();

            //create a thread to handle communication 
            //with connected client
            Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
            this.isConnected = true;
            clientThread.Start(client);
        }

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[5];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    bytesRead = clientStream.Read(message, 0, 5);
                    clientStream.Flush();
                }
                catch
                {
                    //a socket error has occured
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    tilt = 0;
                    break;
                }

                //Message Read
                if (message[0] == 0x0001)
                {
                    //Accerometer Update                    
                    tilt = BitConverter.ToInt32(message, 1);
                    tilt *= -1;
                    System.Diagnostics.Debug.WriteLine(tilt);
                }
                if (message[0] == 0x0002)
                {
                    //Button Update
                    //1 = Left button, 2 = right; 1 = on, 0 = off
                    if (message[1] == 0x0001)
                    {
                        if (message[2] == 0x0001) { left = true; } else { left = false; }
                    }
                    else if (message[1] == 0x0002)
                    {
                        if (message[2] == 0x0001) { right = true; } else { right = false; }
                    }
                    
                }
            }

            tcpClient.Close();
        }

        public String getData()
        {
            return data;
        }
    }
}
