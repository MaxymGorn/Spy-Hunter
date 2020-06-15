using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp15.Model
{
    public class LogText
    {
        public LogText()
        {

        }
        [Key]
        public int id { get; set; }
        public string Desciption { get; set; }
        public int UserId { get; set; }
        public virtual User Users { get; set; }
    }
}