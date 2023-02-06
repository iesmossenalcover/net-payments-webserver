namespace Domain.People;

public class Person
{
    public long Id { get; set; }
    public string DocumentId { get; set; } = default!;
    public string Name  { get; set; }  = default!;
    public string Surname1  { get; set; }  = default!;
    public string Surname2  { get; set; }  = default!;
    public string ContactPhone { get; set; }  = default!;
    public string ContactMail  { get; set; }  = default!;
}