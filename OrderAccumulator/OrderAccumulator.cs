using QuickFix;
using QuickFix.Fields;

namespace OrderAccumulator
{
    public class OrderAccumulator : QuickFix.MessageCracker, QuickFix.IApplication
    {
        private List<Wallet> _wallets;

        public OrderAccumulator(List<Wallet> wallets)
        {
            _wallets = wallets;
        }

        public void OnMessage(QuickFix.FIX44.NewOrderSingle ord, SessionID sessionID)
        {
            var price = ord.Price;
            var qntd = ord.OrderQty;
            var symbol = ord.Symbol;
            var side = ord.Side;
            var amount = price.getValue() * qntd.getValue();

            if (side.Equals(Side.BUY))
            {
                if (_wallets.Any(x => x.Symbol == symbol.getValue()))
                {
                    var wallet = _wallets.FirstOrDefault(x => x.Symbol == symbol.getValue());

                    var baseCalculed = wallet.Amount + amount;
                    if (baseCalculed < decimal.Parse("1000000"))
                    {
                        wallet.Amount += amount;
                        OnMessage(new QuickFix.FIX44.ExecutionReport(), sessionID);
                    }
                    else
                        OnMessage(new QuickFix.FIX44.OrderCancelReject(), sessionID);
                    
                }
                else
                {
                    var addWallet = new Wallet(symbol.getValue(), amount);
                    _wallets.Add(addWallet);
                }
            }
            else
            {
                if (_wallets.Any(x => x.Symbol == symbol.getValue()))
                {
                    var wallet = _wallets.FirstOrDefault(x => x.Symbol == symbol.getValue());

                    var baseCalculed = wallet.Amount - amount;
                    if (baseCalculed < decimal.Parse("1000000"))
                    {
                        wallet.Amount -= amount;
                        OnMessage(new QuickFix.FIX44.ExecutionReport(), sessionID);
                    }
                    else
                        OnMessage(new QuickFix.FIX44.OrderCancelReject(), sessionID);
                }
                else
                {
                    var addWallet = new Wallet(symbol.getValue(), amount);
                    _wallets.Add(addWallet);
                }
            }

            foreach (var item in _wallets)
                Console.WriteLine($"Symbol: {item.Symbol} - Amount {item.Amount}");
            
        }

        public void OnMessage(QuickFix.FIX44.ExecutionReport m, SessionID s)
        {
            Console.WriteLine("======= VALOR BASE ACEITO =======");
        }

        public void OnMessage(QuickFix.FIX44.OrderCancelReject m, SessionID s)
        {
            Console.WriteLine("======= VALOR BASE RECUSADO =======");
        }

        #region QuickFix.Application Methods

        public void FromApp(Message message, SessionID sessionID)
        {
            Crack(message, sessionID);
            Console.WriteLine("IN:  " + message);
        }

        public void ToApp(Message message, SessionID sessionID)
        {
            Crack(message, sessionID);
            Console.WriteLine("OUT: " + message);
        }

        public void FromAdmin(Message message, SessionID sessionID)
        {
            Console.WriteLine("IN:  " + message.ToString());
            try
            {
                Crack(message, sessionID);
            }
            catch (Exception ex)
            {
                Console.WriteLine("==Cracker exception==");
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void ToAdmin(Message message, SessionID sessionID)
        {
            Console.WriteLine("OUT:  " + message);
            try
            {
                Crack(message, sessionID);
            }
            catch (Exception ex)
            {
                Console.WriteLine("==Cracker exception==");
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void OnCreate(SessionID sessionID) { }
        public void OnLogout(SessionID sessionID) { }
        public void OnLogon(SessionID sessionID) { }
        #endregion
    }
}