using System;
using QuickFix;
using QuickFix.Fields;
using System.Collections.Generic;

namespace OrderGenerator
{
    public class OrderGenerator : QuickFix.MessageCracker, QuickFix.IApplication
    {
        Session _session = null;

        public IInitiator MyInitiator = null;

        #region IApplication interface overrides

        public void OnCreate(SessionID sessionID)
        {
            _session = Session.LookupSession(sessionID);
        }

        public void OnLogon(SessionID sessionID) { Console.WriteLine("Logon - " + sessionID.ToString()); }
        public void OnLogout(SessionID sessionID) { Console.WriteLine("Logout - " + sessionID.ToString()); }
        public void FromAdmin(Message message, SessionID sessionID) { }
        public void ToAdmin(Message message, SessionID sessionID) { }
        public void FromApp(Message message, SessionID sessionID)
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
        public void ToApp(Message message, SessionID sessionID)
        {
            try
            {
                bool possDupFlag = false;
                if (message.Header.IsSetField(QuickFix.Fields.Tags.PossDupFlag))
                {
                    possDupFlag = QuickFix.Fields.Converters.BoolConverter.Convert(
                        message.Header.GetString(QuickFix.Fields.Tags.PossDupFlag)); /// FIXME
                }
                if (possDupFlag)
                    throw new DoNotSend();
            }
            catch (FieldNotFoundException)
            { }

            Console.WriteLine();
            Console.WriteLine("OUT: " + message.ToString());
        }
        #endregion


        #region MessageCracker handlers
        public void OnMessage(QuickFix.FIX44.ExecutionReport m, SessionID s)
        {
            Console.WriteLine("Received execution report");
        }

        public void OnMessage(QuickFix.FIX44.OrderCancelReject m, SessionID s)
        {
            Console.WriteLine("Received order cancel reject");
        }


        #endregion
        public void Run()
        {
            while (true)
            {
                try
                {
                    QueryEnterOrder();
                    System.Threading.Thread.Sleep(1000);
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("Message Not Sent: " + e.Message);
                    Console.WriteLine("StackTrace: " + e.StackTrace);
                }
            }
            Console.WriteLine("Program shutdown.");
        }

        private void SendMessage(Message m)
        {
            if (_session != null)
                _session.Send(m);
            else
            {
                Console.WriteLine("Can't send message: session not created.");
            }
        }

        private void QueryEnterOrder()
        {
            Console.WriteLine("\nNewOrderSingle");

            QuickFix.FIX44.NewOrderSingle m = QueryNewOrderSingle44();

            m.Header.GetString(Tags.BeginString);
            SendMessage(m);
        }

        #region Message creation functions
        private QuickFix.FIX44.NewOrderSingle QueryNewOrderSingle44()
        {
            var getSymblolRandom = symbolRandom();
            QuickFix.Fields.OrdType ordType = null;
            Random randNum = new Random();

            QuickFix.FIX44.NewOrderSingle newOrderSingle = new QuickFix.FIX44.NewOrderSingle(
                new ClOrdID("1234"),
                new Symbol(getSymblolRandom.Trim()),
                new QuickFix.Fields.Side(randNum.Next(2) == 1 ? Side.BUY : Side.SELL),
                new TransactTime(DateTime.Now),
                new OrdType(OrdType.LIMIT));

            newOrderSingle.Set(new HandlInst('1'));
            newOrderSingle.Set(new OrderQty(Convert.ToDecimal(randNum.Next(0, 99999))));
            newOrderSingle.Set(new Price(multipleOf()));
            newOrderSingle.Set(new TimeInForce(TimeInForce.DAY));

            if (ordType != null)
            {
                if (ordType.getValue() == OrdType.LIMIT || ordType.getValue() == OrdType.STOP_LIMIT)
                    newOrderSingle.Set(QueryPrice());
                if (ordType.getValue() == OrdType.STOP || ordType.getValue() == OrdType.STOP_LIMIT)
                    newOrderSingle.Set(QueryStopPx());
            }

            return newOrderSingle;
        }

        public string symbolRandom()
        {
            Random randNum = new Random();
            var intRandom = randNum.Next(3);
            if (intRandom == 1)
                return "PETR4";
            else if (intRandom == 2)
                return "VALE3";
            else
                return "VIIA4";
        }

        public decimal multipleOf()
        {
            Random randNum = new Random();
            var valueRandom = randNum.Next(0, 999);

            while (valueRandom % 0.01 == 0)
            {
                return valueRandom;
                valueRandom += randNum.Next(0, 999);
            }

            return valueRandom;
        }


        #endregion

        #region field query private methods
        private Price QueryPrice()
        {
            Console.WriteLine();
            Console.Write("Price? ");
            return new Price(Convert.ToDecimal(Console.ReadLine().Trim()));
        }

        private StopPx QueryStopPx()
        {
            Console.WriteLine();
            Console.Write("StopPx? ");
            return new StopPx(Convert.ToDecimal(Console.ReadLine().Trim()));
        }

        #endregion
    }
}
