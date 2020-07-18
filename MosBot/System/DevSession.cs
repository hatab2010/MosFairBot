using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Internal;
using OpenQA.Selenium.DevTools.Performance;
using OpenQA.Selenium.DevTools.Debugger;
using OpenQA.Selenium.DevTools.Console;

namespace MosBot
{
    public class DevSession
    {
        DevToolsSession session { set; get; }

        private IWebDriver driver;
        private IDevTools devTools;
        private List<string> _blockUrls;

        private List<string> blockUrls
        {
            set
            {
                if (_blockUrls != value)
                {
                    _blockUrls = value;
                    UpdateSessionState();
                }
            }

            get
            {
                return _blockUrls;
            }
        }

        private static List<DevSession> sessionList = new List<DevSession>();

        public DevSession(IWebDriver driver)
        {
            this.driver = driver;

            devTools = driver as IDevTools;
            session = devTools.CreateDevToolsSession();
            session.Network.Enable(new OpenQA.Selenium.DevTools.Network.EnableCommandSettings());

            session.Network.SetBlockedURLs(new OpenQA.Selenium.DevTools.Network.SetBlockedURLsCommandSettings()
            {
                Urls = new string[] {
                    "https://metrika.mos.ru",
                    "https://mc.yandex.ru/webvisor/",
                    "https://www.mos.ru/etp/SioprRest/getPeriod"
                }
            });
            blockUrls = new List<string>();            
        }

        public static DevSession GetDevSession(IWebDriver driver)
        {
            var currentSession = sessionList.FirstOrDefault(_ => _.driver == driver);

            if (currentSession == null)
            {
                return new DevSession(driver);
            }
            else
            {
                return currentSession;
            }
        }

        public void AddBlockUrl(params string[] blockUrls)
        {
            foreach (var newBlockUrl in blockUrls)
            {
                var contain = this.blockUrls.FirstOrDefault(_ => _.Contains(newBlockUrl));

                if (contain == null)
                {
                    this.blockUrls.Add(newBlockUrl);
                }
            }
        }

        public void RefrashAllBlockUrls()
        {
            blockUrls.Clear();
        }

        public void RemoveBlockUrl(params string[] blockUrls)
        {
            foreach (var item in blockUrls)
            {
                var removeItem = this.blockUrls.FirstOrDefault(_ => _.Contains(item));

                if (removeItem != null)
                {
                    this.blockUrls.Remove(removeItem);
                }
            }
        }

        void UpdateSessionState()
        {
            var arr = blockUrls.ToArray();

            session.Network.SetBlockedURLs(new OpenQA.Selenium.DevTools.Network.SetBlockedURLsCommandSettings()
            {
                Urls = arr
            });
        }
    }
}
