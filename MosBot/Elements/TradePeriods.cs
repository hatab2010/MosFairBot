using System.Collections.Generic;
using OpenQA.Selenium;

namespace MosBot.Elements
{
    public class TradePeriods : DinamicElement
    {
        List<DinamicElement> itemContainers;

        public static TradePeriods GetTardePeriods(IWebDriver driver)
        {
            var path = "//div[@class='documents-build']";
            return new TradePeriods(driver, path);
        }

        private TradePeriods(IWebDriver driver, string xpath)
            : base(driver, xpath)
        {

        }

        public void SelectElementByName(string name)
        {
            //Wating for loads element
            while (itemContainers.Count < 1)
            {
                itemContainers = GetItems();
            }

            foreach (var item in itemContainers)
            {
                item.Interaction((el) =>
                {
                    if (el.Text.ToLower().Contains(name.ToLower()))
                    {
                        var checkbox = item.FindeElement("//label");

                        checkbox.Interaction((_) =>
                        {
                            _.Click();
                        });
                    }
                });
            }
        }

        private List<DinamicElement> GetItems() 
        {
            return Driver.GetDinamicElements($"{XPath}//div[contains(@class, 'holder')]");
        }
    }
}
