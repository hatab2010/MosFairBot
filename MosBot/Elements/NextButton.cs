using OpenQA.Selenium;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Threading;

namespace MosBot.Elements
{
    public class NextButton : DinamicElement
    {
        public static NextButton GetNextButton(IWebDriver driver)
        {
            var path = "//a[@id='button_next']";
            return new NextButton(driver, path);
        }

        private NextButton(IWebDriver driver, string xpath)
            : base(driver, xpath)
        {

        }

        public void Next()
        {

            Interaction((el) =>
            {
                var debug = el.GetAttribute("disabled");
                while (el.GetAttribute("disabled") != null)
                {
                    Thread.Sleep(50);
                }

                el.Click();
            });

        }
    }
}
