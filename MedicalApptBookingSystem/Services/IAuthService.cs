using MedicalApptBookingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalApptBookingSystemTest
{
    public interface IAuthService
    {
        string GenerateToken(User user);
    }
}
