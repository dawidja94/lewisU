using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace HW4
{
    public static class ByteExtension //extension to convert byte array to string
    {
        public static string GetString(this byte[] arr, int k)
        {
            return Encoding.ASCII.GetString(arr, 0, k);
        }
    }
    public static class StringExtension
    {
        public static byte[] ToByteArray(this string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }
    }

    class Chatroom
    {
        private Dictionary<Socket, string> Connections = new Dictionary<Socket, string>();  //dictionary stores keys and values

        public Chatroom()
        {
            SendGlobalMessage +=Message;
        }

        public void Message(Socket s, string msg)
        {

            //format of sent messages  - [User]: ......
            string newMessage = String.Format("[{0}]: {1}\r\n", Connections[s], msg);

            foreach (Socket soc in Connections.Keys)
            {
                soc.Send(newMessage.ToByteArray());
            }
        }
        public void AddConnection(Socket s)
        {
            //ask user for a username(or assign random name)

            string username = "username";
            s.Send(ASCIIEncoding.ASCII.GetBytes("Please enter a username(or press enter to receive a random name): "));
            byte[] b = new byte[1024];
            int k = s.Receive(b);
            Random rnd = new Random();   //random number for a username
            int number = rnd.Next(0, 100);

            for (int i = 0; i < k; i++)
                
                username = b.GetString(k);
            if (username.Contains("\r\n")) //no username, assign random one
            {
                
                username = $"username{number}";
            }
            ;
            Connections[s] = username;
            Console.WriteLine(username + " entered the chat");
            Thread t = new Thread(ReadData);
            t.Start(s);
        }


        public void PingConnections() { }  //
        public void ReadData(object socket)
        { 
            //unbox a socket to Socket object
            Socket s = (Socket)socket;
            byte[] b = new byte[1024];

            while (true)
            {
                string msg = "";
             
                while (true)
                {
                    int k = s.Receive(b);
                    string entry = b.GetString(k);
                    if (entry.Contains("\r\n")) //if there is anything in the msg
                        break;
                    msg += entry;
                 
                  
                }

                SendGlobalMessage(s, msg); //fire off event to send a global mesage


            }
            
        }

        public delegate void EventMessage(Socket s, string msg);  //event prototype
        public event EventMessage SendGlobalMessage;

    }

    class Server : TcpListener
    {

        
        public Server() : base(IPAddress.Loopback, 5000)      //Loopback is: 127.0.0.1    to bind to any IP, use 'IPAddress.Any'
        {
            Start();
            Thread t = new Thread(handleConnections);

            t.Start();
            Console.WriteLine("CONNECTED");
        }


        public void handleConnections()
        {
           Socket s;

            while (true)
            {
                s = this.AcceptSocket();

                OnSocketAccept(s);
            }

                //spin up a new thread that handles 
                //incoming data from that client, 
                //then llop to accept another connection

                //  }
            
        }


        public delegate void EventConnection(Socket s); //event prototype  
        public event EventConnection OnSocketAccept;    //this gets fired when new socket is accepted (list of method pointers)
    }
    class Program
    {


        static void Main(string[] args)
        {
            Server svr = new Server();

            Chatroom room = new Chatroom();


            svr.OnSocketAccept += room.AddConnection; //add socket to chatroom when accepted

            while (true)
            {
                Thread.Sleep(100);  //purpose of this loop is not to grind the CPU
            }

          
        }
    }
}
