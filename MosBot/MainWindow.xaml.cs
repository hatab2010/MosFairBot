using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.Internal;
using OpenQA.Selenium.DevTools.Performance;
using System.Threading;
using OpenQA.Selenium.DevTools.Debugger;
using OpenQA.Selenium.DevTools.Console;
using RuCaptcha;
using System.Text.RegularExpressions;

namespace MosBot
{
    //9096300900
    //Munis2013!
    [Serializable]
    public class FormData
    {
        public string County;
        public string District;
        public string Products;
        public string Category;
        public string Periods;
        public string OGRNIP;
        public string INN;
        public string FirstName;
        public string Surname;
        public string FatherName;
        public string Snils;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string formDataPath = $" {Directory.GetCurrentDirectory()}/Data/FormData.bin";
        FormData formData;
        private IWebDriver driver;

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            Init();
        }

        void LoadData()
        {
            if (File.Exists(formDataPath))
            {
                using (Stream stream = File.Open(formDataPath, FileMode.Open))
                {
                    var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    formData = (FormData)binaryFormatter.Deserialize(stream);
                }
            }

            СountyBox.Text = formData.County;
            DistrictBox.Text = formData.District;
            ProductsBox.Text = formData.Products;
            PeriodsBox.Document.Blocks.Clear();
            PeriodsBox.Document.Blocks.Add(new Paragraph(new Run(formData.Periods)));
            CategoryBox.Document.Blocks.Clear();
            CategoryBox.Document.Blocks.Add(new Paragraph(new Run(formData.Category)));
            OGRNIPBox.Text = formData.OGRNIP;
            INNBox.Text = formData.INN;
            FatherNameBox.Text = formData.FatherName;
            FirstnameBox.Text = formData.FirstName;
            SurnameBox.Text = formData.Surname;
            SnilsBox.Text = formData.Snils;
        }

        void SaveData()
        {
            using(Stream stream = File.Open(formDataPath, FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, formData);
                stream.Close();
            }
        }

        async void Init()
        {
            await InitDriver();
            WaitLogin();
        }

        void Window_Closed(object sender, EventArgs e)
        {
            formData = new FormData
            {
                County = СountyBox.Text,
                District = DistrictBox.Text,
                Products = ProductsBox.Text,
                Category = new TextRange(CategoryBox.Document.ContentStart, CategoryBox.Document.ContentEnd).Text,
                Periods = new TextRange(PeriodsBox.Document.ContentStart, PeriodsBox.Document.ContentEnd).Text,
                OGRNIP = OGRNIPBox.Text,
                FirstName = FirstnameBox.Text,
                Surname = SurnameBox.Text,
                FatherName = FatherNameBox.Text,
                Snils = SnilsBox.Text,
                INN = INNBox.Text
            };

            if (formData != null)
            {
                SaveData();
            }

            driver?.Dispose();
        }

        async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;

            var currentTime = TimeStartBox.Text;

            var nowTime = DateTime.Now;
            var strTime = TimeStartBox.Text.Split(':');
            var minute = int.Parse(strTime[1]);
            var hour = int.Parse(strTime[0]);

            DateTime curTime = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, hour, minute, 0);
            if (nowTime.Hour >= hour && nowTime.Minute >= minute)
            {
                curTime = curTime.AddDays(1);
            }

            var interval = Math.Abs((curTime - nowTime).TotalSeconds);

            var openPageDelay = (int)((interval - 60) * 1000);
            var captchaDelay = (int)((interval - 30) * 1000);
            var filingDelay = (int)interval * 1000;

#pragma warning disable CS4014
            Task.Run(() =>
            {
                Thread.Sleep(openPageDelay);
                WaitServicePage();
            });

            var captchaKey = Task.Run(() =>
            {
                Thread.Sleep(captchaDelay);
                return decisionRecaptch();
            });

            await Task.Run(() => {
                Thread.Sleep(filingDelay);
                Dispatcher.Invoke(() =>
                {
                    Filing();
                });                            
            });
