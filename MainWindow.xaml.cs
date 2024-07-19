using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using WinUI_3.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUI_3
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public User User { get; set; } = new User();
        
        public Dictionary<string, string> Options { get; set; }
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        //private StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        private int battleWithoutHeal = 0;
        public MainWindow()
        {
            this.InitializeComponent();
            User.Initialize();

            string serializedOptions  =null;
            try
            {
                serializedOptions = localSettings.Values["options"].ToString();
            }
            catch (Exception ex) { };

            if ( serializedOptions==null)
            {
                Options = OptionsInitializer.Initialize();
            }
            else
            {
               Options = JsonSerializer.Deserialize<Dictionary<string, string>>(serializedOptions);
            }
            //remove on production
           // Options = OptionsInitializer.Initialize();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await MyWebView.ExecuteScriptAsync("document.getElementById('txtLogin').value = 'SqueeCoder'");
                await MyWebView.ExecuteScriptAsync("document.getElementById('txtPass').value = 'Test1234'");
                await MyWebView.ExecuteScriptAsync("document.getElementById('btnSignin').click();");
            } 
            catch (Exception exception) {
                Console.WriteLine($"login error");
            };
            try
            {
                await MyWebView.ExecuteScriptAsync("document.getElementById('aToGame').click();");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"aToGame error");
            };
            try
            {
                await MyWebView.ExecuteScriptAsync("document.getElementsByClassName('button btnStart withtext')[0].click();");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"btnStart error");
            };

        }
        private async void GoToLocationButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
           await TeleportationAsync();
            if (button.Name == "btnGo1") return;
            switch (button.Name)
            {
                case "btnGo146":
                    await JumpToAsync("btnGo6");
                    await JumpToAsync("btnGo146");
                    break;
                    
            }

           
            async Task JumpToAsync(string location)
            {
                await Task.Delay(500);
                await MyWebView.ExecuteScriptAsync($"document.getElementById('{location}').click();");
            }
                       async Task TeleportationAsync()
           {
                string answer = (await MyWebView.ExecuteScriptAsync("document.querySelector('#divLocTitleText').innerText")).Trim('"');
                if (answer != "Площадь Голденрода")
                {
                    await MyWebView.ExecuteScriptAsync("" +
                    "document.querySelector('#divOnlineUser > div > div.emblem').click();" +
                    "document.evaluate(`//node()[normalize-space(text())='${'Tелепортация'.trim()}']`, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.click();");
                }
           }         
            
        }
        private async void GoButton_Click(object sender, RoutedEventArgs e)
        {
            //await MyWebView.ExecuteScriptAsync($"alert({answer.Length})");
           
        }

        private async void ActionButton_Click(object sender, RoutedEventArgs e)
        {

            ButtonBase button = (ButtonBase)sender;
            if(button.Name == "huntButton")
            {
                ToggleButton toggleButton = (ToggleButton)button;
                if (toggleButton.IsChecked==true)
                {
                    try { User.Statuses.Add(UserStatus.Hunting); } catch (Exception ex) { };
                    await MyWebView.ExecuteScriptAsync("" +
                    "if(document.getElementsByClassName('button btnSwitchWilds withtext pressed').length ==0){" +
                    "document.getElementsByClassName('button btnSwitchWilds withtext')[0].click()" +
                    "}");
                    HuntAsync(User);
                }
                else
                {
                    //await MyWebView.ExecuteScriptAsync("alert(1)");
                    try { User.Statuses.Remove(UserStatus.Hunting); } catch (Exception ex) { } ;
                    await MyWebView.ExecuteScriptAsync("" +
                        "if(document.getElementsByClassName('button btnSwitchWilds withtext pressed').length ==1){" +
                        "document.getElementsByClassName('button btnSwitchWilds withtext')[0].click()" +
                        "}");
                }

            }
        }
        private async void HuntAsync(User user)
        {
            string answer = "";
            int randomDelay;
            int battleBeforeHeal;
            Int32.TryParse(Options["battlesBeforeHeal"], out battleBeforeHeal);
            if (battlesBeforeHeal == null)
                Options.Add("battlesBeforeHeal", "1");
            while (true)
            {
                if(!user.Statuses.Contains(UserStatus.Hunting)) return;
                //await MyWebView.ExecuteScriptAsync("document.getElementsByClassName('divMoveInfo clickable')[1].click();");
                Random rnd = new Random();
                randomDelay = (int)rnd.Next(3,10);
                await Task.Delay(randomDelay*1000);
                answer = await MyWebView.ExecuteScriptAsync("document.getElementById('divFightAction').innerHTML;");
                if (answer == "\"Ваш ход\"")
                {
                    EnemyMonster enemyMonster = await GatherEnemyMonsterInfoAsync();
                    await CatchEnemyMonsterAsync(enemyMonster);
                    while (answer == "\"Ваш ход\"")
                    {
                        //await MyWebView.ExecuteScriptAsync($"alert({answer})");
                        await MyWebView.ExecuteScriptAsync($"document.getElementsByClassName('divMoveInfo clickable')[{Options["optionsAttackNumberCombobox"]}].click();");
                        await Task.Delay(1500);
                        answer = await MyWebView.ExecuteScriptAsync("document.getElementById('divFightAction').innerHTML;");
                    }
                    await Task.Delay(1000);

                    Int32.TryParse(Options["battlesBeforeHeal"],out battleBeforeHeal);
                    battleWithoutHeal++;
                    if (battleWithoutHeal >= battleBeforeHeal)
                    {
                        battleWithoutHeal = 0;
                    await MyWebView.ExecuteScriptAsync("document.getElementsByClassName('button pc btnLocHeal withtext')[0].click()");
                    await Task.Delay(1000);
                    await MyWebView.ExecuteScriptAsync("document.getElementsByClassName('menuHealAll')[0].click()");
                    }
                }
            }
        }


        private async void OptionsSaveButton_Click(object sender, RoutedEventArgs e)
        {
            string serializedOptions =JsonSerializer.Serialize(Options);
            localSettings.Values["options"] = serializedOptions;
            //   await MyWebView.ExecuteScriptAsync($"alert({Options["optionsAttackNumberCombobox"]})");
        }

        private void CatchButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            if (toggleButton.IsChecked==true)
            {
                Options["catch"] = "true";
            }
            else
            {
                Options["catch"] = "false";
            }
        }
        private async Task CatchEnemyMonsterAsync(EnemyMonster enemyMonster)
        {
            if (Options["catch"] == "false") return;
            if (true)//enemyMonster.Name == "Мотль")
            {
                //make hp low
                string answer = "";
                do
                {
                    await MyWebView.ExecuteScriptAsync($"document.getElementsByClassName('divMoveInfo clickable')[{0}].click();");
                    await Task.Delay(1500);
                    answer = await MyWebView.ExecuteScriptAsync("document.getElementsByClassName('progressbar barHP min').length");
                } while (answer != "1");

                answer = await MyWebView.ExecuteScriptAsync("document.getElementById('divFightAction').innerHTML;");
                while (answer == "\"Ваш ход\"")
                {
                    await MyWebView.ExecuteScriptAsync("document.evaluate(`//node()[normalize-space(text())='${'Использовать предмет...'.trim()}']`, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.click();");
                    await Task.Delay(1000);
                    await MyWebView.ExecuteScriptAsync("document.querySelector(\"img[src$='//img.league17.ru/pub/balls/tiny/15.png']\").parentElement.click();");
                    await Task.Delay(1000);
                    await MyWebView.ExecuteScriptAsync("document.evaluate(`//node()[normalize-space(text())='${'Использовать в битве'.trim()}']`, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.click();");
                    await Task.Delay(1000);
                    answer = await MyWebView.ExecuteScriptAsync("document.getElementById('divFightAction').innerHTML;");

/*          
                    await MyWebView.ExecuteScriptAsync($"document.querySelectorAll('#divFightI > div > div.minicardContainer > div > div.boxleft.antioverlaybug > img')[0].click();");
                    await Task.Delay(1500);
                    await MyWebView.ExecuteScriptAsync($"document.querySelectorAll('#body > div.divContext > div.divElements > div:nth-child(8)')[0].click();");
                */}

            }
        }
        private async Task<EnemyMonster> GatherEnemyMonsterInfoAsync()
        {
            EnemyMonster monster = new();
            string answer = "";
            answer = await MyWebView.ExecuteScriptAsync("document.querySelector('#divFightH').getElementsByClassName('name')[0].innerHTML");
            monster.Name = answer.Trim(new char[] { '\'', '\"' });

            return monster;
        }
    }
}
