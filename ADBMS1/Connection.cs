using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADBMS1
{
    // Change 'internal' to 'public static'
    public static class Connection
    {
        // This is the variable your other forms will call
        public static string stringConn = @"Data Source=DESKTOP-46BKLDC\SQLEXPRESS;Initial Catalog=CafeManagementDB;Integrated Security=True";
    }
}