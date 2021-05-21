using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Net.Http.Json;

namespace GithubExplorer
{
    class Program
    {
        static string userName;
        static string token;
        static HttpClient client = new HttpClient();
        static HttpContent content;
        static HttpResponseMessage response;
        static GitHubUser user;
        static JsonSerializerOptions option;
        static string result;

        static async Task Main(string[] args)
        {
            Task t = new Task(HttpGetUser);
            bool enterGithub = false;
            while (!enterGithub)
            {
                Console.WriteLine("Welcome to qreenify's GitHub! Please write a username: ");
                userName = Console.ReadLine();

                Console.WriteLine(userName + " - Great! Enter authorization token now: ");
                token = Console.ReadLine();

                t.Start();
                enterGithub = true;
            }
            GithubOptions();
        }



        public static async void HttpGetUser()
        {
            if (userName != null)
            {
                var URL = "https://api.github.com/users/" + userName;
                Console.WriteLine("GET: " + URL + "\n");
                client.DefaultRequestHeaders.UserAgent.Add(
                    new System.Net.Http.Headers.ProductInfoHeaderValue("GitHubExplorer", "0.1"));
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Token", token);

                client.DefaultRequestHeaders.Add("User-Agent", "C# App");
                response = await client.GetAsync(URL);
                content = response.Content;
                result = await content.ReadAsStringAsync();

                if (result != null)
                {
                    option = new JsonSerializerOptions();
                    option.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    user = JsonSerializer.Deserialize<GitHubUser>(result, option);

                    Console.WriteLine(user + "\n");
                    var r = client.GetStringAsync($"{user.Repos_url}").Result;
                    var l = JsonSerializer.Deserialize<List<Repo>>(r, option);

                    Console.WriteLine("Current Repositories: \n");
                    foreach (var repo in l)
                    {
                        Console.WriteLine("* " + repo.Name);
                    }
                    Console.WriteLine("\n\n");
                    Console.WriteLine("Please choose between number 0 & 3 to check out the repositories: ");
                }
            }
        }

        public static void GithubOptions()
        {
            bool correctChoice = false;
            while (!correctChoice)
            {
                var input = Console.ReadLine();
                int value;

                if (int.TryParse(input, out value))
                {
                    int repositoryNumber = Convert.ToInt32(input);
                    if (repositoryNumber < 0 || repositoryNumber > 3)
                    {
                        Console.WriteLine("This number is out of range. Please try again: \n");
                    }
                    else
                    {
                        ChooseRepository(repositoryNumber);
                        correctChoice = true;
                    }
                }
                else
                {
                    Console.WriteLine("This is not a number. Please try again: \n");
                }
            }
        }

        static void ChooseRepository(int repositoryNumber)
        {
            bool hasChosenAnAction = false;
            option = new JsonSerializerOptions();
            option.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            var r = client.GetStringAsync($"{user.Repos_url}").Result;
            var l = JsonSerializer.Deserialize<List<Repo>>(r, option);
            Repo repo = l[repositoryNumber];

            Console.WriteLine("Repository Name: " + repo.Name);
            if (repo.Description != null)
            {
                Console.WriteLine("Repository description: " + repo.Description);
            }
            else
            {
                Console.WriteLine("Description: No description available");
            }
            Console.WriteLine("Repository html: " + repo.Html_url);
            Console.WriteLine("Repository url: " + repo.Url);
            Console.WriteLine("\n");
            
            Console.WriteLine("Press 0 to exit this repository");
            Console.WriteLine("Want to add a new issue? Press 1.");
            Console.WriteLine("Want to edit an issue? Want to add a comment, or edit or delete  an existing comments? Press 2.");


            while (!hasChosenAnAction)
            {
                var response = Console.ReadLine();
                int value;
                if (int.TryParse(response, out value))
                {
                    int responseValue = Convert.ToInt32(response);
                    if (responseValue != 0 && responseValue != 1 && responseValue != 2)
                    {
                        Console.WriteLine("This is an invalid answer. Please try again.");
                    }
                    else
                    {
                        switch (responseValue)
                        {
                            case 0:
                                Console.WriteLine("Please choose between number 0 & 3 to check out the repositories: ");
                                GithubOptions();
                                break;
                            case 1:
                                CreateNewIssue(repositoryNumber);
                                break;
                            case 2:
                                CreateOrEditIssueandComments(repositoryNumber);
                                break;
                        }

                        hasChosenAnAction = true;
                    }
                }
                else
                {
                    Console.WriteLine("This is not a number. Please try again");
                }
            }
        }

