using MedicalApptBookingSystem.Controllers;
using MedicalApptBookingSystem.Data;
using MedicalApptBookingSystem.DTO.Requests;
using MedicalApptBookingSystem.Util;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalApptBookingSystemTest.Tests.UsersControllerTests
{
    public class EditUserAsyncTests
    {
        private readonly ApplicationDbContext _context;
        private readonly ConvertToDto _convertToDto;
        private readonly UsersController _controller;

        public EditUserAsyncTests()
        {
            _context = TestDbContextFactory.Create();
            _convertToDto = new ConvertToDto();
            _controller = new UsersController(_context, _convertToDto);
        }

        [Fact]
        public async Task WithValidCredentials_SuccessfullyEditUser()
        {
            // Arrange -- Set up ChangeUserRequest
            // Request to change user Patient1's full name and email
            var request = new ChangeUserRequest
            {
                Id = 1,
                NewFullName = "Test",
                NewEmail = "test@gmail.com"
            };

            // Act -- Pass Dto to controller
            var response = await _controller.EditUserAsync(request);

            // Assert -- Returns OkResult
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal("User has been updated!", okResult.Value);
        }

        [Fact]
        public async Task UsingExistingEmailToEditUser_ReturnBadRequest()
        {
            // Arrange -- Set up requst obj using an email that another User has registered with
            // In this case, "patient2@example.com" has already been saved by another user
            var request = new ChangeUserRequest
            {
                Id = 1,
                NewFullName = "Test",
                NewEmail = "patient2@example.com"
            };

            // Act -- Pass request to controller
            var response = await _controller.EditUserAsync(request);

            // Assert -- Returns BadRequest
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal("This requested new email has already been taken!", badRequestRes.Value);
        }

        [Fact]
        public async Task UsingInvalidEmailToEditUser_ReturnBadRequest()
        {
            // Arrange -- Setup request using an invalid email string
            var request = new ChangeUserRequest
            {
                Id = 1,
                NewFullName = "Test",
                NewEmail = "abc"
            };

            // Act -- Pass request to controller
            var response = await _controller.EditUserAsync(request);

            // Assert -- Returns BadRequest
            var badRequestRes = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal("Invalid email format", badRequestRes.Value);
        }

        [Fact]
        public async Task AttemptingToEditNonexistingUser_ReturnNotFound()
        {
            // Arrange -- Set up request using Id that doesn't pertain to any User
            var request = new ChangeUserRequest
            {
                Id = 999,
                NewFullName = "Test",
                NewEmail = "asdfjkl@gmail.com"
            };

            // Act -- Pass request to controller
            var response = await _controller.EditUserAsync(request);

            // Assert -- Returns NotFound
            var notFoundRes = Assert.IsType<NotFoundObjectResult>(response);
            Assert.Equal("User not found!", notFoundRes.Value);
        }
    }
}
