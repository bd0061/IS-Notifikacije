using MailBackend.DTOs;
using MailBackend.Exceptions;
using MailBackend.Models;
using MailBackend.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MailBackend.Controllers
{
    [Route("api/")]
    [ApiController]
    public class MejloviController : ControllerBase
    {
        private readonly IRepository _repository;
        public MejloviController(IRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("Kursevi")]
        public async Task<ActionResult<List<KursDTO>>> GetKurseve()
        {
            try
            {
                var kursevi = await _repository.vratiKurseve();
                return Ok(kursevi);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data");
            }
        }
        [HttpGet("MejlPoSiframa")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<ActionResult<List<StudentDTO>>> vratiMejloveStudenataKojiImajuSifre([FromQuery]List<string> kursSifre)
        {
            try
            {
                return Ok( new ApiResponse<List<StudentDTO>> 
                { 
                    Success = true,
                    Data = await _repository.vratiMejloveStudenataKojiImajuSifre(kursSifre) 
                });
            }
            catch(BadCoursesException e)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Error = new ApiError { ErrorCode = 5, ErrorMessage = e.Message }
                });
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data");
            }
        }
        [HttpPost("Mejl")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<ActionResult> DodajMejl([FromBody] DodajMejlRequest dmr)
        {
            try
            {
                await _repository.dodajMejl(dmr.Mejl, dmr.sifre);
                return Ok(new ApiResponse<string> 
                {   Success = true, 
                    Data = "Dodavanje uspesno." 
                });
            }
            catch(EmptyMailException e1)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Error = new ApiError { ErrorCode = 1, ErrorMessage = e1.Message }
                });
            }
            catch (InvalidMailFormatException e2)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Error = new ApiError { ErrorCode = 2, ErrorMessage = e2.Message }
                });
            }
            catch (NotStudentMailException e3)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Error = new ApiError { ErrorCode = 3, ErrorMessage = e3.Message }
                });
            }
            catch (BadCoursesException e4)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Error = new ApiError { ErrorCode = 4, ErrorMessage = e4.Message }
                });
            }
            catch(Exception)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Error = new ApiError { ErrorCode = -1, ErrorMessage = "Neočekivan error" }
                });
            }
        }
        [HttpGet("verify")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<ActionResult> Verify([FromQuery]string token, [FromQuery]List<string> sifre)
        {
            try
            {
                await _repository.Verify(token,sifre);
                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = "Mejl uspešno verifikovan."
                });
            }
            catch(InvalidTokenException e1)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Error = new ApiError { ErrorCode = 6, ErrorMessage = e1.Message }
                });
            }
            catch (BadCoursesException e2)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Error = new ApiError { ErrorCode = 4, ErrorMessage = e2.Message }
                });
            }
            catch (Exception)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Error = new ApiError { ErrorCode = -1, ErrorMessage = "Neočekivan error" }
                });
            }
        }
        [HttpDelete("unsubscribe")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<ActionResult> Obrisi([FromQuery]string token)
        {
            try
            {
                await _repository.Obrisi(token);
                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = "Uspešno ste se odjavlili sa liste."
                });
            }
            catch(InvalidTokenException e1)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Error = new ApiError { ErrorCode = 6, ErrorMessage = e1.Message }
                });
            }
            catch (Exception)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Error = new ApiError { ErrorCode = -1, ErrorMessage = "Neočekivan error" }
                });
            }
        }
    }
}
