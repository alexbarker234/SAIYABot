using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SAIYA
{
    public class PingServer
    {
        public static void CreateListener() =>  _ = Run();
        private static async Task Run()
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Any, 6969);
            TcpListener listener = new(ipEndPoint);
            listener.Start();

            Console.WriteLine("Uptime Robot listener up");

            try
            {
                while (true)
                {
                    using TcpClient handler = await listener.AcceptTcpClientAsync();
                    await using NetworkStream stream = handler.GetStream();

                    var dateTimeBytes = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\n\nbean");
                    await stream.WriteAsync(dateTimeBytes);
                }
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}
