using System;
/// <summary>
/// Delays an action until its disposed
/// </summary>
public class DeferAction : IDisposable {
    private readonly Action Action;
    public bool Cancel = false;
    public DeferAction(Action action) {
        Action = action;
    }
    public void Dispose() {
        if(!Cancel)
            Action?.Invoke();
    }
}

//Example Usage:
//
//using var d = new DeferAction(() => {
//  /* code */
//});
//
//The code inside of /* */ will run once the scope of the function ends or returns
