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
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public MessagesController(IMapper mapper,IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto){
            var username = User.GetUsername();

            if(username == createMessageDto.RecipientUsername.ToLower()){
                return BadRequest("You can't send messages to yourself");
            }
            
            var sender = await _unitOfWork.UserRepository.GetUserByUsername(username);
            var recipient = await _unitOfWork.UserRepository.GetUserByUsername(createMessageDto.RecipientUsername);

            if(recipient == null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            _unitOfWork.MessageRepository.AddMessage(message);

            if(await _unitOfWork.Complete()) return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetMessagesForUser([FromQuery]
        MessageParams messageParams){
            
            messageParams.Username = User.GetUsername();
            var messages = await _unitOfWork.MessageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage,messages.PageSize,
            messages.TotalCount,messages.TotalPages);

            return Ok(messages);
        }
        
        
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id){
            var username = User.GetUsername();
            var message = await _unitOfWork.MessageRepository.GetMessage(id);

            if(message == null){
                return BadRequest("No message");
            }
            if(message.SenderUsername != username && message.RecipientUsername != username){
                return Unauthorized();
            }

            if(message.SenderUsername == username) message.SenderDeleted = true;
            if(message.SenderUsername == username) message.RecipientDeleted = true;

            if(message.SenderDeleted && message.RecipientDeleted){
                _unitOfWork.MessageRepository.DeleteMessage(message);
            }
            
            if(await _unitOfWork.Complete()) return Ok(message);

            return BadRequest("Problem deleting the message");
        }
    }
}