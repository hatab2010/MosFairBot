using System;
using OpenQA.Selenium;
using System.Linq;
using System.Collections.Generic;
using OpenQA.Selenium.DevTools;
using MosBot.Elements;

namespace MosBot
{

    public class DinamicElement
    {
        public string XPath { protected set; get; }
        public IWebElement Element { protected set; get; }
        public IWebDriver Driver { protected set; get; }
        public DinamicElement(IWebDriver driver, string xpath)
        {
            Driver = driver;
            XPath = xpath;

            findElement();
        }

        public TextInput GetTextInputByLabel(string label)
        {
            TextInput result = null;

            Interaction((el) =>
            {
                result = TextInput.GetByLabel(Driver, XPath, label);
            });

            return result;
        }

        protected void findElement()
        {
            while (true)
            {
                try
                {
                    Element = Driver.FindElement(By.XPath(XPath));
                    break;
                }
                catch (Exception ex)
                {
                    //TODO добавить таймаут. Шоб в бесконечный цикл не уходить
                }
            }            
        }

        public virtual void Interaction(Action<IWebElement> action)
        {
            
            while (true)
            {
                Start:

                try
                {
                    action?.Invoke(Element);
                    break;
                }
                catch (Exception ex)
                {
                    findElement();

                    //Close fucking pop-up
                    if (ex.Message.Contains("popup_messagebox_shadow"))
                    {
                        Driver.GetDinamicElement("//a[contains(@class, 'btn-close-pop')]")
                              .Interaction((el) =>
                              {
                                  el.Click();
                              });
                    }

                    goto Start;  //Sorry
                }
            }

        }

        public DinamicElement FindeElement(string xpath)
        {
            var path = $"{XPath}/{xpath}";
            if (path.Contains("///"))
                path = path.Replace("///","//");
            return new DinamicElement(Driver, path);
        }

        public List<DinamicElement> FindeElements(string xpath)
        {
            var path = $"{XPath}/{xpath}";

            if (path.Contains("///"))
                path = path.Replace("///", "//");

            var result = new List<DinamicElement>();

            Interaction((el) => 
            {
                result.Clear();
                var col =Driver.FindElements(By.XPath(path)).ToList();

                for (int i = 1; i <= col.Count; i++)
                {
                    result.Add(new DinamicElement(Driver, $"({path})[{i}]"));
                }

            });

            return result;
        }
    }    
}
