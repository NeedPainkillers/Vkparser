using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace WindowsFormsApp1
{

    public partial class Form1 : Form
    {
        //  private 
        private ChromeDriver ChDriver;
        public Form1()
        {
            InitializeComponent();

        }

        public List<cNews> UnitePosts(string fileName, List<cNews> parcedPosts)
        {
            if (!File.Exists(fileName))
            {
                return parcedPosts;
            }
            List<cNews> writtenPosts = new List<cNews>();

            string serializedWrittenPosts = File.ReadAllText(fileName);

            writtenPosts = JsonConvert.DeserializeObject<List<cNews>>(serializedWrittenPosts);

            return parcedPosts.Union(writtenPosts, new CNewsEqualityComparer()).ToList();
        }

        public List<IWebElement> FindIWIWebElementByClassName(IWebElement element, string className)
        {
            return (from item in element.FindElements(By.ClassName(className)) where item.Displayed && item.Enabled select item).ToList();
        }

        private void WriteToJson(String fileName, byte mod, List<cNews> news, ref FileOperation fileOperation)
        {
            bool lockTaken = false;

            try
            {
                Monitor.TryEnter(fileOperation, -1, ref lockTaken);
                if (lockTaken)
                {


                    while (fileOperation.IsOpen())
                    {
                        Thread.Sleep(2000);
                    }
                    // fileIsOpen = true;
                    fileOperation.CreateLockMarker();

                    List<cNews> toBeWritten = UnitePosts(fileName, news);

                    Action breakThisShittyCode = () =>
                    {
                        FileStream forBreakingGoals = File.Open(fileName, FileMode.Append);
                        Thread.Sleep(3000);
                        forBreakingGoals.Close();
                    };
                    Action action;
                    switch (mod)
                    {
                        case 1:
                            
                            //var aCText = new { id = "" , text = ""};
                            //var aCTextList = (new[] { aCText }).ToList();
                            //foreach (cNews item in news)
                            //{
                            //    aCTextList.Add(new { id = item.id, text = item.text });
                            //}
                            //aCTextList.Remove(new { id = "", text = "" });
                            //File.AppendAllText(fileName, JsonConvert.SerializeObject(aCTextList, Formatting.Indented));

                            breakThisShittyCode();
                            File.WriteAllText(fileName, JsonConvert.SerializeObject(toBeWritten.Select(cNews => new { cNews.Id, cNews.Text }), Formatting.Indented));

                            action = () => { textBox4.Text = Convert.ToString(toBeWritten.Count); progressBar1.Value += 30; };

                            if (textBox1.InvokeRequired || progressBar1.InvokeRequired)
                                Invoke(action);
                            else
                                action();

                            break;
                        case 2:
                            breakThisShittyCode();
                            File.WriteAllText(fileName, JsonConvert.SerializeObject(toBeWritten.Select(cNews => new { cNews.Id, cNews.Link }), Formatting.Indented));
                            //File.AppendAllText(fileName, new JavaScriptSerializer().Serialize(news.Select(cNews => new { cNews.id, cNews.link }))); 

                            action = () => { progressBar1.Value += 30; };

                            if (textBox1.InvokeRequired || progressBar1.InvokeRequired)
                                Invoke(action);
                            else
                                action();

                            break;
                        case 3:
                            breakThisShittyCode();
                            File.WriteAllText(fileName, JsonConvert.SerializeObject(toBeWritten.Select(cNews => new { cNews.Id, cNews.ImageLink }), Formatting.Indented));

                            action = () => { progressBar1.Value += 30; };

                            if (textBox1.InvokeRequired || progressBar1.InvokeRequired)
                                Invoke(action);
                            else
                                action();

                            break;
                        default:
                            break;

                    }

                    //fileIsOpen = false;

                    fileOperation.DeleteLockMarker();
                }
                else
                {
                    Action debug = () => MessageBox.Show("something went wrong");
                    if (InvokeRequired)
                        Invoke(debug);
                    else
                        debug();
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(fileOperation);
                }
            }

        }
        /*ref bool[] fileIsOpen,*/
        private void ReadFromJson(string[] fileName, ref FileOperation[] fileOperations)
        {
            //    Thread.Sleep(3000);
            TextBox[] textBoxes = new TextBox[3] { textBox1, textBox2, textBox3 };
            bool[] fileIsReaded = new bool[3] { false, false, false };
            bool lockTaken;
            while (!(fileIsReaded[0] && fileIsReaded[1] && fileIsReaded[2]))
            {
                for (int i = 0; i < 3; i++)
                {
                    //Thread.Sleep(100);
                    lockTaken = false;

                    try
                    {
                        Monitor.TryEnter(fileOperations[i], 100, ref lockTaken);
                        if (fileOperations[i].IsOpen() || !lockTaken)
                            continue;
                        //fileIsOpen[i] = true;
                        fileOperations[i].CreateLockMarker();

                        Action action = () => textBoxes[i].Text = File.ReadAllText(fileName[i]);
                        if (InvokeRequired)
                        {
                            Invoke(action);
                            fileIsReaded[i] = true;
                        }
                        else
                            action();
                        // fileIsOpen[i] = false;
                        fileOperations[i].DeleteLockMarker();
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            Monitor.Exit(fileOperations[i]);
                        }
                    }
                }
                Thread.Sleep(3000);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            List<cNews> news;
            news = new List<cNews>();
            foreach (var elem in ChDriver.FindElements(By.ClassName("wall_post_cont")))
            {
                cNews toBeAddedObject = new cNews();
                List<IWebElement> postTexts = FindIWIWebElementByClassName(elem, "wall_post_text");
                List<IWebElement> imagLinks = FindIWIWebElementByClassName(elem, "page_post_sized_thumbs");

                toBeAddedObject.Id = elem.GetAttribute("id");

                if (postTexts.Any())
                {
                    toBeAddedObject.Text = postTexts[0].Text;

                    //foreach (IWebElement link in postTexts[0].FindElements(By.TagName("a")))
                    //{
                    //    if (string.IsNullOrEmpty(link.GetAttribute("href")))
                    //        continue;
                    //    toBeAddedObject.link.Add(link.GetAttribute("href"));//linq
                    //}
                    toBeAddedObject.Link.AddRange((from link in postTexts[0].FindElements(By.TagName("a"))
                                                   where !string.IsNullOrEmpty(link.GetAttribute("href"))
                                                   select link.GetAttribute("href")));
                }

                if (imagLinks.Any())
                {
                    foreach (IWebElement iml in imagLinks[0].FindElements(By.TagName("a")))
                    {
                        string temp = iml.GetCssValue("background-image");
                        if (!string.IsNullOrEmpty(temp) && temp.Length > 7)
                        {
                            temp = temp.Substring(5);
                            temp = temp.Remove(temp.Length - 2);
                            toBeAddedObject.ImageLink.Add(temp);
                        }
                    }
                }
                if (toBeAddedObject.ImageLink.Any() || toBeAddedObject.Link.Any() || !string.IsNullOrEmpty(toBeAddedObject.Text))
                {
                    news.Add(toBeAddedObject);
                }

            }
            progressBar1.Value = 5;

            // bool[] fileIsBusy = new bool[3] { false, false, false };
            string[] fileName = new string[3] { "JStext.txt", "JSlink.txt", "JSimageLink.txt" };
            for (int i = 0; i < 3; i++)
            {
                if (FileOperation.IsOpen(fileName[i]))
                    FileOperation.DeleteLockMarker(fileName[i]);
            }


            // writeToJson(fileName[0], 1, news, ref fileIsBusy[0]);
            FileOperation[] fileOperations = new FileOperation[3] {
                 new FileOperation(fileName[0]),
                 new FileOperation(fileName[1]),
                 new FileOperation(fileName[2]), };


            Task[] tasks = new Task[4]
            {
                new Task(() => WriteToJson(fileName[0], 1, news, ref fileOperations[0])),
                new Task(() => WriteToJson(fileName[1], 2, news, ref fileOperations[1])),
                new Task(() => WriteToJson(fileName[2], 3, news, ref fileOperations[2])),
                new Task(() => ReadFromJson(fileName, ref fileOperations)),
            };

            //Task.Factory.StartNew(() => writeToJson("JStext.txt", 1, news,));
            //Task.Factory.StartNew(() => writeToJson("JSlink.txt", 2, news,));
            //Task.Factory.StartNew(() => writeToJson("JSimageLink.txt", 3, news,));

            for (int i = 0; i < 4; i++)
            {
                tasks[i].Start();
            }
            progressBar1.Value += 5;
        }

        public delegate IWebElement findElements(string by);

        private void button2_Click(object sender, EventArgs e)
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--disk-cache-size=1", "--incognito");
            ChDriver = new ChromeDriver(options);
            ChDriver.Navigate().GoToUrl("https://vk.com/feed");
            Thread.Sleep(1500);

            findElements findElementById = id => ChDriver.FindElement(By.Id(id));

            string mail = "";
            string pass = "";

            findElementById("email").SendKeys(mail);
            findElementById("pass").SendKeys(pass);

            // findElements findElementClick = id => ChDriver.FindElement(By.Id(id)).Click();
            findElementById("login_button").Click();

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void button3_Click(object sender, EventArgs e)
        {
            int iterator;
            if (Int32.TryParse(maxPosts.Text, out iterator))
            {
                IJavaScriptExecutor js = ChDriver as IJavaScriptExecutor;
                for (int i = 0; i < iterator; i++)
                {
                    Thread.Sleep(100);
                    js.ExecuteScript("feed.showMore(10)");
                }
                maxPosts.Text = "Done";
            }
            else
            {
                maxPosts.Text = "Error";
            }
        }
    }
   
   
}

//deserializedPosts = JsonConvert.DeserializeObject<List<C_Post>>(fileText);