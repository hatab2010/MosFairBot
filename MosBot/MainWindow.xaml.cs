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

namespace MosBot
{
    //9096300900
    //Munis2013!
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BotManager worker;
        Data data;

        public MainWindow()
        {
            data = new Data();
            worker = new BotManager(data);
            InitializeComponent();
        }

        List<string> LogMessage = new List<string>();
        private void ConsoleLog(object sender, DevToolsSessionLogMessageEventArgs e)
        {
            LogMessage.Add(e.Message);
        }

        List<string> message = new List<string>();

        private void Window_Closed(object sender, EventArgs e)
        {
            worker.Dispoce();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            //Переходим на страницу услуги
            if (!driver.Url.Contains("dtiu/030301"))
            {
                try
                {
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

                    Task.Run(() =>
                    {
                        js.ExecuteScript("window.location = \"https://www.mos.ru/pgu/ru/application/dtiu/030301/\"");
                    });


                    //driver.Url = "https://www.mos.ru/pgu/ru/application/dtiu/030301/";
                }
                catch
                {
                }
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


            //Период проведения ярморки
            var periodMenu = driver.GetDropdownmenuByLabel("Период проведения ярмарок");
            periodMenu.SelectItemByIndex(0);

            //Округ
            var county = driver.GetDropdownmenuByLabel("Округ");
            county.SelectItemByName("Восточный административный округ");

            //Район
            var district = driver.GetDropdownmenuByLabel("Район");
            district.SelectItemByName("район Богородское");

            //Ярмарка
            var fair = driver.GetDropdownmenuByLabel("Ярмарка");
            fair.SelectItemByIndex(0);

            //Группа товаров
            var products = driver.GetDropdownmenuByLabel("Группа товаров");
            products.SelectItemByName("Овощи и фрукты");

            //Категория
            var categoryBlock = driver.GetCheckboksGroup(ChckboxGroup.Products);
            categoryBlock.SelectItemByName("Грибы");
            categoryBlock.SelectItemByName("Джемы");

            //Этап2
            var nextButton = driver.GetNextButton();
            nextButton.Next();

            var periods = driver.GetCheckboksGroup(ChckboxGroup.Periods);
            periods.SelectItemByName("17.07.2020-19.07.2020");
            periods.SelectItemByName("07.08.2020-09.08.2020");
            periods.SelectItemByName("11.09.2020-13.09.2020");

            //Этап3
            nextButton.Next();

            //Организационно прововая форма
            var orgForm = driver.GetDropdownmenuByLabel("Организационно-правовая форма");
            orgForm.SendKey("123");
            orgForm.SelectItemByName("Объединения крестьянских хозяйств - 123");

            //ОГРНИП
            var ogrnip = driver.GetTextInputByLabel("ОГРНИП");
            ogrnip.SendKey("123456789012345");

            //ИНН
            var inn = driver.GetTextInputByLabel("ИНН");
            inn.SendKey("123456789012");

            var newEmpoyeeButton = driver.GetDinamicElement("//span[contains(text(), 'Добавить доверенное лицо')]");
            newEmpoyeeButton.Interaction((el) =>
            {
                el.Click();
            });

            {
                var employeerAddedBlock = driver.GetDinamicElement("//legend[contains(text(), '№1')]/..");

                employeerAddedBlock.GetTextInputByLabel("Фамилия")
                      .SendKey("Жопка");

                employeerAddedBlock.GetTextInputByLabel("Имя")
                      .SendKey("Жопка");

                employeerAddedBlock.GetTextInputByLabel("Отчество")
                      .SendKey("Отчество");

                employeerAddedBlock.GetTextInputByLabel("СНИЛС")
                      .SendKey("12313265465");
            }
        }

        DinamicElement GetRowByLabel(string label)
        {
            var pathStr = $"//div[contains(@class, \"row\")]/label[contains(text(), \"{label}\")]/..";
            return driver.GetDinamicElement(pathStr);
        }
    }
}
