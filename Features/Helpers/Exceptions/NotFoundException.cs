namespace ProductManagement.Features.Helpers.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string entityName, int id)
            : base($"{entityName} with id {id} was not found.")
        {
            EntityName = entityName;
            EntityId = id;
        }

        public string EntityName { get; }
        public int EntityId { get; }
    }
}