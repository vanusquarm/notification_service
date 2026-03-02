using System.Threading.Tasks;

namespace GTBStatementService.Services
{
    public interface IEmailService
    {
        Task SendStatementAsync(string receiverEmail, string subject, string body, byte[] attachment, string fileName);
        Task SendStatementAsync(string receiverEmail, string subject, string body, IReadOnlyList<byte[]> pdfFiles);
    }
}
