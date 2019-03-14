using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using System.Threading;

namespace WindowsFormsApp1
{
    public class FileOperation
    {
       // public Semaphore semaphore;
        private string fileName;

        public string FileName { get => fileName; set => fileName = value; }

        public FileOperation()
        {
            //semaphore = new Semaphore(2, 1);
        }

        public FileOperation(string str)
        {
           // semaphore = new Semaphore(0, 1);
            FileName = str;
        }
        
        public static bool IsOpen(string fileName)
        {
            if (File.Exists(fileName + "Locked"))
                return true;
            return false;
        }
        
        public static void CreateLockMarker(string fileName)
        {
            File.Create(fileName + "Locked").Close();
        }
        public static void DeleteLockMarker(string fileName)
        {
            File.Delete(fileName + "Locked");
        }

        public bool IsOpen()
        {
            if (File.Exists(FileName + "Locked"))
                return true;
            return false;
        }
        public void CreateLockMarker()
        {
            File.Create(FileName + "Locked").Close();
        }
        public void DeleteLockMarker()
        {
            File.Delete(FileName + "Locked");
        }
    }
    

    

    public class cNews
    {
        private string id;
        private string text;
        private List<string> link;
        private List<string> imageLink;

        public string Id { get => id; set => id = value; }
        public string Text { get => text; set => text = value; }
        public List<string> Link { get => link; set => link = value; }
        public List<string> ImageLink { get => imageLink; set => imageLink = value; }

        public cNews()
        {
            Link = new List<string>();
            ImageLink = new List<string>();
        }      
    }
    class CNewsEqualityComparer : IEqualityComparer<cNews>
    {
        public bool Equals(cNews b1, cNews b2)
        {
            return b1.Id.Equals(b2.Id);
        }

        public int GetHashCode(cNews bx)
        {
            int hCode = bx.Id.GetHashCode();
            return hCode.GetHashCode();
        }
    }


}
