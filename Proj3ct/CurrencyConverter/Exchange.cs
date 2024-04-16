using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace CurrencyConverter
{
     class Exchange
    {
        [Key]
        public string Text { get; set; }
        public double Value { get; set; }
    }
}
