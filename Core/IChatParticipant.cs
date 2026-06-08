using System.Threading.Tasks;
using ConsoleChatApp.Models;

namespace ConsoleChatApp.Core
{
    // Demonstrates Abstraction. Any client or simulated entity that participates
    // in the chat room must implement this contract.
    public interface IChatParticipant
    {
        string Name { get; }
        Task SendMessageAsync(Message message);
        void Disconnect();
    }
}
