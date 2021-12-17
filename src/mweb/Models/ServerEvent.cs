namespace Minustar.Website.Models
{
    public class ServerEvent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(ShortLength)]

        public string ServerVersion { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        [Required]
        [MaxLength(TitleLength)]
        public string Category { get; set; }

        [Required]
        [MaxLength(TitleLength)]
        public string EventType { get; set; }

        public string? Payload { get; set; }

        public ServerEvent()
        {
            ServerVersion= string.Empty;
            Category = "Unknown";
            EventType=string.Empty;
            Payload = null;
        }

        public T? GetPayload<T>()
        {
            try
            {
                if (Payload is null)
                    return default;

                var result = JsonSerializer.Deserialize<T>(Payload);
                return result;
            }
            catch (Exception)
            {
            }

            return default;
        }
    }
}
