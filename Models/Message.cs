using System;
using System.Text.Json.Serialization;

namespace ConsoleChatApp.Models
{
    // The abstract base Message class demonstrating Abstraction and Inheritance.
    // JsonDerivedType attributes allow System.Text.Json to serialize and deserialize
    // polymorphic types over the socket connection.
    [JsonDerivedType(typeof(TextMessage), typeDiscriminator: "text")]
    [JsonDerivedType(typeof(SystemMessage), typeDiscriminator: "system")]
    [JsonDerivedType(typeof(PrivateMessage), typeDiscriminator: "private")]
    public abstract class Message
    {
        public string Sender { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Polymorphic method to format messages for display in the console.
        public abstract string Format();
    }

    // A standard public chat message.
    public class TextMessage : Message
    {
        public string Content { get; set; } = string.Empty;

        public override string Format()
        {
            return $"[{Timestamp:HH:mm:ss}] {Sender}: {Content}";
        }
    }

    // A system notification (e.g., user joined/left).
    public class SystemMessage : Message
    {
        public string Content { get; set; } = string.Empty;

        public override string Format()
        {
            return $"[{Timestamp:HH:mm:ss}] [SYSTEM] {Content}";
        }
    }

    // A private direct message between two participants.
    public class PrivateMessage : Message
    {
        public string Recipient { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public override string Format()
        {
            return $"[{Timestamp:HH:mm:ss}] (Private) {Sender} -> {Recipient}: {Content}";
        }
    }
}
