using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinUI_3.Models
{
    public static class OptionsInitializer
    {
        public static Dictionary<string, string> Initialize()
        {
            var options = new Dictionary<string, string>();
            options.Add("optionsAttackNumberCombobox", "0");
            options.Add("catch", "false");
            options.Add("battlesBeforeHeal", "1");
            return options;
        }
    }
}
