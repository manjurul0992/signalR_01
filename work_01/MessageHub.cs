using Microsoft.AspNetCore.SignalR;
using work_01.Models;

namespace work_01
{
    public class MessageHub : Hub
    {
        private static readonly Dictionary<string, string> users = new Dictionary<string, string>();
        private readonly IWebHostEnvironment env;
        public MessageHub(IWebHostEnvironment env)
        {
            this.env = env;
        }
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Connected", "You are connected!!!");
        }
        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            users.Remove(Context.ConnectionId);
            await Clients.All.SendAsync("UpdateUsers", users.Values.ToArray());
        }

        public async Task AddMe(string username)
        {
            if (users.Values.Contains(username))
            {
                await Clients.Caller.SendAsync("DuplicateUser", "Choose a different name!!");
            }
            else
            {
                users.Add(Context.ConnectionId, username);
                await Clients.Caller.SendAsync("Joined", "You are joined!!");
                await Clients.All.SendAsync("UpdateUsers", users.Values.ToArray());
            }
        }
        public async Task Send(string user, string message)
        {
            await Clients.All.SendAsync("Message", user, message);
        }
        public async Task Upload(string user, ImageData data)
        {
            string path = Path.Combine(this.env.WebRootPath, "Images");
            path = Path.Combine(path, data.FileName);
            data.Image = data.Image.Substring(data.Image.LastIndexOf(',') + 1);
            string converted = data.Image.Replace('-', '+');
            converted = converted.Replace('_', '/');
            byte[] buffer = Convert.FromBase64String(converted);
            FileStream fs = new FileStream($"{path}", FileMode.Create);
            fs.Write(buffer, 0, buffer.Length);
            fs.Close();

            if (data.FileName.Contains(".jpg") || data.FileName.Contains(".png") || data.FileName.Contains(".gif"))
            {
                await Clients.All.SendAsync("Message", user, $"<a target='_blank' href='/Images/{data.FileName}'><img src='/Images/{data.FileName}' width='40px' class='img-thumbnail-circle' /></a>");
            }
            else
            {
                await Clients.All.SendAsync("Message", user, $"<a target='_blank' href='/Images/{data.FileName}'><img src='/Images/docs.png' width='40px' class='img-thumbnail-circle' /></a>");
            }

        }
    }
}
