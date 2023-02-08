using API.Interfaces;
using API.Extensions;
using Microsoft.AspNetCore.SignalR;
using API.DTOs;
using API.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

namespace API.SignalR
{
    [Authorize]
    public class MessageHub :Hub
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public MessageHub(IMessageRepository messageRepository,IUserRepository userRepository,IMapper mapper)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _messageRepository = messageRepository;

        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"];
            var groupName = GetGroupName(Context.User.GetUsername(),otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId,groupName);

            var messages = await _messageRepository.GetMessageThread(Context.User.GetUsername(),otherUser);

            await Clients.Group(groupName).SendAsync("ReceiveMessageThread",messages);
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception); 
        }

        public async Task SendMessage(CreateMessageDto createMessageDto){
             var username = Context.User.GetUsername();

            if(username == createMessageDto.RecipientUsername.ToLower()){
               throw new HubException("You cannot send messages to yourself");
            }
            
            var sender = await _userRepository.GetUserByUsername(username);
            var recipient = await _userRepository.GetUserByUsername(createMessageDto.RecipientUsername);

            if(recipient == null) throw new HubException("Not found user");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            _messageRepository.AddMessage(message);

            if(await _messageRepository.SaveAllAsync()) 
            {
                var group = GetGroupName(sender.UserName,recipient.UserName);
                await Clients.Group(group).SendAsync("NewMessage",_mapper.Map<MessageDto>(message));
            }
            
            }

        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller,other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }
}