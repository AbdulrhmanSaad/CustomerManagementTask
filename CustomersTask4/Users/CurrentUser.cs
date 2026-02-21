namespace CustomersTask4.Users
{
    public record CurrentUser(string Id,string Name,IEnumerable<string> Roles)
    {
        public bool IsInRole(string role)
        {
            return Roles.Contains(role);
        }
    }
}
