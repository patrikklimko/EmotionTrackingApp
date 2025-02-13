namespace DTO;

public class UserReturnDto
{
  public required int Id { get; set; }
  public required string Username { get; set; }
  public required string Email { get; set; }
  public int? Streak { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}