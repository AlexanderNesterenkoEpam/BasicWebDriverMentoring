using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Yandex
{
	[TestFixture]
	public class YandexTests
	{
		private IWebDriver _driver;
		private string _baseUrl;
		private string _login = "nesterenko.mentoring";
		private string _password = "mentoring2017";

		[SetUp]
		public void SetupTest()
		{
			SetBrowser();

			this._baseUrl = "https://mail.yandex.com/";
			
			this._driver.Navigate().GoToUrl(this._baseUrl);

			this._driver.Manage().Window.Maximize();
		}

		[Test]
		public void Test_LogIn()
		{
			LogIn(_login, _password);

			// Assert that login is successful
			Assert.IsTrue(_driver.Url.Contains("#inbox"));
		}

		[Test]
		public void Test_EmailPresentsInDraftsFolder()
		{
			LogIn(_login, _password);

			// Compose an Email
			string adressee = "Selvin95@mail.ru";
			string subject = "Greeting";
			string body = "Hello, Alex :)";

			ComposeEmail(adressee, subject, body);

			NavigateToDrafts();
			
			AcceptNotificationAlert();

			IList<IWebElement> listOfDraftsNames = _driver
			.FindElements(By.XPath(".//*[@class = 'mail-MessageSnippet-FromText']"));
			string lastEmailAddressee = listOfDraftsNames.ElementAt(0).Text;

			Assert.AreEqual(lastEmailAddressee.ToLower(), adressee.ToLower());
		}

		[Test]
		public void Test_CheckMailContent()
		{
			LogIn(_login, _password);

			// Compose an Email
			string adressee = "Selvin95@mail.ru";
			string subject = "Greeting";
			string body = "Hello, Alex :)";

			ComposeEmail(adressee, subject, body);

			NavigateToDrafts();

			AcceptNotificationAlert();

			OpenLastDraftNote(adressee);

			Assert.IsTrue(CheckAdresseeSubjectBody(adressee, subject, body));
		}

		[Test]
		public void Test_SendEmail()
		{
			LogIn(_login, _password);
			_driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(3));

			// Compose an Email
			string adressee = "Selvin95@mail.ru";
			string subject = "Greeting";
			string body = "Hello, Alex :)";

			ComposeEmail(adressee, subject, body);

			NavigateToDrafts();

			AcceptNotificationAlert();

			OpenLastDraftNote(adressee);

			IWebElement sendButton = _driver.FindElement(By.XPath(".//*[contains(@title, 'Send message')]"));
			sendButton.Click();

			WaitForElementIsVisible(By.XPath(".//*[@class = 'b-messages__placeholder-item']"), 15);
			IWebElement noMessagesInDrafts = _driver.FindElement(By.XPath(".//*[@class = 'b-messages__placeholder-item']"));

			Assert.IsNotEmpty(noMessagesInDrafts.Text);

			IWebElement sent_Link = _driver.FindElement(By.XPath(".//*[contains(@href, '#sent')]"));
			sent_Link.Click();

			IList<IWebElement> sentToEmails_Links = _driver.FindElements(By.ClassName("mail-MessageSnippet-FromText"));
			Assert.AreEqual(sentToEmails_Links.First().Text.ToLower(), adressee.ToLower()); 
		}

		[TearDown]
		public void CleanUp()
		{
			SignOut();
			this._driver.Quit();
			_driver = null;
		}

		#region Methods

		private void LogIn(string login, string password)
		{
			IWebElement loginInput = this._driver.FindElement(By.Name("login"));
			IWebElement passwordInput = this._driver.FindElement(By.Name("passwd"));
			IWebElement authButton = this._driver.FindElement(By.XPath(".//*[@type = 'submit']"));

			loginInput.Clear();
			loginInput.SendKeys(login);
			passwordInput.Clear();
			passwordInput.SendKeys(password);
			authButton.Click();

			WaitForElementIsVisible(By.XPath(".//*[contains(@href, '#compose')]"), 5);
		}

		private void ComposeEmail(string adressee, string subject, string body)
		{
			IWebElement button_Compose = _driver.FindElement(By.XPath(".//*[@title = 'Compose (w, c)']"));
			button_Compose.Click();

			WaitForElementIsVisible(By.Name("to"), 2);

			// Fill adressee, subject and body fields
			IWebElement input_To = _driver.FindElement(By.Name("to"));
			input_To.Clear();
			input_To.SendKeys(adressee);

			IWebElement input_Subject = _driver.FindElement(By.Name("subj"));
			input_Subject.Clear();
			input_Subject.SendKeys(subject);

			IWebElement input_Body = _driver.FindElement(By.XPath(".//*[@role='textbox']"));
			input_Body.Clear();
			input_Body.SendKeys(body);
		}

		private void NavigateToDrafts()
		{
			IWebElement drafts_Link = _driver
			.FindElement(By.XPath("	.//*[contains(@href, '#draft')]"));
			drafts_Link.Click();
		}


		private void AcceptNotificationAlert()
		{
			IWebElement saveAndGo_Button = _driver.FindElement(By.XPath(".//*[@data-action='save']"));
			saveAndGo_Button.Click();
			WaitForElementIsVisible(By.XPath(".//*[@class = 'mail-MessageSnippet-FromText']"), 10);
		}

		public void OpenLastDraftNote(string email)
		{
			IWebElement lastElement = _driver.FindElement(By.XPath(".//span[@title ='" + email.ToLower() + "']"));
			lastElement.Click();
		}

		private bool CheckAdresseeSubjectBody(string adressee, string subject, string body)
		{
			WaitForElementIsVisible(By.Name("to"), 4);
			IWebElement inputTo = _driver.FindElement(By.Name("to"));
			IWebElement inputSubject = _driver.FindElement(By.Name("subj"));
			IWebElement inputBody = _driver.FindElement(By.XPath(".//*[@role='textbox']"));

			return (adressee.Equals(inputTo.Text) | subject.Equals(inputSubject.Text) | body.Equals(inputBody.Text));
		}

		private void WaitForElementIsVisible(By by, int seconds)
		{
			WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(seconds));
			wait.Until(ExpectedConditions.ElementIsVisible(by));
		}

		private void SetBrowser()
		{
			var service = ChromeDriverService.CreateDefaultService();
			this._driver = new ChromeDriver(service);
		}

		private void SignOut()
		{
			_driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(5));
			IWebElement mailUserName = _driver.FindElement(By.ClassName("mail-User-Name"));
			mailUserName.Click();

			IWebElement LogOut_Link = _driver
			.FindElement(By.XPath(".//*[@class ='_nb-popup-content']//div[9]/a"));
			LogOut_Link.Click();
		}
		#endregion
	}
}