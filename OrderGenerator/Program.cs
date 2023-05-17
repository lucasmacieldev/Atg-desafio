namespace OrderGenerator
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string file = "tradeclient.cfg";

            try
            {
                QuickFix.SessionSettings settings = new QuickFix.SessionSettings(file);
                OrderGenerator application = new OrderGenerator();
                QuickFix.IMessageStoreFactory storeFactory = new QuickFix.FileStoreFactory(settings);
                QuickFix.ILogFactory logFactory = new QuickFix.ScreenLogFactory(settings);
                QuickFix.Transport.SocketInitiator initiator = new QuickFix.Transport.SocketInitiator(application, storeFactory, settings, logFactory);
                application.MyInitiator = initiator;

                initiator.Start();
                application.Run();
                initiator.Stop();
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            Environment.Exit(1);
        }
    }
}
