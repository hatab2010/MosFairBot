using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MosBot
{
    public class BotManager
    {
        public Data Data { private set; get; }
        public BotProcessState State { private set; get; }

        private IWebDriver driver;
        private DevSession session;
        private WorkStepBase currentStep;

        public BotManager(Data data)
        {
            this.Data = data;
            InitDriver();
            WaitLogin();
        }

        public void Start()
        {
            
        }

        public void Dispoce()
        {
            driver?.Dispose();
        }

        void InitDriver()
        {
            var dataPath = Directory.GetCurrentDirectory() + "/Data/";

            //Create default setting and options from driver
            ChromeOptions options = new ChromeOptions();
            ChromeDriverService services = ChromeDriverService.CreateDefaultService();

            //Set options
            options.AddArgument($"--user-data-dir={dataPath}");
            options.PageLoadStrategy = PageLoadStrategy.Normal;

            //Set settings
            services.HideCommandPromptWindow = true;

            driver = new ChromeDriver(services, options);

            Task.Run(() =>
            {
                session = driver.GetDevSession();

                //Block webvisor and metric requset
                session.AddBlockUrl("https://metrika.mos.ru",
                                    "https://mc.yandex.ru",
                                    "https://www.mos.ru/etp/SioprRest/getPeriod"); //Debug block url                
            });
        }
        void WaitLogin()
        {
            driver.Url = "https://www.mos.ru/services";
            driver.GetDinamicElement("//a[contains(@class, Caption_messages__35nZT)]");     //Wait element occurence in the page
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
    }

    public class InitDriver : WorkStepBase
    {
        public InitDriver(IWebDriver driver) : base(driver)
        {
        }

        public override void Execute()
        {
            var dataPath = Directory.GetCurrentDirectory() + "/Data/";

            //Create default setting and options from driver
            ChromeOptions options = new ChromeOptions();
            ChromeDriverService services = ChromeDriverService.CreateDefaultService();

            //Set options
            options.AddArgument($"--user-data-dir={dataPath}");
            options.PageLoadStrategy = PageLoadStrategy.Normal;

            //Set settings
            services.HideCommandPromptWindow = true;

            driver = new ChromeDriver(services, options);

            Task.Run(() =>
            {                
                session = driver.GetDevSession();

                //Block webvisor and metric requset
                session.AddBlockUrl("https://metrika.mos.ru",
                                    "https://mc.yandex.ru",
                                    "https://www.mos.ru/etp/SioprRest/getPeriod"); //Debug block url

                //Navigate to login page
            });
        }
    }

    public class WaitLogin : WorkStepBase
    {
        public WaitLogin(IWebDriver driver) : base(driver)
        {
            State = BotProcessState.Login;
        }

        public override void Execute()
        {
            //Navigate to login page
            driver.Url = "https://www.mos.ru/services";
            driver.GetDinamicElement("//a[contains(@class, Caption_messages__35nZT)]");     //Wait element occurence in the page
        }
    }

    public abstract class WorkStepBase
    {
        public BotProcessState State;

        protected IWebDriver driver;
        protected DevSession session;

        protected WorkStepBase(IWebDriver driver)
        {
            this.driver = driver;
            session = driver.GetDevSession();
        }
        
        public virtual void Execute() { }
    }
}
