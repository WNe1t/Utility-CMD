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

        public string LocalIpAdress()
        {
            IPAddress[] addresses = Dns.GetHostAddresses(LocalName()); // Dns.GetHostAddresses - возвращает IP-адреса для указанного узла

            StringBuilder ipAddresses = new StringBuilder();

            foreach (IPAddress address in addresses)
            {
                if (!IPAddress.IsLoopback(address))
                {
                    AddressNetwork addressNetwork = new AddressNetwork();
                    string ipAddressInfo = addressNetwork.ProcessIpAddress(address);
                    ipAddresses.AppendLine(ipAddressInfo);
                }
            }
            return ipAddresses.ToString(); //возвращает строковый айпиадресс
        }
    }

    class AddressNetwork
    {
        public string ProcessIpAddress(IPAddress address)
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface @interface in interfaces) //Перебор интерфейсов
            {
                if (@interface.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue; //проверка типа сетевого интерфейса

                IPInterfaceProperties ipProps = @interface.GetIPProperties(); //Получение свойств IP-адресов интерфейса
                UnicastIPAddressInformationCollection UnicastIPA = ipProps.UnicastAddresses;
                IpAddressesMask(UnicastIPA, address);
            }
            return null;
        }

        public void IpAddressesMask(UnicastIPAddressInformationCollection UnicastIPA, IPAddress address)
        {
            foreach (UnicastIPAddressInformation unicastIPInfo in UnicastIPA) // перебираем все уникальные IP-адреса
            {
                if (unicastIPInfo.Address.Equals(address))
                {
                    byte[] subnetMaskBytes = unicastIPInfo.IPv4Mask.GetAddressBytes();
                    byte[] addressBytes = address.GetAddressBytes(); // получаем байты маски подсети для текущего IP-адреса
                    int subnetMask = BitConverter.ToInt32(subnetMaskBytes, 0); // конвертируем байты в int32 типа (инт)

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

                    ScanPortNetwork scanner = new ScanPortNetwork();
                    scanner.ScanPort(address); // переход к нахождению и отображения порта в консоле
                }
            }
        }

        private bool IsPrivateIP(IPAddress address) //Проверка если вернёт true, выполнится конструкция if
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
class ScanPortNetwork
{
    public void ScanPort(IPAddress ipAddress, int startPort = 80, int endPort = 8080) //Не обязательные порты, но можно вручную настроить проверку
    {
        List<int> listOpenPort = new List<int>();
        for (int port = startPort; port <= endPort; port++) //цикл с начальной точки до конечной заданной у меня 79 и 50000
        {
            if (IsPortOpen(ipAddress, port)) //проверка функцией
            {
                listOpenPort.Add(port);
                foreach (var itemPort in listOpenPort)
                {
                    Console.WriteLine($"{itemPort} открытый порт");
                }
            }
            // else if (port == 80 || port == 554 || port == 8000 || port == 8200)
            // {
            //     // можно вписать для вывода закрытых портов
            // }
        }
    }

    private bool IsPortOpen(IPAddress address, int port)
    {
        using (TcpClient tcpClient = new TcpClient()) //для подключения, отправки и получения потоковых данных по сети в синхронном режиме блокировки
        {
            try //Обработка исключений конструкция try...catch...finally в друг связь оборвётся
            {
                tcpClient.Connect(address, port); //Connect(IPAddress, Int32) Подключает клиента к удаленному TCP-узлу, используя указанный IP-адрес и номер порта.
                return true;
            }
            catch (SocketException) //Конструктор без параметров для свойства ошибки ОС
            {
                return false;
            }
        }
    }
}
