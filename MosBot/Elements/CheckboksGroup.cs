using System.Collections.Generic;
using OpenQA.Selenium;


namespace MosBot.Elements
{
    public class CheckboksGroup : DinamicElement
    {
        List<DinamicElement> itemContainers = new List<DinamicElement>();
        public static CheckboksGroup GetCheckboksGroup(IWebDriver driver, string xpath) 
        {
            return new CheckboksGroup(driver, xpath);
        }

        private CheckboksGroup(IWebDriver driver, string xpath)
            : base(driver, xpath)
        {
            
        }

        public void SelectItemByName(string name)
        {   
            //Waiting to the loads items
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

        List<DinamicElement> GetItems()
        {
            return Driver.GetDinamicElements($"{XPath}//div[contains(@class, 'holder')]");
        }
    }
}
