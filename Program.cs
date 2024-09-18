using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace UtilityCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("Введите команду: ");
                string? command = Console.ReadLine();
                ApiMaskAddress apiMaskAddress = new ApiMaskAddress();
                string[] mainMenu = new string[] { "\nhelp\n", "localname", "ipAdress", "\nexit\n" };

                switch (command)
                {
                    case "localname":
                        string localname = apiMaskAddress.LocalName();
                        Console.WriteLine(localname); // Выводим имя хоста в консоли
                        break;
                    case "ipAdress":
                        string ipAddressInfo = apiMaskAddress.LocalIpAdress();
                        Console.WriteLine(ipAddressInfo); // Выводим информацию о сетевом адресе
                        break;
                    case "help":
                        Console.WriteLine("Список команд: ");
                        foreach (string help in mainMenu)
                            Console.WriteLine(help);
                        break;
                    case "exit":
                        break;
                }
                break;
            }
        }
    }

    class ApiMaskAddress
    {
        public string LocalName()
        {
            string host = Dns.GetHostName(); // Записывается в переменную host, получаем имя хоста локального компьютера.
            return host;
        }

        internal string LocalIpAdress()
        {
            IPAddress[] addresses = Dns.GetHostAddresses(LocalName()); // Dns.GetHostAddresses - возвращает IP-адреса для указанного узла

            StringBuilder ipAddresses = new StringBuilder();
            string ipAddressInfo = ""; // ??/?
            foreach (IPAddress address in addresses)
            {
                CalculatingAllIpAddresses addressNetwork = new CalculatingAllIpAddresses();
                if (!IPAddress.IsLoopback(address))
                {
                    ipAddressInfo = addressNetwork.interfaceIpAdress(address);
                    ipAddresses.AppendLine(ipAddressInfo);
                }
            }
            return ipAddressInfo;
        }
    }

    // class AN описывает вычисления айпи адресов и масок, выводом на экран
    class CalculatingAllIpAddresses
    {
        internal string interfaceIpAdress(IPAddress address)
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces(); // ,GetAllNetworkInterfaces - возвращает интерфейсы

            foreach (NetworkInterface @interface in interfaces) //Перебор интерфейсов
            {
                if (@interface.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue; //проверка типа сетевого интерфейса

                IPInterfaceProperties ipProps = @interface.GetIPProperties(); //Получение свойств IP-адресов интерфейса
                UnicastIPAddressInformationCollection UnicastIPA = ipProps.UnicastAddresses;

                IpAddressesMask(UnicastIPA, address);
            }
            return address.ToString();
        }

        internal void IpAddressesMask(UnicastIPAddressInformationCollection UnicastIPA, IPAddress address)
        {
            foreach (UnicastIPAddressInformation unicastIPInfo in UnicastIPA) // перебираем все уникальные IP-адреса
            {
                if (unicastIPInfo.Address.Equals(address))
                {
                    byte[] subnetMaskBytes = unicastIPInfo.IPv4Mask.GetAddressBytes(); //GetAddressBytes() представляет наш IPAddress в виде массива байтов
                    byte[] addressBytes = address.GetAddressBytes(); //GetAddressBytes() представляет наши маски подсети для текущего IP-адреса в виде массива байтов
                    byte[] networkAddressBytes = new byte[4]; // массив для хранения байтов сетевого адреса
                    for (int i = 0; i < 4; i++)
                    {
                        networkAddressBytes[i] = (byte)(addressBytes[i] & subnetMaskBytes[i]); // логическое И (AND) между байтами IP-адреса и байтами маски подсети
                    }

                    IPAddress networkAddress = new IPAddress(networkAddressBytes); // создаем новый IP-адрес из полученных байтов сетевого адреса.

                    if (IsPrivateIP(address))
                        Console.WriteLine($"\n{address} ; Маска сети: {new IPAddress(subnetMaskBytes)} ; Адрес сети: {networkAddress}");
                    else
                    {
                        Console.WriteLine($"\n{address} ; Маска сети: {new IPAddress(subnetMaskBytes)} ; Адрес сети: {networkAddress}");
                    }
                }
            }
            ScanningOpenClosedPorts scanner = new ScanningOpenClosedPorts();
            scanner.PortScan(address); // переход к нахождению и отображения порта в консоле
        }

        private bool IsPrivateIP(IPAddress address)
        {
            byte[] bytes = address.GetAddressBytes(); // Создаём одномерный массив
            int firstOctet = BitConverter.ToInt32(bytes, 0); // Конвертируем типы: (байты) в (инт)
            if (firstOctet == 10) return true;
            if (firstOctet == 172 && firstOctet >= 16 && firstOctet <= 31) return true;
            if (firstOctet == 192 && firstOctet == 168) return true;
            return false;
        }
    }
}
class ScanningOpenClosedPorts
{
    public void PortScan(IPAddress ipAddress, int startPort = 80, int endPort = 8000)
    {
        List<int> listOpenPort = new List<int>();
        List<int> listClosedPort = new List<int>();
        for (int port = startPort; port <= endPort; port++)
        {
            if (IsPortOpen(ipAddress, port))
                listOpenPort.Add(port);
            else if (port == 80 || port == 554 || port == 8000)
                listClosedPort.Add(port);
        }
        foreach (var openItemPort in listOpenPort)
        {
            Console.WriteLine($"{openItemPort} открытый порт");
        }
        foreach (var closedItemPort in listClosedPort)
        {
            Console.WriteLine($"{closedItemPort} закрытый порт");
        }
    }

    private bool IsPortOpen(IPAddress address, int port)
    {
        using (TcpClient tcpClient = new TcpClient()) //для подключения, отправки и получения потоковых данных по сети в синхронном режиме блокировки
        {
            try //Обработка исключений конструкция try...catch...
            {
                tcpClient.Connect(address, port); //Connect(IPAddress, Int32) Подключает клиента к удаленному TCP-узлу, используя указанный IP-адрес и номер порта
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }
}
