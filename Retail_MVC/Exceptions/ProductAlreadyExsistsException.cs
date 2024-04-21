namespace Retail_MVC.Exceptions
{
    public class ProductAlreadyExsistsException : ApplicationException
    {
        public ProductAlreadyExsistsException() { }
        public ProductAlreadyExsistsException(string? message) : base(message) { }
    }
}
