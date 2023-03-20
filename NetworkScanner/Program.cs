using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Linq;

namespace NetworkScanner
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            foreach (var (ipAddress, hostEntry) in
            // Tüm ağ arayüzlerini döngüye al
            from networkInterface in NetworkInterface.GetAllNetworkInterfaces()// Sadece etkin ağ arayüzlerini seç
            where networkInterface.OperationalStatus == OperationalStatus.Up// Tüm IP adreslerini döngüye al
            from ipAddressInfo in networkInterface.GetIPProperties().UnicastAddresses// IPv4 adresi kontrolü yap
            where ipAddressInfo.Address.AddressFamily == AddressFamily.InterNetwork
            let ipAddress = ipAddressInfo.Address// DNS sorgusu yap
            let hostEntry = Dns.GetHostEntry(ipAddress)
            select (ipAddress, hostEntry))
            {
                // IP adresi ve bilgisayar adını ekrana yazdır
                Console.WriteLine($"IP address: {ipAddress}, Computer name: {hostEntry.HostName}");
            }

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();
            IPEndPoint[] udpEndPoints = properties.GetActiveUdpListeners();
            TcpConnectionInformation[] tcpConnections = properties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation info in tcpConnections)
            {
                Console.WriteLine("{0} <==> {1} ({2})", info.LocalEndPoint, info.RemoteEndPoint, info.State);
            }

            foreach (IPEndPoint endpoint in tcpEndPoints)
            {
                Console.WriteLine("{0}", endpoint);
            }

            foreach (IPEndPoint endpoint in udpEndPoints)
            {
                Console.WriteLine("{0}", endpoint);
            }

            IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (IPAddress address in addresses)
            {
                Console.WriteLine("IP Address: {0}", address);

                try
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(address);

                    foreach (IPAddress ip in hostEntry.AddressList)
                    {
                        Console.WriteLine("    Host: {0} ({1})", hostEntry.HostName, ip);

                        foreach (int port in new int[] { 80, 8080, 443, 3306 }) // Burada kontrol edilecek port numaralarını değiştirebilirsiniz.
                        {
                            try
                            {
                                using (TcpClient client = new TcpClient())
                                {
                                    client.Connect(ip, port);
                                    Console.WriteLine("        Port {0}: Open", port);
                                }
                            }
                            catch (SocketException ex)
                            {
                                if (ex.SocketErrorCode == SocketError.ConnectionRefused)
                                {
                                    Console.WriteLine("        Port {0}: Closed", port);
                                }
                                else
                                {
                                    Console.WriteLine("        Port {0}: Error ({1})", port, ex.SocketErrorCode);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("    Error: {0}", ex.Message);
                }
            }


        }
    }
}