        static void CreateNewIssue(int repositoryNumber)
        {
            var r = client.GetStringAsync($"{user.Repos_url}").Result;
            var l = JsonSerializer.Deserialize<List<Repo>>(r, option);
            Repo repo = l[repositoryNumber];

            foreach (var issueInfo in l)
            {
                var index = issueInfo.Issues_url.IndexOf("{");
                issueInfo.Issues_url = issueInfo.Issues_url.Remove(index);
            }

            Console.WriteLine("\n\n");

            var repoInfo = client.GetStringAsync(l[repositoryNumber].Issues_url).Result;
            var issues = JsonSerializer.Deserialize<List<Issue>>(repoInfo, option);

            Console.WriteLine("Current issues in " + l[repositoryNumber].Issues_url + ":");
            foreach (var issue in issues)
            {
                Console.WriteLine($"Title: {issue.title}\r\nInfo: {issue.body} \n");
            }

            Console.WriteLine("\n\n");
            var newIssue = new Issue();
            Console.WriteLine("Start by entering the Title to your issue: ");
            string Title = Console.ReadLine();
            newIssue.title = Title;
            Console.WriteLine("Enter the Body to your issue:");
            string Body = Console.ReadLine();
            newIssue.body = Body;
            var response = client.PostAsJsonAsync(repo.Issues_url, newIssue).Result;
            Console.WriteLine("\n\n");
            if (response.StatusCode == HttpStatusCode.Created)
            {
                Console.WriteLine("Title: " + newIssue.title);
                Console.WriteLine("Info: " + newIssue.body);
            }

            Console.WriteLine("Please choose between number 0 & 3 to check out the repositories: ");
            GithubOptions();
        }

