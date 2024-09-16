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
            while(true){

            
            Console.Write("Введите команду: ");
            string command = Console.ReadLine();
            NameLocalHost localHost = new NameLocalHost(); // Даём область памяти на наш класс
            localHost.GetLocalName(command); // Выводим Имя локального хоста
            ApiMaskAddress apiMaskAddress = new ApiMaskAddress();
            apiMaskAddress.LocalIpAdress(command); // Выводим Маску сети и Адрес сети
            AddressNetwork addressNetwork = new AddressNetwork();
            ScanByPort scanByPort = new ScanByPort();
            if(command == "exit")
            {
                break;
            }
            }

        }
    }


    class NameLocalHost
    {
        public void GetLocalName(string command)
        {
            if (command == "localname")
            {
            string host = Dns.GetHostName(); // Записывается в переменную host, получаем имя хоста локального компьютера.
            Console.WriteLine(host);
            }
        }

        // public void HelpCommand()
        // {
            
        //     foreach(string item in )
        //     {

        //     }
        // }
    }

    class ApiMaskAddress : NameLocalHost
    {
        public void LocalIpAdress(string command)
        {
            if(command == "ipadress")
            {
            string host = Dns.GetHostName(); // Записывается в переменную host, получаем имя хоста локального компьютера.
            IPAddress[] addresses = Dns.GetHostAddresses(host); // Dns.GetHostAddresses - возвращает IP-адреса для указанного узла

            foreach (IPAddress address in addresses)
            {
                if (!IPAddress.IsLoopback(address))
                {
                    AddressNetwork addressNetwork = new AddressNetwork();
                    addressNetwork.ProcessIpAddress(address);
                }
            }
            }
        }
    }

    class AddressNetwork
    {
        public void ProcessIpAddress(IPAddress address)
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces(); //получение всех сетевых интерфейсов

            foreach (NetworkInterface @interface in interfaces) //Перебор интерфейсов
            {
                if (@interface.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue; //проверка типа сетевого интерфейса

                IPInterfaceProperties ipProps = @interface.GetIPProperties(); //Получение свойств IP-адресов интерфейса
                UnicastIPAddressInformationCollection UnicastIPA = ipProps.UnicastAddresses;
                IpAddressesMask(UnicastIPA, address); //
            }
        }

        private void IpAddressesMask(UnicastIPAddressInformationCollection UnicastIPA, IPAddress address)
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

                    IPAddress networkAddress = new IPAddress(networkAddressBytes);  // создаем новый IP-адрес из полученных байтов сетевого адреса.

                    if (IsPrivateIP(address))
                        Console.WriteLine($"\n{address} ; Маска сети: {new IPAddress(subnetMaskBytes)} ; Адрес сети: {networkAddress}");
                    else
                        Console.WriteLine($"\n{address} ; Маска сети: {new IPAddress(subnetMaskBytes)} ; Адрес сети: {networkAddress}");

                    ScanByPort scanner = new ScanByPort();
                    scanner.GetScanByPort(address); // переход к нахождению и отображения порта в консоле
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

    class ScanByPort
    {
        public void GetScanByPort(IPAddress ipAddress,string command = null, int startPort = 80, int endPort = 8080) //Не обязательные порты, но можно вручную настроить проверку
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
                    else if (port == 80 || port == 554 || port == 8000 || port == 8200)
                    {
                        // можно вписать для вывода закрытых портов
                    }
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
}
