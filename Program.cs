using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Collections.Generic;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            NameLocalHost localHost = new NameLocalHost();
            localHost.LocalName();

            ApiMaskAddress apiMaskAddress = new ApiMaskAddress();
            apiMaskAddress.LocalIpAdress();

            AddressNetwork addressNetwork = new AddressNetwork();

            ScanByPort scanByPort = new ScanByPort();
        }
    }

    class NameLocalHost
    {
        public void LocalName()
        {
            string host = Dns.GetHostName();
            Console.WriteLine(host);
        }
    }

    class ApiMaskAddress : NameLocalHost
    {
        public void LocalIpAdress()
        {
            string host = Dns.GetHostName();
            IPAddress[] addresses = Dns.GetHostAddresses(host);

            foreach (IPAddress address in addresses)
            {
                if (!IPAddress.IsLoopback(address))
                {
                    Console.WriteLine($"IP Address: {address}");
                    AddressNetwork addressNetwork = new AddressNetwork();
                    addressNetwork.ProcessIpAddress(address);
                }
            }
        }
    }

    class AddressNetwork
    {
        public void ProcessIpAddress(IPAddress address)
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface @interface in interfaces)
            {
                if (@interface.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                IPInterfaceProperties ipProps = @interface.GetIPProperties();
                UnicastIPAddressInformationCollection unicastIPInfoCol = ipProps.UnicastAddresses;
                IpAddressesMask(unicastIPInfoCol, address);
            }
        }

        private void IpAddressesMask(UnicastIPAddressInformationCollection unicastIPInfoCol, IPAddress address)
        {
            foreach (UnicastIPAddressInformation unicastIPInfo in unicastIPInfoCol)
            {
                if (unicastIPInfo.Address.Equals(address))
                {
                    byte[] subnetMaskBytes = unicastIPInfo.IPv4Mask.GetAddressBytes();
                    byte[] addressBytes = address.GetAddressBytes();
                    int subnetMask = BitConverter.ToInt32(subnetMaskBytes, 0);

                    byte[] networkAddressBytes = new byte[4];
                    for (int i = 0; i < 4; i++)
                    {
                        networkAddressBytes[i] = (byte)(addressBytes[i] & subnetMaskBytes[i]);
                    }

                    IPAddress networkAddress = new IPAddress(networkAddressBytes);

                    if (IsPrivateIP(address))
                        Console.WriteLine($"\n{address} ; Маска сети: {new IPAddress(subnetMaskBytes)} ; Адрес сети: {networkAddress}");
                    else
                        Console.WriteLine($"\n{address} ; Маска сети: {new IPAddress(subnetMaskBytes)} ; Адрес сети: {networkAddress}");

                    ScanByPort scanner = new ScanByPort();
                    scanner.GetScanByPort(address);
                }
            }
        }

        private bool IsPrivateIP(IPAddress address)
        {
            byte[] bytes = address.GetAddressBytes();
            int firstOctet = BitConverter.ToInt32(bytes, 0);
            if (firstOctet == 10) return true;
            if (firstOctet == 172 && firstOctet >= 16 && firstOctet <= 31) return true;
            if (firstOctet == 192 && firstOctet == 168) return true;
            return false;
        }
    }

    class ScanByPort
    {
        public void GetScanByPort(IPAddress ipAddress, int startPort = 80, int endPort = 100)
        {
            List<int> listOpenPort = new List<int>();
            for (int port = startPort; port <= endPort; port++)
            {
                if (IsPortOpen(ipAddress, port))
                {
                    listOpenPort.Add(port);
                    foreach (var itemPort in listOpenPort)
                    {
                        Console.WriteLine($"{itemPort} открытый порт");
                    }
                }
                else if (port == 80 || port == 554 || port == 8000 || port == 8200)
                {
                    // можно вписать для вывода закрытых портов
                }
            }
        }

        private bool IsPortOpen(IPAddress address, int port)
        {
            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    tcpClient.Connect(address, port);
                    return true;
                }
                catch (SocketException)
                {
                    return false;
                }
            }
        }
    }
}
