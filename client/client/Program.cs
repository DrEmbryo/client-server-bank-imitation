using System;
using System.Net.Sockets;
using System.Text;

namespace client
{
    class Program
    {
        const int port = 8888;
        const string address = "127.0.0.1";
        static void Main()
        {
            TcpClient client = null;
            try
            {
                client = new TcpClient(address, port);
                NetworkStream stream = client.GetStream();

                while (true)
                {
                    // ввод сообщения
                    // UserID; Password; AccountID; opeation; value;
                    string message;

                    Console.WriteLine("UserID: ");
                    string UserID = Console.ReadLine();

                    Console.WriteLine("Password: ");
                    string Password = Console.ReadLine();

                    Console.WriteLine("AccountID: ");
                    string AccountID = Console.ReadLine();

                    Console.WriteLine("opeation: ");
                    string opeation = Console.ReadLine();

                    Console.WriteLine("value: ");
                    string value = Console.ReadLine();

                    message = $"{UserID};{Password};{AccountID};{opeation};{value}";
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    stream.Write(data, 0, data.Length);

                    // получаем ответ
                    data = new byte[1024];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);
                    message = builder.ToString();
                    Console.WriteLine($"message: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                client.Close();
            }
        }
    }
}
