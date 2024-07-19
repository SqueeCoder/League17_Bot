using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.System;

namespace WinUI_3.Models
{
    public class User
    {
        public List<UserStatus> Statuses { get; set; }
        public User()
        {
            Statuses = new List<UserStatus>();
        }
        public void Initialize()
        {
        }
        public void StartHunt()
        {
        }
    }

    public enum UserStatus
    { 
        Idle,
        Hunting
    }
}