namespace Domain.Model
{
  public class HttpResponse<T>
  {
    public int status { get; set; }
    public bool success { get; set; }
    public string message { get; set; }
    public T data { get; set; }
    public object error { get; set; }
  }
}