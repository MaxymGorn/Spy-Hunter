using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp15.Model
{
    public class InfoPc
    {
        public InfoPc()
        {

        }
        [Key]
        public int id { get; set; }
        public int UserId { get; set; }
        public string ComputerName { get; set; }
        public string ComputerCpu { get; set; }
        public string ComputerGpu { get; set; }
        public string ComputerRamAmount_MB { get; set; }
        public string ComputerAntivirus { get; set; }
        public string ComputerOs { get; set; }
        public string Country { get; set; }
        public string RegionName { get; set; }
        public string City { get; set; }
        public virtual User Users { get; set; }
    }
}
