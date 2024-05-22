namespace AuthProvider.CamInterface;

public class UserActionResult
{
    public bool FoundUser { get; protected set; }

    public bool OperationSuccess { get; protected set; }

    public static UserActionResult UserNotFound()
    {
        return new UserActionResult
        {
            FoundUser = false,
            OperationSuccess = false,
        };
    }

    public static UserActionResult Unsucessfull()
    {
        return new UserActionResult
        {
            FoundUser = true,
            OperationSuccess = false,
        };
    }

    public static UserActionResult Successfull()
    {
        return new UserActionResult
        {
            FoundUser = true,
            OperationSuccess = true,
        };
    }
}

public class UserActionResult<T> : UserActionResult
{
    public T Output { get; protected set; }

    public static UserActionResult<T> UserNotFound()
    {
        return new UserActionResult<T>
        {
            Output = default,
            FoundUser = false,
            OperationSuccess = false,
        };
    }

    public static UserActionResult<T> Unsuccessful()
    {
        return new UserActionResult<T>
        {
            Output = default,
            FoundUser = true,
            OperationSuccess = false,
        };
    }

    public static UserActionResult<T> Successful(T output)
    {
        return new UserActionResult<T>
        {
            Output = output,
            FoundUser = true,
            OperationSuccess = true,
        };
    }
}