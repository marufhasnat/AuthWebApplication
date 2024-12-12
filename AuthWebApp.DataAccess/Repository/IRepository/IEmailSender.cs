using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthWebApp.DataAccess.Repository.IRepository
{
    public interface IEmailSender
    {
        Task<bool> EamilSendAsynce(string email, string subject, string message);
    }
}
