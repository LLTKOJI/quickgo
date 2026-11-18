namespace quickgo.API.Security;

public class JwtOptions {
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "quickgo";
    public string Audience { get; set; } = "quickgo";
    public int ExpiraMinutos { get; set; } = 60;
}
