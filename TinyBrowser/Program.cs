using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace TinyBrowser
{
    class Program
    {
        static List<Group> opt = new List<Group>();

        static void Main(string[] args)
        {
            Start();
            Console.ReadKey();
        }

        static void Start()
            {
                var tcpClient = new TcpClient("acme.com", 80);
                var stream = tcpClient.GetStream();

                var bytes = Encoding.ASCII.GetBytes("GET / HTTP/1.1\r\nHost: acme.com\r\n\r\n");
                stream.Write(bytes);

                string httpTitle = Encoding.ASCII.GetString(bytes);
                Console.WriteLine(httpTitle);

                byte[] bytesResult = new byte[124 * 124];
                var totalBytesReceived = 0;
                var bytesReceived = 1;
                while (bytesReceived != 0)
                {
                    bytesReceived = stream.Read(bytesResult, totalBytesReceived,
                        bytesResult.Length - totalBytesReceived);
                    totalBytesReceived += bytesReceived;
                }

                string website = Encoding.ASCII.GetString(bytesResult, 0, totalBytesReceived);
                GetTitleOfWebsite(website, "<title>", "</title>");
                GetHRefConnections(website);
                
                tcpClient.Close();
                stream.Close();
            }

        public static void GetTitleOfWebsite(string text, string firstString, string lastString)
            {
                string content = text;
                string stringFirst = firstString;
                string stringLast = lastString;

                int pos1 = content.IndexOf(stringFirst) + stringFirst.Length;
                int pos2 = content.IndexOf(stringLast);
                string finalString = content.Substring(pos1, pos2 - pos1);
                Console.WriteLine("Title: " + finalString + "\r\n\r\n");
            }
        private static void GetHRefConnections(string website) 
        { 
            Regex regex = new Regex("href\\s*=\\s*(?:\"(?<1>[^\"]*)\"|(?<1>\\S+))", RegexOptions.IgnoreCase);
            Match match;
            for (match = regex.Match(website); match.Success; match = match.NextMatch())
            {
                opt.Add(match);
                Console.WriteLine(opt.IndexOf(match));
                Console.WriteLine("Href found!");
                foreach (Group group in match.Groups)
                {
                    Console.WriteLine("Group: {0}", group);
                }

                Console.WriteLine("\r\n");
            }
            ConnectToHref(opt);
        }

        public static void ConnectToHref(List<Group> hrefs)
        {
            Console.WriteLine("Amount of website connections: " + hrefs.Count);
            while (true)
            {
                Console.WriteLine("Please write a number to pick one of these indexes(starts at 0): ");
                string c = Console.ReadLine();
                int value;

                if (int.TryParse(c, out value))
                {
                    if (value < hrefs.Count && value >= 0)
                    {
                        ExtractHref(hrefs, value);
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Error! This is not a number! Please try again.");
                }
            }
        }

        private static void ExtractHref(List<Group> hrefs, int chosenNumber)
        {
            Console.WriteLine("Chosen number: " + chosenNumber);
            for (int i = 0; i <= hrefs.Count; i++)
            {
                Group chosenConnection = hrefs[chosenNumber];
                EnterHrefTcpClient(chosenConnection);
                break;
            }
        }

        public static void EnterHrefTcpClient(Group hrefConnection)
        {
            var connection = hrefConnection.ToString();
            int i;
            string tcpName = connection.Substring(connection.IndexOf('"') + 1);
            tcpName = tcpName.Remove(tcpName.Length - 1);
            Console.WriteLine(tcpName);
            string[] websiteNames = {"google", "paypal", "mapper.acme"};
            for (i = 0; i <= websiteNames.Length; i++)
            {
                if (tcpName.Contains(websiteNames[i]))
                {
                    switch (i)
                    {
                         case 0:
                            var tcpClient = new TcpClient("google.com", 80);
                            var stream = tcpClient.GetStream();

                            var bytes = Encoding.ASCII.GetBytes("GET / HTTP/1.1\r\nHost: www.google.com\r\n");
                            stream.Write(bytes);
                            string httpRequestTitle = Encoding.ASCII.GetString(bytes);
                            Console.WriteLine(httpRequestTitle);
                            byte[] resultBytes = new byte[224 * 224];
                            var totalBytesReceived = 0;
                            var bytesReceived = 1;
                            while (bytesReceived != 0)
                            {
                                bytesReceived = stream.Read(resultBytes, totalBytesReceived,
                                    resultBytes.Length - totalBytesReceived);
                                totalBytesReceived += bytesReceived;
                                string website = Encoding.ASCII.GetString(resultBytes, 0, totalBytesReceived);
                                Console.WriteLine(website);

                            }

                            tcpClient.Close();
                            stream.Close();
                            break;

                        case 1:
                            var secondtcpClient = new TcpClient("paypal.com", 80);
                            var secondstream = secondtcpClient.GetStream();

                            var secondbytes = Encoding.ASCII.GetBytes("GET / HTTP/1.1\r\nHost: www.paypal.com\r\n");
                            secondstream.Write(secondbytes);
                            string secondhttpRequestTitle = Encoding.ASCII.GetString(secondbytes);
                            Console.WriteLine(secondhttpRequestTitle);
                            byte[] secondresultBytes = new byte[124 * 124];
                            var secondtotalBytesReceived = 0;
                            var secondbytesReceived = 1;
                            while (secondbytesReceived != 0)
                            {
                                bytesReceived = secondstream.Read(secondresultBytes, secondtotalBytesReceived,
                                    secondresultBytes.Length - secondtotalBytesReceived);
                                secondtotalBytesReceived += bytesReceived;
                                string website =
                                    Encoding.ASCII.GetString(secondresultBytes, 0, secondtotalBytesReceived);
                                Console.WriteLine(website);

                            }

                            secondtcpClient.Close();
                            secondstream.Close();
                            break;

                        case 2:
                            var thirdtcpClient = new TcpClient("mapper.acme.com", 80);
                            var thirdstream = thirdtcpClient.GetStream();

                            var thirdbytes = Encoding.ASCII.GetBytes("GET / HTTP/1.1\r\nHost: mapper.acme.com\r\n");
                            thirdstream.Write(thirdbytes);
                            string thirdhttpRequestTitle = Encoding.ASCII.GetString(thirdbytes);
                            Console.WriteLine(thirdhttpRequestTitle);
                            byte[] thirdresultBytes = new byte[224 * 224];
                            var thirdtotalBytesReceived = 0;
                            var thirdbytesReceived = 1;
                            while (thirdbytesReceived != 0)
                            {
                                bytesReceived = thirdstream.Read(thirdresultBytes, thirdtotalBytesReceived,
                                    thirdresultBytes.Length - thirdtotalBytesReceived);
                                thirdtotalBytesReceived += bytesReceived;
                                string website = Encoding.ASCII.GetString(thirdresultBytes, 0, thirdtotalBytesReceived);
                                Console.WriteLine(website);
                            }

                            thirdtcpClient.Close();
                            thirdstream.Close();
                            break;


                    }
                }
                else
                {
                    if (tcpName.Contains("images"))
                    {
                        Console.WriteLine("Image! Try another link: ");
                        ConnectToHref(opt);
                    }
                }
            }

        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
    }
}
