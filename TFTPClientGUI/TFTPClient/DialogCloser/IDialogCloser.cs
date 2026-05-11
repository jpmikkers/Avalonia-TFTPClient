namespace UIClient.DialogCloser;

public interface IDialogCloser
{
    void Close();
    void Close(object? result);
}
