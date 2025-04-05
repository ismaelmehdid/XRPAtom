using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XRPAtom.Core.Interfaces;
using XRPAtom.Core.DTOs;
using XRPAtom.Core.Domain;
using System.Security.Claims;

namespace XRPAtom.API.Controllers
{
    [ApiController]
    [Route("api/devices")]
    [Authorize]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceService _deviceService;
        private readonly ILogger<DeviceController> _logger;

        public DeviceController(
            IDeviceService deviceService, 
            ILogger<DeviceController> logger)
        {
            _deviceService = deviceService;
            _logger = logger;
        }

        /// <summary>
        /// Get all devices for the current user
        /// </summary>
        /// <returns>List of user's devices</returns>
        [HttpGet]
        public async Task<IActionResult> GetUserDevices()
        {
            try 
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Invalid user identifier" });
                }

                var devices = await _deviceService.GetDevicesByUserIdAsync(userId);
                return Ok(devices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user devices");
                return StatusCode(500, new { error = "An unexpected error occurred while retrieving devices" });
            }
        }

        /// <summary>
        /// Get a specific device by ID
        /// </summary>
        /// <param name="deviceId">Device ID</param>
        /// <returns>Device details</returns>
        [HttpGet("{deviceId}")]
        public async Task<IActionResult> GetDeviceById(string deviceId)
        {
            try 
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                var device = await _deviceService.GetDeviceByIdAsync(deviceId);
                
                if (device == null)
                {
                    return NotFound(new { error = "Device not found" });
                }

                // Ensure user can only access their own devices
                if (device.UserId != userId)
                {
                    return Forbid();
                }

                return Ok(device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving device {deviceId}");
                return StatusCode(500, new { error = "An unexpected error occurred while retrieving the device" });
            }
        }

        /// <summary>
        /// Create a new device
        /// </summary>
        /// <param name="createDeviceDto">Device creation details</param>
        /// <returns>Created device</returns>
        [HttpPost]
        public async Task<IActionResult> CreateDevice([FromBody] CreateDeviceDto createDeviceDto)
        {
            try 
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Invalid user identifier" });
                }

                // Override UserId with the authenticated user's ID for security
                createDeviceDto.UserId = userId;

                var device = await _deviceService.CreateDeviceAsync(createDeviceDto);
                return CreatedAtAction(nameof(GetDeviceById), new { deviceId = device.Id }, device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating device");
                return StatusCode(500, new { error = "An unexpected error occurred while creating the device" });
            }
        }

        /// <summary>
        /// Update an existing device
        /// </summary>
        /// <param name="deviceId">Device ID to update</param>
        /// <param name="updateDeviceDto">Device update details</param>
        /// <returns>Updated device</returns>
        [HttpPut("{deviceId}")]
        public async Task<IActionResult> UpdateDevice(
            string deviceId, 
            [FromBody] UpdateDeviceDto updateDeviceDto)
        {
            try 
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                // Verify the device exists and belongs to the user
                var existingDevice = await _deviceService.GetDeviceByIdAsync(deviceId);
                
                if (existingDevice == null)
                {
                    return NotFound(new { error = "Device not found" });
                }

                if (existingDevice.UserId != userId)
                {
                    return Forbid();
                }

                var updatedDevice = await _deviceService.UpdateDeviceAsync(deviceId, updateDeviceDto);
                return Ok(updatedDevice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating device {deviceId}");
                return StatusCode(500, new { error = "An unexpected error occurred while updating the device" });
            }
        }

        /// <summary>
        /// Delete a device
        /// </summary>
        /// <param name="deviceId">Device ID to delete</param>
        /// <returns>Success or error message</returns>
        [HttpDelete("{deviceId}")]
        public async Task<IActionResult> DeleteDevice(string deviceId)
        {
            try 
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                // Verify the device exists and belongs to the user
                var existingDevice = await _deviceService.GetDeviceByIdAsync(deviceId);
                
                if (existingDevice == null)
                {
                    return NotFound(new { error = "Device not found" });
                }

                if (existingDevice.UserId != userId)
                {
                    return Forbid();
                }

                var result = await _deviceService.DeleteDeviceAsync(deviceId);
                
                if (!result)
                {
                    return BadRequest(new { error = "Failed to delete device" });
                }

                return Ok(new { message = "Device deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting device {deviceId}");
                return StatusCode(500, new { error = "An unexpected error occurred while deleting the device" });
            }
        }

        /// <summary>
        /// Update device enrollment status
        /// </summary>
        /// <param name="deviceId">Device ID</param>
        /// <param name="enrollmentDto">Enrollment status</param>
        /// <returns>Success or error message</returns>
        [HttpPatch("{deviceId}/enrollment")]
        public async Task<IActionResult> UpdateDeviceEnrollment(
            string deviceId, 
            [FromBody] DeviceEnrollmentDto enrollmentDto)
        {
            try 
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                // Verify the device exists and belongs to the user
                var existingDevice = await _deviceService.GetDeviceByIdAsync(deviceId);
                
                if (existingDevice == null)
                {
                    return NotFound(new { error = "Device not found" });
                }

                if (existingDevice.UserId != userId)
                {
                    return Forbid();
                }

                var result = await _deviceService.EnrollDeviceAsync(deviceId, enrollmentDto.Enrolled);
                if (!result)
                {
                    return BadRequest(new { error = "Failed to update device enrollment" });
                }

                return Ok(new { 
                    message = $"Device {(enrollmentDto.Enrolled ? "enrolled" : "unenrolled")} successfully",
                    enrolled = enrollmentDto.Enrolled 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating device {deviceId} enrollment");
                return StatusCode(500, new { error = "An unexpected error occurred while updating device enrollment" });
            }
        }

        /// <summary>
        /// Update device curtailment level
        /// </summary>
        /// <param name="deviceId">Device ID</param>
        /// <param name="curtailmentDto">Curtailment level details</param>
        /// <returns>Success or error message</returns>
        [HttpPatch("{deviceId}/curtailment")]
        public async Task<IActionResult> UpdateDeviceCurtailmentLevel(
            string deviceId, 
            [FromBody] DeviceCurtailmentLevelDto curtailmentDto)
        {
            try 
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                // Verify the device exists and belongs to the user
                var existingDevice = await _deviceService.GetDeviceByIdAsync(deviceId);
                
                if (existingDevice == null)
                {
                    return NotFound(new { error = "Device not found" });
                }

                if (existingDevice.UserId != userId)
                {
                    return Forbid();
                }

                var result = await _deviceService.UpdateCurtailmentLevelAsync(
                    deviceId, 
                    curtailmentDto.CurtailmentLevel
                );
                
                if (!result)
                {
                    return BadRequest(new { error = "Failed to update curtailment level" });
                }

                return Ok(new { 
                    message = "Curtailment level updated successfully",
                    curtailmentLevel = curtailmentDto.CurtailmentLevel 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating device {deviceId} curtailment level");
                return StatusCode(500, new { error = "An unexpected error occurred while updating curtailment level" });
            }
        }

        /// <summary>
        /// Get device types and their counts for the current user
        /// </summary>
        /// <returns>Dictionary of device types and their counts</returns>
        [HttpGet("types")]
        public async Task<IActionResult> GetDeviceTypesSummary()
        {
            try 
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Invalid user identifier" });
                }

                var devicesByType = await _deviceService.GetDevicesByTypeAsync(userId);
                
                // Transform the dictionary to include device counts
                var deviceTypeSummary = devicesByType.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => new 
                    {
                        Count = kvp.Value.Count,
                        Devices = kvp.Value.Select(d => new 
                        {
                            d.Id,
                            d.Name,
                            d.Manufacturer,
                            d.Model,
                            d.Status,
                            d.Enrolled,
                            d.CurtailmentLevel
                        })
                    }
                );

                return Ok(deviceTypeSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving device types summary");
                return StatusCode(500, new { error = "An unexpected error occurred while retrieving device types" });
            }
        }

        /// <summary>
        /// Get devices eligible for curtailment
        /// </summary>
        /// <returns>List of devices eligible for curtailment</returns>
        [HttpGet("curtailable")]
        public async Task<IActionResult> GetCurtailableDevices()
        {
            try 
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Invalid user identifier" });
                }

                var curtailableDevices = await _deviceService.GetEligibleDevicesForCurtailmentAsync(userId);
                
                return Ok(curtailableDevices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving curtailable devices");
                return StatusCode(500, new { error = "An unexpected error occurred while retrieving curtailable devices" });
            }
        }

        /// <summary>
        /// Get total curtailment capacity for the user's devices
        /// </summary>
        /// <returns>Total curtailment capacity</returns>
        [HttpGet("curtailment-capacity")]
        public async Task<IActionResult> GetTotalCurtailmentCapacity()
        {
            try 
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Invalid user identifier" });
                }

                var totalCapacity = await _deviceService.GetTotalCurtailmentCapacityAsync(userId);
                
                return Ok(new 
                { 
                    totalCurtailmentCapacity = totalCapacity,
                    unit = "kWh"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total curtailment capacity");
                return StatusCode(500, new { error = "An unexpected error occurred while calculating curtailment capacity" });
            }
        }
    }
}