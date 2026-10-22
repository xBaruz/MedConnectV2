// using MedConnect.Core;
// using MedConnect.Core.Commands;
// using MedConnect.Services;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;

// namespace MedConnect.Controllers;

// [ApiController]
// [Route("api/v1/[controller]")]
// public class AppointmentController(IAppointmentService appointmentService) : ControllerBase
// {
//     [Authorize]
//     [HttpGet]
//     public async Task<IActionResult> CreateAppointment([FromQuery] CreateAppointmentCommand command)
//     {
//         var result = await appointmentService.CreateAppointment(command);

//         return result.Match<IActionResult>(
//             success => Ok(success),
//             failure => failure switch
//             {
//                 Error.Validation val => BadRequest(new { message = val.Message }),
//                 Error.Conflict conf => Conflict(new { message = conf.Message }),
//                 _ => StatusCode(500, new { message = "Wystąpił nieoczekiwany błąd." })
//             }
//         );
//     }
// }