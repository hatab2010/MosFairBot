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
        }

        public void Start()
        {

        }

        public void Dispoce()
        {
            driver?.Dispose();
        }
    }

    public class InitDraiver : WorkStepBase
    {
        public InitDraiver(IWebDriver driver) : base(driver)
        {
        }

        public override async void Execute()
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

            await Task.Run(() =>
            {
                driver = new ChromeDriver(services, options);
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
