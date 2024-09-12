using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;


namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            NameLocalHost commands = new NameLocalHost(); // Даём область памяти на наш класс
            commands.LocalName(); // Выводим Имя локального хоста
            
            commands.LocalIpAdress(); // Выводим Маску сети и Адрес сети

            ScanByPort scanner = new ScanByPort();
            scanner.GetScanByPort("172.29.13.0", "80,443,631,110");
        }
    }

    class NameLocalHost
    {
        public void LocalName()
        {
            string host = Dns.GetHostName(); // Записывается в переменную host, получаем имя хоста локального компьютера.
            Console.WriteLine(host);
        }
        public void LocalIpAdress() 
        {
            string host = Dns.GetHostName(); // Записывается в переменную host, получаем имя хоста локального компьютера.
            IPAddress[] addresses = Dns.GetHostAddresses(host); // Dns.GetHostAddresses - возвращает IP-адреса для указанного узла

            foreach (IPAddress address in addresses)
            {
                if (!IPAddress.IsLoopback(address)) ProcessIpAddress(address);
            }
        }
        private void ProcessIpAddress(IPAddress address)
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface @interface in interfaces) 
            {
                if (@interface.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue; //Проверка типа сетевого интерфейса

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
                    byte[] addressBytes = address.GetAddressBytes(); // Получаем байты из address
                    int subnetMask = BitConverter.ToInt32(subnetMaskBytes, 0); // Преобразуем байты в int32

                    byte[] networkAddressBytes = new byte[4];
                    for (int i = 0; i < 4; i++) 
                    {
                        networkAddressBytes[i] = (byte)(addressBytes[i] & subnetMaskBytes[i]); // Логическим И сравнивая байты
                    }

                    IPAddress networkAddress = new IPAddress(networkAddressBytes);

                    if (IsPrivateIP(address))
                        Console.WriteLine($"{address} ; Маска сети: {new IPAddress(subnetMaskBytes)} ; Адрес сети: {networkAddress}");
                    else
                        Console.WriteLine($"{address} ; Маска сети: {new IPAddress(subnetMaskBytes)} ; Адрес сети: {networkAddress}");
                }    
            }
        }
        private bool IsPrivateIP(IPAddress address)
        {
            byte[] bytes = address.GetAddressBytes(); // Создаём одномерный массив
                int firstOctet = BitConverter.ToInt32(bytes, 0); // Конвертируем типы байты в инт
                if (firstOctet == 10) return true;
                if (firstOctet == 172 && firstOctet >= 16 && firstOctet <= 31) return true;
                if (firstOctet == 192 && firstOctet == 168) return true;
                return false;
        }
    }

    class ScanByPort : NameLocalHost
    {
        public void GetScanByPort(string ipAddress, string ports)
        {
            
            IPAddress address;

            if (!IPAddress.TryParse(ipAddress, out address)) {
                Console.WriteLine($"Неправильный формат IP-адреса. ваш йпи {ipAddress} правильно так:");
                return; 
            }
            string[] portArray = ports.Split(',');

            foreach (string port in portArray) {
                if (int.TryParse(port.Trim(), out int portNumber))
                {
                    if (IsPortOpen(address, portNumber))
                        Console.WriteLine($"Порт {portNumber} открыт.");
                    else
                        Console.WriteLine($"Порт {portNumber} закрыт.");
                }
                else
                    Console.WriteLine($"Неправильный формат порта: {port}");
            }
        }

        private bool IsPortOpen(IPAddress address, int port)
        {
            using (TcpClient tcpClient = new TcpClient())
            {
                try {
                    tcpClient.Connect(address, port);
                    return true;
                }
                catch (SocketException) {
                    return false;
                }
            }
        }
    }
}