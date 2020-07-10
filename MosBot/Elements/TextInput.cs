using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MosBot.Elements
{
    public class TextInput : DinamicElement
    {
        DinamicElement container;
        DinamicElement input;

        private TextInput(IWebDriver driver, string xpath)
            : base(driver, xpath)
        {
            Interaction((el) =>
            {
                container = new DinamicElement(driver, $"{XPath}");
            });

            Interaction((el) =>
            {
                input = container.FindeElement("//input[contains(@class, 'form-control')]");
            });
        }

        public static TextInput GetByLabel(IWebDriver driver, string label)
        {
            var pathStr = $"(//div[contains(@class, 'row')]/label[contains(text(), '{label}') " +
                $"and parent::*[not(contains(@style, 'display: none'))]]/..)[parent::*[not(contains(@style, 'display: none'))]]";
            TextInput result = new TextInput(driver, pathStr);
            return result;
        }

        public static TextInput GetByLabel(IWebDriver driver, string xpathParent, string label)
        {
            var pathStr = $"({xpathParent}//div[contains(@class, 'row')]/label[contains(text(), '{label}') " +
            $"and parent::*[not(contains(@style, 'display: none'))]]/..)[parent::*[not(contains(@style, 'display: none'))]]";            


            TextInput result = new TextInput(driver, pathStr);
            return result;
        }

        public void SendKey(string text)
        {
            input.Interaction((el) =>
            {
                el.SendKeys(text);
            });
        }
    }
}
