using API.Interfaces;
using API.Extensions;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub :Hub
    {
        private readonly IMessageRepository _messageRespository;
        public MessageHub(IMessageRepository messageRespository)
        {
            _messageRespository = messageRespository;

        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"];
            var groupName = GetGroupName(Context.User.GetUsername(),otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId,groupName);

            var messages = await _messageRespository.GetMessageThread(Context.User.GetUsername(),otherUser);

            await Clients.Group(groupName).SendAsync("ReceiveMessageThread",messages);
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception); 
        }

        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller,other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }
}