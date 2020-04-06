using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

/*
    服务端有下面几个流程，需要注意这里面的英文，后面我们在编程的时候对应的方法名也是这些单词：

        1 创建socket对象

        2 bind：绑定IP地址和端口，相当于获取一个手机号码。

        3 listen：开始监听绑定的IP地址和端口，等待客户端的连接，相当于手机插好卡，开机，就可以等别人给你打电话了。

        4 accept：如果有客户端发起连接，通过accept接受连接请求，连接成功后会复制一个socket出来用于和当前接受连接的客户端进行通信。
        * 注意：服务端最初创建的那个socket只是用来监听并建立连接用的，实际和客户端通信并不是最初的socket，而是在accept这一步会自动创建一个新的socket出来和客户端通信。

        5 read/write：使用新的socket读写数据

        6 close：关闭socket，如果关闭的是服务端的监听socket，则无法接收新的连接，但是已经创建的和客户端的连接不会被关闭。
 
   客户端流程：

        1 创建socket对象

        2 connect：连接服务端，连接成功后系统会自动分配端口

        3 read/write：连接成功后，就可以进行数据的读写了，这里读写使用的socket还是第一步创建的socket对象。

        4 close：关闭连接。
 
 */

namespace Echo
{
    class EchoServer
    {

        static void Main(string[] args)
        {

            new ChatServer().Init();

            // 接收一个键盘输入的字符，目的是不让命令行自动关闭
            Console.ReadKey();
        }
    }


    class Client
    {
        public const ushort BUFFER_LENGTH = 1024;
        public Socket socket;
        public byte[] buffer = new byte[BUFFER_LENGTH];
    }

    class ChatServer
    {
        Dictionary<Socket, Client> ClientDic = new Dictionary<Socket, Client>();

        public void Init()
        {
            Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, 9999));
            socket.Listen(100);
            // 开始异步accept客户端的连接，不会阻塞
            socket.BeginAccept(AcceptCallback, socket);
            Console.WriteLine("服务端启动成功");
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Socket socket = ar.AsyncState as Socket;
            Socket clientSocket = socket.EndAccept(ar);
            string msg = string.Format("有新的客户端连接:{0:G}", clientSocket.RemoteEndPoint);
            Console.WriteLine(msg);

            // 新建一个客户端对象(数据)
            Client c = new Client();
            c.socket = clientSocket;
            ClientDic.Add(clientSocket, c);

            // 5. 异步接收客户端消息，接收到数据后会回调到ReceiveCallback方法
            byte[] buffer = c.buffer;
            clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, c);

            // 继续Accept
            socket.BeginAccept(AcceptCallback, socket);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            Client client = ar.AsyncState as Client;
            Socket clientSocket = client.socket;
            byte[] buffer = client.buffer;
            string msg = string.Empty;
            try
            {
                int length = client.socket.EndReceive(ar);
                if (length > 0)
                {
                    msg = Encoding.UTF8.GetString(buffer, 0, length);
                    msg = string.Format("接收到客户端的消息:{0:G}", msg);
                    Console.WriteLine(msg);

                    // 6. 将收到的消息返回给所有客户端，优化了发送的字节数量，只发送有数据内容的长度
                    foreach (KeyValuePair<Socket, Client> keyValue in ClientDic)
                    {
                        keyValue.Key.Send(buffer, length, SocketFlags.None);
                    }
                    // 重新开始接收
                    clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, client);
                }
                else // 接收到长度为0的数据，代表客户端断开连接
                {
                    OnClientDisconnect(clientSocket);
                }
            }
            catch (SocketException ex)
            {
                // 如果服务端有向客户端A未发送完的数据，客户端A主动断开时会触发10054异常，在此捕捉
                if (ex.SocketErrorCode == SocketError.ConnectionReset)
                    OnClientDisconnect(clientSocket);
            }



        }


        private void OnClientDisconnect(Socket clientSocket)
        {
            //Console.WriteLine($"客户端断开连接：{clientSocket.RemoteEndPoint}");
            string msg = string.Format("客户端断开连接:{0:G}", clientSocket.RemoteEndPoint);
            Console.WriteLine(msg);

            // 移除client列表中该客户端连接
            ClientDic.Remove(clientSocket);

            clientSocket.Close();
        }
    }
}
