using MedicalApptBookingSystem.Controllers;
using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.DTO;
using MedicalApptBookingSystem.DTO.Reponses;
using MedicalApptBookingSystem.Models;
using MedicalApptBookingSystem.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MedicalApptBookingSystemTest.Tests.UsersControllerTests
{
    public class GetAllUsersAsyncTests
    {

        private readonly ApplicationDbContext _context;
        private readonly ConvertToDto _convertToDto;
        private readonly UsersController _controller;

        public GetAllUsersAsyncTests()
        {
            _context = TestDbContextFactory.Create();
            _convertToDto = new ConvertToDto();
            _controller = new UsersController(_context, _convertToDto);
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsAllUsers()
        {
            // Act
            var result = await _controller.GetAllUsersAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);       // Returns Ok
            var users = Assert.IsType<List<UserDto>>(okResult.Value);   // OkResult obj contains list of UsersDto
            Assert.Equal(4, users.Count());                             // There are only 4 in the list
        }
    }
}
