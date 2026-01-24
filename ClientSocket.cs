using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Ambulance
{
    class ClientSocket
    {
        public void SocketSend()
        {
            try
            {
                // Устанавливаем удаленную точку для сокета
                IPHostEntry ipHost = Dns.GetHostEntry("localhost");
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);

                Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sender.Connect(ipEndPoint);
                byte[] msg = Encoding.UTF8.GetBytes("123");
                sender.Send(msg);



                // Освобождаем сокет
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch
            {

            }
        }
    }
}
