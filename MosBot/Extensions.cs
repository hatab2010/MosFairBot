using MosBot.Elements;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MosBot
{
    public enum ChckboxGroup
    {
        Products, Periods
    }
    public static class Extensions
    {
        public static TradePeriods GetTardePeriods(this IWebDriver driver)
        {
            return TradePeriods.GetTardePeriods(driver);
        }

        public static DevSession GetDevSession(this IWebDriver driver)
        {
            return new DevSession(driver);
        }

        public static CheckboksGroup GetCheckboksGroup(this IWebDriver driver, ChckboxGroup type)
        {
            string path = string.Empty;
            switch (type)
            {
                case ChckboxGroup.Products:
                    path = "//div[@class = 'block']";
                    break;
                case ChckboxGroup.Periods:
                    path = "//div[@class='documents-build']";
                    break;
                default:
                    break;
            }
            return CheckboksGroup.GetCheckboksGroup(driver, path);
        }

        public static NextButton GetNextButton(this IWebDriver driver)
        {
            return NextButton.GetNextButton(driver);
        }

        public static DropdawnMenu GetDropdownmenuByLabel(this IWebDriver driver, string label)
        {
            return DropdawnMenu.GetByLabel(driver, label);
        }

        public static TextInput GetTextInputByLabel(this IWebDriver driver, string label)
        {
            return TextInput.GetByLabel(driver, label);
        }


        public static DinamicElement GetDinamicElement(this IWebDriver driver, string XPathString)
        {
            while (true)
            {
                try
                {
                    var el = driver.FindElement(By.XPath(XPathString));
                    var result = new DinamicElement(driver, XPathString);
                    return result;
                }
                catch (Exception)
                {
                }
            }
        }

        public static List<DinamicElement> GetDinamicElements(this IWebDriver driver, string XPathString)
        {
            List<DinamicElement> result = new List<DinamicElement>();
            while (true)
            {
                try
                {
                    result.Clear();
                    var col = driver.FindElements(By.XPath(XPathString)).ToList();

                    for (int i = 1; i <= col.Count; i++)
                    {
                        result.Add(new DinamicElement(driver, $"({XPathString})[{i}]"));
                    }
                    
                    return result;
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
