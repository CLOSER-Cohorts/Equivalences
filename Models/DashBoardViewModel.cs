using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ColecticaSdkMvc.Models
{
    public class DashBoardViewModel
    {
        public int Id { get; set; }
        public decimal TotalSales { get; set; }
        public string Url { get; set; }
        public string MyDate { get; set; }
    }
}