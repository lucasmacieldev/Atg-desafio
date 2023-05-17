using QuickFix.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderAccumulator
{
    public class Wallet
    {
        public Wallet(string symbol, decimal amount)
        {
            Symbol = symbol;
            Amount = amount;
        }

        public string Symbol { get; set; }
        public decimal Amount { get; set; }
    }
}
