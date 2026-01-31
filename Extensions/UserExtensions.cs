using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminUP.Models
{
    public partial class User
    {
        public string FullName => $"{last_name} {first_name} {middle_name}".Trim();
    }
}
