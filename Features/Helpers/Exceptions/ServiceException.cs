namespace ProductManagement.Features.Helpers.Exceptions
{
    public class ServiceException : Exception
    {
        public ServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}