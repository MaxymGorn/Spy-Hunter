using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp15.Model
{
    public class User
    {
        public User()
        {

        }
        [Key]
        public int id { get; set; }
        public string email { get; set; }
        public virtual List<Files> Files { get; set; }
        public virtual List<LogText> LogTexts { get; set; }
        public virtual List<InfoPc> InfoPcs { get; set; }
    }
}
