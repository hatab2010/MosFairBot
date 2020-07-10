using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MosBot
{ 
    public class DropdawnMenu : DinamicElement
    {
        const string XPATH_HANDLER = "div/div/a[contains(@class, 'chosen-single')]";
        const string XPATH_CHOSEN_DROP = "/div[contains(@class, 'chosen-drop')]";

        DinamicElement handler;
        DinamicElement chosenDrop;
        DinamicElement chosenContainer;

        private DropdawnMenu(IWebDriver driver, string xpath)
            : base(driver, xpath)
        {
            Interaction((el) =>
            {
                handler = new DinamicElement(driver, $"{XPath}/{XPATH_HANDLER}");
            });

            Interaction((el) =>
            {
                chosenDrop = new DinamicElement(driver, $"{XPath}/{XPATH_CHOSEN_DROP}");
            });

            chosenContainer = FindeElement("//div[contains(@class, \"osen-container\")]");
        }

        public static DropdawnMenu GetByLabel(IWebDriver driver, string label)
        {
            var pathStr = $"(//div[contains(@class, 'row')]/label[contains(text(), '{label}') " +
                $"and parent::*[not(contains(@style, 'display: none'))]]/..)[parent::*[not(contains(@style, 'display: none'))]]";
            DropdawnMenu result = new DropdawnMenu(driver, pathStr);
            return result;
        }

        public void SendKey(string text)
        {
            Open();
            var searchUnput = chosenDrop.FindeElement("//input[contains(@class, 'chosen-search-input')]");

            searchUnput.Interaction((el) =>
            {
                Open();
                el.SendKeys(text);
            });
        }

        public void Open()
        {
            if (checkForOpen())
                return;

            handler.Interaction((el) =>
            {
                el.Click();
            });

            while (checkForOpen() == false)
            {
                Thread.Sleep(50);
            }

            bool checkForOpen()
            {
                bool result = false;

                chosenContainer.Interaction((el) =>
                {
                    result = el.GetAttribute("class").Contains("chosen-with-drop");
                });

                return result;
            }
        }

        public void SelectItemByIndex(int index)
        {
            Open();
            var items = WaitLiItemsDownloads(chosenDrop);

            items[index].Interaction((_) =>
            {
                Open();

                while (string.IsNullOrEmpty(_.Text))
                {
                    Scroll(chosenDrop);
                }

                _.Click();
            });
        }

        public void SelectItemByName(string name)
        {
            Open();
            var liItems = WaitLiItemsDownloads(chosenDrop);
            Scroll(chosenDrop, -99999);

            foreach (var item in liItems)
            {
                bool isFinish = false;

                item.Interaction((_) =>
                {                   
                    var debug = _.Text;

                    while (string.IsNullOrEmpty(_.Text))
                    {
                        Scroll(chosenDrop);
                    }

                    if (debug.ToLower().Trim() == name.ToLower().Trim())
                    {
                        try
                        {
                            _.Click();
                            isFinish = true;
                        }
                        catch (Exception ex)
                        {
                            Open();
                            _.Click();
                            isFinish = true;
                        }

                    }
                });

                if (isFinish) break;
            }
        }

        List<DinamicElement> WaitLiItemsDownloads(DinamicElement liParent)
        {
            var items = liParent.FindeElements("//li");

            while (items.Count == 0)
            {
                items = liParent.FindeElements("//li");
            }

            //Waiting item list download
            items[0].Interaction((_) =>
            {
                while (_.Text.ToLower().Contains("выбрать") || (_.Text.ToLower().Contains("выберите") && items.Count == 1))
                {
                    Thread.Sleep(50);
                }
            });

            return items;
        }

        void Scroll(DinamicElement scrollEl)
        {
            scrollEl.Interaction(el =>
            {
                var scrolabelBlock = el.FindElement(By.TagName("ul"));
                IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
                js.ExecuteScript($"arguments[0].scrollBy(0, 100)", scrolabelBlock);
            });
        }
        void Scroll(DinamicElement scrollEl, int x)
        {
            scrollEl.Interaction(el =>
            {
                var scrolabelBlock = el.FindElement(By.TagName("ul"));
                IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
                js.ExecuteScript($"arguments[0].scrollBy(0, {x})", scrolabelBlock);
            });
        }
    }
}
