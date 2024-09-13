using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
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
                        Console.WriteLine($"\n{address} ; Маска сети: {new IPAddress(subnetMaskBytes)} ; Адрес сети: {networkAddress}");
                    else {
                        Console.WriteLine($"\n{address} ; Маска сети: {new IPAddress(subnetMaskBytes)} ; Адрес сети: {networkAddress}");
                    }
                    
                    ScanByPort scanner = new ScanByPort();
                    scanner.GetScanByPort(address);
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

    class ScanByPort
    {
        public void GetScanByPort( IPAddress ipAddress, int startPort = 79, int endPort = 50000) //Не обязательные порты, но можно вручную настроить проверку
    {
        List<int> listOpenPort = new List<int>();
        for (int port = startPort; port <= endPort; port++) //цикл с начальной точки до конечной заданной у меня 79 и 50000
        {
            if (IsPortOpen(ipAddress, port)) //проверка функцией
            {
                if(port != port)
                {
                    listOpenPort.Add(port);
                }
                foreach (var itemPort in listOpenPort)
                {
                    Console.WriteLine($"{itemPort} открытый порт");
                }
            }
            else if(port == 80 || port == 554 || port == 8000 || port == 8200)
            {
                // можно вписать для вывода закрытых портов
            }
        }
        Console.WriteLine("Вывести списки? Y/N");
        string listOut = null;
        listOut = Console.ReadLine();
        if(listOut == "Y")
        {
            foreach (var itemPort in listOpenPort)
            {
                Console.WriteLine($"{itemPort} открытый порт");
            }
        }
        else
        {
        
        }
    }

    private bool IsPortOpen(IPAddress address, int port)
    {
        using (TcpClient tcpClient = new TcpClient()) //для подключения, отправки и получения потоковых данных по сети в синхронном режиме блокировки
        {
            try //Обработка исключений конструкция try...catch...finally
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
    
}
