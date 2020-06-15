using Google.Apis.Drive.v3.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp15.Model
{
    public class Files
    {
        public Files(Files files)
        {
            this.DateTime = files.DateTime;
            this.id = files.id;
            this.path = files.path;
            this.UserId = files.UserId;
        }
        public Files()
        {

        }
        [Key]
        public int id { get; set; }
        public string path { get; set; }
        public DateTime DateTime { get; set; }
        public int UserId { get; set; }
        public virtual User Users { get; set; }
    }
}
