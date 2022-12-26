using API.DTOs;
using API.Interfaces;
using API.Helpers;
using API.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using API.Entities;

namespace API.Controllers
{
    public class MessagesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        public MessagesController(IUserRepository userRepository,IMessageRepository messageRepository,IMapper mapper)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _messageRepository = messageRepository;
        }

        
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto){
            var username = User.GetUsername();

            if(username == createMessageDto.RecipientUsername.ToLower()){
                return BadRequest("You can't send messages to yourself");
            }
            
            var sender = await _userRepository.GetUserByUsername(username);
            var recipient = await _userRepository.GetUserByUsername(createMessageDto.RecipientUsername);

            if(recipient == null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            _messageRepository.AddMessage(message);

            if(await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetMessagesForUser([FromQuery]
        MessageParams messageParams){
            
            messageParams.Username = User.GetUsername();
            var messages = await _messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage,messages.PageSize,
            messages.TotalCount,messages.TotalPages);

            return Ok(messages);
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetMessageThread (string username){
            var currentUserName = User.GetUsername();
            return Ok(await _messageRepository.GetMessageThread(currentUserName,username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id){
            var username = User.GetUsername();
            var message = await _messageRepository.GetMessage(id);

            if(message == null){
                return BadRequest("No message");
            }
            if(message.SenderUsername != username && message.RecipientUsername != username){
                return Unauthorized();
            }

            if(message.SenderUsername == username) message.SenderDeleted = true;
            if(message.SenderUsername == username) message.RecipientDeleted = true;

            if(message.SenderDeleted && message.RecipientDeleted){
                _messageRepository.DeleteMessage(message);
            }
            
            if(await _messageRepository.SaveAllAsync()) return Ok(message);

            return BadRequest("Problem deleting the message");
        }
    }
}