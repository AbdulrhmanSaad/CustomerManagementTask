using System.Security.Claims;

namespace AuthServer.ResultModle
{
    public class Result
    {
        public bool Succeeded { get; set; }
        public List<string> Errors { get; set; } = new();

        public static Result Success() => new Result { Succeeded = true };
        public static Result Failure(IEnumerable<string> errors) =>
            new Result { Succeeded = false, Errors = errors.ToList() };
    }
}
