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
    class Program
    {

        public class ClientSocketAndBuffer:Object
        {
            public Socket clientSocket;
            public byte[] buffer;
        }
        //同步使用Socket
        static void UseSocket()
        {
            Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            socket.Bind(new IPEndPoint(IPAddress.Any, 9999));

            socket.Listen(100);

            var clientSocket = socket.Accept();
            var buffer = new byte[1024];
            int length = clientSocket.Receive(buffer);

            string msg = string.Format("接收到客户端的消息:{0:G}", Encoding.UTF8.GetString(buffer));
            Console.WriteLine(msg);

            // 6. 将收到的消息返回给客户端
            clientSocket.Send(buffer);

            // 关闭两个socket
            clientSocket.Close();
            socket.Close();

            // 接收一个键盘输入的字符，目的是不让命令行自动关闭
            Console.ReadKey();
        }

        //异步使用Socket
        static void UseSocketAsyn()
        {
            Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, 9999));
            socket.Listen(100);

            //开始异步accept客户端的连接，不会阻塞
            socket.BeginAccept(AcceptCallback, socket);

            // 接收一个键盘输入的字符，目的是不让命令行自动关闭
            Console.ReadKey();
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            var socket = ar.AsyncState as Socket;
            var clientSocket = socket.EndAccept(ar);
            string msg = string.Format("有新的客户端连接:{0:G}", clientSocket.RemoteEndPoint);
            Console.WriteLine(msg);

            // 5. 接收客户端消息，如果客户端不发送数据，服务端会阻塞（挂起）在这个位置
            var buffer = new byte[1024];
            ClientSocketAndBuffer clientAndBuffer = new ClientSocketAndBuffer();
            clientAndBuffer.clientSocket = clientSocket;
            clientAndBuffer.buffer = buffer;
            clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, clientAndBuffer);
             // 继续Accept
            socket.BeginAccept(AcceptCallback, socket);
        }
        private static void ReceiveCallback(IAsyncResult ar)
        {
            ClientSocketAndBuffer data = ar.AsyncState as ClientSocketAndBuffer;
            Socket clientSocket = data.clientSocket;
            var buffer = data.buffer;
            int length = clientSocket.EndReceive(ar);
            string msg = string.Empty;
            if (length > 0)
            {
                msg = Encoding.UTF8.GetString(buffer, 0, length);
                msg = string.Format("接收到客户端的消息:{0:G}", msg);
                Console.WriteLine(msg);

                clientSocket.Send(buffer, length, SocketFlags.None);
                // 重新开始接收
                ClientSocketAndBuffer clientAndBuffer = new ClientSocketAndBuffer();
                clientAndBuffer.clientSocket = clientSocket;
                clientAndBuffer.buffer = buffer;
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, clientAndBuffer);
            }
            else
            { 
                 msg = string.Format("客户端断开连接:{0:G}", clientSocket.RemoteEndPoint);
                 Console.WriteLine(msg);
                 clientSocket.Close();
            }
        }
        //static void Main(string[] args)
        //{
        //   // UseSocket();

        //    UseSocketAsyn();

        //}
    }
}
