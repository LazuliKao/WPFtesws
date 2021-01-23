using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
//using PFWebsocketAPI.Model;
using PFWebsocketAPI.PFWebsocketAPI.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WebSocketSharp;

namespace MinecraftToolKit.Pages
{
    public class SelectedToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value >= 0;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
    public class Up0ToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value > 0;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
    public class Index1ToVis : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value == 1 ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
    public class Index0ToVis : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
    //public class SelectedClientToBool : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    { 
    //            return ((Tesws.WS)((ComboBoxItem)value).Tag).client.IsAlive; 
    //    }
    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    //}
    /// <summary>
    /// tesws.xaml 的交互逻辑
    /// </summary>
    public partial class Tesws : Page
    {
        public Tesws()
        {
            InitializeComponent();
            //if (true)
            //{

            //}
            if (File.Exists(Environment.CurrentDirectory + "\\tesws.json"))
            {
                OutPut($"检测到位于{Environment.CurrentDirectory}\\tesws.json的配置文件");
                try
                {
                    foreach (JObject item in JObject.Parse(File.ReadAllText(Environment.CurrentDirectory + "\\tesws.json"))["Servers"])
                    {
                        WebSocket webSocket = new WebSocket(item["Address"].ToString());
                        webSocket.OnOpen += (senderClient, ClientE) =>
                        {
                            OutPut((senderClient as WebSocket).Url, $"已连接");
                        };
                        webSocket.OnError += (senderClient, ClientE) =>
                        {
                            OutPutErr((senderClient as WebSocket).Url, $"出错啦{ClientE.Message}");
                        };
                        webSocket.OnMessage += WebSocket_OnMessage;
                        webSocket.OnClose += (senderClient, ClientE) =>
                        {
                            OutPutErr((senderClient as WebSocket).Url, $"连接已关闭\t{ClientE.Code}=>{ClientE.Reason}");
                        };
                        SelectServer.Items.Add(new ComboBoxItem() { Content = item["Address"].ToString(), Tag = new WS() { client = webSocket, info = item } });
                    }
                    OutPut("配置文件读取成功");
                }
                catch (Exception err)
                { OutPutErr("配置文件读取失败" + err.Message); }
            }
#if false
//test
            try
            {
                string GetMD5(string sDataIn)
                {
                    var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                    byte[] bytValue, bytHash;
                    bytValue = Encoding.UTF8.GetBytes(sDataIn);
                    bytHash = md5.ComputeHash(bytValue);
                    md5.Clear();
                    string sTemp = "";
                    for (int i = 0, loopTo = bytHash.Length - 1; i <= loopTo; i++)
                        sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
                    return sTemp.ToUpper();
                }
                {
                    string text = "操";
                    string password = "1234ddddddddddddddddddddddddddd56a";
                    string md5 = GetMD5(password);
                    //byte[] md5bytes = Encoding.UTF8.GetBytes(md5);
                    OutPut("raw\t\t" + text);
                    OutPut("password\t" + password);
                    OutPut("passwordMd5\t" + md5);
            #region AES_CBC
                    {
                        //byte[] keyRaw = new byte[16];
                        //byte[] ivRaw = new byte[16];
                        //for (int i = 0; i < keyRaw.Length; i++)
                        //    keyRaw[i] = md5bytes[i];
                        //for (int i = 0; i < ivRaw.Length; i++)
                        //    ivRaw[i] = md5bytes[i];
                        //string iv = Encoding.UTF8.GetString(ivRaw);
                        //string key = Encoding.UTF8.GetString(keyRaw); 
                        string iv = md5.Substring(16);
                        string key = md5.Remove(16);
                        OutPut("key\t" + key);
                        OutPut("iv\t" + iv);
                        var encryptString =  EasyEncryption.AES.Encrypt(text, key, iv);
                        OutPut("encrypted\t" + encryptString);
                        var result = EasyEncryption.AES.Decrypt(encryptString, key, iv);
                        OutPut("result\t" + result);
                    }
            #endregion
                }
            }
            catch (Exception e)
            {
                OutPutErr(e);
            }
#endif

        }
        #region 添加/编辑
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                JObject jObject = new JObject() { new JProperty("Servers", new JArray()) };
                foreach (var item in SelectServer.Items)
                {
                    ((JArray)jObject["Servers"]).Add(((WS)((ComboBoxItem)item).Tag).info);
                }
                File.WriteAllText(Environment.CurrentDirectory + "\\tesws.json", jObject.ToString());
                OutPut("配置文件已保存到" + Environment.CurrentDirectory + "\\tesws.json");
            }
            catch (Exception) { }
        }
        private bool AddMode = true;
        private void AddServerButton_Click(object sender, RoutedEventArgs e)
        {
            AddAddress.Clear();
            AddPWD.Clear();
            AddTip.Visibility = Visibility.Collapsed;
            Dialog.IsOpen = true;
            AddMode = true;
        }
        private void EditServerButton_Click(object sender, RoutedEventArgs e)
        {
            AddAddress.Text = ((WS)((ComboBoxItem)SelectServer.SelectedItem).Tag).info["Address"].ToString();
            AddPWD.Text = ((WS)((ComboBoxItem)SelectServer.SelectedItem).Tag).info["Password"].ToString();
            AddTip.Visibility = Visibility.Collapsed;
            Dialog.IsOpen = true;
            AddMode = false;
        }
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WebSocket webSocket = new WebSocket(AddAddress.Text);
                webSocket.OnOpen += (senderClient, ClientE) =>
                {
                    OutPut((senderClient as WebSocket).Url, $"已连接");
                };
                webSocket.OnError += (senderClient, ClientE) =>
               {
                   OutPutErr((senderClient as WebSocket).Url, $"出错啦{ClientE.Message}");
               };
                webSocket.OnMessage += WebSocket_OnMessage;
                webSocket.OnClose += (senderClient, ClientE) =>
                {
                    OutPut((senderClient as WebSocket).Url, $"连接已关闭\t{ClientE.Code}=>{ClientE.Reason}");
                };
                if (AddMode)
                {
                    SelectServer.Items.Add(new ComboBoxItem() { Content = AddAddress.Text, Tag = new WS() { client = webSocket, info = new JObject() { new JProperty("Address", AddAddress.Text), new JProperty("Password", AddPWD.Text) } } });
                    if (SelectServer.SelectedIndex == -1) { SelectServer.SelectedIndex = 0; }
                    OutPut($"添加成功=>{AddAddress.Text}");
                }
                else
                {
                    if (((WS)((ComboBoxItem)SelectServer.SelectedItem).Tag).client.IsAlive)
                    { ((WS)((ComboBoxItem)SelectServer.SelectedItem).Tag).client.CloseAsync(); }
                    SelectServer.Items.Remove(SelectServer.SelectedItem);
                    SelectServer.Items.Add(new ComboBoxItem() { Content = AddAddress.Text, Tag = new WS() { client = webSocket, info = new JObject() { new JProperty("Address", AddAddress.Text), new JProperty("Password", AddPWD.Text) } } });
                    SelectServer.SelectedIndex = SelectServer.Items.Count - 1;
                    OutPut($"修改成功=>{AddAddress.Text}");
                }
                Dialog.IsOpen = false;
            }
            catch (Exception err)
            {
                AddTip.Visibility = Visibility.Visible;
                AddTip.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                AddTip.Text = "填写内容不合规\n" + err.Message;
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Dialog.IsOpen = false;
        }
        private void RemoveServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (((WS)((ComboBoxItem)SelectServer.SelectedItem).Tag).client.IsAlive)
            {
                ((WS)((ComboBoxItem)SelectServer.SelectedItem).Tag).client.CloseAsync();
            };
            SelectServer.Items.Remove(SelectServer.SelectedItem);
        }
        private void CleanButton_Click(object sender, RoutedEventArgs e)
        {
            OutPutText.Document.Blocks.Clear();
        }
        #endregion
        public void UnInstall()
        {
            foreach (var item in SelectServer.Items)
            {
                ((WS)((ComboBoxItem)item).Tag).client.Close();
                this.Content = null;
            }
        }
        //private string GetMD5(string sDataIn)
        //{
        //    System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        //    byte[] bytValue, bytHash;
        //    bytValue = Encoding.UTF8.GetBytes(sDataIn);
        //    bytHash = md5.ComputeHash(bytValue);
        //    md5.Clear();
        //    string sTemp = "";
        //    for (int i = 0; i < bytHash.Length; i++)
        //    {
        //        sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
        //    }
        //    return sTemp.ToUpper();
        //}

        public string GetCmdReq(string token, string cmd)
        {
            ActionRunCmd raw = new ActionRunCmd(cmd, cmd.GetHashCode().ToString(), null);
            return GetEncryptedReq(raw.ToString(), token);
        }
        public string GetEncryptedReq(string from, string token)
        {
            EncryptedPack encrypted = new EncryptedPack(EncryptionMode.aes_cbc_pck7padding, from, token);
            return encrypted.ToString();
        }
        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectServer.SelectedIndex != -1)
            {
                if (((WS)((ComboBoxItem)SelectServer.SelectedItem).Tag).client.IsAlive)
                {
                    switch (SelectAction.SelectedIndex)
                    {
                        case 0:
                            try
                            {
                                var raw = SendText.Text;
                                OutPut(raw);
                                string passwd = ((WS)((ComboBoxItem)SelectServer.SelectedItem).Tag).info["Password"].ToString();
                                string getStr = raw;
                                if (ECCB.IsChecked == true) getStr = GetEncryptedReq(raw, passwd);
                                ((WS)((ComboBoxItem)SelectServer.SelectedItem).Tag).client.SendAsync(getStr, (state) => OutPut(state ? "发送成功=>" + getStr : "发送失败!?"));
                                OutPut(getStr);
                            }
                            catch (Exception err) { OutPutErr(err); }
                            break;
                        case 1:
                            string cmdStr = GetCmdReq(((WS)((ComboBoxItem)SelectServer.SelectedItem).Tag).info["Password"].ToString(), SendText.Text);
                            ((WS)((ComboBoxItem)SelectServer.SelectedItem).Tag).client.SendAsync(cmdStr, (state) => OutPut(state ? "发送成功=>" + cmdStr : "发送失败!?"));
                            //SendText.Text 
                            break;
                        default:
                            OutPutErr("还没选择消息内容类别啊啊啊啊啊啊！！！");
                            break;
                    }
                }
                else
                {
                    OutPutErr("还没连接啊啊啊啊啊啊！！！");
                }
            }
            else
            {
                OutPutErr("把你想要发送的Server选上再点啊啊啊啊啊啊！！！");
            }
        }
        public struct WS
        {
            public WebSocket client;
            public JObject info;
        }
        #region OUTPUT
        private void OutPut(object input) => Dispatcher.Invoke(() =>
        {
            OutPutText.Document.Blocks.Add(new Paragraph(new Run(DateTime.Now.ToString()) { Foreground = Brushes.Blue }));
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new PackIcon() { Kind = PackIconKind.ConsoleLine, Margin = new Thickness(0, -3, 0, -3), Foreground = Brushes.Purple });
            paragraph.Inlines.Add(new Run(input.ToString()) { Foreground = Brushes.Black });
            OutPutText.Document.Blocks.Add(paragraph);
            OutPutText.ScrollToEnd();
        });
        private void OutPutErr(object input) => Dispatcher.Invoke(() =>
        {
            OutPutText.Document.Blocks.Add(new Paragraph(new Run(DateTime.Now.ToString()) { Foreground = Brushes.Blue }));
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new PackIcon() { Kind = PackIconKind.ErrorOutline, Margin = new Thickness(0, -3, 0, -3), Foreground = Brushes.Red });
            paragraph.Inlines.Add(new Run(input.ToString()) { Foreground = Brushes.Red });
            OutPutText.Document.Blocks.Add(paragraph);
            OutPutText.ScrollToEnd();
        });
        private void OutPutErr(Uri from, object input) => Dispatcher.Invoke(() =>
        {
            Paragraph paragraphFrom = new Paragraph();
            paragraphFrom.Inlines.Add(new Run(DateTime.Now.ToString()) { Foreground = Brushes.Blue });
            paragraphFrom.Inlines.Add(new Run("=>") { Foreground = Brushes.Yellow });
            paragraphFrom.Inlines.Add(new Run($"From {from.ToString()}") { Foreground = Brushes.Green });
            OutPutText.Document.Blocks.Add(paragraphFrom);
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new PackIcon() { Kind = PackIconKind.ErrorOutline, Margin = new Thickness(0, -3, 0, -3), Foreground = Brushes.Red });
            paragraph.Inlines.Add(new Run(input.ToString()) { Foreground = Brushes.Red });
            OutPutText.Document.Blocks.Add(paragraph);
            OutPutText.ScrollToEnd();
        });
        private void OutPut(Uri from, string type, object input) => Dispatcher.Invoke(() =>
        {
            Paragraph paragraphFrom = new Paragraph();
            paragraphFrom.Inlines.Add(new Run(DateTime.Now.ToString()) { Foreground = Brushes.Blue });
            paragraphFrom.Inlines.Add(new Run("=>") { Foreground = Brushes.Yellow });
            paragraphFrom.Inlines.Add(new Run($"From {from.ToString()}") { Foreground = Brushes.Green });
            OutPutText.Document.Blocks.Add(paragraphFrom);
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new PackIcon() { Kind = PackIconKind.ConsoleLine, Margin = new Thickness(0, -3, 0, -3), Foreground = Brushes.Purple });
            paragraph.Inlines.Add(new Run(type.ToString()) { Foreground = Brushes.DarkMagenta });
            OutPutText.Document.Blocks.Add(paragraph);
            Paragraph paragraph1 = new Paragraph();
            paragraph1.Inlines.Add(new Run(Regex.Replace(input.ToString(), "\n+$", "")) { Foreground = Brushes.Gray });
            OutPutText.Document.Blocks.Add(paragraph1);
            OutPutText.ScrollToEnd();
        });
        private void OutPut(Uri from, object input) => Dispatcher.Invoke(() =>
        {
            Paragraph paragraphFrom = new Paragraph();
            paragraphFrom.Inlines.Add(new Run(DateTime.Now.ToString()) { Foreground = Brushes.Blue });
            paragraphFrom.Inlines.Add(new Run("=>") { Foreground = Brushes.Yellow });
            paragraphFrom.Inlines.Add(new Run($"From {from.ToString()}") { Foreground = Brushes.Green });
            OutPutText.Document.Blocks.Add(paragraphFrom);
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new PackIcon() { Kind = PackIconKind.ConsoleLine, Margin = new Thickness(0, -3, 0, -3), Foreground = Brushes.Purple });
            paragraph.Inlines.Add(new Run(Regex.Replace(input.ToString(), "\n+$", "")) { Foreground = Brushes.Gray });
            OutPutText.Document.Blocks.Add(paragraph);
            OutPutText.ScrollToEnd();
        });
        #endregion
        private void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            ProcessMessage((WebSocket)sender, e.Data);
        }
        private void ProcessMessage(WebSocket sender, string message)
        {
            try
            {
                JObject receive = JObject.Parse(message);
                switch (Enum.Parse(typeof(PackType), receive["type"].ToString()))
                {
                    case PackType.pack:
                        switch (Enum.Parse(typeof(ServerCauseType), receive["cause"].ToString()))
                        {
                            case ServerCauseType.runcmdfeedback:
                                CauseRuncmdFeedback feedback = new CauseRuncmdFeedback(receive);
                                OutPut(sender.Url, "命令执行反馈", feedback.@params.result);
                                return;
                        }
                        //if (receive["Auth"].ToString() == "Failed")
                        //{
                        //    OutPut(((WebSocket)sender).Url, "命令执行反馈", "密码不匹配！！！");
                        //}
                        //else
                        //{
                        // 
                        //}
                        break;
                    case PackType.encrypted:
                        EncryptedPack ep = new EncryptedPack(receive);
                        //switch (ep.@params.mode)
                        //{
                        //    case EncryptionMode.aes_cbc_pck7padding:
                        OutPut(sender.Url, "解密包:" + message);
                        string passwd = null;
                        Dispatcher.Invoke(() => passwd = ((WS)((ComboBoxItem)SelectServer.SelectedItem).Tag).info["Password"].ToString());
                        string decoded = ep.Decode(passwd);
                        ProcessMessage(sender, decoded);
                        //return;
                        //}
                        break;
                    //return;
                    default:
                        break;
                }
            }
            catch (Exception) { }
            try
            {
                OutPut(sender.Url, $"收信:\n{JObject.Parse(message)}");
            }
            catch (Exception)
            {
                OutPutErr(sender.Url, $"解析失败:{message}");
            }
        }

        private void SelectServer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectServer.SelectedIndex != -1)
            {
                if (((WS)((ComboBoxItem)SelectServer.SelectedItem).Tag).client.IsAlive)
                {
                    OutPutErr("已经连上了啊啊啊啊啊啊！！！");
                }
                else
                {
                    OutPut("正在尝试连接至" + ((WS)((ComboBoxItem)SelectServer.SelectedItem).Tag).client.Url);
                    ((WS)((ComboBoxItem)SelectServer.SelectedItem).Tag).client.ConnectAsync();
                }
            }
            else
            {
                OutPutErr("把你想要发送的Server选上再点啊啊啊啊啊啊！！！");
            }
        }
    }
}
