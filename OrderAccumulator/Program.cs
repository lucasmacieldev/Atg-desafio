using QuickFix;

namespace OrderAccumulator
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string file = "executor.cfg";
            try
            {
                 var wallets = new List<Wallet>();
                 SessionSettings settings = new SessionSettings(file);
                IApplication app = new OrderAccumulator(wallets);
                IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
                ILogFactory logFactory = new FileLogFactory(settings);
                IAcceptor acceptor = new ThreadedSocketAcceptor(app, storeFactory, settings, logFactory);

                acceptor.Start();
                Console.WriteLine("press <enter> to quit");
                Console.Read();
                acceptor.Stop();
            }
            catch (System.Exception e)
            {
                Console.WriteLine("==FATAL ERROR==");
                Console.WriteLine(e.ToString());
            }
        }
    }
}