        static void CreateOrEditIssueandComments(int repositoryNumber)
        {
            option = new JsonSerializerOptions();
            option.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            bool choiceHavebeenMade = false;
            var r = client.GetStringAsync($"{user.Repos_url}").Result;
            var l = JsonSerializer.Deserialize<List<Repo>>(r, option);

            Repo repo = l[repositoryNumber];

            var issues = repo.GetIssues(repositoryNumber);
            foreach (var issue in issues)
            {
                var comments = issue.GetComments();
                while (!choiceHavebeenMade)
                {
                    Console.WriteLine("What would you like to do?\n\n " +
                                      "0: edit an existing comment?\n " +
                                      "1: create a comment?\n " +
                                      "2: delete a comment?\n " +
                                      "3: edit an existing issue?\n " +
                                      "4: Exit and return to Repositories?\n");

                    var chosenResponse = Console.ReadLine();
                    int value;
                    if (int.TryParse(chosenResponse, out value))
                    {
                        int convertedResponse = Convert.ToInt32(chosenResponse);
                        if (convertedResponse != 0 && convertedResponse != 1 && convertedResponse != 2 &&
                            convertedResponse != 3 && convertedResponse != 4)
                        {
                            Console.WriteLine("This is an invalid number. Please try again");
                        }
                        else
                        {
                            switch (convertedResponse)
                            {
                                case 0:
                                    if (comments.Any())
                                    {
                                        Console.WriteLine("Please enter a comment ID: ");
                                        var readPatchLine = Console.ReadLine();
                                        int patchValue;
                                        foreach (var comment in comments)
                                        {
                                            if (int.TryParse(readPatchLine, out patchValue))
                                            {
                                                int readValue = Convert.ToInt32(readPatchLine);

                                                if (comment.id == readValue)
                                                {
                                                    comment.PatchComment();
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("No comments available to patch.");
                                            }
                                        }
                                    }
                                    break;
                                
                                case 1:
                                    Console.WriteLine("Type in an issue ID: ");
                                    var readCreateCommentLine = Console.ReadLine();
                                    int createCommentID;
                                    if (int.TryParse(readCreateCommentLine, out createCommentID))
                                    {
                                        int createCommentValue = Convert.ToInt32(readCreateCommentLine);

                                        foreach (var createCommentIssue in issues)
                                        {
                                            if (createCommentIssue.number == createCommentValue)
                                            {
                                                createCommentIssue.CreateComment();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("No comments available to delete.");
                                    }
                                    break;

                                case 2:
                                    Console.WriteLine("Type in a comment ID: ");
                                    var readDeleteLine = Console.ReadLine();
                                    int deleteID;
                                    if (int.TryParse(readDeleteLine, out deleteID))
                                    {
                                        foreach (var comment in comments)
                                        {

                                            int readValue = Convert.ToInt32(readDeleteLine);

                                            if (comment.id == readValue)
                                            {
                                                comment.DeleteComment();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("No comments available to delete.");
                                    }
                                    break;
                                
                                case 3:
                                    Console.WriteLine("Type in the issue ID: ");
                                    var readIssueLine = Console.ReadLine();
                                    int editIssueID;
                                    if ((int.TryParse(readIssueLine, out editIssueID)))
                                    {
                                        int readEditIssueValue = Convert.ToInt32(editIssueID);
                                        foreach (var editIssue in issues)
                                        {
                                            if (editIssue.number == readEditIssueValue)
                                            {
                                                editIssue.UpdateIssue();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("No issue available to update.");
                                    }
                                    break;

                                case 4:
                                    Console.WriteLine(
                                        "Please choose between number 0 & 3 to check out the repositories: ");
                                    GithubOptions();
                                    break;
                            }
                            comments = new List<Comment>();
                            Console.WriteLine("Please choose between number 0 & 3 to check out the repositories: ");
                            GithubOptions();
                            choiceHavebeenMade = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("This is not a number. Please try again.");
                    }
                }
            }
            Console.WriteLine("Please choose between number 0 & 3 to check out the repositories: ");
            GithubOptions();
        }

        public class Issue
        {
            public string title { get; set; }
            public string body { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public string comments_url { get; set; }
            public Uri url { get; set; }
            public int number { get; set; }

            public List<Comment> GetComments()
            {
                JsonSerializerOptions options;
                options = new JsonSerializerOptions();
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

                var commentJSon = client.GetStringAsync(comments_url).Result;
                var comments = JsonSerializer.Deserialize<List<Comment>>(commentJSon, options);
                var list = new List<Comment>();
                Console.WriteLine("Comments: \n");
                if (!comments.Any())
                {
                    Console.WriteLine("No comments available.");
                }

                foreach (var comment in comments)
                {
                    Console.WriteLine($"Comment : {comment.body}\r\n" +
                                      $"Created at : {comment.created_at}\r\n" +
                                      $"ID : {comment.id}\r\n" +
                                      $"----------------------");
                    list.Add(comment);
                }
                return list;
            }
            
            public void CreateComment()
            {
                Console.WriteLine("Write your comment:");
                var comment = Console.ReadLine();
                var newComment = new Comment();
                newComment.body = comment;
                var response = client.PostAsJsonAsync(comments_url, newComment).Result;
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    Console.WriteLine("Congratulations! You created a comment!");
                }
                else
                {
                    Console.WriteLine("Ah shoot! You could not create comment.");
                }
            }

            public void UpdateIssue()
            {
                Console.WriteLine("Enter new title: ");
                var title = Console.ReadLine();
                Console.WriteLine("Enter new body: ");
                var body = Console.ReadLine();
                var newIssue = new Issue();
                newIssue.title = title;
                newIssue.body = body;
                var x = client.PostAsJsonAsync(url, newIssue).Result;
                if (x.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Issue has been successfully updated.");
                }
                else
                {
                    Console.WriteLine("Ah shoot! Issue was not updated.");
                }
            }
        }

        public class Comment
        {
            public int id { get; set; }
            public string body { get; set; }
            public Uri url { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public string comments_url { get; set; }
            public void PatchComment()
            {
                Console.WriteLine("Enter your comment: ");
                var body = Console.ReadLine();
                var newComment = new Comment();
                newComment.body = body;

                var response = client.PostAsJsonAsync(url, newComment).Result;
                Console.WriteLine("\n\n");
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    Console.WriteLine("Your comment: " + newComment.body);
                }
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
            }

            public void DeleteComment()
            {
                var response = client.DeleteAsync(url).Result;
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    Console.WriteLine("Congratulations! You deleted a comment!");
                }
                else
                {
                    Console.WriteLine("A shoot! You could not delete the comment!");
                }
            }
        }

        public class Repo
        {
            public string Name { get; set; }
            public string Html_url { get; set; }
            public string Url { get; set; }
            public string Description { get; set; }
            public string Issues_url { get; set; }
            public string Comments_url { get; set; }
            JsonSerializerOptions options = new JsonSerializerOptions();
            
            public List<Issue> GetIssues(int repositoryNumber)
            {
                var r = client.GetStringAsync($"{user.Repos_url}").Result;
                var l = JsonSerializer.Deserialize<List<Repo>>(r, option);
                foreach (var issueInfo in l)
                {
                    var index = issueInfo.Issues_url.IndexOf("{");
                    issueInfo.Issues_url = issueInfo.Issues_url.Remove(index);
                }

                Console.WriteLine("\n\n");

                var repoInfo = client.GetStringAsync(l[repositoryNumber].Issues_url).Result;
                var issues = JsonSerializer.Deserialize<List<Issue>>(repoInfo, option);

                Console.WriteLine("Current issues in " + l[repositoryNumber].Issues_url);
                foreach (var issue in issues)
                {
                    Console.WriteLine($"Title: {issue.title}\r\nInfo: {issue.body} \n");
                }

                var list = new List<Issue>();
                foreach (var issue in issues)
                {
                    list.Add(issue);
                    Console.WriteLine($"Title : {issue.title}\r\n" +
                                      $"Id : {issue.number}\r\n" +
                                      $"----------------------");
                }

                return list;
            }
        }

    }

    public class GitHubUser
    {
        public string Login { get; set; }
        public string Repos_url { get; set; }
        public string Followers_url { get; set; }
        public string Organinzations_url { get; set; }
        public string Company { get; set; }
        public string Blog { get; set; }
        public string Location { get; set; }
        public string Email { get; set; }
        public string Hireable { get; set; }
        public string Bio { get; set; }
        public int Public_repos { get; set; }
        public int Followers { get; set; }
        public int Following { get; set; }
        public string Created_at { get; set; }
        public string Updated_at { get; set; }

        string PrintToConsole()
        {
            if (string.IsNullOrEmpty(Company))
            {
                Company = "Not available";
            }

            if (string.IsNullOrEmpty(Location))
            {
                Location = "Not available";
            }

            if (string.IsNullOrEmpty(Email))
            {
                Email = "Not available";
            }

            if (string.IsNullOrEmpty(Blog))
            {
                Blog = "Not available";
            }

            if (string.IsNullOrEmpty(Hireable))
            {
                Hireable = "Unknown";
            }

            if (string.IsNullOrEmpty(Bio))
            {
                Bio = "Not available";
            }

            return $"User : {Login}\r\n" +
                   $"Location : {Location}\r\n" +
                   $"Email : {Email}\r\n" +
                   $"Hireable : {Hireable}\r\n" +
                   $"Blog : {Blog}\r\n" +
                   $"Bio : {Bio}\r\n" +
                   $"Public Repos : {Public_repos}\r\n" +
                   $"Followers : {Followers}\r\n" +
                   $"Following : {Following}\r\n" +
                   $"Created at : {Created_at}\r\n" +
                   $"Updated at : {Updated_at}";
        }

        public override string ToString()
        {
            return PrintToConsole();
        }
    }
}
