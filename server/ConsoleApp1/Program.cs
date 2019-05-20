using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace ConsoleApp1
{
    class Bank {
        private List<AccountUser> Accounts = new List<AccountUser>();
        public void AddAccount(AccountUser Account) {
            Accounts.Add(Account);
        }

        public void RemoveAccount(string ID)
        {
            Accounts.Remove(FindUserByID(ID));
        }
        public AccountUser FindUserByID(string ID) {
            AccountUser searchRes = Accounts.Find(acc => acc.UserID == ID);
            return searchRes;
        }
    }

    class AccountUser {
        private string UserPassword { get; set; }
        public string UserID;
        private List<BankAccount> Accounts = new List<BankAccount>();

        public BankAccount GetAccountData (string Password, string AccountID) {
            if (PassCheck(Password))
            {  
                return FindAccountByID(AccountID);
            } else return null;
        }

        public BankAccount FindAccountByID(string ID)
        {
            BankAccount searchRes = Accounts.Find(acc => acc.BankAccountID == ID);
            return searchRes;
        }

        public Boolean PassCheck(string Password) {
            if (UserPassword == Password)
                return true;
            else return false;
        }

        public void AddAccount(BankAccount Account)
        {
            Accounts.Add(Account);
        }

        public void RemoveAccount(string AccountID)
        {
            Accounts.Remove(FindAccountByID(AccountID));
        }

        public AccountUser(string UserID , string UserPassword) {
            this.UserID = UserID;
            this.UserPassword = UserPassword;
        }
    }
    class BankAccount {
        public string BankAccountID ;
        private double BankAccountBalance { get; set; }
        public double Balance(string operation, double value = 0) {
            switch (operation)
            {
                case "inc":
                    BankAccountBalance += value;
                    return 1;
                case "dec":
                    BankAccountBalance -= value;
                    return 1;
                case "show":
                    return BankAccountBalance;
                default:
                    return 0;
            }
        }
        public BankAccount(string ID, double AccountBalance) {
            BankAccountID = ID;
            BankAccountBalance = AccountBalance;
        }
    }


    class ClientObject
    {
        public TcpClient client;
        public Bank bank;
        public ClientObject(TcpClient tcpClient, Bank local_bank)
        {
            client = tcpClient;
            bank = local_bank;
        }

        public string GetMessage() {
            try
            {
                var stream = client.GetStream();
                byte[] data = new byte[1024]; 
                while (true)
                {
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);
                    string message = builder.ToString();
                    return message;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string SendMessage(string message) {
            try
            {
                var stream = client.GetStream();
                byte[] data = new byte[1024];
                    data = Encoding.Unicode.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                Console.WriteLine(message);
                return null;
            }
            catch (Exception ex)
            {
                 return ex.Message;
            }
        }

        public void Process()
        {
        NetworkStream stream = null;
            try
            {
                object locker = new object();
                string req = GetMessage();
                string[] SplitReq = req.Split(';');
                // UserID; Password; AccountID; opeation; value;
                string message = "test message";
                if (bank.FindUserByID(SplitReq[0]) != null)
                {
                    var User = bank.FindUserByID(SplitReq[0]);
                    if (User.GetAccountData(SplitReq[1], SplitReq[2]) != null)
                    {
                        Monitor.Enter(locker);
                        var Account = User.GetAccountData(SplitReq[1], SplitReq[2]);
                        message = Account.Balance(SplitReq[3], Convert.ToDouble(SplitReq[4])).ToString();
                        Console.WriteLine(message);
                        Thread.Sleep(1000);
                        Monitor.Exit(locker);
                        Console.WriteLine(SendMessage(message));
                    }
                    else {
                        message = "Invalid userID or bankAccountID";
                    };
                }
                else
                {
                    message = "No user with this ID";
                }
                SendMessage(message);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
            }
        }
    }


    class Program
    {
        const int port = 8888;
        static TcpListener listener;
        static void Main()
        {
            Bank bank = new Bank();
            AccountUser user1 = new AccountUser("1","test");
            AccountUser user2 = new AccountUser("2", "test");
            AccountUser user3 = new AccountUser("3", "test");
            BankAccount Account_1_1 = new BankAccount("1_1", 242);
            BankAccount Account_1_2 = new BankAccount("1_2", 570.31);
            user1.AddAccount(Account_1_1);
            user1.AddAccount(Account_1_2);
            bank.AddAccount(user1);
            bank.AddAccount(user2);
            bank.AddAccount(user3);

            try
            {
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
                listener.Start();
                Console.WriteLine("Ожидание подключений...");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    ClientObject clientObject = new ClientObject(client,bank);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }
    }
}
