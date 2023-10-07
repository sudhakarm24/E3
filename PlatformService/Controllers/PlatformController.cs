using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.DTOs;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        public readonly IPlatformRepo _repo;
        public readonly IMapper _mapper;

        public ICommandDataClient _commandDataClient;

        private readonly IMessageBusClient _messageBusClient;
        public PlatformsController(IPlatformRepo repo, IMapper mapper, ICommandDataClient commandDataClient,
        IMessageBusClient messageBusClient)
        {
            _repo = repo;
            _mapper = mapper;
            _commandDataClient = commandDataClient;
            _messageBusClient = messageBusClient;
        }
        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms(){
            Console.WriteLine("--> getting platforms");

            var platformItems = _repo.GetAllPlatforms();
            return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platformItems));
        }
        [HttpGet("{id}", Name = "GetPlatformById")]
        public ActionResult<PlatformReadDto> GetPlatformById(int id){
           var platform = _repo.GetPlatformById(id);
           if(platform != null){
            return Ok(_mapper.Map<PlatformReadDto>(platform));
           }

           return NotFound();
        }
        [HttpPost]
        public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto platformCreateDto){
            var platFormModel = _mapper.Map<Platform>(platformCreateDto);
            _repo.CreatePlatform(platFormModel);
            _repo.SaveChanges();

            var platFromReadDto = _mapper.Map<PlatformReadDto>(platFormModel);

            try{
               await _commandDataClient.SendPlatformToCommand(platFromReadDto);
            }
            catch(Exception ex){
                Console.WriteLine($"--> Could not send synchronously: {ex.Message}");
            }

            //Send Async Message
            try
            {
                var platformPublishedDto = _mapper.Map<PlatformPublishedDto>(platFromReadDto);
                platformPublishedDto.Event = "Platform_Published";
                _messageBusClient.PublishNewPlatform(platformPublishedDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not send asynchronously: {ex.Message}");
            }

            return CreatedAtRoute(nameof(GetPlatformById), new {id = platFromReadDto.id}, platFromReadDto);
        }
    }
}