#pragma warning restore CS4014

            Finish(await captchaKey);
            StartButton.IsEnabled = true;
        }

        void Finish(string capthaKey)
        {
            var js = (IJavaScriptExecutor)driver;
            js.ExecuteScript($"$(\".g-recaptcha-response\").val('{capthaKey}')");

            var nextButton = driver.GetNextButton();
            nextButton.Interaction((el) =>
            {
                el.Click();
            });
        }

        void WaitServicePage()
        {
            if (!driver.Url.Contains("dtiu/030301"))
            {
                driver.Navigate().GoToUrl("https://www.mos.ru/pgu/ru/application/dtiu/030301/");
            }

            //Wait load page
            while (true)
            {
                try
                {
                    var el = driver.FindElement(By.TagName("h1"));
                    if (el.Text.Contains("Предоставление места для продажи товаров"))
                        break;
                }
                catch (Exception)
                {
                }
            }
        }

        void Filing()
        {   
            //Период проведения ярморки
            var periodMenu = driver.GetDropdownmenuByLabel("Период проведения ярмарок");
            periodMenu.SelectItemByIndex(0);

            //Выбираем категорию заявителя
            List<DinamicElement> UserCategoryRadioButtons = null;

            do
            {
                UserCategoryRadioButtons = driver.GetDinamicElements("//div[contains(@class, 'radiogroup')]/*/label");
            }
            while (UserCategoryRadioButtons == null || UserCategoryRadioButtons.Count != 2);

            //TODO проверить что радиобутон перешёл в позицию селект
            {
                var radioButtons = RadioGroup.Children.OfType<RadioButton>().ToList();
                var selectUserCategoryIndex = radioButtons.IndexOf(radioButtons.First(_ => _.IsChecked == true));
                UserCategoryRadioButtons[selectUserCategoryIndex].Interaction((el) =>
                {
                    el.Click();
                });
            }            

            //Округ
            var county = driver.GetDropdownmenuByLabel("Округ");
            county.SelectItemByName(СountyBox.Text);

            //Район
            var district = driver.GetDropdownmenuByLabel("Район");
            district.SelectItemByName(DistrictBox.Text);

            //Ярмарка
            var fair = driver.GetDropdownmenuByLabel("Ярмарка");
            fair.SelectItemByIndex(0);

            //Группа товаров
            var products = driver.GetDropdownmenuByLabel("Группа товаров");
            products.SelectItemByName(ProductsBox.Text);

            //Категория
            var categoryStrArr = formatedArrStrForm(CategoryBox);
            var categoryBlock = driver.GetCheckboksGroup(ChckboxGroup.Products);

            foreach (var item in categoryStrArr)
            {
                categoryBlock.SelectItemByName(item.Trim());
            }

            //Этап2
            var nextButton = driver.GetNextButton();
            nextButton.Next();

            var periodsStrArr = new TextRange(PeriodsBox.Document.ContentStart, PeriodsBox.Document.ContentEnd)
              .Text
              .Split(';');

            var periods = driver.GetCheckboksGroup(ChckboxGroup.Periods);

            foreach (var item in periodsStrArr)
            {
                periods.SelectItemByName(item.Trim());
            }

            //Этап3
            nextButton.Next();

            //Организационно прововая форма
            //var orgForm = driver.GetDropdownmenuByLabel("Организационно-правовая форма");
            //orgForm.SendKey("123");
            //orgForm.SelectItemByName("Объединения крестьянских хозяйств - 123");

            //ОГРНИП
            var ogrnip = driver.GetTextInputByLabel("ОГРНИП");
            ogrnip.SendKey(OGRNIPBox.Text);

            //ИНН
            var inn = driver.GetTextInputByLabel("ИНН");
            inn.SendKey(INNBox.Text);

            var newEmpoyeeButton = driver.GetDinamicElement("//span[contains(text(), 'Добавить доверенное лицо')]");
            newEmpoyeeButton.Interaction((el) =>
            {
                el.Click();
            });

            {
                var employeerAddedBlock = driver.GetDinamicElement("//legend[contains(text(), '№1')]/..");

                employeerAddedBlock.GetTextInputByLabel("Фамилия")
                      .SendKey(SurnameBox.Text);

                employeerAddedBlock.GetTextInputByLabel("Имя")
                      .SendKey(FirstnameBox.Text);

                employeerAddedBlock.GetTextInputByLabel("Отчество")
                      .SendKey(FatherNameBox.Text);

                employeerAddedBlock.GetTextInputByLabel("СНИЛС")
                      .SendKey(SnilsBox.Text);
            }

            var finishCheckboks = driver.GetDinamicElements("//legend[contains(text(), 'Согласие')]" +
                                                            "/..//label[not(contains(@class, 'error'))]");

            foreach (var item in finishCheckboks)
            {
                item.Interaction((el) => 
                {
                    el.Click();
                });
            }
            
            //var js = (IJavaScriptExecutor)driver;
            //js.ExecuteScript($"$(\".g-recaptcha-response\").val('{await captchaKey}')");


            //nextButton.Interaction((el) =>
            //{
            //    el.Click();
            //});

            List<string> formatedArrStrForm(RichTextBox box)
            {
                var str = new TextRange(box.Document.ContentStart, box.Document.ContentEnd)
                                            .Text;
                var formateStr = Regex.Replace(str, @"\t|\n|\r", "").Trim();
                var list = formateStr.Split(';').ToList();
                list.RemoveAll(_ => string.IsNullOrEmpty(_));

                return list;
            }
        }

        async Task<string> decisionRecaptch()
        {
            var captcha = new CaptchaClient("b4fe7c5c6917758f6e0ebd99babc00b7");
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            var siteKey = (string)js.ExecuteScript("return $(\".g-recaptcha\").attr(\"data-sitekey\")");


            var captchaResult = await captcha.ReCaptchaAsync(siteKey, driver.Url);

            return captchaResult.request;
        }

        async Task InitDriver()
        {
            var dataPath = Directory.GetCurrentDirectory() + "/Data/";

            //Create default setting and options from driver
            ChromeOptions options = new ChromeOptions();
            ChromeDriverService services = ChromeDriverService.CreateDefaultService();

            //Set options
            options.AddArgument($"--user-data-dir={dataPath}");
            options.PageLoadStrategy = PageLoadStrategy.Normal;

            //Set settings
            //services.HideCommandPromptWindow = true;

            await Task.Run(() =>
            {
                driver = new ChromeDriver(services, options);
                driver.Navigate().GoToUrl("https://www.mos.ru/services");

                var devTools = driver as IDevTools;
                var session = devTools.CreateDevToolsSession();
                session.Network.Enable(new OpenQA.Selenium.DevTools.Network.EnableCommandSettings());

                session.Network.SetBlockedURLs(new OpenQA.Selenium.DevTools.Network.SetBlockedURLsCommandSettings()
                {
                    Urls = new string[] 
                    {
                        "https://metrika.mos.ru",
                        "https://mc.yandex.ru/webvisor/",
                        //"https://www.mos.ru/etp/SioprRest/getPeriod"
                    }
                });

                Task.Run(() =>
                {
                    Thread.Sleep(15000);
                    session.Network.SetBlockedURLs(new OpenQA.Selenium.DevTools.Network.SetBlockedURLsCommandSettings()
                    {
                        Urls = new string[] {"https://metrika.mos.ru", "https://mc.yandex.ru/webvisor/" }
                    });
                });
            });
        }
        void WaitLogin()
        {
            driver.GetDinamicElement("//a[contains(@class, Caption_messages__35nZT)]");     //Wait element occurence in the page
        }

        private void TimeStartBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = "0123456789: ".IndexOf(e.Text) < 0;
            var regex = new Regex(@"24");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TimeStartBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            var regex = new Regex(@"24:\d\d");
            if (regex.IsMatch(tb.Text))
            {
                tb.Text = "00:" + tb.Text.Split(':')[1];
            }
        }
    }
}
